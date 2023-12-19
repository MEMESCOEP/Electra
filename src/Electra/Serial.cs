/* DIRECTIVES */
#region DIRECTIVES
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO.Ports;
#endregion

/* NAMESPACES */
#region NAMESPACES
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
        /* VARIABLES */
        #region VARIABLES
        // Serial ports
        public static SerialPort? SP;
        #endregion

        /* FUNCTIONS */
        #region FUNCTIONS
        /// <summary>
        /// Initialize serial
        /// </summary>
        /// <param name="MaxIntensity">The maximum operation intensity</param>
        /// <param name="MaxDuration">The maximum operation duration</param>
        /// <param name="WriteTimeout">The maximum timeout in ms to use when writing to a serial device</param>
        /// <param name="ReadTimeout">The maximum timeout in ms to use when reading from a serial device</param>
        public static void Initialize(string COMPortName, int MaxIntensity, int MaxDuration, int WriteTimeout, int ReadTimeout)
        {
            // Create a way to communicate with the serial port
            SP = new SerialPort(COMPortName, 115200, Parity.None, 8, StopBits.One);

            // Set the serial connection properties
            SP.Handshake = Handshake.None;
            SP.DtrEnable = false;
            SP.RtsEnable = false;
            SP.DataReceived += new SerialDataReceivedEventHandler(Serial.SerialDataReceived);
            SP.WriteTimeout = WriteTimeout;
            SP.ReadTimeout = ReadTimeout;

            // Open the serial port and send an information request
            SP.Open();
            SendCommand("info", 0, 0, 0);

            PiShockAPI.UpdateMaximums(MaxIntensity, MaxDuration);
            PiShockAPI.ShockerInfo.MaxIntensity = PiShockAPI.MaxIntensity;
            PiShockAPI.ShockerInfo.MaxDuration = PiShockAPI.MaxDuration;
            PiShockAPI.ShockerInfo.Name = $"Serial ({PiShockAPI.ShockerInfo.ID})";
        }

        /// <summary>
        /// Get the COM port name of the hub
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string GetCOMPort()
        {
            ProcessStartInfo PyScript = new ProcessStartInfo();

            // Set the execution properties for the process
            PyScript.FileName = "GetCOMName.pye";
            PyScript.UseShellExecute = false;
            PyScript.CreateNoWindow = true;
            PyScript.RedirectStandardOutput = true;

            Process COMProcess = new Process();
            COMProcess.StartInfo = PyScript;
            COMProcess.StartInfo.RedirectStandardOutput = true;
            COMProcess.Start();

            StreamReader SR = COMProcess.StandardOutput;
            string COMPort = SR.ReadLine();
            COMProcess.WaitForExit();
            COMProcess.Close();

            if (string.IsNullOrEmpty(COMPort))
            {
                throw new Exception("Failed to find the hub because the COM port finder returned null.\n\n" +
                    "The most common causes for this failure are:\n" +
                    "    1. The hub is not connected to the computer via USB\n" +
                    "    2. The USB cable you used cannot transmit data\n" +
                    "    3. You don't have the CP210x drivers installed\n");
            }

            return COMPort;
        }

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

            if (SP == null || !SP.IsOpen)
            {
                throw new Exception("The serial port is null or not open!");
            }

            if (Operation.Equals(ShockerOperations.Info))
            {
                SP.Write($@"{{""cmd"": ""info""}}");
            }
            else
            {
                SP.Write($@"{{""cmd"": ""operate"", ""value"": {{""id"": {ShockerID}, ""op"": ""{Operation}"", ""duration"": {Duration}, ""intensity"": {Intensity}}}}}");
            }
        }

        // Get serial data from the hub
        /// <summary>
        /// Event handler for when serial data is received
        /// </summary>
        public static void SerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Wait for 50 ms to give the hub time to receive and parse a previous command
            Thread.Sleep(50);

            // Read the data the hub is sending
            string SerialData = SP.ReadLine();

            // If the received data is an information command, reconfigure
            if (SerialData.StartsWith("TERMINALINFO:"))
            {
                SerialData = SerialData.Substring(SerialData.IndexOf('{'));
                var ParsedResult = JObject.Parse(SerialData);

                //MessageBox(0, SerialData, "Serial Output", 0);

                foreach (var Shocker in ParsedResult.SelectToken("shockers"))
                {
                    PiShockAPI.ShockerInfo.ID = Shocker.SelectToken("id").Value<int>();
                    PiShockAPI.ShockerInfo.Name = $"Serial ({PiShockAPI.ShockerInfo.ID})";
                }
            }
        }

        /// <summary>
        /// Close the serial port if it's in use
        /// </summary>
        public static void Close()
        {
            if (SP != null && SP.IsOpen)
            {
                SP.WriteTimeout = -1;
                SP.Close();
                SP.Dispose();
            }
        }
        #endregion
    }
    #endregion
}
#endregion