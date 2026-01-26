
namespace Content.Shared.PID
{
    /// <summary>
    ///     Provides a rudimentary discrete PID controller function with min/max Value limiters
    ///     and integral runaway protection
    /// </summary>

    public struct PID
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="input">Input parameter</param>
        /// <param name="setpoint">Controller's setpoint</param>
        /// <param name="kp">Proportional term</param>
        /// <param name="ki">Integral term</param>
        /// <param name="kd">Derivative term</param>
        /// <param name="integral">Integral Sum</param>
        /// <param name="previousError">Value of the error in the previous iteration</param>
        /// <param name="frametime"></param>
        /// <returns></returns>
        public static float Controller(
            float input,
            float setpoint,
            float kp,
            float ki,
            float kd,
            float minVal,
            float maxVal,
            float frametime,
            ref float integral,
            ref float previousError
            )
        {
            var error = input - setpoint;

            var pOut = kp * error;

            integral += error * frametime;
            var iOut = ki * integral;

            var dOut = kd * ((error - previousError) / frametime);
            previousError = error;

            var output = pOut + iOut + dOut;

            if (output > maxVal || output < minVal)
            {
                output = MathF.Max(minVal, MathF.Min(maxVal, output)); //quick and dirty clamp (which doesn't exist in mathF for some reason??)
                integral -= error * frametime; //don't endlessly increase the integral if output is at the extreme already
            }

            return output;
        }
    }
}
