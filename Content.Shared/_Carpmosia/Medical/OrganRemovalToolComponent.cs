using Content.Shared.Body;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared.Medical;

/// <summary>
/// Component for the simple surgical tool used for brain extraction.
/// </summary>
[RegisterComponent, AutoGenerateComponentState, Access(typeof(SharedOrganRemovalToolSystem))]
public sealed partial class OrganRemovalToolComponent : Component
{
    /// <summary>
    /// Time that it will take for this tool to perform its function.
    /// </summary>
    [DataField]
    public TimeSpan SurgeryDelay = TimeSpan.FromSeconds(12);

    /// <summary>
    ///  Audio stream that plays the useSound.
    /// </summary>
    public EntityUid? PlayingStream;

    /// <summary>
    /// Sound to play on doafter start.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier StartSound = new SoundPathSpecifier("/Audio/_Carpmosia/Items/Medical/startsound.ogg");

    /// <summary>
    /// Sound to play on doafter end.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier EndSound = new SoundPathSpecifier("/Audio/_Carpmosia/Items/Medical/endsound.ogg");

    /// <summary>
    /// What kind of organ is targeted by this tool
    /// </summary>
    [DataField("category", required: true)]
    public ProtoId<OrganCategoryPrototype>? Category;

}
