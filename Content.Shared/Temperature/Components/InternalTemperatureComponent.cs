//using Content.Server.Temperature.Systems; Carpmosia-edit - InternalTemp insulation
using Content.Shared.Atmos;
using Content.Shared.Temperature.Components;
using Content.Shared.Temperature.HeatContainer;
// Carpmosia-start - InternalTemp insulation
using Content.Shared.Temperature.Systems;
using Robust.Shared.GameStates;
// Carpmosia-end - InternalTemp insulation

namespace Content.Shared.Temperature.Components; // Carpmosia-edit - InternalTemp insulation

/// <summary>
/// Entity has an internal temperature which conducts heat from its surface.
/// Requires <see cref="TemperatureComponent"/> to function.
/// </summary>
/// <remarks>
/// Currently this is only used for cooking but animal metabolism could use it too.
/// Too hot? Suffering heatstroke, start sweating to cool off and increase thirst.
/// Too cold? Suffering hypothermia, start shivering to warm up and increase hunger.
/// </remarks>
[RegisterComponent, Access(typeof(SharedTemperatureSystem)), NetworkedComponent, AutoGenerateComponentState] // Carpmosia-edit - InternalTemp insulation
public sealed partial class InternalTemperatureComponent : Component, IHeatContainer
{
    // TODO: These values probably shouldn't be duplicated from temperature component, but they're only used for the chef atm so low priority.
    /// <summary>
    /// Internal temperature which is modified by surface temperature.
    /// This gets set to <see cref="TemperatureComponent.Temperature"/> on mapinit.
    /// </summary>
    [DataField, AutoNetworkedField] // Carpmosia-edit - InternalTemp insulation
    public float Temperature { get; set; } = Atmospherics.T20C;

    /// <summary>
    /// Heat capacity of our internal temperature.
    /// This gets set to <see cref="TemperatureComponent.HeatCapacity"/> on mapinit.
    /// </summary>
    [DataField, AutoNetworkedField] // Carpmosia-edit - InternalTemp insulation
    public float HeatCapacity { get; set; }

    /// <summary>
    /// Thermal Conductance in W/K to this entity's <see cref="TemperatureComponent"/>.
    /// Roughly estimated by multiplying meat's thermal conductivity of about 0.4 W/(m*K) by the total surface area of the meat,
    /// and dividing by the thickness of the meat.
    /// Then we multiply by four because we should only care about half the thickness typically, and also we're sharing a heat capacity.
    /// Yes this is stupid. I'll care when chef has content or this is used by BodySystem.
    /// No I'm not doing a custom value for each piece of meat.
    /// </summary>
    [DataField, AutoNetworkedField] // Carpmosia-edit - InternalTemp insulation
    public float Conductance = 40f;

    // Carpmosia-start - InternalTemp insulation
    /// <summary>
    /// If the ambient temperature should be conducted to <see cref="InternalTemperatureComponent.Temperature"/>
    /// then <see cref="TemperatureComponent.Temperature"/> rather than the opposite. This is useful for entities
    /// which are insulated and should have things like damage and cryo-temps decoupled from the ambient temperature.
    /// External conduction (not to <see cref="TemperatureComponent"/>) will use the parameters set in TemperatureComponent.
    /// When set to true, InternalTemperature specific alerts will be shown.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ConductAmbient;
    // Carpmosia-end - InternalTemp insulation
}
