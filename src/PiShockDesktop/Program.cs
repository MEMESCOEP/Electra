using System.Runtime.InteropServices;
using System.Text;
using Raylib_CsLo;
using IniParser.Model;
using IniParser;

namespace PiShockDesktop
{
    internal class Program
    {
        /* VARIABLES */
        // Booleans
        private static bool ExitProgram = false;
        private static bool DragLock = false;

        // Integers
        private static int ScreenHeight = 420;
        private static int ScreenWidth = 450;
        private static int SleepTime = 0;

        // Floats
        private static float DragOffsetX = 1f;
        private static float DragOffsetY = 1f;

        // Points
        private static System.Drawing.Point MousePos;

        // Rectangles
        private static Rectangle ShareCodeTextBoxRect = new Rectangle(16, 224, ScreenWidth - 32, 32);
        private static Rectangle UsernameTextBoxRect = new Rectangle(16, 264, ScreenWidth - 32, 32);
        private static Rectangle APIKeyTextBoxRect = new Rectangle(16, 184, ScreenWidth - 32, 32);
        private static Rectangle MinimizeRect = new Rectangle(ScreenWidth - 96, 0, 48, 48);
        private static Rectangle CloseRect = new Rectangle(ScreenWidth - 48, 0, 48, 48);
        private static Rectangle DragRect = new Rectangle(0, 0, ScreenWidth - 96, 48);

        // Images
        private static Image TitlebarLogo = Raylib.LoadImage(@"Assets/Logo_32x32.png");
        private static Image TaskbarLogo = Raylib.LoadImage(@"Assets/Logo_128x128.png");

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

        // String builders
        private static StringBuilder ShareCodeTextBoxText = new StringBuilder("SHARE CODE", 36);
        private static StringBuilder UsernameTextBoxText = new StringBuilder("USERNAME", 36);
        private static StringBuilder APIKeyTextBoxText = new StringBuilder("API KEY", 36);

        // INI configuration parser(s)
        private static FileIniDataParser INIConfigParser = new FileIniDataParser();

        /* DLL IMPORTS */
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(ref System.Drawing.Point lpPoint);

        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(IntPtr h, string m, string c, int type);

        /* FUNCTIONS */
        private static void Main(string[] args)
        {
            // Read the INI configuration file
            IniData ConfigData = INIConfigParser.ReadFile(@"Assets/PiShockDesktopConfiguration.ini");

            // Configure the PiShock API
            PiShockAPI.Configure(ConfigData["API"]["PiShockAccountName"], ConfigData["API"]["YourName"], ConfigData["API"]["ShareCode"], ConfigData["API"]["APIKey"]);
            
            // Set the window flags
            Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_UNDECORATED);
            Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_ALWAYS_RUN);
            Raylib.SetConfigFlags(ConfigFlags.FLAG_VSYNC_HINT);

            // Create a window
            Raylib.InitWindow(ScreenWidth, ScreenHeight, "PiShock Desktop");
            Raylib.SetWindowTitle("PiShock Desktop");
            Raylib.SetWindowIcon(TaskbarLogo);

            // Set the style manually because raylib doesn't want to load a style properly >:(
            // Everything starting with '0x' is a color. Some are unchecked because of uint to int conversion
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

            RayGui.GuiSetStyle((int)GuiControl.SLIDER, (int)GuiControlProperty.BASE_COLOR_NORMAL, 0x3299B4FF);
            RayGui.GuiSetStyle((int)GuiControl.SLIDER, (int)GuiControlProperty.BASE_COLOR_FOCUSED, 0x3299B4FF);
            RayGui.GuiSetStyle((int)GuiControl.SLIDER, (int)GuiControlProperty.BASE_COLOR_PRESSED, unchecked((int)0xFFBC51FF));
            RayGui.GuiSetStyle((int)GuiControl.SLIDER, (int)GuiControlProperty.BASE_COLOR_DISABLED, 0x024658FF);

            RayGui.GuiSetStyle((int)GuiControl.SLIDER, (int)GuiControlProperty.TEXT_COLOR_NORMAL, 0x51BFD3FF);
            RayGui.GuiSetStyle((int)GuiControl.SLIDER, (int)GuiControlProperty.TEXT_COLOR_FOCUSED, unchecked((int)0xB6E1EAFF));
            RayGui.GuiSetStyle((int)GuiControl.SLIDER, (int)GuiControlProperty.TEXT_COLOR_PRESSED, unchecked((int)0xD86F36FF));
            RayGui.GuiSetStyle((int)GuiControl.SLIDER, (int)GuiControlProperty.TEXT_COLOR_DISABLED, 0x51BFD3FF);

            RayGui.GuiSetStyle((int)GuiControl.SLIDER, (int)GuiControlProperty.BORDER_COLOR_NORMAL, 0x2F7486FF);
            RayGui.GuiSetStyle((int)GuiControl.SLIDER, (int)GuiControlProperty.BORDER_COLOR_FOCUSED, unchecked((int)0x82CDE0FF));
            RayGui.GuiSetStyle((int)GuiControl.SLIDER, (int)GuiControlProperty.BORDER_COLOR_PRESSED, unchecked((int)0xEB7630FF));
            RayGui.GuiSetStyle((int)GuiControl.SLIDER, (int)GuiControlProperty.BORDER_COLOR_DISABLED, 0x2F7486FF);

