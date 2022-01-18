using UnityEngine;

namespace GravityOverhaul
{
    public class GravityOverhaulData : MonoBehaviour
    {
        // GRAVITY JUMP SETTINGS
        public float minThrowVelocity;
        public float maxThrowVelocity;
        public float yGravMult;
        public float xzGravMult;
        public float twoHandGravMult;
        public float yAirBoostMult;
        public float bubbleMult;
        public float twoHandListenDuration;
        public bool parkourMode;

        // SHOCKWAVE SETTINGS
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

        // GRAVITY ARROW SETTINGS
        public float arrowShockwaveCD;
        public float arrowShockwaveMult;
        public float arrowDestableImbueCost;
        public float arrowShockwaveImbueCost;
        public float arrowShockwaveMinVelocity;
        public float arrowShockwaveMaxVelocity;
        public float arrowShockwaveMinRadius;
        public float arrowShockwaveMaxRadius;
        public float arrowShockwaveMinForce;
        public float arrowShockwaveMaxForce;

        // GRAVITY BUBBLE SETTINGS
        public float bubbleDuration;
        public float bubbleImbueCost;
        public float bubbleSizeMult;

        // WEAPON SHOCKWAVE SETTINGS
        public float wepShockwaveCD;
        public float wepShockwaveMult;
        public float wepShockwaveImbueCost;
        public float wepShockwaveMinVelocity;
        public float wepShockwaveMaxVelocity;
        public float wepShockwaveMinRadius;
        public float wepShockwaveMaxRadius;
        public float wepShockwaveMinForce;
        public float wepShockwaveMaxForce;

        // GRAVITY FLY SETTINGS
        public float oneHandHorizontalSpeed;
        public float oneHandVerticalAcceleration;
        public float twoHandHorizontalSpeed;
        public float twoHandVerticalAcceleration;
    }
}