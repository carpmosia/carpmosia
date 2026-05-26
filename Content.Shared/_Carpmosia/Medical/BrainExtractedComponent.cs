using Robust.Shared.GameStates;

namespace Content.Shared.Medical;

/// <summary>
/// Component to be added to entities that have had their brain removed. Used to add special Examine text.
/// </summary>
[RegisterComponent]
[Access(typeof(SharedOrganRemovalToolSystem))]
public sealed partial class BrainExtractedComponent : Component;
