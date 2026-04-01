using Content.Shared.Mobs; // Carpmosia-edit - dead/crit friction changes
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

        /// <summary>
        /// Friction modifier applied to an entity in the downed state.
        /// </summary>
        [DataField, AutoNetworkedField]
        public float DownFrictionMod = 0.4f;

        // Carpmosia-start - dead/crit friction changes
        /// <summary>
        /// Friction modifiers applied to a downed entity at various states of health.
        /// </summary>
        [DataField, AutoNetworkedField]
        public Dictionary<MobState, float> DownFrictionModDict = new()
        {
            {MobState.Alive, 0.4f},
            {MobState.Critical, 1.4f},
            {MobState.Dead, 2.4f},
        };
        // Carpmosia-end - dead/crit friction changes

        /// <summary>
        ///     List of fixtures that had their collision mask changed when the entity was downed.
        ///     Required for re-adding the collision mask.
        /// </summary>
        [DataField, AutoNetworkedField]
        public List<string> ChangedFixtures = new();
    }
}
