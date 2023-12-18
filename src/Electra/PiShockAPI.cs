using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Text;
using Raylib_CsLo;

namespace Electra
{
    /* WARNING SUPRESSION */
    // These are disabled because they piss me off (and they aren't a concern)
    #region WARNING SUPRESSION
    #pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
    #pragma warning disable CS8604 // Possible null reference argument.
    #endregion

    /* CLASSSES */
    #region CLASSES
    public class PiShockAPIConfiguration
    {
        public string? Username { get; set; } = "None";
        public string? Name { get; set; } = "None";
        public string? Code { get; set; } = "None";
        public string? Apikey { get; set; } = "None";
        public float Intensity { get; set; } = 0f;
        public float Duration { get; set; } = 0f;
        public PiShockAPI.CommandType Op { get; set; }
    }

    public class PiShockShockerInfo
    {
        public string? Name { get; set; } = "NO SHOCKER INFO";
        public bool? IsPaused { get; set; } = false;
        public bool? IsOnline { get; set; } = false;
        public float? MaxIntensity { get; set; } = 0f;
        public float? MaxDuration { get; set; } = 0f;
        public int? ID { get; set; } = 0;
    }

    public class PiShockAPI
    {
        /* ENUMS */
        #region ENUMS
        public enum CommandType : int
        {
            Shock = 0,
            Vibrate = 1,
            Beep = 2,
            GetShockerInfo = 3
        }
        #endregion

        /* VARIABLES */
        #region VARIABLES
        // API Configuration(s)
        public static PiShockAPIConfiguration APIConfig = new PiShockAPIConfiguration();

        // API Configuration(s)
        public static PiShockShockerInfo ShockerInfo = new PiShockShockerInfo();

        // Strings
        private static string APIUrl = "https://do.pishock.com/api/apioperate/";
        private static string APIShockerInfoURL = "https://do.pishock.com/api/GetShockerInfo";

        // HTTP Client(s)
        private static HttpClient HTTP = new HttpClient();

        // Floats
        public static float MaxIntensity { get; private set; } = 0f;
        public static float MaxDuration { get; private set; } = 0f;
        public static float Intensity = 0f;
        public static float Duration = 0f;
        #endregion

        /* FUNCTIONS */
        #region FUNCTIONS
        public static void Configure(string PiShockAccountName, string YourName, string ShareCode, string Key)
        {
            // Set the API configuration variables
            APIConfig = new PiShockAPIConfiguration()
            {
                Username = PiShockAccountName,
                Name = YourName,
                Code = ShareCode,
                Apikey = Key,
                Intensity = 0f,
                Duration = 0f,
                Op = CommandType.Vibrate
            };

            UpdateShockerInfo();
        }

        public static void UpdateMaximums(int Intensity, int Duration)
        {
            // Set the maximum values
            MaxIntensity = Intensity;
            MaxDuration = Duration;
        }

        public static void UpdateShockerInfo()
        {
            // Set the mouse cursor
            Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_NOT_ALLOWED);

            try
            {
                if (Program.EnableSerial)
                {
                    Serial.SendCommand("info", 0, 0, 0);
                }
                else
                {
                    // Serialize the configuration and POST it to the PiShock API via HTTP
                    StringContent Content = new StringContent(JsonSerializer.Serialize(APIConfig), Encoding.UTF8, "application/json");
                    HttpResponseMessage PostResult = HTTP.PostAsync(APIShockerInfoURL, Content).Result;
                    string StringResult = PostResult.Content.ReadAsStringAsync().Result;

                    // Make sure we get a valid json response from the PiShock API
                    if (ParseAPIResult(StringResult))
                    {
                        // Parse the JSON response
                        var ParsedResult = JObject.Parse(StringResult);

                        // Set the max intensity and duration limits
                        UpdateMaximums(ParsedResult.SelectToken("maxIntensity").Value<int>(), ParsedResult.SelectToken("maxDuration").Value<int>());

                        // Update the shocker information class
                        ShockerInfo.IsPaused = ParsedResult.SelectToken("paused").Value<bool>();
                        ShockerInfo.IsOnline = ParsedResult.SelectToken("online").Value<bool>();
                        ShockerInfo.Name = ParsedResult.SelectToken("name").Value<string>();
                        ShockerInfo.ID = ParsedResult.SelectToken("id").Value<int>();
                        ShockerInfo.MaxIntensity = MaxIntensity;
                        ShockerInfo.MaxDuration = MaxDuration;
                    }
                }
            }
            catch(Exception ex)
            {
                Program.MessageBox(0, $"An error has occurred.\n\n{ex.Message}", "Electra - Error", 16);
            }

