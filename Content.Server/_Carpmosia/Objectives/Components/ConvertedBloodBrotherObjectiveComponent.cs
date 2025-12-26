using Content.Server.BloodBrothers.EntitySystems;

namespace Content.Server.Objectives.Components;

/// <summary>
/// Marker component to show that an objective should be removed when the blood brother is deconverted.
/// </summary>
[RegisterComponent, Access(typeof(BloodBrotherSystem))]
public sealed partial class ConvertedBloodBrotherObjectiveComponent : Component;
