using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using ThunderRoad;
using UnityEngine;

namespace GravityOverhaul
{
    public class GravityWeaponModule : LevelModule
    {
        public float wepShockwaveCD;
        public float wepShockwaveMult;
        public float wepShockwaveImbueCost;
        public float wepShockwaveMinVelocity;
        public float wepShockwaveMaxVelocity;
        public float wepShockwaveMinRadius;
        public float wepShockwaveMaxRadius;
        public float wepShockwaveMinForce;
        public float wepShockwaveMaxForce;
        
        private static GravityOverhaulData data;
        public static float? wepShockwaveTimer;

        public override IEnumerator OnLoadCoroutine()
        {
            data = GameManager.local.gameObject.AddComponent<GravityOverhaulData>();
            new Harmony("OnImbueCollisionStart").PatchAll();
            new Harmony("Penetrate").PatchAll();

            return base.OnLoadCoroutine();
        }
        
        private void InitValues()
        {
            data.wepShockwaveCD = wepShockwaveCD;
            data.wepShockwaveMult = wepShockwaveMult;
            data.wepShockwaveImbueCost = wepShockwaveImbueCost;
            data.wepShockwaveMinVelocity = wepShockwaveMinVelocity;
            data.wepShockwaveMaxVelocity = wepShockwaveMaxVelocity;
            data.wepShockwaveMinRadius = wepShockwaveMinRadius;
            data.wepShockwaveMaxRadius = wepShockwaveMaxRadius;
            data.wepShockwaveMinForce = wepShockwaveMinForce;
            data.wepShockwaveMaxForce = wepShockwaveMaxForce;
        }

        public override void Update()
        {
            base.Update();
            if (wepShockwaveTimer != null)
            {
                wepShockwaveTimer -= Time.deltaTime;
                if (wepShockwaveTimer < 0f)
                    wepShockwaveTimer = null;
            }
            InitValues();
        }

        /*===========================================
         * BLUNT WEAPONS
         ===========================================*/
        [HarmonyPatch(typeof(SpellCastGravity), "OnImbueCollisionStart")]
        class CollisionFix
        {
            public static void Postfix(CollisionInstance collisionInstance)
            {
                Collider target = collisionInstance.targetCollider;
                Imbue imbueOnWep = collisionInstance.sourceColliderGroup.imbue;
                Item weapon = collisionInstance.sourceCollider.GetComponentInParent<Item>();
                bool validCollision = !target.GetComponentInParent<Creature>() && !target.GetComponentInParent<Item>();

                // if weapon exists && is not on cooldown && is blunt && target collider is ground && impact velocity is large enough && weapon has enough imbue
                if (weapon && 
                    wepShockwaveTimer == null &&
                    IsBlunt(weapon.data) && 
                    validCollision &&
                    collisionInstance.impactVelocity.magnitude > data.wepShockwaveMinVelocity &&
                    imbueOnWep.CanConsume(data.wepShockwaveImbueCost))
                {
                    StartShockwave(collisionInstance);
                }
            }

            private static bool IsBlunt(ItemData data)
            {
                foreach (ItemData.Damager damager in data.damagers)
                    if (!damager.transformName.Contains("Handle") && !damager.damagerID.Contains("Handle") && damager.transformName.Equals("Blunt"))
                        return true;
                return false;
            }
        }
        
        /*===========================================
         * SHARP WEAPONS
         ===========================================*/
        [HarmonyPatch(typeof(Damager), "Penetrate")]
        class PenetrateFix
        {
            public static void Postfix(CollisionInstance collisionInstance, bool usePressure)
            {
                Item weapon = collisionInstance.sourceCollider.GetComponentInParent<Item>();
                Imbue imbueOnWeapon = collisionInstance.sourceColliderGroup.imbue;
                Collider target = collisionInstance.targetCollider;
                bool penetratedGround = !target.GetComponentInParent<Creature>() && !target.GetComponentInParent<Item>();

                if (imbueOnWeapon.spellCastBase?.id == "Gravity"
                    && weapon.data.moduleAI.weaponClass != ItemModuleAI.WeaponClass.Arrow
                    && penetratedGround
                    && wepShockwaveTimer == null
                    && collisionInstance.impactVelocity.magnitude > data.wepShockwaveMinVelocity
                    && imbueOnWeapon.CanConsume(data.wepShockwaveImbueCost))
                {
                    StartShockwave(collisionInstance);
                }
            }
        }

