
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

        args.Integral += error * frametime;
        var iOut = args.Ti * args.Integral;

        var dOut = args.Td * ((error - args.PrevError) / frametime);
        args.PrevError = error;

        var output = pOut + iOut + dOut;

        if (output > args.MaxVal || output < args.MinVal)
        {
            output = MathF.Max(args.MinVal, MathF.Min(args.MaxVal, output)); //quick and dirty clamp (which doesn't exist in mathF for some reason??)
            args.Integral -= error * frametime; //don't endlessly increase the integral if output is at the extreme already
        }

        return output;
    }
}

