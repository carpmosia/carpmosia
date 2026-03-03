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

        // Carpmosia-start - make dead/crit bodies much harder to pull
        [DataField, AutoNetworkedField]
        public bool Incapacitated { get; set; } = false;
        // Carpmosia-end - make dead/crit bodies much harder to pull

        /// <summary>
        /// Friction modifier applied to an entity in the downed state.
        /// </summary>
        [DataField, AutoNetworkedField]
        public float DownFrictionMod = 0.4f;

        // Carpmosia-start - make dead/crit bodies much harder to pull
        /// <summary>
        /// Friction modifier applied to an entity in an incapacitated (crit/dead) state.
        /// </summary>
        [DataField, AutoNetworkedField]
        public float LimpFrictionMod = 3f;
        // Carpmosia-end - make dead/crit bodies much harder to pull

        /// <summary>
        ///     List of fixtures that had their collision mask changed when the entity was downed.
        ///     Required for re-adding the collision mask.
        /// </summary>
        [DataField, AutoNetworkedField]
        public List<string> ChangedFixtures = new();
    }
}
