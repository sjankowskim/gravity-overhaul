using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using ThunderRoad;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GravityOverhaul
{
    public class GravityArrowModule : LevelModule
    {
        /*public static SpellMergeGravity gravityMergeData;
        private static GravityOverhaulData data;
        public static float? arrowShockwaveTimer;

        public static EffectInstance bubbleEffectOnArrow;
        public static Item imbuedArrow;
        public static Collider arrowHead;
        public static Vector3[] randomTorqueArray;
        public static float? bubbleEffectTimer;
        public Vector3 randomTorqueRange;
        
        // GRAVITY ARROW SETTINGS
        public float arrowShockwaveCD;
        public float arrowDestableImbueCost;
        public float arrowShockwaveImbueCost;
        public float arrowShockwaveMult;
        public float arrowShockwaveMinVelocity;
        public float arrowShockwaveMaxVelocity;
        public float arrowShockwaveMinRadius;
        public float arrowShockwaveMaxRadius;
        public float arrowShockwaveMinForce;
        public float arrowShockwaveMaxForce;

        // GRAVITY BUBBLE SETTINGS
        public float bubbleEffectDuration;
        public float bubbleDuration;
        public float bubbleImbueCost;
        public float bubbleSizeMult;

        public override IEnumerator OnLoadCoroutine()
        {
            gravityMergeData = Catalog.GetData<SpellMergeGravity>("GravityMerge");
            data = GameManager.local.gameObject.AddComponent<GravityOverhaulData>();
            InitValues();
            new Harmony("Penetrate").PatchAll();
            randomTorqueArray = new Vector3[5];
            for (int i = 0; i < randomTorqueArray.Length; i++)
                randomTorqueArray[i] = new Vector3(Random.Range(-randomTorqueRange.x, randomTorqueRange.x), Random.Range(-randomTorqueRange.y, randomTorqueRange.y), Random.Range(-randomTorqueRange.z, randomTorqueRange.z));

            return base.OnLoadCoroutine();
        }

        private void InitValues()
        {
            data.arrowShockwaveCD = arrowShockwaveCD;
            data.arrowDestableImbueCost = arrowDestableImbueCost;
            data.arrowShockwaveImbueCost = arrowShockwaveImbueCost;
            data.arrowShockwaveMult = arrowShockwaveMult;
            data.arrowShockwaveMinVelocity = arrowShockwaveMinVelocity;
            data.arrowShockwaveMaxVelocity = arrowShockwaveMaxVelocity;
            data.arrowShockwaveMinRadius = arrowShockwaveMinRadius;
            data.arrowShockwaveMaxRadius = arrowShockwaveMaxRadius;
            data.arrowShockwaveMinForce = arrowShockwaveMinForce;
            data.arrowShockwaveMaxForce = arrowShockwaveMaxForce;

            data.bubbleDuration = bubbleDuration;
            data.bubbleImbueCost = bubbleImbueCost;
            data.bubbleSizeMult = bubbleSizeMult;
        }

        public override void Update()
        {
            base.Update();
            if (bubbleEffectTimer != null)
            {
                bubbleEffectTimer -= Time.deltaTime;
                if (bubbleEffectTimer < 0f)
                    bubbleEffectTimer = null;
            }
            
            if (arrowShockwaveTimer != null)
            {
                arrowShockwaveTimer -= Time.deltaTime;
                if (arrowShockwaveTimer < 0f)
                    arrowShockwaveTimer = null;
            }
            
            // if the player exists...
            if (Player.currentCreature)
            {
                leftFireGrav = IsCastingGravity(Player.currentCreature.mana.casterLeft);
                rightFireGrav = IsCastingGravity(Player.currentCreature.mana.casterRight);

                if (imbuedArrow == null)
                {
                    foreach (Item item in Item.allActive)
                    {
                        if (item.data.moduleAI?.weaponClass == ItemModuleAI.WeaponClass.Arrow && item.imbues[0].spellCastBase?.id == "Gravity" && item.imbues[0].CanConsume(bubbleImbueCost))
                        {
                            imbuedArrow = item;

                            EffectData chargeOnArrowEffectData = Catalog.GetData<EffectData>(gravityMergeData.chargeEffectId);

                            if (chargeOnArrowEffectData != null)
                            {
                                // Find the arrow head collider
                                foreach (ColliderGroup group in item.colliderGroups)
                                    if (group.imbue != null)
                                        arrowHead = group.colliders[0];

                                bubbleEffectOnArrow = chargeOnArrowEffectData.Spawn(arrowHead.transform);
                                bubbleEffectOnArrow.SetIntensity(0.8f);
                                bubbleEffectOnArrow.Play();
                            }

                            if (Imbue.infiniteImbue)
                                bubbleEffectTimer = bubbleEffectDuration;
                            break;
                        }
                    }
                } else {
                    if (!imbuedArrow.imbues[0].CanConsume(bubbleImbueCost) || (Imbue.infiniteImbue && bubbleEffectTimer == null))
                    {
                        if (Imbue.infiniteImbue)
                            imbuedArrow.imbues[0].energy = bubbleImbueCost - 1;
                        imbuedArrow = null;
                        bubbleEffectOnArrow.Despawn();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Damager), "Penetrate")]
        class PenetrateFix
        {
            public static Trigger captureTrigger;
            public static List<CollisionHandler> capturedObjects = new List<CollisionHandler>();
            
            public static void Postfix(CollisionInstance collisionInstance, bool usePressure)
            {
                Item weapon = collisionInstance.sourceCollider.GetComponentInParent<Item>();
                Imbue imbueOnWeapon = collisionInstance.sourceColliderGroup.imbue;

                if (imbueOnWeapon.spellCastBase?.id == "Gravity" && arrowShockwaveTimer == null && collisionInstance.impactVelocity.magnitude > data.arrowShockwaveMinVelocity)
                {
                    Collider target = collisionInstance.targetCollider;
                    bool penetratedGround = !target.GetComponentInParent<Creature>() && !target.GetComponentInParent<Item>();

                    if (weapon.data.moduleAI.weaponClass == ItemModuleAI.WeaponClass.Arrow)
                    {
                        if (imbuedArrow && (target.GetComponentInParent<Creature>() || penetratedGround) &&
                            weapon == imbuedArrow && !gravityMergeData.bubbleActive)
                            Player.currentCreature.mana.StartCoroutine(SpawnGravityBubble(collisionInstance.contactPoint, data.bubbleDuration));
                        else if (target.GetComponentInParent<Creature>() && imbueOnWeapon.CanConsume(data.arrowDestableImbueCost))
                            GravityOnArrow(collisionInstance);
                        else if (penetratedGround && imbueOnWeapon.CanConsume(data.arrowShockwaveImbueCost))
                        {
                            collisionInstance.sourceColliderGroup.imbue.ConsumeInstant(data.arrowShockwaveImbueCost);
                            GravityMainModule.SpawnShockwave(collisionInstance.contactPoint, 
                                collisionInstance.contactNormal, 
                                collisionInstance.sourceColliderGroup.transform.up, 
                                collisionInstance.impactVelocity, 
                                data,
                                true);
                        }
                    }

                    arrowShockwaveTimer = data.arrowShockwaveCD;
                }
            }
            
             private static IEnumerator SpawnGravityBubble(Vector3 contactPoint, float duration)
            {
                gravityMergeData.bubbleActive = true;
                bubbleEffectOnArrow.Despawn();
                imbuedArrow.imbues[0].ConsumeInstant(grav.bubbleImbueCost);

                EffectData bubbleEffectData = Catalog.GetData<EffectData>(gravityMergeData.bubbleEffectId);
                EffectInstance bubbleEffect = null;
                if (bubbleEffectData != null)
                {
                    bubbleEffect = bubbleEffectData.Spawn(contactPoint, Quaternion.identity);
                    bubbleEffect.SetIntensity(0.0f);
                    bubbleEffect.Play(0);
                }
                yield return new WaitForFixedUpdate();
                StartCapture();
                if (Imbue.infiniteImbue)
                    imbuedArrow.imbues[0].energy = grav.bubbleImbueCost - 1;
                imbuedArrow = null;
                captureTrigger.transform.SetParent(null);
                float startTime = Time.time;
                while (Time.time - startTime < duration)
                {
                    if (!captureTrigger)
                        yield break;
                    else
                    {
                        float num = gravityMergeData.bubbleScaleCurveOverTime.Evaluate((Time.time - startTime) / duration) * grav.bubbleSizeMult;
                        captureTrigger.SetRadius(num * gravityMergeData.bubbleEffectMaxScale * 0.5f);
                        bubbleEffect?.SetIntensity(num);
                        yield return null;
                    }
                }
                bubbleEffect?.End(false, -1f);
                StopCapture();
                gravityMergeData.bubbleActive = false;
            }

            private static void StartCapture()
            {
                captureTrigger = new GameObject("GravityTrigger").AddComponent<Trigger>();
                captureTrigger.transform.SetParent(arrowHead.transform);
                captureTrigger.transform.localPosition = Vector3.zero;
                captureTrigger.transform.localRotation = Quaternion.identity;
                captureTrigger.SetCallback(new Trigger.CallBack(OnTrigger));
                captureTrigger.SetLayer(GameManager.GetLayer(LayerName.MovingObject));
                captureTrigger.SetRadius(0.0f);
                captureTrigger.SetActive(true);
            }

            private static void OnTrigger(Collider other, bool enter)
            {
                if (other.attachedRigidbody && !other.attachedRigidbody.isKinematic)
                {
                    CollisionHandler component = other.attachedRigidbody.GetComponent<CollisionHandler>();
                    if (!component || component.item && component.item.data.type == ItemData.Type.Body)
                        return;
                    if (enter)
                    {
                        if (!component.item && (!gravityMergeData.bubbleActive || !component.ragdollPart || !(component.ragdollPart.ragdoll != Player.currentCreature.ragdoll)))
                            return;
                        if (component.ragdollPart && (component.ragdollPart.ragdoll.creature.state == Creature.State.Alive || component.ragdollPart.ragdoll.standingUp))
                        {
                            component.ragdollPart.ragdoll.SetState(Ragdoll.State.Destabilized);
                            component.ragdollPart.ragdoll.AddNoStandUpModifier(gravityMergeData);
                        }
                        component.SetPhysicModifier(gravityMergeData, 2, 0.0f, 1f, gravityMergeData.liftDrag, -1f, null);
                        Vector3 force = -Physics.gravity.normalized * Mathf.Lerp(gravityMergeData.liftMinForce, gravityMergeData.liftMaxForce, UnityEngine.Random.Range(0.0f, 1f));
                        if (component.ragdollPart)
                            force *= gravityMergeData.liftRagdollForceMultiplier;
                        component.rb.AddForce(force, ForceMode.VelocityChange);
                        component.rb.AddTorque(randomTorqueArray[UnityEngine.Random.Range(0, 5)], ForceMode.VelocityChange);
                        capturedObjects.Add(component);
                    }
                    else
                    {
                        component.RemovePhysicModifier(gravityMergeData);
                        if (component.ragdollPart && component.ragdollPart.ragdoll != Player.currentCreature.ragdoll)
                            component.ragdollPart.ragdoll.RemoveNoStandUpModifier(gravityMergeData);
                        capturedObjects.Remove(component);
                    }
                }
            }

            private static void StopCapture()
            {
                captureTrigger.SetActive(false);
                for (int i = capturedObjects.Count - 1; i >= 0; --i)
                {
                    capturedObjects[i].RemovePhysicModifier(gravityMergeData);
                    if (capturedObjects[i].ragdollPart && capturedObjects[i].ragdollPart.ragdoll != Player.currentCreature.ragdoll)
                        capturedObjects[i].ragdollPart.ragdoll.RemoveNoStandUpModifier(gravityMergeData);
                    capturedObjects.RemoveAt(i);
                }
                UnityEngine.Object.Destroy(captureTrigger.gameObject);
            }

            private static void GravityOnArrow(CollisionInstance collisionInstance)
            {
                EffectData graviyThrowEffectData = Catalog.GetData<EffectData>(gravityData.throwEffectId, true);
                Imbue imbue = collisionInstance.sourceColliderGroup.imbue;

                if (imbue.energy >= grav.minImbueToDestable && graviyThrowEffectData != null)
                {
                    EffectInstance effectInstance = graviyThrowEffectData.Spawn(collisionInstance.sourceCollider.GetComponentInParent<Item>().transform, true, Array.Empty<Type>());
                    effectInstance.SetTarget(collisionInstance.sourceCollider.GetComponentInParent<Item>().transform);
                    effectInstance.Play();

                    BrainData brainData = collisionInstance.targetCollider.GetComponentInParent<Creature>().brain.instance;
                    brainData.creature.TryPush(Creature.PushType.Magic, collisionInstance.impactVelocity, (int) Mathf.Round(Mathf.Lerp(0.0f, (float) gravityData.level.castThrow, Mathf.InverseLerp(this.halfSphereRadius, 0.0f, num))), (RagdollPart.Type) 0);
                }
            }
        }*/
    }
}