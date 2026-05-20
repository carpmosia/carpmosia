using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Medical;

/// <summary>
/// Component for the simple surgical tool used for brain extraction.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SurgicalToolComponent : Component
{
    /// <summary>
    /// Time that it will take for this tool to perform its function.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan SurgeryDelay = TimeSpan.FromSeconds(7);

    /// <summary>
    ///  Audio stream that plays the useSound.
    /// </summary>
    public EntityUid? PlayingStream;

    /// <summary>
    /// Sound to play when the tool is in use.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier StartSound = new SoundPathSpecifier("/Audio/_Carpmosia/Items/Medical/startsound.ogg");

    /// <summary>
    /// Sound to play when the tool is in use.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier EndSound = new SoundPathSpecifier("/Audio/_Carpmosia/Items/Medical/endsound.ogg");

}
