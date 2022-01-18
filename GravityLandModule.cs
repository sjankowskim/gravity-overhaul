using System.Collections;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace GravityOverhaul
{
    public class GravityLandModule : LevelModule
    {
        public float shockwaveOneHandMult;
        public float shockwaveOneHandManaCost;
        public float shockwaveTwoHandMult;
        public float shockwaveTwoHandManaCost;
        public float shockwaveMinVelocity;
        public float shockwaveMaxVelocity;
        public float shockwaveMinRadius;
        public float shockwaveMaxRadius;
        public float shockwaveMinForce;
        public float shockwaveMaxForce;
        
        private GravityOverhaulData data;
        private bool canShockwave;

        public override IEnumerator OnLoadCoroutine()
        {
            data = GameManager.local.gameObject.AddComponent<GravityOverhaulData>();
            EventManager.onPossess += PossessionEvent;
            return base.OnLoadCoroutine();
        }

        private void InitValues()
        {
            data.shockwaveOneHandMult = shockwaveOneHandMult;
            data.shockwaveOneHandManaCost = shockwaveOneHandManaCost;
            data.shockwaveTwoHandMult = shockwaveTwoHandMult;
            data.shockwaveTwoHandManaCost = shockwaveTwoHandManaCost;
            data.shockwaveMinVelocity = shockwaveMinVelocity;
            data.shockwaveMaxVelocity = shockwaveMaxVelocity;
            data.shockwaveMinRadius = shockwaveMinRadius;
            data.shockwaveMaxRadius = shockwaveMaxRadius;
            data.shockwaveMinForce = shockwaveMinForce;
            data.shockwaveMaxForce = shockwaveMaxForce;
        }

        public override void Update()
        {
            base.Update();
            if (Player.currentCreature && !Player.local.locomotion.isGrounded)
            {
                canShockwave = (PlayerControl.GetHand(Side.Left).gripPressed &&
                                GravityMainModule.IsCastingGravity(Player.currentCreature.mana.casterLeft) &&
                                Player.currentCreature.equipment.GetHeldWeapon(Side.Left) == null) ||
                               (PlayerControl.GetHand(Side.Right).gripPressed &&
                                GravityMainModule.IsCastingGravity(Player.currentCreature.mana.casterRight) &&
                                Player.currentCreature.equipment.GetHeldWeapon(Side.Right) == null);
            }
            InitValues();
        }

        private void PossessionEvent(Creature creature, EventTime eventTime)
        {
            if (eventTime == EventTime.OnEnd)
                Player.local.locomotion.OnGroundEvent += OnGroundEvent;
        }

        private void OnGroundEvent(Vector3 groundPoint, Vector3 velocity, Collider groundcollider)
        {
            if (canShockwave && velocity.magnitude >= shockwaveMinVelocity) 
                GameManager.local.StartCoroutine(ShockwaveCoroutine(
                    groundPoint, 
                    new Vector3(0f, velocity.y, 0f),
                    new Vector3(),
                    velocity));
            canShockwave = false;        
        }
        
        private IEnumerator ShockwaveCoroutine(Vector3 contactPoint, Vector3 contactNormal, Vector3 contactNormalUpward, Vector3 impactVelocity)
        {
            SpellCastGravity gravityData = Catalog.GetData<SpellCastGravity>("Gravity");
            EffectData imbueHitGroundEffectData = Catalog.GetData<EffectData>(gravityData.imbueHitGroundEffectId);
            float t;
            float manaCost;
            float explosionRadius, explosionForce;
            
            if (GravityMainModule.IsCastingGravity(Player.currentCreature.mana.casterLeft)
                && GravityMainModule.IsCastingGravity(Player.currentCreature.mana.casterRight))
            {
                t = data.shockwaveTwoHandMult;
                manaCost = data.shockwaveTwoHandManaCost;
            }
            else
            {
                t = data.shockwaveOneHandManaCost;
                manaCost = data.shockwaveOneHandManaCost;
            }

            if (Player.currentCreature.mana.currentMana >= manaCost)
                Player.currentCreature.mana.ConsumeMana(manaCost);
            else
            {
                t *= Mathf.InverseLerp(0, manaCost, Player.currentCreature.mana.currentMana);
                Player.currentCreature.mana.ConsumeMana(Player.currentCreature.mana.currentMana - 1);
            }

            t *= Mathf.InverseLerp(data.shockwaveMinVelocity, data.shockwaveMaxVelocity, impactVelocity.magnitude);
            explosionRadius = Mathf.Lerp(data.shockwaveMinRadius, data.shockwaveMaxRadius, t);
            explosionForce = Mathf.Lerp(data.shockwaveMinForce, data.shockwaveMaxForce, t);

            if (imbueHitGroundEffectData != null)
            {
                EffectInstance effectInstance = imbueHitGroundEffectData.Spawn(contactPoint, Quaternion.LookRotation(contactNormal));
                effectInstance.Play();
                effectInstance.SetIntensity(t);
            }

            List<Creature> creaturesPushed = new List<Creature>();
            List<Rigidbody> rigidbodiesPushed = new List<Rigidbody>();
            Collider[] sphereContacts = Physics.OverlapSphere(contactPoint, Mathf.Lerp(data.shockwaveMinRadius, data.shockwaveMaxRadius, t), gravityData.pushLayerMask);
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