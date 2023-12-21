/* DIRECTIVES */
#region DIRECTIVES
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Text;
using Raylib_CsLo;
using SDL2;
#endregion

/* NAMESPACES */
#region NAMESPACES
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
        public float Intensity { get; set; } = 1f;
        public float Duration { get; set; } = 1f;
        public PiShockAPI.CommandType Op { get; set; }
    }

    public class PiShockShockerInfo
    {
        public string? Name { get; set; } = "NO SHOCKER INFO";
        public bool? IsPaused { get; set; } = false;
        public bool? IsOnline { get; set; } = false;
        public float? MaxIntensity { get; set; } = 1f;
        public float? MaxDuration { get; set; } = 1f;
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
        private static string APIShockerInfoURL = "https://do.pishock.com/api/GetShockerInfo";
        private static string APIUrl = "https://do.pishock.com/api/apioperate/";

        // HTTP Client(s)
        private static HttpClient HTTP = new HttpClient();

        // Floats
        public static float MaxIntensity { get; private set; } = 1f;
        public static float MaxDuration { get; private set; } = 1f;
        public static float Intensity = 1f;
        public static float Duration = 1f;
        #endregion

        /* FUNCTIONS */
        #region FUNCTIONS
        /// <summary>
        /// Configure the PiShock API.
        /// </summary>
        /// <param name="PiShockAccountName">The PiShock account name you made when you created an account.</param>
        /// <param name="YourName">Your username, handle, real name, etc.</param>
        /// <param name="ShareCode">The share code for the shocker.</param>
        /// <param name="Key">The API key for your account.</param>
        public static void Configure(string PiShockAccountName, string YourName, string ShareCode, string Key)
        {
            // Set the API configuration variables
            APIConfig = new PiShockAPIConfiguration()
            {
                Username = PiShockAccountName,
                Name = YourName,
                Code = ShareCode,
                Apikey = Key,
                Intensity = 1f,
                Duration = 1f,
                Op = CommandType.Vibrate
            };

            // Update the shocker information
            UpdateShockerInfo();
        }

        /// <summary>
        /// Update the maximum intensity and duration values.
        /// </summary>
        /// <param name="Intensity">The maximum operation intensity.</param>
        /// <param name="Duration">The maximum operation duration.</param>
        public static void UpdateMaximums(int Intensity, int Duration)
        {
            // Set the maximum intensity and duration values
            ShockerInfo.MaxIntensity = Intensity;
            ShockerInfo.MaxDuration = Duration;
            MaxIntensity = Intensity;
            MaxDuration = Duration;
        }

        /// <summary>
        /// Update the shocker information class.
        /// </summary>
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
                    }
                }
            }
            catch(Exception ex)
            {
                SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Electra - Refresh Error", $"An error has occurred.\n\n{ex.Message}\n\nStack trace: {ex.StackTrace}\n\nInner exception: {ex.InnerException}", 0);
            }

            // Set the mouse cursor
            Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_DEFAULT);
        }

        /// <summary>
        /// Send a command to the hub via the PiShock API or serial.
        /// </summary>
        /// <param name="Intensity">The intensity of the operation.</param>
        /// <param name="Duration">The duration of the operation.</param>
        /// <param name="Command">The operation to perform.</param>
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

                    // Don't send unnecessary information to the API if it isn't required. Bandwidth is expensive!
                    if (Command == CommandType.GetShockerInfo)
                    {
                        PostResult = HTTP.PostAsync(APIShockerInfoURL, Content).Result;
                    }
                    else
                    {
                        PostResult = HTTP.PostAsync(APIUrl, Content).Result;
                    }

                    // Read the result from the API POST request
                    string StringResult = PostResult.Content.ReadAsStringAsync().Result;
                    PostResult.Dispose();
                    Content.Dispose();

                    // Parse the API result
                    ParseAPIResult(StringResult);
                }            
            }
            catch (Exception ex)
            {
                SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Electra - Error", $"An error has occurred.\n\n{ex.Message}\n\nStack trace: {ex.StackTrace}", 0);
            }

            // Set the mouse cursor
            Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_DEFAULT);
        }

        /// <summary>
        /// Parse the result that the PiShock API sent back, if it sent a response at all.
        /// </summary>
        /// <param name="Result">The JSON string that the API sent.</param>
        /// <returns>Returns true if the operation succeeded, and false if there was an error..</returns>
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
                    SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Electra - Error", $"This share code \"{PiShockAPI.APIConfig.Code}\" doesn't exist.", 0);
                    return false;

                case "Not Authorized.":
                    SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Electra - Error", $"The username or API key is incorrect, or your account hasn't been activated yet.", 0);
                    return false;

                case "Shocker is Paused or does not exist. Unpause to send command.":
                    SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Electra - Error", $"This shocker is currently paused, or does not exist.", 0);
                    return false;

                case "Device currently not connected.":
                    SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Electra - Error", $"The hub is not connected.", 0);
                    return false;

                case "This share code has already been used by somebody else.":
                    SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Electra - Error", $"The share code \"{APIConfig.Code}\" is already in use.", 0);
                    return false;

                case "Unknown Op, use 0 for shock, 1 for vibrate and 2 for beep.":
                    SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Electra - Error", $"The operation \"{APIConfig.Op.ToString()}\" is invalid.", 0);
                    return false;

                case "Beep not allowed.":
                    SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Electra - Error", $"Beeping has been disabled for this share code.", 0);
                    return false;

                case "Shock not allowed.":
                    SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Electra - Error", $"Shocking has been disabled for this share code.", 0);
                    return false;

                case "Vibrate not allowed.":
                    SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Electra - Error", $"Vibrating has been disabled for this share code.", 0);
                    return false;

                case "Operation Succeeded.":
                case "Operation Attempted.":
                    return true;

                default:
                    if (string.IsNullOrEmpty(Result))
                    {
                        SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Electra - Error", $"The PiShock API did not respond.\n\nMake sure that:\n  1. Both you and the API are online\n  2. Your JSON configuration is valid", 0);
                    }
                    else
                    {
                        SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Electra - Error", $"The PiShock API has sent an invalid response. Make sure that your JSON configuration is valid.\n\nRecieved data: {Result}", 0);
                    }
                    
                    return false;
            }
        }
        #endregion
    }
    #endregion
}
#endregion