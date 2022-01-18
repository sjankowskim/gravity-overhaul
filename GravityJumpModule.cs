using System.Collections;
using HarmonyLib;
using ThunderRoad;
using UnityEngine;

namespace GravityOverhaul
{
    public class GravityJumpModule : LevelModule
    {
        public float minThrowVelocity;
        public float maxThrowVelocity;
        public float yGravMult;
        public float xzGravMult;
        public float twoHandGravMult;
        public float yAirBoostMult;
        public float bubbleMult;
        public bool parkourMode;
        
        private static bool inGravBubble;
        private static GravityOverhaulData data;
        private static float? twoHandListenTimer;
        private static bool leftFire, rightFire;
        private float twoHandListenDuration = 0.1f;

        public override IEnumerator OnLoadCoroutine()
        {
            data = GameManager.local.gameObject.AddComponent<GravityOverhaulData>();
            new Harmony("Throw").PatchAll();
            new Harmony("OnTrigger").PatchAll();
            return base.OnLoadCoroutine();
        }

        private void InitValues()
        {
            data.minThrowVelocity = minThrowVelocity;
            data.maxThrowVelocity = maxThrowVelocity;
            data.yGravMult = yGravMult;
            data.xzGravMult = xzGravMult;
            data.twoHandGravMult = twoHandGravMult;
            data.yAirBoostMult = yAirBoostMult;
            data.bubbleMult = bubbleMult;
            data.twoHandListenDuration = twoHandListenDuration;
            data.parkourMode = parkourMode;
        }

        public override void Update()
        {
            base.Update();
            
            if (twoHandListenTimer != null)
            {
                twoHandListenTimer -= Time.deltaTime;
                if (twoHandListenTimer < 0f)
                    twoHandListenTimer = null;
            }

            if (Player.currentCreature)
            {
                leftFire = GravityMainModule.IsCastingGravity(Player.currentCreature.mana.casterLeft);
                rightFire = GravityMainModule.IsCastingGravity(Player.currentCreature.mana.casterRight);
            }
            InitValues();
        }

        [HarmonyPatch(typeof(SpellCastGravity), "Throw")]
        class ThrowFix
        {
            public static bool Prefix(Vector3 velocity)
            {
                if ((PlayerControl.GetHand(Side.Left).gripPressed && !Player.local.creature.equipment.GetHeldWeapon(Side.Left)) ||
                    (PlayerControl.GetHand(Side.Right).gripPressed && !Player.local.creature.equipment.GetHeldWeapon(Side.Right)) ||
                    data.parkourMode)
                {
                    if (-velocity.y > data.minThrowVelocity)
                    {
                        Vector3 gravityJump;

                        // Feature: Parkour mode adds the x & z component to their gravity jump, allowing forward, backward, and side jumps
                        if (data.parkourMode)
                            gravityJump = new Vector3(-velocity.x * data.xzGravMult, data.yGravMult, -velocity.z * data.xzGravMult);
                        else
                            gravityJump = new Vector3(0f, data.yGravMult, 0f);

                        if (Player.local.locomotion.isGrounded || twoHandListenTimer != null)
                        {
                            // Feature: If parkour mode is enabled, the player can choose to do a vertical-only 
                            // gravity jump by holding either of the grips during the gravity jump
                            if (data.parkourMode)
                            {
                                if ((PlayerControl.GetHand(Side.Left).gripPressed && leftFire) || (PlayerControl.GetHand(Side.Right).gripPressed && rightFire))
                                    gravityJump.Set(0f, gravityJump.y, 0f);
                            }


                            // Feature: Maximum gravity so the player doesn't physically harm themselves trying to get absurdly high jumps
                            if (-velocity.y > data.maxThrowVelocity)
                                gravityJump[1] *= data.maxThrowVelocity;
                            else
                                gravityJump[1] *= -velocity.y;

                            // Feature: Gravity bubble multiplies the effects of a gravity jump
                            if (inGravBubble)
                                gravityJump *= data.bubbleMult;

                            // Feature: Player can two-handed gravity jump, resulting in a stronger jump
                            // (waits duration of listenTimer to consider a 2nd gravity jump a grounded jump)
                            if (twoHandListenTimer == null)
                                twoHandListenTimer = data.twoHandListenDuration;
                            else
                                gravityJump[1] *= data.twoHandGravMult;
                        }
                        // Feature: Vertical component of gravity jump is weaker when not grounded
                        else
                            gravityJump[1] *= data.yAirBoostMult * -velocity.y;
                        Player.local.locomotion.rb.AddForce(gravityJump, ForceMode.Impulse);
                    }
                }
                return true;
            }
        }
        
        [HarmonyPatch(typeof(SpellMergeGravity), "OnTrigger")]
        class TriggerPatch
        {
            static void Postfix(Collider other, bool enter)
            {
                if (other.attachedRigidbody && other.attachedRigidbody.isKinematic)
                {
                    if (Player.currentCreature && ((SpellMergeGravity)Player.currentCreature.mana.mergeInstance).bubbleActive)
                        inGravBubble = enter;
                }
            }
        }
    }
}