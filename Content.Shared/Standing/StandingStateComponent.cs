using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Standing
{
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    [Access(typeof(StandingStateSystem))]
    public sealed partial class StandingStateComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField]
        public SoundSpecifier? DownSound { get; private set; } = new SoundCollectionSpecifier("BodyFall");

        [DataField, AutoNetworkedField]
        public bool Standing { get; set; } = true;

        [DataField, AutoNetworkedField]
        public bool Incapacitated { get; set; } = false;

        /// <summary>
        /// Friction modifier applied to an entity in the downed state.
        /// </summary>
        [DataField, AutoNetworkedField]
        public float DownFrictionMod = 0.4f;

        /// <summary>
        /// Friction modifier applied to an entity in an incapacitated (crit/dead) state.
        /// </summary>
        [DataField, AutoNetworkedField]
        public float LimpFrictionMod = 3f;

        /// <summary>
        ///     List of fixtures that had their collision mask changed when the entity was downed.
        ///     Required for re-adding the collision mask.
        /// </summary>
        [DataField, AutoNetworkedField]
        public List<string> ChangedFixtures = new();
    }
}
