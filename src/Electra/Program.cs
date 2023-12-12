using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using Raylib_CsLo;
using DiscordRPC;
using SharpHook;

namespace Electra
{
    /********** NOTE **********/
    // Rivatuner Statistics Server (framerate counter) seems to cause memory leaks as well as a 20 MB increase in RAM usage.
    // I'm unsure why this happens, but I'm trying to find a fix. Fraps does not seem to have this issue as of right now.
    // For now, I'm using a timer to call the garbage collector until I figure out what's going on, as there seems to be 
    // a memory leak somewhere else in my code.

    /* WARNING SUPRESSION */
    // These are disabled because they piss me off (and they aren't a concern)
    #region WARNING SUPRESSION
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
    #endregion

    /* CLASSES */
    #region CLASSES
    internal class Program
    {
        /* VARIABLES */
        #region VARIABLES
        // Booleans
        private static bool UseStationaryFPSLimit = true;
        private static bool UseDraggingFPSLimit = true;
        private static bool EnableVerticalSync = true;
        private static bool EnableDiscordRPC = false;
        private static bool ExitProgram = false;
        private static bool DragLock = false;

        // Integers
        private static int DragSleepTime = 0;
        private static int ScreenHeight = 400;
        private static int ScreenWidth = 450;

        // Floats
        private static float DragOffsetX = 1f;
        private static float DragOffsetY = 1f;

        // Lists
        private static List<string> ConfigKeys = new List<string>() { "PiShockAccountName", "YourName", "ShareCodes", "APIKey", "DiscordAppID", "UseStationaryFPSLimit", "UseDraggingFPSLimit", "EnableDiscordRPC", "EnableVSync" };

        // Points
        private static System.Drawing.Point MousePos = new System.Drawing.Point(0, 0);

        // Rectangles
        private static Rectangle IntensityRect = new Rectangle(16, 224, ScreenWidth - 72, 16);
        private static Rectangle DurationRect = new Rectangle(16, 244, ScreenWidth - 72, 16);
        private static Rectangle MinimizeRect = new Rectangle(ScreenWidth - 96, 0, 48, 48);
        private static Rectangle RefreshRect = new Rectangle(16, 184, ScreenWidth - 32, 32);
        private static Rectangle VibrateRect = new Rectangle(16, 104, ScreenWidth - 32, 32);
        private static Rectangle CloseRect = new Rectangle(ScreenWidth - 48, 0, 48, 48);
        private static Rectangle ShockRect = new Rectangle(16, 144, ScreenWidth - 32, 32);
        private static Rectangle InfoRect = new Rectangle(16, 268, ScreenWidth - 32, 114);
        private static Rectangle BeepRect = new Rectangle(16, 64, ScreenWidth - 32, 32);
        private static Rectangle DragRect = new Rectangle(0, 0, ScreenWidth - 96, 48);

        // Images
        private static Image TitlebarLogo;
        private static Image TaskbarLogo;

        // Textures
        private static Texture TitlebarLogoTexture;

        // Colors
        private static Color MinimizePressedColor = new Color(64, 64, 64, 255);
        private static Color MinimizeNormalColor = new Color(32, 32, 32, 255);
        private static Color ClosePressedColor = new Color(192, 0, 0, 255);
        private static Color CloseNormalColor = new Color(64, 0, 0, 255);
        private static Color TitlebarColor = new Color(12, 12, 12, 255);
        private static Color BorderColor = new Color(50, 50, 50, 255);
        private static Color BGColor = new Color(18, 18, 18, 255);

        // Timers
        private static Timer GarbageCollectionTimer = new Timer(5, new Action(() => GC.Collect()));
        private static Timer DiscordRPCUpdateTimer = new Timer(1, new Action(() => UpdateRPC()));
        private static Timer DragLockTimer = new Timer(0.1, new Action(() => RayGui.GuiUnlock()));

        // Strings
        private static string Logo128Path = @"Assets/Logo_128x128.png";
        private static string Logo32Path = @"Assets/Logo_32x32.png";
        private static string ConfigPath = @"Assets/ElectraConfig.json";

        // Hooks
        private static SimpleGlobalHook MouseHook = new SimpleGlobalHook();