        private static void StartShockwave(CollisionInstance collisionInstance)
        {
            collisionInstance.sourceColliderGroup.imbue.ConsumeInstant(data.wepShockwaveImbueCost);
            GameManager.local.StartCoroutine(ShockwaveCoroutine(
                collisionInstance.contactPoint, 
                collisionInstance.contactNormal,
                collisionInstance.sourceColliderGroup.transform.up,
                collisionInstance.impactVelocity));
            wepShockwaveTimer = data.wepShockwaveCD;
        }
        
        private static IEnumerator ShockwaveCoroutine(Vector3 contactPoint, Vector3 contactNormal, Vector3 contactNormalUpward, Vector3 impactVelocity)
        {
            SpellCastGravity gravityData = Catalog.GetData<SpellCastGravity>("Gravity");
            EffectData imbueHitGroundEffectData = Catalog.GetData<EffectData>(gravityData.imbueHitGroundEffectId);
            float t = data.wepShockwaveMult;
            float explosionRadius, explosionForce;
            Collider[] sphereContacts;
            
            t *= Mathf.InverseLerp(data.wepShockwaveMinVelocity, data.wepShockwaveMaxVelocity, impactVelocity.magnitude);
            explosionRadius = Mathf.Lerp(data.wepShockwaveMinRadius, data.wepShockwaveMaxRadius, t);
            explosionForce = Mathf.Lerp(data.wepShockwaveMinForce, data.wepShockwaveMaxForce, t);
            sphereContacts = Physics.OverlapSphere(contactPoint, Mathf.Lerp(data.wepShockwaveMinRadius, data.wepShockwaveMaxRadius, t), gravityData.pushLayerMask);

            if (imbueHitGroundEffectData != null)
            {
                EffectInstance effectInstance = imbueHitGroundEffectData.Spawn(contactPoint, Quaternion.LookRotation(-contactNormal, contactNormalUpward));
                effectInstance.Play();
                effectInstance.SetIntensity(t);
            }

            List<Creature> creaturesPushed = new List<Creature>();
            List<Rigidbody> rigidbodiesPushed = new List<Rigidbody>();
            float waveDistance = 0.0f;
            while (waveDistance < explosionRadius)
            {
                waveDistance += gravityData.imbueHitGroundWaveSpeed * gravityData.imbueHitGroundWaveUpdateRate;
                foreach (Collider collider in sphereContacts)
                {
                    if (collider.attachedRigidbody && !collider.attachedRigidbody.isKinematic && Vector3.Distance(contactPoint, collider.transform.position) < waveDistance)
                    {
                        if (collider.attachedRigidbody.gameObject.layer == GameManager.GetLayer(LayerName.NPC) || collider.attachedRigidbody.gameObject.layer == GameManager.GetLayer(LayerName.Ragdoll))
                        {
                            RagdollPart component = collider.attachedRigidbody.gameObject.GetComponent<RagdollPart>();
                            if (component && !creaturesPushed.Contains(component.ragdoll.creature))
                            {
                                component.ragdoll.creature.TryPush(Creature.PushType.Magic, (component.ragdoll.rootPart.transform.position - contactPoint).normalized, gravityData.level.crystalShockwave);                                
                                creaturesPushed.Add(component.ragdoll.creature);
                            }
                        }
                        if (collider.attachedRigidbody.gameObject.layer != GameManager.GetLayer(LayerName.NPC) && !rigidbodiesPushed.Contains(collider.attachedRigidbody))
                        {
                            collider.attachedRigidbody.AddExplosionForce(explosionForce, contactPoint, explosionRadius, gravityData.imbueHitGroundExplosionUpwardModifier, gravityData.imbueHitGroundForceMode);
                            rigidbodiesPushed.Add(collider.attachedRigidbody);
                        }
                    }
                }
            }
            yield return new WaitForSeconds(gravityData.imbueHitGroundWaveUpdateRate);
        }
    }
}