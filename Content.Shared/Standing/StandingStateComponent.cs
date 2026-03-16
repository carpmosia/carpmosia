using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Content.Shared.Mobs; // Carpmosia-edit - make dead/crit bodies less hard to pull v2

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

        // Carpmosia-start - make dead/crit bodies much harder to pull v2
        [DataField, AutoNetworkedField]
        public MobState Incapacitation { get; set; } = MobState.Alive;

        /// <summary>
        /// Friction modifier applied to an entity in a critical state.
        /// </summary>
        [DataField, AutoNetworkedField]
        public float CritFrictionMod = 1.4f;

        /// <summary>
        /// Friction modifier applied to an entity in a dead state.
        /// </summary>
        [DataField, AutoNetworkedField]
        public float DeadFrictionMod = 2.4f;
        // Carpmosia-end - make dead/crit bodies much harder to pull v2

        /// <summary>
        ///     List of fixtures that had their collision mask changed when the entity was downed.
        ///     Required for re-adding the collision mask.
        /// </summary>
        [DataField, AutoNetworkedField]
        public List<string> ChangedFixtures = new();
    }
}
