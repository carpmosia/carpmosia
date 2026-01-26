namespace Content.Server.PID.Components;

[DataDefinition]
public struct PIDParams
{
    /// <summary>
    /// Proportional constant
    /// </summary>
    [DataField]
    public float Kp;

    /// <summary>
    /// Integral constant
    /// </summary>
    [DataField]
    public float Ti;

    /// <summary>
    /// Derivative constant
    /// </summary>
    [DataField]
    public float Td;

    /// <summary>
    /// Accumulated integral
    /// </summary>
    [ViewVariables]
    public float Integral;

    /// <summary>
    /// Previous error value
    /// </summary>
    [ViewVariables]
    public float PrevError;

    /// <summary>
    /// Upper controller output value constraint
    /// </summary>
    [DataField]
    public float MaxVal;

    /// <summary>
    /// Lower controller output value constraint
    /// </summary>
    [DataField]
    public float MinVal;
}
