using System.Numerics;

namespace Content.Server.Shuttles.Components
{
    [RegisterComponent]
    public sealed partial class ShuttleComponent : Component
    {
        [ViewVariables]
        public bool Enabled = true;

        [ViewVariables]
        public Vector2[] CenterOfThrust = new Vector2[4];

        /// <summary>
        /// Thrust gets multiplied by this value if it's for braking.
        /// </summary>
        public const float BrakeCoefficient = 1.5f;

        /// <summary>
        /// Maximum velocity.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public float BaseMaxLinearVelocity = 60f;

        public const float MaxAngularVelocity = 4f;

        /// <summary>
        /// The cached thrust available for each cardinal direction
        /// </summary>
        [ViewVariables]
        public readonly float[] LinearThrust = new float[4];

        /// <summary>
        /// The thrusters contributing to each direction for impulse.
        /// </summary>
        // No touchy
        public readonly List<EntityUid>[] LinearThrusters = new List<EntityUid>[]
        {
            new(),
            new(),
            new(),
            new(),
        };

        /// <summary>
        /// The thrusters contributing to the angular impulse of the shuttle.
        /// </summary>
        public readonly List<EntityUid> AngularThrusters = new();

        [ViewVariables]
        public float AngularThrust = 0f;

        /// <summary>
        /// A bitmask of all the directions we are considered thrusting.
        /// </summary>
        [ViewVariables]
        public DirectionFlag ThrustDirections = DirectionFlag.None;

        /// <summary>
        /// Base damping modifier applied to the shuttle's physics component when not in FTL.
        /// </summary>
        [DataField]
        public float BodyModifier = 0.25f;

        /// <summary>
        /// Final Damping Modifier for a shuttle.
        /// This value is set to 0 during FTL. And to BodyModifier when not in FTL.
        /// </summary>
        [DataField]
        public float DampingModifier;

        /// Carpmosia-start - rotate shuttle along movement vector
        /// <summary>
        /// PID Controller proportional term
        /// </summary>
        [DataField]
        public float Kp = 80f;
        /// <summary>
        /// PID Controller integral term
        /// </summary>
        [DataField]
        public float Ki = 20f;
        /// <summary>
        /// PID Controller derivative term
        /// </summary>
        [DataField]
        public float Kd = 5f;
        /// <summary>
        /// PID Controller accumulated integral
        /// </summary>
        [DataField]
        public float Integral = 0f;
        /// <summary>
        /// Used for calculating the derivative term in the PID controller
        /// </summary>
        [ViewVariables]
        public float PreviousError = 0f;

        [DataField]
        public float TargetSpeed = 0f;
        /// Carpmosia-end - rotate shuttle along movement vector
    }
}
