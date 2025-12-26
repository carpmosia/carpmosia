using Content.Server.BloodBound.EntitySystems;

namespace Content.Server.Objectives.Components;

/// <summary>
/// Marker component to show that an objective should be removed when the blood bound is deconverted.
/// </summary>
[RegisterComponent, Access(typeof(BloodBoundSystem))]
public sealed partial class ConvertedBloodBoundObjectiveComponent : Component;