        // Discord RPC clients
        private static DiscordRpcClient? RPCClient;
        #endregion

        /* DLL IMPORTS */
        #region DLL IMPORTS
        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(IntPtr h, string m, string c, int type);
        #endregion

        /* FUNCTIONS */
        #region FUNCTIONS
        private static void Main(string[] args)
        {
            try
            {
                // Make sure the configuration file exists
                if (!File.Exists(ConfigPath))
                {
                    throw new FileNotFoundException("The configuration file could not be found.");
                }

                // Read the JSON configuration file
                JObject ConfigData = JObject.Parse(File.ReadAllText(ConfigPath));
                JArray ShareCodeArray = (JArray)ConfigData["ShareCodes"];

                // Make sure our configuration has all the required keys
                foreach (var JSONKey in ConfigKeys)
                {
                    if (!ConfigData.ContainsKey(JSONKey))
                    {
                        throw new Exception($"Your configuration is invalid because it's missing the \"{JSONKey}\" key.");
                    }
                }

                // Configure the PiShock API
                PiShockAPI.Configure(ConfigData["PiShockAccountName"].ToString(), ConfigData["YourName"].ToString(), ShareCodeArray[0].ToString(), ConfigData["APIKey"].ToString());

                // Enable/Disable VSync and apply FPS limits if required
                UseStationaryFPSLimit = ConfigData.SelectToken("UseStationaryFPSLimit").Value<bool>();
                UseDraggingFPSLimit = ConfigData.SelectToken("UseDraggingFPSLimit").Value<bool>();
                EnableVerticalSync = ConfigData.SelectToken("EnableVSync").Value<bool>();
                EnableDiscordRPC = ConfigData.SelectToken("EnableDiscordRPC").Value<bool>();

                // Set up the discord RPC client
                RPCClient = new DiscordRpcClient(ConfigData["DiscordAppID"].ToString());
                RPCClient.Initialize();

                // Load images
                TitlebarLogo = Raylib.LoadImage(Logo32Path);
                TaskbarLogo = Raylib.LoadImage(Logo128Path);
            }
            catch (Exception ex)
            {
                if(MessageBox(0, "The configuration could not be loaded. Please fix any issues and restart the application.\n\nWould you like to view extra error information?", "Electra - Configuration Error", 20) == 6)
                {
                    MessageBox(0, $"Error: {ex.Message}\n\nConfiguration path: \"{Path.GetFullPath(ConfigPath)}\"\n\nStack trace: {ex.StackTrace}", "Electra - Configuration Error", 16);
                }

                DiscordRPCUpdateTimer.Stop();

                if(RPCClient != null)
                {
                    RPCClient.Dispose();
                }

                Environment.Exit(ex.HResult);
            }

            // Assign the mouse hook events
            MouseHook.MouseMoved += OnMouseMoved;
            MouseHook.MouseDragged += OnMouseMoved;

            // Set the window flags
            Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_UNDECORATED);
            Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_ALWAYS_RUN);

            if (EnableVerticalSync)
            {
                Raylib.SetConfigFlags(ConfigFlags.FLAG_VSYNC_HINT);
            }

            // Create a window and set it's icon
            Raylib.InitWindow(ScreenWidth, ScreenHeight, "Electra");
            Raylib.SetWindowIcon(TaskbarLogo);

            // Set the style manually because raylib doesn't seem to want to load a style from the disk properly >:(
            // Everything starting with '0x' is a color, some are unchecked due to uint -> int conversion. And yes, this
            // is probably a shitty way to accomplish this, I just don't know any better ways as of right now
            // Button
            RayGui.GuiSetStyle((int)GuiControl.BUTTON, (int)GuiControlProperty.BASE_COLOR_NORMAL, 0x024658FF);
            RayGui.GuiSetStyle((int)GuiControl.BUTTON, (int)GuiControlProperty.BASE_COLOR_FOCUSED, 0x3299B4FF);
            RayGui.GuiSetStyle((int)GuiControl.BUTTON, (int)GuiControlProperty.BASE_COLOR_PRESSED, unchecked((int)0xFFBC51FF));
            RayGui.GuiSetStyle((int)GuiControl.BUTTON, (int)GuiControlProperty.BASE_COLOR_DISABLED, 0x024658FF);

