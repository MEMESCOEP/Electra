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

        // Textures
        private static Texture TitlebarLogoTexture;

        /* DLL IMPORTS */
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(ref System.Drawing.Point lpPoint);

        /* FUNCTIONS */
        private static void Main(string[] args)
        {
            // Set the window flags
            Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_UNDECORATED);
            Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_ALWAYS_RUN);
            Raylib.SetConfigFlags(ConfigFlags.FLAG_VSYNC_HINT);

            // Create a window
            Raylib.InitWindow(ScreenWidth, ScreenHeight, "PiShock Desktop");
            Raylib.SetWindowIcon(TaskbarLogo);

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
                    DragLock = true;
                }

                // Move the window if the user is dragging it
                if (DragLock)
                {
                    Raylib.SetWindowPosition((int)(MousePos.X - DragOffsetX), (int)(MousePos.Y - DragOffsetY));
                }

                if (Raylib.IsMouseButtonUp(0))
                {
                    DragLock = false;
                }

                // Update the screen
                UpdateScreen();

                // Stop drawing
                Raylib.EndDrawing();

                // Uses slightly less CPU, at minimal framerate cost
                Thread.Sleep(10);
            }

            Thread.Sleep(50);
            Raylib.CloseWindow();
        }

        // Draw UI elements
        private static void UpdateScreen()
        {
            Raylib.ClearBackground(new Color(18, 18, 18, 255));
            Raylib.DrawRectangle(0, 0, ScreenWidth, 48, new Color(12, 12, 12, 255));

            // Check if the user is closing the window
            if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), CloseRect) && Raylib.IsWindowFocused())
            {
                if (Raylib.IsMouseButtonReleased(0))
                {
                    Raylib.DrawRectangle(ScreenWidth - 48, 0, 48, 48, new Color(192, 0, 0, 255));
                    ExitProgram = true;
                }
                else
                {
                    Raylib.DrawRectangle(ScreenWidth - 48, 0, 48, 48, new Color(64, 0, 0, 255));
                }
            }

            // Check if the user is minimizing the window
            else if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), MinimizeRect) && Raylib.IsWindowFocused())
            {
                if (Raylib.IsMouseButtonReleased(0))
                {
                    Raylib.MinimizeWindow();
                }
                else
                {
                    Raylib.DrawRectangle(ScreenWidth - 96, 0, 48, 48, new Color(32, 32, 32, 255));
                }
            }

            Raylib.DrawText("PiShock Desktop", 40, 14, 20, new Color(255, 255, 255, 255));
            Raylib.DrawTexture(TitlebarLogoTexture, 6, 6, new Color(255, 255, 255, 255));
            Raylib.DrawRectangle(418, 16, 16, 16, new Color(255, 0, 0, 255));
            Raylib.DrawText("x", 421, 13, 20, new Color(255, 255, 255, 255));
            Raylib.DrawLine(ScreenWidth - 80, 24, ScreenWidth - 64, 24, new Color(255, 255, 255, 255));
            Raylib.DrawRectangleLines(0, 0, ScreenWidth, ScreenHeight, new Color(50, 50, 50, 255));
            //Raylib.DrawRectangleLines(0, 0, ScreenWidth, ScreenHeight, new Color(System.Drawing.SystemColors.Desktop.R, System.Drawing.SystemColors.Desktop.G, System.Drawing.SystemColors.Desktop.B, System.Drawing.SystemColors.Desktop.A));
        }
    }
}
