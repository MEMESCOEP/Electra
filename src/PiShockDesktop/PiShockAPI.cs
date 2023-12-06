using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.Json;

namespace PiShockDesktop
{
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
        private static PiShockAPIConfiguration APIConfig = new PiShockAPIConfiguration();

        // Strings
        private static string APIUrl = "https://do.pishock.com/api/apioperate/";
        private static string APIShockerInfoURL = "https://do.pishock.com/api/GetShockerInfo";

        // HTTP Client(s)
        private static HttpClient HTTP = new HttpClient();

        // Floats
        private static float MaxIntensity = 0f;
        private static float MaxDuration = 0f;
        #endregion

        /* FUNCTIONS */
        #region FUNCTIONS
        public static void Configure(string PiShockUsername, string YourName, string ShareCode, string Key)
        {
            // Set the API configuration variables
            APIConfig = new PiShockAPIConfiguration()
            {
                Username = PiShockUsername,
                Name = YourName,
                Code = ShareCode,
                Apikey = Key,
                Intensity = 0f,
                Duration = 0f,
                Op = CommandType.Vibrate
            };

            // Serialize the configuration and POST it to the PiShock API via HTTP
            StringContent Content = new StringContent(JsonSerializer.Serialize(APIConfig), Encoding.UTF8, "application/json");
            HttpResponseMessage PostResult = HTTP.PostAsync(APIShockerInfoURL, Content).Result;
            string StringResult = PostResult.Content.ReadAsStringAsync().Result;

            // Set the max intensity and duration limits
            MaxIntensity = (float)JObject.Parse(StringResult)["maxIntensity"];
            MaxDuration = (float)JObject.Parse(StringResult)["maxDuration"];
        }

        public static void SendCommand(float Intensity, float Duration, CommandType Command)
        {
            // Clamp the intensty and duration
            // Update the API configuration
            APIConfig.Intensity = Math.Clamp(Intensity, 0f, MaxIntensity);
            APIConfig.Duration = Math.Clamp(Duration, 0f, MaxDuration);
            APIConfig.Op = Command;

            // Serialize the configuration and POST it to the PiShock API via HTTP
            StringContent Content = new StringContent(JsonSerializer.Serialize(APIConfig), Encoding.UTF8, "application/json");
            HttpResponseMessage PostResult;
            
            if(Command == CommandType.GetShockerInfo)
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
            
            Program.MessageBox(0, StringResult, "PiShock Desktop", 64);
        }
        #endregion
    }
    #endregion
}
