/* DIRECTIVES */
#region DIRECTIVES
using DiscordRPC;
#endregion

/* NAMESPACES */
#region NAMESPACES
namespace Electra
{
    public class DiscordRPC
    {
        /* WARNING SUPRESSION */
        // These are disabled because they piss me off (and they aren't a concern)
        #region WARNING SUPRESSION
        #pragma warning disable CS8602 // Dereference of a possibly null reference.
        #endregion

        /* VARIABLES */
        #region VARIABLES
        // Discord RPC clients
        private static DiscordRpcClient? RPCClient;
        #endregion

        /* FUNCTIONS */
        #region FUNCTIONS
        /// <summary>
        /// Initialize the Discord RPC client
        /// </summary>
        /// <param name="DiscordClientID">The client ID for the Discord RPC application</param>
        public static void Initialize(string DiscordClientID)
        {
            RPCClient = new DiscordRpcClient(DiscordClientID);
            RPCClient.Initialize();
        }

        /// <summary>
        /// Set the Discord RPC data
        /// </summary>
        public static void UpdateRPC()
        {
            RPCClient.SetPresence(new RichPresence()
            {
                Details = PiShockAPI.ShockerInfo.Name,
                State = $"Intensity: {PiShockAPI.Intensity}%   ||   Duration: {PiShockAPI.Duration}s",
                Assets = new Assets()
                {
                    LargeImageKey = "https://pishock.com/statics/icons/favicon-128x128.png",
                    LargeImageText = "PiShock Logo",
                    SmallImageKey = "https://pishock.com/statics/icons/favicon-32x32.png"
                }
            });
        }

        /// <summary>
        /// Close and dispose of the Discord RPC client
        /// </summary>
        public static void Close()
        {
            if (RPCClient != null)
            {
                RPCClient.Dispose();
            }
        }
        #endregion
    }
}
#endregion