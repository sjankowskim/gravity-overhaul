using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace GravityOverhaul
{
    public class GravityMainModule : LevelModule
    {
        public override IEnumerator OnLoadCoroutine()
        {
            Debug.Log("(Gravity Overhaul) Loaded successfully!");
            return base.OnLoadCoroutine();
        }

        public static bool IsCastingGravity(SpellCaster spellCaster)
        {
            if (spellCaster.spellInstance != null && spellCaster.spellInstance.id.Equals("Gravity"))
                return spellCaster.isFiring;
            return false;
        }
        
        /*
        public static void SpawnShockwave(Vector3 contactPoint, Vector3 contactNormal, Vector3 contactNormalUpward, Vector3 impactVelocity, GravityOverhaulData data, bool isMini)
        {
            SpellCastGravity gravityData = Catalog.GetData<SpellCastGravity>("Gravity");
            EffectData imbueHitGroundEffectData = Catalog.GetData<EffectData>(gravityData.imbueHitGroundEffectId);
            // t only ever starts at not 1 if its a gravity shockwave on penetrating arrow
            float t = miniShockwaveMult;
            float explosionRadius, explosionForce;
            Collider[] sphereContacts;

            // Only non-mini shockwaves (i.e. shockwaves caused by the player, not weapons) should drain player mana
            if (!isMini)
            {
                float manaCost = data.shockwaveTwoHandManaCost;
                if (!IsCastingGravity(Player.currentCreature.mana.casterLeft) || !IsCastingGravity(Player.currentCreature.mana.casterRight))
                {
                    manaCost = data.shockwaveOneHandManaCost; // Feature: one-handed shockwave has reduced mana cost
                    t = data.shockwaveOneHandMult; // Feature: one-handed shockwave is weaker than two-handed
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
                sphereContacts = Physics.OverlapSphere(contactPoint, Mathf.Lerp(data.shockwaveMinRadius, data.shockwaveMaxRadius, t), gravityData.pushLayerMask);
            }
            else
            {
                t *= Mathf.InverseLerp(data.miniShockwaveMinVelocity, data.miniShockwaveMaxVelocity, impactVelocity.magnitude);
                explosionRadius = Mathf.Lerp(data.miniShockwaveMinRadius, data.miniShockwaveMaxRadius, t);
                explosionForce = Mathf.Lerp(data.miniShockwaveMinForce, data.miniShockwaveMaxForce, t);
                sphereContacts = Physics.OverlapSphere(contactPoint, Mathf.Lerp(data.miniShockwaveMinRadius, data.miniShockwaveMaxRadius, t), gravityData.pushLayerMask);
            }

            // resets miniShockwaveMult for other instances of shockwave
            miniShockwaveMult = 1.0f;

            if (imbueHitGroundEffectData != null)
            {
                EffectInstance effectInstance;
                if (!isMini)
                    effectInstance = imbueHitGroundEffectData.Spawn(contactPoint, Quaternion.LookRotation(contactNormal));
                else
                    effectInstance = imbueHitGroundEffectData.Spawn(contactPoint, Quaternion.LookRotation(-contactNormal, contactNormalUpward));
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
                                component.ragdoll.creature.TryPush(Creature.PushType.Magic, (component.ragdoll.rootPart.transform.position - contactPoint).normalized, gravityData.level.crystalShockwave);                                creaturesPushed.Add(component.ragdoll.creature);
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
        }*/
    }
}