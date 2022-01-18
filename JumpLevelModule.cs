using System.Collections;
using ThunderRoad;

namespace GravityOverhaul
{
    public class JumpLevelModule : LevelModule
    {
        public float twoHandJumpForce;
        public float oneHandJumpForce;
        private float oldJumpForce;

        public override IEnumerator OnLoadCoroutine()
        {
            EventManager.onPossess += OnPossessionEvent;
            return base.OnLoadCoroutine();
        }

        private void OnPossessionEvent(Creature creature, EventTime eventTime)
        {
            if (eventTime == EventTime.OnEnd) 
                oldJumpForce = Player.local.locomotion.jumpGroundForce;
        }

        public override void Update()
        {
            base.Update();

            if (Player.currentCreature)
            {
                bool leftFire = GravityMainModule.IsCastingGravity(Player.currentCreature.mana.casterLeft);
                bool rightFire = GravityMainModule.IsCastingGravity(Player.currentCreature.mana.casterRight);
                    
                if (leftFire && rightFire)
                    Player.local.locomotion.jumpGroundForce = twoHandJumpForce;
                else if (!leftFire && !rightFire)
                    Player.local.locomotion.jumpGroundForce = oldJumpForce;
                else
                    Player.local.locomotion.jumpGroundForce = oneHandJumpForce;
            }
        }
    }
}