            RayGui.GuiSetStyle((int)GuiControl.BUTTON, (int)GuiControlProperty.TEXT_COLOR_NORMAL, 0x51BFD3FF);
            RayGui.GuiSetStyle((int)GuiControl.BUTTON, (int)GuiControlProperty.TEXT_COLOR_FOCUSED, unchecked((int)0xB6E1EAFF));
            RayGui.GuiSetStyle((int)GuiControl.BUTTON, (int)GuiControlProperty.TEXT_COLOR_PRESSED, unchecked((int)0xD86F36FF));
            RayGui.GuiSetStyle((int)GuiControl.BUTTON, (int)GuiControlProperty.TEXT_COLOR_DISABLED, 0x51BFD3FF);

            RayGui.GuiSetStyle((int)GuiControl.BUTTON, (int)GuiControlProperty.BORDER_COLOR_NORMAL, 0x2F7486FF);
            RayGui.GuiSetStyle((int)GuiControl.BUTTON, (int)GuiControlProperty.BORDER_COLOR_FOCUSED, unchecked((int)0x82CDE0FF));
            RayGui.GuiSetStyle((int)GuiControl.BUTTON, (int)GuiControlProperty.BORDER_COLOR_PRESSED, unchecked((int)0xEB7630FF));
            RayGui.GuiSetStyle((int)GuiControl.BUTTON, (int)GuiControlProperty.BORDER_COLOR_DISABLED, 0x2F7486FF);

            // Slider
            RayGui.GuiSetStyle((int)GuiControl.SLIDER, (int)GuiControlProperty.BASE_COLOR_NORMAL, 0x024658FF);
            RayGui.GuiSetStyle((int)GuiControl.SLIDER, (int)GuiControlProperty.BASE_COLOR_FOCUSED, unchecked((int)0xFFBC51FF));
            RayGui.GuiSetStyle((int)GuiControl.SLIDER, (int)GuiControlProperty.BASE_COLOR_PRESSED, 0x3299B4FF);
            RayGui.GuiSetStyle((int)GuiControl.SLIDER, (int)GuiControlProperty.BASE_COLOR_DISABLED, 0x024658FF);

            RayGui.GuiSetStyle((int)GuiControl.SLIDER, (int)GuiControlProperty.TEXT_COLOR_NORMAL, 0x51BFD3FF);
            RayGui.GuiSetStyle((int)GuiControl.SLIDER, (int)GuiControlProperty.TEXT_COLOR_FOCUSED, unchecked((int)0xB6E1EAFF));
            RayGui.GuiSetStyle((int)GuiControl.SLIDER, (int)GuiControlProperty.TEXT_COLOR_PRESSED, unchecked((int)0xD86F36FF));
            RayGui.GuiSetStyle((int)GuiControl.SLIDER, (int)GuiControlProperty.TEXT_COLOR_DISABLED, 0x51BFD3FF);

            RayGui.GuiSetStyle((int)GuiControl.SLIDER, (int)GuiControlProperty.BORDER_COLOR_NORMAL, 0x2F7486FF);
            RayGui.GuiSetStyle((int)GuiControl.SLIDER, (int)GuiControlProperty.BORDER_COLOR_FOCUSED, unchecked((int)0x82CDE0FF));
            RayGui.GuiSetStyle((int)GuiControl.SLIDER, (int)GuiControlProperty.BORDER_COLOR_PRESSED, unchecked((int)0xEB7630FF));
            RayGui.GuiSetStyle((int)GuiControl.SLIDER, (int)GuiControlProperty.BORDER_COLOR_DISABLED, 0x2F7486FF);
            
            // Calculate the drag sleep time in milliseconds. I would use raylib's vsync here, but it increases CPU Usage of up to 30% when
            // using it on lower end hardware. Calculating this allows for reduced resource usage, and since this is a GUI application, it
            // shouldn't need to use a bunch of resources.
            DragSleepTime = (int)((1f / Raylib.GetMonitorRefreshRate(Raylib.GetCurrentMonitor())) * 1000f) - 1;