            // Calculate the sleep time
            SleepTime = (int)((1f / Raylib.GetMonitorRefreshRate(Raylib.GetCurrentMonitor())) * 1000f) - 1;

            // Create textures from images
            TitlebarLogoTexture = Raylib.LoadTextureFromImage(TitlebarLogo);

            // Main loop
            while (!ExitProgram && !Raylib.WindowShouldClose())
            {
                // Start drawing
                Raylib.BeginDrawing();

                // Get the screen space mouse position (the position on the screen in pixels)
                GetCursorPos(ref MousePos);

                // Check if the user is dragging the window
                if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), DragRect) && Raylib.IsWindowFocused() && Raylib.IsMouseButtonPressed(0))
                {
                    DragOffsetX = Math.Abs(Raylib.GetWindowPosition().X - MousePos.X);
                    DragOffsetY = Math.Abs(Raylib.GetWindowPosition().Y - MousePos.Y);
                    Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_RESIZE_ALL);
                    RayGui.GuiDisable();
                    DragLock = true;
                }

                // Update the screen
                UpdateScreen();

                // Stop drawing
                Raylib.EndDrawing();

                if (Raylib.IsMouseButtonUp(0))
                {
                    Raylib.SetMouseCursor(MouseCursor.MOUSE_CURSOR_DEFAULT);
                    RayGui.GuiEnable();
                    DragLock = false;
                }

                // Move the window if the user is dragging it
                if (DragLock)
                {
                    Raylib.SetWindowPosition((int)(MousePos.X - DragOffsetX), (int)(MousePos.Y - DragOffsetY));
                    
                    // Uses less CPU, at minimal framerate cost. (Monitor refresh rate)
                    Thread.Sleep(SleepTime);
                }
                else
                {
                    // Uses less CPU, at minimal framerate cost. (25 FPS)
                    Thread.Sleep(40);
                }
            }

            Raylib.CloseWindow();
        }

        // Draw UI elements
        private static void UpdateScreen()
        {
            Raylib.ClearBackground(BGColor);
            Raylib.DrawRectangle(0, 0, ScreenWidth, 48, TitlebarColor);

            PiShockAPI.Intensity = (int)RayGui.GuiSlider(new Rectangle(16, 224, ScreenWidth - 72, 16), null, $"INT: {PiShockAPI.Intensity}", PiShockAPI.Intensity, 0, PiShockAPI.MaxIntensity);
            PiShockAPI.Duration = (int)RayGui.GuiSlider(new Rectangle(16, 244, ScreenWidth - 72, 16), null, $"DUR: {PiShockAPI.Duration}", PiShockAPI.Duration, 0, PiShockAPI.MaxDuration);

            // Draw and process buttons
            if (RayGui.GuiButton(new Rectangle(16, 184, ScreenWidth - 32, 32), "REFRESH INFO"))
            {
                PiShockAPI.UpdateShockerInfo();
            }

            if (RayGui.GuiButton(new Rectangle(16, 144, ScreenWidth - 32, 32), "SHOCK"))
            {
                PiShockAPI.SendCommand(PiShockAPI.Intensity, PiShockAPI.Duration, PiShockAPI.CommandType.Shock);
            }

            if (RayGui.GuiButton(new Rectangle(16, 104, ScreenWidth - 32, 32), "VIBRATE"))
            {
                PiShockAPI.SendCommand(PiShockAPI.Intensity, PiShockAPI.Duration, PiShockAPI.CommandType.Vibrate);
            }

            if (RayGui.GuiButton(new Rectangle(16, 64, ScreenWidth - 32, 32), "BEEP"))
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

            Raylib.DrawRectangleLinesEx(new Rectangle(16, 268, ScreenWidth - 32, 134), 2, Raylib.GetColor(0x2F7486FF));
            Raylib.DrawRectangle(18, 270, ScreenWidth - 36, 130, Raylib.GetColor(0x024658FF));
            Raylib.DrawText($"{PiShockAPI.ShockerInfo.Name}:", 22, 274, 20, Raylib.GOLD);
            Raylib.DrawText($"    ID: {PiShockAPI.ShockerInfo.ID}", 22, 294, 20, Raylib.GOLD);
            Raylib.DrawText($"    Paused: {PiShockAPI.ShockerInfo.IsPaused}", 22, 314, 20, Raylib.GOLD);
            Raylib.DrawText($"    Online: {PiShockAPI.ShockerInfo.IsOnline}", 22, 334, 20, Raylib.GOLD);
            Raylib.DrawText($"    Max int & dur: {PiShockAPI.ShockerInfo.MaxIntensity}, {PiShockAPI.ShockerInfo.MaxDuration}", 22, 354, 20, Raylib.GOLD);
            Raylib.DrawText($"    Share code: {PiShockAPI.APIConfig.Code}", 22, 374, 20, Raylib.GOLD);

            Raylib.DrawText("PiShock Desktop", 40, 14, 20, Raylib.GOLD);
            Raylib.DrawTexture(TitlebarLogoTexture, 6, 6, Raylib.WHITE);
            Raylib.DrawRectangle(418, 16, 16, 16, Raylib.RED);
            Raylib.DrawText("x", 421, 13, 20, Raylib.WHITE);
            Raylib.DrawLine(ScreenWidth - 80, 24, ScreenWidth - 64, 24, Raylib.WHITE);

            Raylib.DrawRectangleLines(0, 0, ScreenWidth, ScreenHeight, BorderColor);
        }
    }
}
