using System.Collections;
using System.IO;
using HarmonyLib;
using ThunderRoad;
using UnityEngine;

namespace GravityOverhaul
{
    public class GravityFlightModule : LevelModule
    {
        public float oneHandHorizontalSpeed;
        public float oneHandVerticalAcceleration;
        public float twoHandHorizontalSpeed;
        public float twoHandVerticalAcceleration;
        
        private static GravityOverhaulData data;
        private static Locomotion loco;
        private static bool isFlying;
        private static bool leftFire, rightFire;
        
        private static bool statsStored;
        private static bool orgFallDamage;
        private static bool orgCrouchOnJump;
        private static float oldDrag;
        private static float oldMass;
        private static float oldSpeed;
        private static float oldMaxAngle;

        public override IEnumerator OnLoadCoroutine()
        {
            data = GameManager.local.gameObject.AddComponent<GravityOverhaulData>();
            if (Directory.Exists(Application.streamingAssetsPath + "\\Mods\\Wings_U10"))
                Debug.LogWarning("(GravityJump) Detected Wings mod. Gravity flight disabled!");
            else
                EventManager.onPossess += OnPossessionEvent;
            return base.OnLoadCoroutine();
        }

        private void InitValues()
        {
            data.oneHandHorizontalSpeed = oneHandHorizontalSpeed;
            data.oneHandVerticalAcceleration = oneHandVerticalAcceleration;
            data.twoHandHorizontalSpeed = twoHandHorizontalSpeed;
            data.twoHandVerticalAcceleration = twoHandVerticalAcceleration;
        }

        public override void Update()
        {
            base.Update();

            if (Player.currentCreature && isFlying)
            {
                leftFire = GravityMainModule.IsCastingGravity(Player.currentCreature.mana.casterLeft);
                rightFire = GravityMainModule.IsCastingGravity(Player.currentCreature.mana.casterLeft);

                if (leftFire && rightFire) {
                    RevertFlightStats();
                    ActivateFly();
                } else if (!leftFire && !rightFire) {
                    RevertFlightStats();
                } else {
                    RevertFlightStats();
                    ActivateFly();
                }

                if (Player.local.locomotion.isGrounded)
                    RevertFlightStats();
            }
            InitValues();
        }

        private void OnPossessionEvent(Creature creature, EventTime eventTime)
        {
            if (eventTime == EventTime.OnEnd)
            {
                loco = Player.local.locomotion;
                new Harmony("Jump").PatchAll();
                new Harmony("Turn").PatchAll();
            }
        }

        [HarmonyPatch(typeof(PlayerControl), "Jump")]
        class JumpFix
        {
            public static void Postfix(bool active)
            {
                if (!active) return;

                if (isFlying)
                    RevertFlightStats();
                else if (!Player.local.locomotion.isGrounded && (leftFire || rightFire))
                    ActivateFly();
            }
        }
        
        private static void ActivateFly()
        {
            if (!statsStored)
            {
                statsStored = true;
                oldSpeed = loco.airSpeed;
                oldMaxAngle = loco.groundAngle;
                oldDrag = loco.rb.drag;
                oldMass = loco.rb.mass;
                orgFallDamage = Player.fallDamage;
                orgCrouchOnJump = Player.crouchOnJump;
            }
            loco.groundAngle = -359f;
            loco.rb.useGravity = false;
            loco.rb.mass = 100000f;
            loco.rb.drag = 0.9f;
            loco.velocity = Vector3.zero; 
            if (leftFire && rightFire)
                loco.airSpeed = data.twoHandHorizontalSpeed;
            else
                loco.airSpeed = data.oneHandHorizontalSpeed;
            Player.fallDamage = false;
            Player.crouchOnJump = false;
            isFlying = true;
        }
        
        private static void RevertFlightStats()
        {
            loco.groundAngle = oldMaxAngle;
            isFlying = false;
            loco.rb.drag = oldDrag;
            loco.rb.useGravity = true;
            loco.rb.mass = oldMass;
            loco.airSpeed = oldSpeed;
            Player.fallDamage = orgFallDamage;
            Player.crouchOnJump = orgCrouchOnJump;
        }

        [HarmonyPatch(typeof(PlayerControl), "Turn")]
        class TurnFix
        {
            public static void Postfix(Side side, Vector2 axis)
            {
                if (isFlying && axis.y != 0.0)
                {
                    if (!Pointer.GetActive() || !Pointer.GetActive().isPointingUI)
                    {
                        if (leftFire && rightFire)
                            loco.rb.AddForce(Vector3.up * data.twoHandVerticalAcceleration * axis.y, ForceMode.Acceleration);
                        else
                            loco.rb.AddForce(Vector3.up * data.oneHandVerticalAcceleration * axis.y, ForceMode.Acceleration);
                    }
                }
            }
        }
    }
}