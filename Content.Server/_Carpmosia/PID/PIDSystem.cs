
namespace Content.Server.PID;

/// <summary>
///     Provides a rudimentary discrete PID controller function with min/max Value limiters
///     and integral runaway protection
/// </summary>

public static class PIDSystem
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="input">Input parameter</param>
    /// <param name="setpoint">Controller's setpoint</param>
    /// <param name="args">PID controller parameters</param>
    /// <param name="frametime"></param>
    /// <returns></returns>
    public static float Controller(
        float input,
        float setpoint,
        ref PIDParams args,
        float frametime
        )
    {
        var error = input - setpoint;

        var pOut = args.Kp * error;

        var safeTi = args.Ti == 0f ? 100000f : args.Ti;

        var step = frametime * (error / safeTi);
        args.Integral += step;
        var iOut = args.Integral;

        var dOut = args.Td * ((args.PrevError - error) / frametime);
        args.PrevError = error;

        var output = pOut + iOut + dOut;

        if (output > args.MaxVal || output < args.MinVal)
        {
            output = Math.Clamp(output, args.MinVal, args.MaxVal);
            args.Integral -= step; //don't endlessly increase the integral if output is at the extreme already
        }

        return output;
    }
}

