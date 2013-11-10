using System;
using System.Threading;
using System.Reflection;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using Microsoft.Xna.Framework.Graphics;
using XnaInput = Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace FractalGpu.Core
{
    public class Tools
    {
		public static string ContentDirectory;

        public static void Write(object obj)
        {
			Write("{0}", obj);
        }

        public static void Write(string str, params object[] objs)
        {
#if WINDOWS
            if (objs.Length == 0) Console.WriteLine(str);
            else Console.WriteLine(str, objs);
#else
			if (objs.Length == 0) System.Diagnostics.Debug.WriteLine(str);
			else System.Diagnostics.Debug.WriteLine(str, objs);
#endif
        }

		/// <summary>
		/// Converts an angle in radians to a normalized direction vector.
		/// </summary>
		/// <param name="Angle">The angle in radians.</param>
		/// <returns></returns>
		public static Vector2 AngleToDir(double Angle)
		{
			return new Vector2((float)Math.Cos(Angle), (float)Math.Sin(Angle));
		}

        public static FractalGpu.FractalGame TheGame;

        public static String[] ButtonNames = { "A", "B", "X", "Y", "RS", "LS", "RT", "LT", "RJ", "RJ", "LJ", "LJ", "DPad", "Start" };
        public static String[] DirNames = { "right", "up", "left", "down" };

        public static bool UsingSpriteBatch;
        public static SpriteBatch spriteBatch;

        public static XnaInput.KeyboardState keybState, PrevKeyboardState;
        public static XnaInput.MouseState CurMouseState, PrevMouseState;
        public static Vector2 DeltaMouse;
        public static bool Editing;

        public static Vector2 MousePos
        {
            get { return new Vector2(CurMouseState.X, CurMouseState.Y); }
            set { XnaInput.Mouse.SetPosition((int)value.X, (int)value.Y); }
        }

        /// <summary>
        /// Whether the left mouse button is currently down.
        /// </summary>
        public static bool CurMouseDown() { return CurMouseState.LeftButton == XnaInput.ButtonState.Pressed; }

        /// <summary>
        /// Whether the left mouse button was down on the last frame.
        /// </summary>
        public static bool PrevMouseDown() { return PrevMouseState.LeftButton == XnaInput.ButtonState.Pressed; }

        /// <summary>
        /// True when the left mouse button was pressed and released.
        /// </summary>
        public static bool MouseReleased() { return !CurMouseDown() && PrevMouseDown(); }

        /// <summary>
        /// True when the left mouse button isn't down currently and also wasn't down on the previous frame.
        /// </summary>
        public static bool MouseNotDown() { return !CurMouseDown() && !PrevMouseDown(); }

        /// <summary>
        /// True when the left mouse button is down currently or was down on the previous frame.
        /// </summary>
        public static bool MouseDown() { return CurMouseDown() || PrevMouseDown(); }


        /// <summary>
        /// Whether the left RightMouse button is currently down.
        /// </summary>
        public static bool CurRightMouseDown() { return CurMouseState.RightButton == XnaInput.ButtonState.Pressed; }

        /// <summary>
        /// Whether the left RightMouse button was down on the last frame.
        /// </summary>
        public static bool PrevRightMouseDown() { return PrevMouseState.RightButton == XnaInput.ButtonState.Pressed; }

        /// <summary>
        /// True when the left RightMouse button was pressed and released.
        /// </summary>
        public static bool RightMouseReleased() { return !CurRightMouseDown() && PrevRightMouseDown(); }

        public static XnaInput.GamePadState[] padState, PrevpadState;

        public static GameTime gameTime;
        public static Random Rnd;

		public static GraphicsDeviceManager DeviceManager;
        public static GraphicsDevice Device;

        public static float t, dt;

        public static bool StepControl = false;
        static int _PhsxSpeed = 1;
        public static int PhsxSpeed { get { return _PhsxSpeed; } set { _PhsxSpeed = value; } }

        /// <summary>
        /// Increases the number of phsx steps taken per frame.
        /// </summary>
        public static void IncrPhsxSpeed()
        {
            Tools.PhsxSpeed += 1;
            if (Tools.PhsxSpeed > 3) Tools.PhsxSpeed = 0;
        }

        /// <summary>
        /// Starts the SpriteBatch if it isn't started already. The quad drawer is flushed first.
        /// </summary>
        public static void StartSpriteBatch()
        {
            if (!UsingSpriteBatch)
            {
                Tools.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                UsingSpriteBatch = true;
            }
        }

        /// <summary>
        /// Core wrapper for drawing text. Assumes SpriteBatch is started.
        /// </summary>
        public static void DrawText(Vector2 pos, string str, SpriteFont font)
        {
            Tools.spriteBatch.DrawString(font, str, pos, Color.Azure, 0, Vector2.Zero, new Vector2(.5f, .5f), SpriteEffects.None, 0);
        }

        /// <summary>
        /// Sets the standard render states.
        /// </summary>
        public static void SetStandardRenderStates()
        {
            Tools.Device.RasterizerState = RasterizerState.CullNone;
            Tools.Device.BlendState = BlendState.AlphaBlend;
            Tools.Device.DepthStencilState = DepthStencilState.DepthRead;
        }

        /// <summary>
        /// Ends the SpriteBatch, if in use, and resets standard render states.
        /// </summary>
        public static void EndSpriteBatch()
        {
            if (UsingSpriteBatch)
            {
                UsingSpriteBatch = false;

                Tools.spriteBatch.End();

                SetStandardRenderStates();
            }
        }
    }
}