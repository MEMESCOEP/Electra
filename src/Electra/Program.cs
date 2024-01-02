/* DIRECTIVES */
#region DIRECTIVES
using Newtonsoft.Json.Linq;
using Raylib_CsLo;
using SharpHook;
using SDL2;
using System.Numerics;
#endregion

/* NAMESPACES */
#region NAMESPACES
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
        private static bool WindowOpened = false;
        private static bool ExitProgram = false;
        private static bool DragLock = false;
        public static bool EnableSerial = true;

        // Integers
        private static int DPIPercentageOf13;
        private static int DPIPercentageOf16;
        private static int DPIPercentageOf20;
        private static int DPIPercentageOf24;
        private static int DPIPercentageOf32;
        private static int DPIPercentageOf48;
        private static int DPIPercentageOf64;
        private static int DPIPercentageOf96;
        private static int SerialWriteTimeout = 5000;
        private static int SerialReadTimeout = 5000;
        private static int DragSleepTime = 0;
        private static int WindowHeight = 400;
        private static int WindowWidth = 450;

        // Floats
        private static float DragOffsetX = 1f;
        private static float DragOffsetY = 1f;

        // Lists
        private static List<string> ConfigKeys = new List<string>() { "PiShockAccountName", "YourName", "ShareCodes", "APIKey", "DiscordAppID", "UseStationaryFPSLimit", "UseDraggingFPSLimit", "EnableDiscordRPC", "EnableVSync", "EnableSerial", "MaxIntensitySerial", "MaxDurationSerial", "SerialShockerID" };

        // Points
        private static System.Drawing.Point MousePos = new System.Drawing.Point(0, 0);

        // Rectangles
        private static Rectangle DPITargetScaleRect;
        private static Rectangle IntensityRect = new Rectangle(16, 224, WindowWidth - 72, 16);
        private static Rectangle DurationRect = new Rectangle(16, 244, WindowWidth - 72, 16);
        private static Rectangle MinimizeRect = new Rectangle(WindowWidth - 96, 0, 48, 48);
        private static Rectangle RefreshRect = new Rectangle(16, 184, WindowWidth - 32, 32);
        private static Rectangle VibrateRect = new Rectangle(16, 104, WindowWidth - 32, 32);
        private static Rectangle CloseRect = new Rectangle(WindowWidth - 48, 0, 48, 48);
        private static Rectangle ShockRect = new Rectangle(16, 144, WindowWidth - 32, 32);
        private static Rectangle InfoRect = new Rectangle(16, 268, WindowWidth - 32, 114);
        private static Rectangle BeepRect = new Rectangle(16, 64, WindowWidth - 32, 32);
        private static Rectangle DragRect = new Rectangle(0, 0, WindowWidth - 96, 48);

        // Images
        private static Image IntensityIcon;
        private static Image DurationIcon;
        private static Image TitlebarLogo;
        private static Image TaskbarLogo;

        // Textures
        private static Texture TitlebarLogoTexture;
        private static Texture IntensityTexture;
        private static Texture DurationTexture;

        // Colors
        private static Color MinimizePressedColor = new Color(64, 64, 64, 255);
        private static Color MinimizeNormalColor = new Color(32, 32, 32, 255);
        private static Color ClosePressedColor = new Color(192, 0, 0, 255);
        private static Color CloseNormalColor = new Color(64, 0, 0, 255);
        private static Color TitlebarColor = new Color(12, 12, 12, 255);
        private static Color BorderColor = new Color(50, 50, 50, 255);
        private static Color BGColor = new Color(18, 18, 18, 255);

        // Timers
        private static Timer GarbageCollectionTimer = new Timer(5, new Action(GC.Collect));
        private static Timer DiscordRPCUpdateTimer = new Timer(1, new Action(DiscordRPC.UpdateRPC));
        private static Timer DPIUpdateTimer = new Timer(0.1, new Action(() => UpdateWindowDPI()));

        private static Timer DragLockTimer = new Timer(0.1, new Action(RayGui.GuiUnlock));

        // Strings
        private static string IntensityIconPath = @"Assets/Intensity.png";
        private static string DurationIconPath = @"Assets/Duration.png";
        private static string Logo128Path = @"Assets/Logo_128x128.png";
        private static string Logo32Path = @"Assets/Logo_32x32.png";
        private static string ConfigPath = @"Assets/ElectraConfig.json";

        // Hooks
        private static SimpleGlobalHook MouseHook = new SimpleGlobalHook();

        // App domain(s)
        private static AppDomain CurrentAppDomain = AppDomain.CurrentDomain;

        // Render textures
        private static RenderTexture TargetRenderTexture;

        // 2D vectors
        private static Vector2 ScaledWindowSize;
        private static Vector2 WindowDPI;

        #endregion

        /* FUNCTIONS */
        #region FUNCTIONS
        /// <summary>
        /// Main program entry function.
        /// </summary>
        private static void Main(string[] args)
        {
            try
            {
                // Set up a handler for uncaught exceptions. This will most likely not handle Raylib errors
                Console.WriteLine("[INFO] >> Setting up exception handler...");
                CurrentAppDomain.UnhandledException += new UnhandledExceptionEventHandler(CrashSafely);

                // Make sure the configuration file exists
                Console.WriteLine("[INFO] >> Making sure config exists...");
                if (!File.Exists(ConfigPath))
                {
                    throw new FileNotFoundException("The configuration file could not be found.");
                }

                // Read the JSON configuration file
                Console.WriteLine("[INFO] >> Testing config keys...");
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

                // Enable/Disable VSync and apply FPS limits if required
                Console.WriteLine("[INFO] >> Applying configuration...");
                UseStationaryFPSLimit = ConfigData.SelectToken("UseStationaryFPSLimit").Value<bool>();
                UseDraggingFPSLimit = ConfigData.SelectToken("UseDraggingFPSLimit").Value<bool>();
                EnableVerticalSync = ConfigData.SelectToken("EnableVSync").Value<bool>();
                EnableDiscordRPC = ConfigData.SelectToken("EnableDiscordRPC").Value<bool>();
                EnableSerial = ConfigData.SelectToken("EnableSerial").Value<bool>();

                // Set up the discord RPC client
                DiscordRPC.Initialize(ConfigData["DiscordAppID"].ToString());

                // Set the window flags
                Console.WriteLine("[INFO] >> Setting window flags...");
                Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_UNDECORATED | ConfigFlags.FLAG_WINDOW_ALWAYS_RUN);

                if (EnableVerticalSync)
                {
                    Raylib.SetConfigFlags(ConfigFlags.FLAG_VSYNC_HINT);
                }

                // Create a window
                Console.WriteLine("[INFO] >> Initializing window...");
                Raylib.InitWindow(96, 32, "Electra");

                // Update the window so the user knows that the program is loading
                Raylib.BeginDrawing();
                Raylib.DrawText("Loading...", 0, 0, 20, Raylib.GOLD);
                Raylib.EndDrawing();

                // Get the monitor's DPI so we can properly scale the window
                WindowDPI = Raylib.GetWindowScaleDPI();

                // Calcutale percentages
                DPIPercentageOf13 = (int)MathUtilities.GetPercentageOfNumber(13, WindowDPI[0] * 100);
                DPIPercentageOf16 = (int)MathUtilities.GetPercentageOfNumber(16, WindowDPI[0] * 100);
                DPIPercentageOf20 = (int)MathUtilities.GetPercentageOfNumber(20, WindowDPI[0] * 100);
                DPIPercentageOf24 = (int)MathUtilities.GetPercentageOfNumber(24, WindowDPI[0] * 100);
                DPIPercentageOf32 = (int)MathUtilities.GetPercentageOfNumber(32, WindowDPI[0] * 100);
                DPIPercentageOf48 = (int)MathUtilities.GetPercentageOfNumber(48, WindowDPI[0] * 100);
                DPIPercentageOf64 = (int)MathUtilities.GetPercentageOfNumber(64, WindowDPI[0] * 100);
                DPIPercentageOf96 = (int)MathUtilities.GetPercentageOfNumber(96, WindowDPI[0] * 100);

                // Load images
                Console.WriteLine("[INFO] >> Loading images...");
                IntensityIcon = Raylib.LoadImage(IntensityIconPath);
                DurationIcon = Raylib.LoadImage(DurationIconPath);
                TitlebarLogo = Raylib.LoadImage(Logo32Path);
                TaskbarLogo = Raylib.LoadImage(Logo128Path);

                // Set up a render texture
                TargetRenderTexture = Raylib.LoadRenderTexture(WindowWidth, WindowHeight);

                // Configure textures
                Raylib.SetTextureFilter(TargetRenderTexture.texture, TextureFilter.TEXTURE_FILTER_POINT);

                // Configure rendering rectangles
                DPITargetScaleRect = new Rectangle(0f, 0f, WindowWidth * WindowDPI[0], WindowHeight * WindowDPI[1]);
                //FinalRenderRect = new Rectangle(0f, 0f, TargetRenderTexture.texture.width, -TargetRenderTexture.texture.height);
                
                // Configure the PiShock API. If serial is enabled, use settings in the JSON configuration 
                if (EnableSerial)
                {
                    // Find the COM port of the PiShock hub if serial is enabled
                    try
                    {
                        Console.WriteLine("[INFO] >> Initializing serial...");
                        Serial.Initialize(Serial.GetCOMPort(), ConfigData.SelectToken("MaxIntensitySerial").Value<int>(), ConfigData.SelectToken("MaxDurationSerial").Value<int>(), SerialWriteTimeout, SerialReadTimeout, ConfigData.SelectToken("SerialShockerID").Value<int>());
                    }
                    catch (Exception ex)
                    {
                        // Display an error message
                        SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Electra - Serial Error", $"Serial hub autodetection failed.\n\nError: {ex.Message}\n\nStack trace: {ex.StackTrace}", 0);

                        // Safely close the application
                        CloseApplication(ex.HResult);
                    }                   
                }
                else
                {
                    Console.WriteLine("[INFO] >> Initializing PiShock API...");
                    PiShockAPI.Configure(ConfigData["PiShockAccountName"].ToString(), ConfigData["YourName"].ToString(), ShareCodeArray[0].ToString(), ConfigData["APIKey"].ToString());
                }
            }
            catch (Exception ex)
            {
                // Display an error message
                SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Electra - Error", $"The configuration could not be loaded. Please fix any issues and restart the application.\n\nError: {ex.Message}\n\nConfiguration path: \"{Path.GetFullPath(ConfigPath)}\"\n\nStack trace: {ex.StackTrace}", 0);

                // Safely close the application
                CloseApplication(ex.HResult);
            }

            // Assign the mouse hook events
            Console.WriteLine("[INFO] >> Setting up mouse handler...");
            MouseHook.MouseDragged += OnMouseMoved;
            MouseHook.MouseMoved += OnMouseMoved;

            // Set the window icon
            Raylib.SetWindowIcon(TaskbarLogo);

            // Set the style manually because raylib doesn't seem to want to load a style from the disk properly >:(
            // Everything starting with '0x' is a color, some are unchecked due to uint -> int conversion. And yes, this
            // is probably a shitty way to accomplish this, I just don't know any better ways as of right now
            Console.WriteLine("[INFO] >> Setting theme...");

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
            Console.WriteLine("[INFO] >> Creating textures from images...");
            TitlebarLogoTexture = Raylib.LoadTextureFromImage(TitlebarLogo);
            IntensityTexture = Raylib.LoadTextureFromImage(IntensityIcon);
            DurationTexture = Raylib.LoadTextureFromImage(DurationIcon);

            // Configure timers
            // Garbage collection timer
            Console.WriteLine("[INFO] >> Setting up timers...");
            GarbageCollectionTimer.Recurring = true;
            GarbageCollectionTimer.Start();

            // Discord RPC timer
            if (EnableDiscordRPC)
            {
                DiscordRPCUpdateTimer.Recurring = true;
                DiscordRPCUpdateTimer.Start();
            }

            // Screen DPI timer
            DPIUpdateTimer.Recurring = true;
            DPIUpdateTimer.Start();

            // Run the mouse hook asynchronously
            MouseHook.RunAsync();

            // This flag is set here because it is used to determine if the window should be closed or not
            WindowOpened = true;

            // Main loop
            Console.WriteLine("[INFO] >> Init finished.");
            while (!ExitProgram && !Raylib.WindowShouldClose())
            {
                // Start drawing
                Raylib.BeginDrawing();
                //Raylib.BeginTextureMode(TargetRenderTexture);

                // Check if the user is dragging the window
                if (!DragLock && Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), DragRect) && Raylib.IsMouseButtonPressed(0))
                {
                    DragOffsetX = Math.Abs(Raylib.GetWindowPosition().X - MousePos.X);
                    DragOffsetY = Math.Abs(Raylib.GetWindowPosition().Y - MousePos.Y);
                    DragLockTimer.Reset();
                    DragLock = true;

                    Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_RESIZE_ALL);
                    RayGui.GuiLock();
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
                DPIUpdateTimer.Update();
                DragLockTimer.Update();

                // Draw window elements
                UpdateScreen();

                // Exit render texture mode
                //Raylib.EndTextureMode();
                
                DPITargetScaleRect.width = ScaledWindowSize[0];
                DPITargetScaleRect.height = ScaledWindowSize[1];

                //Raylib.DrawTexturePro(TargetRenderTexture.texture, FinalRenderRect, DPITargetScaleRect, Vector2.Zero, 0f, Raylib.WHITE);

                // Stop drawing
                Raylib.EndDrawing();
            }

            // Safely close the application
            CloseApplication(0);
        }

        /// <summary>
        /// Draw UI elements on the window.
        /// </summary>
        private static void UpdateScreen()
        {
            // Clear the window and draw the titlebar
            Raylib.ClearBackground(BGColor);
            Raylib.DrawRectangle(0, 0, (int)ScaledWindowSize[0], (int)MathUtilities.GetPercentageOfNumber(48, WindowDPI[0] * 100), TitlebarColor);

            // Get the intensity and duration from the sliders
            PiShockAPI.Intensity = (int)RayGui.GuiSlider(IntensityRect, null, $"    {PiShockAPI.Intensity}%", PiShockAPI.Intensity, 1, PiShockAPI.MaxIntensity);
            PiShockAPI.Duration = (int)RayGui.GuiSlider(DurationRect, null, $"    {PiShockAPI.Duration}s", PiShockAPI.Duration, 1, PiShockAPI.MaxDuration);

            // Draw and process buttons
            // Refresh button
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
            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), CloseRect) && !DragLock)
            {
                if (Raylib.IsMouseButtonReleased(0) && Raylib.IsWindowFocused())
                {
                    ExitProgram = true;
                }
                else if(Raylib.IsMouseButtonDown(0))
                {
                    Raylib.DrawRectangle((int)ScaledWindowSize[0] - DPIPercentageOf48, 0, DPIPercentageOf48, DPIPercentageOf48, ClosePressedColor);
                }
                else
                {
                    Raylib.DrawRectangle((int)ScaledWindowSize[0] - DPIPercentageOf48, 0, DPIPercentageOf48, DPIPercentageOf48, CloseNormalColor);
                }
            }

            // Check if the user is minimizing the window
            else if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), MinimizeRect) && !DragLock)
            {
                if (Raylib.IsMouseButtonReleased(0) && Raylib.IsWindowFocused())
                {
                    Raylib.MinimizeWindow();
                }
                else if (Raylib.IsMouseButtonDown(0))
                {
                    Raylib.DrawRectangle((int)ScaledWindowSize[0] - DPIPercentageOf96, 0, DPIPercentageOf48, DPIPercentageOf48, MinimizePressedColor);
                }
                else
                {
                    Raylib.DrawRectangle((int)ScaledWindowSize[0] - DPIPercentageOf96, 0, DPIPercentageOf48, DPIPercentageOf48, MinimizeNormalColor);
                }
            }

            // Draw the shocker information.
            Raylib.DrawRectangleLinesEx(InfoRect, 2, Raylib.GetColor(0x2F7486FF));
            Raylib.DrawRectangle(18, 270, (int)ScaledWindowSize[0] - 36, 110, Raylib.GetColor(0x024658FF));
            Raylib.DrawText($"{PiShockAPI.ShockerInfo.Name}:", 22, 274, 20, Raylib.GOLD);
            Raylib.DrawText($"    ID: {PiShockAPI.ShockerInfo.ID}", 22, 294, 20, Raylib.GOLD);
            Raylib.DrawText($"    Paused: {PiShockAPI.ShockerInfo.IsPaused}", 22, 314, 20, Raylib.GOLD);
            Raylib.DrawText($"    Max int & dur: {PiShockAPI.ShockerInfo.MaxIntensity}, {PiShockAPI.ShockerInfo.MaxDuration}", 22, 334, 20, Raylib.GOLD);
            Raylib.DrawText($"    Share code: {PiShockAPI.APIConfig.Code}", 22, 354, 20, Raylib.GOLD);

            // Draw the titlebar decorations.
            // Title & Icon.
            Raylib.DrawText("Electra", 40, 14, DPIPercentageOf20, Raylib.GOLD);
            Raylib.DrawTexture(TitlebarLogoTexture, 6, 6, Raylib.WHITE);

            // Close button.
            //16 + (int)Math.Ceiling(WindowDPI[1] - 1f)
            Raylib.DrawRectangle((int)ScaledWindowSize[0] - DPIPercentageOf32, DPIPercentageOf16, DPIPercentageOf16, DPIPercentageOf16, Raylib.RED);
            Raylib.DrawText("x", (int)ScaledWindowSize[0] - (int)MathUtilities.GetPercentageOfNumber(29, WindowDPI[0] * 100), DPIPercentageOf13, DPIPercentageOf20, Raylib.WHITE);

            // Minimize button.
            Raylib.DrawLine((int)ScaledWindowSize[0] - (int)MathUtilities.GetPercentageOfNumber(80, WindowDPI[0] * 100), DPIPercentageOf24, (int)ScaledWindowSize[0] - DPIPercentageOf64, DPIPercentageOf24, Raylib.WHITE);

            // Draw textures.
            Raylib.DrawTexture(IntensityTexture, (int)ScaledWindowSize[0] - 54, 225, Raylib.WHITE);
            Raylib.DrawTexture(DurationTexture, (int)ScaledWindowSize[0] - 54, 244, Raylib.WHITE);

            // Draw the window border.
            Raylib.DrawRectangleLines(0, 0, (int)ScaledWindowSize[0], (int)ScaledWindowSize[1], BorderColor);
        }

        /// <summary>
        /// Get the screen space mouse position when the mouse has been moved (the non window-relative position on the screen in pixels).
        /// </summary>
        private static void OnMouseMoved(object? sender, MouseHookEventArgs e)
        {
            MousePos.X = e.Data.X;
            MousePos.Y = e.Data.Y;
        }

        /// <summary>
        /// Safely handle uncaught exceptions.
        /// </summary>
        private static void CrashSafely(object sender, UnhandledExceptionEventArgs args)
        {
            Exception ex = (Exception)args.ExceptionObject;

            SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR, "Electra - Error", $"An error occurred and was not caught: {ex.Message}\n\nStack trace: {ex.StackTrace}\n\nIs terminating: {args.IsTerminating}", 0);
            CloseApplication(ex.HResult);
        }

        /// <summary>
        /// Update the DPI scale vector and resize the window if required.
        /// </summary>
        private static void UpdateWindowDPI()
        {
            // Update the DPI scale.
            WindowDPI = Raylib.GetWindowScaleDPI();
            ScaledWindowSize = new Vector2(WindowWidth * WindowDPI[0], WindowHeight * WindowDPI[1]);

            // Scale the window with the DPI.
            Raylib.SetWindowSize((int)ScaledWindowSize[0], (int)ScaledWindowSize[1]);

            if (Raylib.GetKeyPressed() != 0)
            {
                SDL.SDL_ShowSimpleMessageBox(SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_INFORMATION, "GUH!!!!!!!!11!1!!1", $"Size: {Raylib.GetScreenWidth()}x{Raylib.GetScreenHeight()}\nUnscaled DPI: {WindowDPI}\nScaled window size: {ScaledWindowSize}", 0);
            }

            // Update percentages.
            DPIPercentageOf13 = (int)MathUtilities.GetPercentageOfNumber(13, WindowDPI[0] * 100);
            DPIPercentageOf16 = (int)MathUtilities.GetPercentageOfNumber(16, WindowDPI[0] * 100);
            DPIPercentageOf20 = (int)MathUtilities.GetPercentageOfNumber(20, WindowDPI[0] * 100);
            DPIPercentageOf24 = (int)MathUtilities.GetPercentageOfNumber(24, WindowDPI[0] * 100);
            DPIPercentageOf32 = (int)MathUtilities.GetPercentageOfNumber(32, WindowDPI[0] * 100);
            DPIPercentageOf48 = (int)MathUtilities.GetPercentageOfNumber(48, WindowDPI[0] * 100);
            DPIPercentageOf64 = (int)MathUtilities.GetPercentageOfNumber(64, WindowDPI[0] * 100);
            DPIPercentageOf96 = (int)MathUtilities.GetPercentageOfNumber(96, WindowDPI[0] * 100);

            // Recalculate rectangle sizes.
            IntensityRect = new Rectangle(16, 224, ScaledWindowSize[0] - 72f, DPIPercentageOf16);
            DurationRect = new Rectangle(16, 244, ScaledWindowSize[0] - 72f, DPIPercentageOf16);
            MinimizeRect = new Rectangle(ScaledWindowSize[0] - DPIPercentageOf96, 0, DPIPercentageOf48, DPIPercentageOf48);
            RefreshRect = new Rectangle(16, 184, ScaledWindowSize[0] - 32, DPIPercentageOf32);
            VibrateRect = new Rectangle(16, 104, ScaledWindowSize[0] - 32, DPIPercentageOf32);
            CloseRect = new Rectangle(ScaledWindowSize[0] - DPIPercentageOf48, 0, DPIPercentageOf48, DPIPercentageOf48);
            ShockRect = new Rectangle(16, 144, ScaledWindowSize[0] - 32, DPIPercentageOf32);
            InfoRect = new Rectangle(16, 268, ScaledWindowSize[0] - 32, 114);
            BeepRect = new Rectangle(16, 64, ScaledWindowSize[0] - 32, DPIPercentageOf32);
            DragRect = new Rectangle(0, 0, ScaledWindowSize[0] - 96, DPIPercentageOf48);
    }

        /// <summary>
        /// Safely close the application.
        /// </summary>
        /// <param name="ExitCode">The exit code that the program should use when closing.</param>
        public static void CloseApplication(int ExitCode)
        {
            // Stop recurring timers.
            Console.WriteLine("[INFO] >> Stopping timers...");
            GarbageCollectionTimer.Stop();
            DiscordRPCUpdateTimer.Stop();

            // Close the Discord RPC client.
            Console.WriteLine("[INFO] >> Closing Discord RPC client...");
            DiscordRPC.Close();

            // Close the serial port if it's in use.
            Console.WriteLine("[INFO] >> Closing serial if required...");
            Serial.Close();

            // Dispose of the mouse hook.
            Console.WriteLine("[INFO] >> Disposing of mouse hook...");
            MouseHook.Dispose();

            // Unload assets to prevent memory leaks and/or file errors.
            Console.WriteLine("[INFO] >> Unloading assets...");
            Raylib.UnloadRenderTexture(TargetRenderTexture);
            Raylib.UnloadTexture(TitlebarLogoTexture);
            Raylib.UnloadImage(TaskbarLogo);
            Raylib.UnloadImage(TitlebarLogo);

            // Close the window.
            Console.WriteLine("[INFO] >> Closing window...");
            if (WindowOpened)
            {
                Raylib.CloseWindow();
            }

            // Exit the application.
            Console.WriteLine("[INFO] >> Exitting...");
            Environment.Exit(ExitCode);
        }
        #endregion
    }
    #endregion
}
#endregion