            // Create textures from images
            TitlebarLogoTexture = Raylib.LoadTextureFromImage(TitlebarLogo);

            // Configure and start the garbage collection timer
            GarbageCollectionTimer.Recurring = true;
            GarbageCollectionTimer.Start();

            if (EnableDiscordRPC)
            {
                DiscordRPCUpdateTimer.Recurring = true;
                DiscordRPCUpdateTimer.Start();
            }                

            // Start the mouse hook asynchronously
            MouseHook.RunAsync();

            // Main loop
            while (!ExitProgram && !Raylib.WindowShouldClose())
            {
                // Start drawing
                Raylib.BeginDrawing();

                // Check if the user is dragging the window
                if (!DragLock && Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), DragRect) && Raylib.IsWindowFocused() && Raylib.IsMouseButtonPressed(0))
                {
                    DragOffsetX = Math.Abs(Raylib.GetWindowPosition().X - MousePos.X);
                    DragOffsetY = Math.Abs(Raylib.GetWindowPosition().Y - MousePos.Y);
                    Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_RESIZE_ALL);
                    RayGui.GuiLock();
                    DragLockTimer.Reset();
                    DragLock = true;
                }

                // Move the window if the user is dragging it
                if (DragLock)
                {
                    // Set the window's position using the mouse position
                    Raylib.SetWindowPosition((int)(MousePos.X - DragOffsetX), (int)(MousePos.Y - DragOffsetY));

                    // If the user has just stopped draging the window, reset the mouse cursor and enable the GUI after 100 ms
                    if (Raylib.IsMouseButtonUp(0))
                    {
                        DragLock = false;
                        Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_DEFAULT);
                        DragLockTimer.Start();
                    }

                    // Uses less CPU, at minimal framerate cost. (Monitor refresh rate)
                    if (UseDraggingFPSLimit)
                    {
                        Thread.Sleep(DragSleepTime);
                    }
                }
                else
                {
                    // Uses less CPU, at minimal framerate cost. (30 FPS)
                    if (UseStationaryFPSLimit)
                    {
                        Thread.Sleep(32);
                    }
                }

                // Update timers
                GarbageCollectionTimer.Update();
                DiscordRPCUpdateTimer.Update();
                DragLockTimer.Update();

                // Draw window elements
                UpdateScreen();

                // Stop drawing
                Raylib.EndDrawing();
            }

            // Unload assets to prevent memory leaks and file errors
            Raylib.UnloadImage(TaskbarLogo);
            Raylib.UnloadImage(TitlebarLogo);
            Raylib.UnloadTexture(TitlebarLogoTexture);

            // Exit the application
            DiscordRPCUpdateTimer.Stop();

            if (RPCClient != null)
            {
                RPCClient.Dispose();
            }

            MouseHook.Dispose();
            Raylib.CloseWindow();
            Environment.Exit(0);
        }

        // Draw UI elements
        private static void UpdateScreen()
        {
            // Clear the window and draw the titlebar
            Raylib.ClearBackground(BGColor);
            Raylib.DrawRectangle(0, 0, ScreenWidth, 48, TitlebarColor);

            // Get the intensity and duration from the sliders
            PiShockAPI.Intensity = (int)RayGui.GuiSlider(IntensityRect, null, $"INT: {PiShockAPI.Intensity}", PiShockAPI.Intensity, 1, PiShockAPI.MaxIntensity);
            PiShockAPI.Duration = (int)RayGui.GuiSlider(DurationRect, null, $"DUR: {PiShockAPI.Duration}", PiShockAPI.Duration, 1, PiShockAPI.MaxDuration);

            // Draw and process buttons
            // Refresh information button
            if (RayGui.GuiButton(RefreshRect, "REFRESH INFO") && !DragLock)
            {
                PiShockAPI.UpdateShockerInfo();
            }

            // Shock button
            if (RayGui.GuiButton(ShockRect, "SHOCK") && !DragLock)
            {
                PiShockAPI.SendCommand(PiShockAPI.Intensity, PiShockAPI.Duration, PiShockAPI.CommandType.Shock);
            }

            // Vibrate button
            if (RayGui.GuiButton(VibrateRect, "VIBRATE") && !DragLock)
            {
                PiShockAPI.SendCommand(PiShockAPI.Intensity, PiShockAPI.Duration, PiShockAPI.CommandType.Vibrate);
            }

            // Beep button
            if (RayGui.GuiButton(BeepRect, "BEEP") && !DragLock)
            {
                PiShockAPI.SendCommand(PiShockAPI.Intensity, PiShockAPI.Duration, PiShockAPI.CommandType.Beep);
            }

            // Check if the user is closing the window
            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), CloseRect))
            {
                if (Raylib.IsMouseButtonReleased(0) && Raylib.IsWindowFocused())
                {
                    ExitProgram = true;
                }
                else if(Raylib.IsMouseButtonDown(0))
                {
                    Raylib.DrawRectangle(ScreenWidth - 48, 0, 48, 48, ClosePressedColor);
                }
                else
                {
                    Raylib.DrawRectangle(ScreenWidth - 48, 0, 48, 48, CloseNormalColor);
                }
            }

            // Check if the user is minimizing the window
            else if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), MinimizeRect))
            {
                if (Raylib.IsMouseButtonReleased(0) && Raylib.IsWindowFocused())
                {
                    Raylib.MinimizeWindow();
                }
                else if (Raylib.IsMouseButtonDown(0))
                {
                    Raylib.DrawRectangle(ScreenWidth - 96, 0, 48, 48, MinimizePressedColor);
                }
                else
                {
                    Raylib.DrawRectangle(ScreenWidth - 96, 0, 48, 48, MinimizeNormalColor);
                }
            }

            // Draw the shocker information
            Raylib.DrawRectangleLinesEx(InfoRect, 2, Raylib.GetColor(0x2F7486FF));
            Raylib.DrawRectangle(18, 270, ScreenWidth - 36, 110, Raylib.GetColor(0x024658FF));
            Raylib.DrawText($"{PiShockAPI.ShockerInfo.Name}:", 22, 274, 20, Raylib.GOLD);
            Raylib.DrawText($"    ID: {PiShockAPI.ShockerInfo.ID}", 22, 294, 20, Raylib.GOLD);
            Raylib.DrawText($"    Paused: {PiShockAPI.ShockerInfo.IsPaused}", 22, 314, 20, Raylib.GOLD);
            Raylib.DrawText($"    Max int & dur: {PiShockAPI.ShockerInfo.MaxIntensity}, {PiShockAPI.ShockerInfo.MaxDuration}", 22, 334, 20, Raylib.GOLD);
            Raylib.DrawText($"    Share code: {PiShockAPI.APIConfig.Code}", 22, 354, 20, Raylib.GOLD);

            // Draw the titlebar decorations
            Raylib.DrawText("Electra", 40, 14, 20, Raylib.GOLD);
            Raylib.DrawTexture(TitlebarLogoTexture, 6, 6, Raylib.WHITE);
            Raylib.DrawRectangle(418, 16, 16, 16, Raylib.RED);
            Raylib.DrawText("x", 421, 13, 20, Raylib.WHITE);
            Raylib.DrawLine(ScreenWidth - 80, 24, ScreenWidth - 64, 24, Raylib.WHITE);

            // Draw the window border
            Raylib.DrawRectangleLines(0, 0, ScreenWidth, ScreenHeight, BorderColor);
        }

        // Get the screen space mouse position when the mouse has been moved (the non-relative position on the screen in pixels)
        private static void OnMouseMoved(object? sender, MouseHookEventArgs e)
        {
            MousePos.X = e.Data.X;
            MousePos.Y = e.Data.Y;
        }

        // Set the Discord RPC data
        private static void UpdateRPC()
        {
            RPCClient.SetPresence(new RichPresence()
            {
                Details = PiShockAPI.ShockerInfo.Name,
                State = $"Intensity: {PiShockAPI.Intensity} || Duration: {PiShockAPI.Duration}",
                Assets = new Assets()
                {
                    LargeImageKey = "https://pishock.com/statics/icons/favicon-128x128.png",
                    LargeImageText = "PiShock Logo",
                    SmallImageKey = "https://pishock.com/statics/icons/favicon-32x32.png"
                }
            });
        }
        #endregion
    }
    #endregion
}
