using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Text;
using Raylib_CsLo;

namespace PiShockDesktop
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
        public string? Username { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? Apikey { get; set; }
        public float Intensity { get; set; }
        public float Duration { get; set; }
        public PiShockAPI.CommandType Op { get; set; }
    }

    public class PiShockShockerInfo
    {
        public string? Name { get; set; } = "NO CONFIG";
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

        public static void UpdateShockerInfo()
        {
            // Set the mouse cursor
            Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_NOT_ALLOWED);

            try
            {
                // Serialize the configuration and POST it to the PiShock API via HTTP
                StringContent Content = new StringContent(JsonSerializer.Serialize(APIConfig), Encoding.UTF8, "application/json");
                HttpResponseMessage PostResult = HTTP.PostAsync(APIShockerInfoURL, Content).Result;
                string StringResult = PostResult.Content.ReadAsStringAsync().Result;

                // Set the max intensity and duration limits
                MaxIntensity = (float)JObject.Parse(StringResult)["maxIntensity"];
                MaxDuration = (float)JObject.Parse(StringResult)["maxDuration"];

                // Update the shocker information class
                ShockerInfo.IsPaused = (bool)JObject.Parse(StringResult)["paused"];
                ShockerInfo.IsOnline = (bool)JObject.Parse(StringResult)["online"];
                ShockerInfo.Name = (string)JObject.Parse(StringResult)["name"];
                ShockerInfo.ID = (int)JObject.Parse(StringResult)["id"];
                ShockerInfo.MaxIntensity = MaxIntensity;
                ShockerInfo.MaxDuration = MaxDuration;
            }
            catch(Exception ex)
            {
                Program.MessageBox(0, $"An error has occurred.\n\n{ex.Message}", "PiShock Desktop - Error", 16);
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
                // Clamp the intensty and duration
                // Update the API configuration
                APIConfig.Intensity = Math.Clamp(Intensity, 0f, MaxIntensity);
                APIConfig.Duration = Math.Clamp(Duration, 0f, MaxDuration);
                APIConfig.Op = Command;

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

                var StringResult = PostResult.Content.ReadAsStringAsync().Result;

                // Get the API return code
                switch (StringResult)
                {
                    case "This code doesn't exist.":
                        Program.MessageBox(0, "The code you entered doesn't exist.", "PiShock Desktop - Error", 16);
                        break;

                    case "Not Authorized.":
                        Program.MessageBox(0, "The username or API key you entered is incorrect, or your account hasn't been activated yet.", "PiShock Desktop - Error", 16);
                        break;

                    case "Shocker is Paused or does not exist. Unpause to send command.":
                        Program.MessageBox(0, "The shocker was paused from the PiShock web interface.", "PiShock Desktop - Error", 16);
                        break;

                    case "Device currently not connected.":
                        Program.MessageBox(0, "The hub is not connected.", "PiShock Desktop - Error", 16);
                        break;

                    case "This share code has already been used by somebody else.":
                        Program.MessageBox(0, "The share code you entered is already in use.", "PiShock Desktop - Error", 16);
                        break;

                    case "Unknown Op, use 0 for shock, 1 for vibrate and 2 for beep.":
                        Program.MessageBox(0, $"The API did not recognize the operation ({APIConfig.Op.ToString()}).", "PiShock Desktop - Error", 16);
                        break;

                    case "Beep not allowed.":
                        Program.MessageBox(0, "Another user has disabled beeping for this shocker.", "PiShock Desktop - Error", 16);
                        break;

                    case "Shock not allowed.":
                        Program.MessageBox(0, "Another user has disabled shocking for this shocker.", "PiShock Desktop - Error", 16);
                        break;

                    case "Vibrate not allowed.":
                        Program.MessageBox(0, "Another user has disabled vibrating for this shocker.", "PiShock Desktop - Error", 16);
                        break;

                    case "Operation Succeeded.":
                    case "Operation Attempted.":
                    default:
                        break;
                }

                if (Command == CommandType.GetShockerInfo)
                {
                    MaxIntensity = (float)JObject.Parse(StringResult)["maxIntensity"];
                    MaxDuration = (float)JObject.Parse(StringResult)["maxDuration"];
                }
            }
            catch (Exception ex)
            {
                Program.MessageBox(0, $"An error has occurred.\n\n{ex.Message}", "PiShock Desktop - Error", 16);
            }

            // Set the mouse cursor
            Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_DEFAULT);

            //Program.MessageBox(0, StringResult, "PiShock Desktop", 64);
        }
        #endregion
    }
    #endregion
}
