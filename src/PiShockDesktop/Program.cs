#define RAYGUI_IMPLEMENTATION
#define RAYGUI_STATIC

using System.Numerics;
using System.Runtime.InteropServices;
using Raylib_CsLo;

namespace PiShockDesktop
{
    internal class Program
    {
        /* VARIABLES */
        private static float DragOffsetX = 1f, DragOffsetY = 1f;
        private static System.Drawing.Point MousePos;
        private static DateTime LastUpdateTime = DateTime.Now;
        static Rectangle TestRect = new Rectangle(0, 0, 128, 128);

        /* DLL IMPORTS */
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(ref System.Drawing.Point lpPoint);

        /* FUNCTIONS */
        static void Main(string[] args)
        {
            // Set the window flags
            Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
            Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_UNDECORATED);
            Raylib.SetConfigFlags(ConfigFlags.FLAG_VSYNC_HINT);

            // Create a window
            Raylib.InitWindow(450, 600, "PiShock Desktop");

            while (!Raylib.WindowShouldClose())
            {
                GetCursorPos(ref MousePos);
                if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), TestRect))
                {

                }

                if (Raylib.IsMouseButtonPressed(0))
                {
                    DragOffsetX = Math.Abs(Raylib.GetWindowPosition().X - MousePos.X);
                    DragOffsetY = Math.Abs(Raylib.GetWindowPosition().Y - MousePos.Y);
                }

                if (Raylib.IsMouseButtonDown(0))
                {
                    Raylib.BeginDrawing();
                    Raylib.SetWindowPosition((int)(MousePos.X - DragOffsetX), (int)(MousePos.Y - DragOffsetY));
                    Raylib.EndDrawing();
                }

                if ((DateTime.Now - LastUpdateTime).TotalMilliseconds >= 140)
                {
                    UpdateScreen();

                    LastUpdateTime = DateTime.Now;
                }

                Thread.Sleep(10);
            }

            Raylib.CloseWindow();
        }

        static void UpdateScreen()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(18, 18, 18, 255));
            Raylib.DrawText("PiShock Desktop", 12, 12, 20, new Color(255, 255, 255, 255));
            Raylib.DrawRectangle(418, 16, 16, 16, new Color(255, 0, 0, 255));
            Raylib.DrawLine(384, 24, 400, 24, new Color(255, 255, 255, 255));
            Raylib.EndDrawing();
        }
    }
}
