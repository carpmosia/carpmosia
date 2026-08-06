namespace Content.Shared.Medical;

/// <summary>
/// Component for Operating Tables, which Surgical Tool System uses.
/// </summary>
[RegisterComponent]
[Access(typeof(SharedOrganRemovalToolSystem))]
public sealed partial class SurgicalTableComponent : Component;
