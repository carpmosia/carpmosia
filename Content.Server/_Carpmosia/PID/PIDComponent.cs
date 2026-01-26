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
    public float Integral = 0f;

    /// <summary>
    /// Previous error value
    /// </summary>
    [ViewVariables]
    public float PrevError = 0f;

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

    public PIDParams
    (
        float kp,
        float ti,
        float td,
        float maxVal,
        float minVal)
    {
        Kp = kp;
        Ti = ti;
        Td = td;
        MaxVal = maxVal;
        MinVal = minVal;
    }
}
