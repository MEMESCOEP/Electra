using System.Numerics;
using System.Runtime.InteropServices;
using Raylib_CsLo;

namespace PiShockDesktop
{
    internal class Program
    {
        /* VARIABLES */
        // Booleans
        private static bool ExitProgram = false;
        private static bool DragLock = false;

        // Integers
        private static int ScreenHeight = 600;
        private static int ScreenWidth = 450;
        private static int SleepTime = 0;

        // Floats
        private static float DragOffsetX = 1f;
        private static float DragOffsetY = 1f;

        // Points
        private static System.Drawing.Point MousePos;

        // Rectangles
        private static Rectangle MinimizeRect = new Rectangle(ScreenWidth - 96, 0, 48, 48);
        private static Rectangle CloseRect = new Rectangle(ScreenWidth - 48, 0, 48, 48);
        private static Rectangle DragRect = new Rectangle(0, 0, ScreenWidth - 96, 48);

        // Images
        private static Image TitlebarLogo = Raylib.LoadImage(@"Assets/Logo_32x32.png");
        private static Image TaskbarLogo = Raylib.LoadImage(@"Assets/Logo_128x128.png");

        // Datetimes
        private static DateTime LastUpdateTime = DateTime.Now;

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

        /* DLL IMPORTS */
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(ref System.Drawing.Point lpPoint);

        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(IntPtr h, string m, string c, int type);

        /* FUNCTIONS */
        private static void Main(string[] args)
        {
            // Configure the PiShock API
            PiShockAPI.Configure("memescoep", "andrew", "2A215BCA618", "cf9358aa-25f3-4752-83c9-64dc89ad69c9");
            
            // Set the window flags
            Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_UNDECORATED);
            Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_ALWAYS_RUN);
            Raylib.SetConfigFlags(ConfigFlags.FLAG_VSYNC_HINT);

            // Create a window
            Raylib.InitWindow(ScreenWidth, ScreenHeight, "PiShock Desktop");
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
                    Raylib.SetMouseCursor(9);
                    RayGui.GuiDisable();
                    DragLock = true;
                }

                // Update the screen
                UpdateScreen();

                // Stop drawing
                Raylib.EndDrawing();

                /*PiShockAPI.SendCommand(1, 0.5f, PiShockAPI.CommandType.Shock);
                PiShockAPI.SendCommand(1, 0.5f, PiShockAPI.CommandType.Vibrate);
                PiShockAPI.SendCommand(1, 0.5f, PiShockAPI.CommandType.Beep);
                PiShockAPI.SendCommand(1, 0.5f, PiShockAPI.CommandType.GetShockerInfo);*/

                if (Raylib.IsMouseButtonUp(0))
                {
                    Raylib.SetMouseCursor(0);
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
                    // Uses less CPU, at minimal framerate cost. (30 FPS)
                    Thread.Sleep(32);
                }
            }

            Raylib.CloseWindow();
        }

        static int h2d(char c)
        {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'A' && c <= 'F') return c - 'A' + 10;
            if (c >= 'a' && c <= 'f') return c - 'a' + 10;
            return 0;
        }

        // Draw UI elements
        private static void UpdateScreen()
        {
            Raylib.ClearBackground(BGColor);
            Raylib.DrawRectangle(0, 0, ScreenWidth, 48, TitlebarColor);

            // Draw and process buttons
            if (RayGui.GuiButton(new Rectangle(16, 144, ScreenWidth - 32, 32), "Shock"))
            {
                PiShockAPI.SendCommand(1f, 1f, PiShockAPI.CommandType.Shock);
            }

            if (RayGui.GuiButton(new Rectangle(16, 104, ScreenWidth - 32, 32), "Vibrate"))
            {
                PiShockAPI.SendCommand(1f, 1f, PiShockAPI.CommandType.Vibrate);
            }

            if (RayGui.GuiButton(new Rectangle(16, 64, ScreenWidth - 32, 32), "Beep"))
            {
                PiShockAPI.SendCommand(1f, 1f, PiShockAPI.CommandType.Beep);
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

            Raylib.DrawText("PiShock Desktop", 40, 14, 20, Raylib.WHITE);
            Raylib.DrawTexture(TitlebarLogoTexture, 6, 6, Raylib.WHITE);
            Raylib.DrawRectangle(418, 16, 16, 16, Raylib.RED);
            Raylib.DrawText("x", 421, 13, 20, Raylib.WHITE);
            Raylib.DrawLine(ScreenWidth - 80, 24, ScreenWidth - 64, 24, Raylib.WHITE);
            Raylib.DrawRectangleLines(0, 0, ScreenWidth, ScreenHeight, BorderColor);
        }
    }
}