            // Set the mouse cursor
            Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_DEFAULT);
        }

        public static void SendCommand(float Intensity, float Duration, CommandType Command)
        {
            // Set the mouse cursor
            Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_NOT_ALLOWED);

            try
            {
                // Clamp the intensty and duration and update the API configuration
                APIConfig.Intensity = Math.Clamp(Intensity, 0f, MaxIntensity);
                APIConfig.Duration = Math.Clamp(Duration, 0f, MaxDuration);
                APIConfig.Op = Command;

                if (Program.EnableSerial)
                {
                    Serial.SendCommand(Command.ToString().ToLower(), (int)Intensity, (uint)Duration * 1000, 8669);
                }
                else
                {
                    // Serialize the configuration and POST it to the PiShock API via HTTP
                    StringContent Content = new StringContent(JsonSerializer.Serialize(APIConfig), Encoding.UTF8, "application/json");
                    HttpResponseMessage PostResult;

                    if (Command == CommandType.GetShockerInfo)
                    {
                        PostResult = HTTP.PostAsync(APIShockerInfoURL, Content).Result;
                    }
                    else
                    {
                        PostResult = HTTP.PostAsync(APIUrl, Content).Result;
                    }

                    string StringResult = PostResult.Content.ReadAsStringAsync().Result;
                    PostResult.Dispose();
                    Content.Dispose();

                    // Parse the API result
                    ParseAPIResult(StringResult);
                }            
            }
            catch (Exception ex)
            {
                Program.MessageBox(0, $"An error has occurred.\n\n{ex.Message}\n\nStack trace: {ex.StackTrace}", "Electra - Error", 16);
            }

            // Set the mouse cursor
            Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_DEFAULT);
        }

        public static bool ParseAPIResult(string Result)
        {
            // Get the API return code
            // If the recieved result has a client ID field, we can assume that a call to the API's GetShockerInfo function was made
            if (Result.Contains("clientId"))
            {
                return true;
            }

            // If the recieved result does NOT have a client ID field, we can assume that we called some other API function or an error occurred
            switch (Result)
            {
                case "This code doesn't exist.":
                    Program.MessageBox(0, $"This share code \"{PiShockAPI.APIConfig.Code}\" doesn't exist.", "Electra - Error", 16);
                    return false;

                case "Not Authorized.":
                    Program.MessageBox(0, "The username or API key is incorrect, or your account hasn't been activated yet.", "Electra - Error", 16);
                    return false;

                case "Shocker is Paused or does not exist. Unpause to send command.":
                    Program.MessageBox(0, "This shocker is currently paused, or does not exist.", "Electra - Error", 16);
                    return false;

                case "Device currently not connected.":
                    Program.MessageBox(0, "The hub is not connected.", "Electra - Error", 16);
                    return false;

                case "This share code has already been used by somebody else.":
                    Program.MessageBox(0, $"The share code \"{PiShockAPI.APIConfig.Code}\" is already in use.", "Electra - Error", 16);
                    return false;

                case "Unknown Op, use 0 for shock, 1 for vibrate and 2 for beep.":
                    Program.MessageBox(0, $"The operation \"{APIConfig.Op.ToString()}\" is invalid.", "Electra - Error", 16);
                    return false;

                case "Beep not allowed.":
                    Program.MessageBox(0, "Beeping has been disabled for this share code.", "Electra - Error", 16);
                    return false;

                case "Shock not allowed.":
                    Program.MessageBox(0, "Shocking has been disabled for this share code.", "Electra - Error", 16);
                    return false;

                case "Vibrate not allowed.":
                    Program.MessageBox(0, "Vibrating has been disabled for this share code.", "Electra - Error", 16);
                    return false;

                case "Operation Succeeded.":
                case "Operation Attempted.":
                    return true;

                default:
                    if (!string.IsNullOrEmpty(Result))
                    {
                        Program.MessageBox(0, $"The PiShock API has sent an invalid response. Make sure that your JSON configuration is valid.\n\nRecieved data: {Result}", "Electra - Error", 16);
                    }
                    else
                    {
                        Program.MessageBox(0, "The PiShock API did not respond.\n\nMake sure that:\n  1. Both you and the API are online\n  2. Your JSON configuration is valid", "Electra - Error", 16);
                    }
                    
                    return false;
            }
        }
        #endregion
    }
    #endregion
}
