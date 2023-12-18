namespace Electra
{
    /* WARNING SUPRESSION */
    // These are disabled because they piss me off (and they aren't a concern)
    #region WARNING SUPRESSION
#pragma warning disable CA1416 // Validate platform compatibility
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.
    #endregion

    /* CLASSES */
    #region CLASSES
    public static class ShockerOperations
    {
        public const string Shock = "shock";
        public const string Vibrate = "vibrate";
        public const string Beep = "beep";
        public const string Info = "info";
    }

    public class Serial
    {
        /* FUNCTIONS */
        #region FUNCTIONS
        /// <summary>
        /// This function sends a JSON-formatted command string to the serial port where the hub is connected.
        /// </summary>
        /// <param name="Operation">The operation the shocker should perform. This should be one of the values defined in the ShockerOperations class.</param>
        /// <param name="Intensity">The intensity the shocker should use when performing an operation. This should be an integer between 1 and 100 (power percentage), and does not apply to beeping.</param>
        /// <param name="Duration">The duration in milliseconds of the operation that the shocker will perform.</param>
        /// <param name="ShockerID">the ID of the shocker.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static void SendCommand(string Operation, int Intensity, UInt32 Duration, int ShockerID)
        {
            if (Intensity < 1 && Intensity > 100)
            {
                throw new ArgumentOutOfRangeException("Intensity must be between 1 and 100!");
            }

            if(Duration < 1 && Duration > UInt32.MaxValue)
            {
                throw new ArgumentOutOfRangeException($"Duration must be between 1 and {UInt32.MaxValue}!");
            }

            if (!Operation.Equals(ShockerOperations.Shock) && !Operation.Equals(ShockerOperations.Vibrate) && !Operation.Equals(ShockerOperations.Beep) && !Operation.Equals(ShockerOperations.Info))
            {
                throw new InvalidOperationException($"The operation \"{Operation}\" is invalid!");
            }

            if(Program.SP == null)
            {
                throw new ArgumentNullException("The serial port is null!");
            }

            if (!Program.SP.IsOpen)
            {
                throw new Exception("The serial port is not open!");
            }

            if (Operation.Equals(ShockerOperations.Info))
            {
                Program.SP.Write($@"{{""cmd"": ""info""}}");
            }
            else
            {
                Program.SP.Write($@"{{""cmd"": ""operate"", ""value"": {{""id"": {ShockerID}, ""op"": ""{Operation}"", ""duration"": {Duration}, ""intensity"": {Intensity}}}}}");
            }
        }
        #endregion
    }
    #endregion
}
