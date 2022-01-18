using System;
using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace GravityOverhaul
{
    public class FallDamageModule : LevelModule
    {
        public bool useExperimentalFallDmgManaCost;
        public float minFallDmgManaCost;
        public float maxFallDmgManaCost;
        public float fallDmgMinMagnitude;
        public float fallDmgMaxMagnitude;
        
        public override IEnumerator OnLoadCoroutine()
        {
            if (useExperimentalFallDmgManaCost)
                EventManager.onPossess += PossessionEvent;
            else 
                Player.fallDamage = false;
            return base.OnLoadCoroutine();
        }

        private void PossessionEvent(Creature creature, EventTime eventTime)
        {
            if (eventTime == EventTime.OnEnd)
                Player.local.locomotion.OnGroundEvent += OnGroundEvent;
        }

        private void OnGroundEvent(Vector3 groundpoint, Vector3 velocity, Collider groundcollider)
        {
            if (GravityMainModule.IsCastingGravity(Player.currentCreature.mana.casterLeft) 
                || GravityMainModule.IsCastingGravity(Player.currentCreature.mana.casterRight))
            {
                if (velocity.magnitude >= fallDmgMinMagnitude)
                {
                    float manaCost;

                    if (velocity.magnitude >= fallDmgMaxMagnitude)
                        manaCost = maxFallDmgManaCost;
                    else
                        manaCost = (float)Math.Pow(Math.Pow(maxFallDmgManaCost - minFallDmgManaCost + 1, 1 / (fallDmgMaxMagnitude - fallDmgMinMagnitude)), velocity.magnitude - fallDmgMinMagnitude) + 9;

                    if (Player.currentCreature.mana.currentMana < manaCost)
                    {
                        float dmgPercent = Mathf.InverseLerp(0f, manaCost, Player.currentCreature.mana.currentMana);
                        float fallDamage = Player.currentCreature.data.playerFallDamageCurve.Evaluate(velocity.magnitude) * (1 - dmgPercent);
                        CollisionInstance fallCollision = new CollisionInstance(new DamageStruct(DamageType.Blunt, fallDamage), null, null);
                        Player.currentCreature.Damage(fallCollision);

                        Player.currentCreature.mana.ConsumeMana(Player.currentCreature.mana.currentMana - 1);
                    }
                    else
                        Player.currentCreature.mana.ConsumeMana(manaCost);
                }
            }
        }
    }
}