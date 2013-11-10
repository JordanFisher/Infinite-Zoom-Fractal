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

using Drawing;

namespace FractalGpu
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

        public static string SourceTextureDirectory()
        {
            return Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Directory.GetCurrentDirectory()))), "Content\\Art");
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

        public static FractalGpu.Game1 TheGame;

        public static String[] ButtonNames = { "A", "B", "X", "Y", "RS", "LS", "RT", "LT", "RJ", "RJ", "LJ", "LJ", "DPad", "Start" };
        public static String[] DirNames = { "right", "up", "left", "down" };

        public static bool UsingSpriteBatch;
        public static SpriteBatch spriteBatch;
        public static EzFont LilFont, ScoreTextFont;
        public static EzFont MonoFont;
        public static EzFont Font_Dylan60, Font_Dylan42, Font_Dylan20, Font_Dylan15, Font_Dylan24, Font_Dylan28;
        public static EzFont Font_DylanThick20, Font_DylanThick24, Font_DylanThick28;
        public static EzFont Font_DylanThin42;

        public static int[] VibrateTimes = { 0, 0, 0, 0 };

#if WINDOWS
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

        //public static Vector2 MouseGUIPos(Vector2 zoom)
        //{
        //    return Tools.ToGUICoordinates(new Vector2(Tools.CurMouseState.X, Tools.CurMouseState.Y), Tools.CurLevel.MainCamera, zoom);
        //}

        //public static Vector2 MouseWorldPos()
        //{
        //    return Tools.ToWorldCoordinates(new Vector2(Tools.CurMouseState.X, Tools.CurMouseState.Y), Tools.CurLevel.MainCamera);
        //}
#else

#endif
        public static XnaInput.GamePadState[] padState, PrevpadState;

        public static GameTime gameTime;
        public static Random Rnd;
        public static EzEffectWad EffectWad;
        public static EzEffect BasicEffect, NoTexture, CircleEffect;
        public static EzTextureWad TextureWad;
        public static ContentManager SoundContentManager;
        public static QuadDrawer QDrawer;
        public static GraphicsDevice Device;
        public static RenderTarget2D DestinationRenderTarget;
        public static bool ScreenshotMode, CapturingVideo;
        public static Texture2D Screenshot;
        public static float t, dt;
        public static int Screenshots = 0;
        public static int DrawCount, PhsxCount;

        public static bool FreeCam = false;
        public static bool DrawBoxes = false;
        public static bool DrawGraphics = true;
        public static bool StepControl = false;
        static int _PhsxSpeed = 1;
        public static int PhsxSpeed { get { return _PhsxSpeed; } set { _PhsxSpeed = value; } }

        public static bool ShowLoadingScreen;

        public static void LoadBasicArt(ContentManager Content)
        {
            TextureWad = new EzTextureWad();
            TextureWad.AddTexture(Content.Load<Texture2D>("White"), "White");
            TextureWad.AddTexture(Content.Load<Texture2D>("Circle"), "Circle");
            TextureWad.AddTexture(Content.Load<Texture2D>("Smooth"), "Smooth");

            TextureWad.DefaultTexture = TextureWad.TextureList[0];
        }

        public static string GetFileName(String FilePath)
        {
            int i = FilePath.LastIndexOf("\\");
            int j = FilePath.IndexOf(".", i);
            if (i < 0) return FilePath.Substring(0, j - 1);
            else return FilePath.Substring(i + 1, j - 1 - i);
        }

        public static string GetFileNamePlusExtension(String FilePath)
        {
            int i = FilePath.LastIndexOf("\\");
            int n = FilePath.Length;
            if (i < 0) return FilePath.Substring(0, n - 1);
            else return FilePath.Substring(i + 1, n - 1 - i);
        }

        public static string GetFileBigName(String FilePath)
        {
            int i = FilePath.LastIndexOf("\\");
            if (i < 0) return FilePath;

            string Path = FilePath.Substring(0, i);
            i = Path.LastIndexOf("\\");

            if (i < 0) return FilePath;
            else return FilePath.Substring(i + 1);
        }

        public static string GetFileName(String path, String FilePath)
        {
            int i = FilePath.IndexOf(path) + path.Length + 1;
            if (i < 0) return "ERROR";
            int j = FilePath.IndexOf(".", i);
            if (j <= i) return "ERROR";

            return FilePath.Substring(i, j - i);
        }

        public static string GetFileExt(String path, String FilePath)
        {
            int j = FilePath.IndexOf(".");
            if (j < 0) return "ERROR";

            return FilePath.Substring(j + 1);
        }

        public static string[] GetFiles(string path, bool IncludeSubdirectories)
        {
            List<string> files = new List<string>();
            files.AddRange(Directory.GetFiles(path));

            if (IncludeSubdirectories)
            {
                string[] dir = Directory.GetDirectories(path);
                for (int i = 0; i < dir.Length; i++)
                    files.AddRange(GetFiles(dir[i], IncludeSubdirectories));
            }

            return files.ToArray();
        }


        public static void PreloadArt(ContentManager Content)
        {
            String path = Path.Combine(TheGame.Content.RootDirectory, "Art");
            string[] files = GetFiles(path, true);

            foreach (String file in files)
            {
                if (GetFileExt(path, file) == "xnb")
                {
                    Tools.TextureWad.AddTexture(null, "Art\\" + GetFileName(path, file));
                }
            }
        }


        public static void LoadEffects(ContentManager Content, bool CreateNewWad)
        {
            if (CreateNewWad)
                EffectWad = new EzEffectWad();

            EffectWad.AddEffect(Content.Load<Effect>("Effects\\BasicEffect"), "Basic");
            EffectWad.AddEffect(Content.Load<Effect>("Effects\\Standard"), "Standard");
            EffectWad.AddEffect(Content.Load<Effect>("Effects\\Fractal"), "Fractal");

            BasicEffect = EffectWad.EffectList[0];
        }

        public static float BoxSize(Vector2 TR, Vector2 BL)
        {
            return (TR.X - BL.X) * (TR.Y - BL.Y);
        }

        public static float CurVolume = -1;


        public static string RemoveComment(String str)
        {
            int CommentIndex = str.IndexOf("//");
            if (CommentIndex > 0)
                str = str.Substring(0, CommentIndex);
            return str;
        }

        public static Vector2 ParseToVector2(String str)
        {
            int CommaIndex = str.IndexOf(",");
            Vector2 Vec = new Vector2();

            String Component1, Component2;
            Component1 = str.Substring(0, CommaIndex);
            Component2 = str.Substring(CommaIndex + 1, str.Length - CommaIndex - 1);
            Vec.X = float.Parse(Component1);
            Vec.Y = float.Parse(Component2);

            return Vec;
        }

        public static Color ParseToColor(String str)
        {
            int CommaIndex = str.IndexOf(",");
            int CommaIndex2 = str.IndexOf(",", CommaIndex + 1);
            int CommaIndex3 = str.IndexOf(",", CommaIndex2 + 1);

            String Component1, Component2, Component3, Component4;
            Component1 = str.Substring(0, CommaIndex);
            Component2 = str.Substring(CommaIndex + 1, CommaIndex2 - CommaIndex - 1);
            Component3 = str.Substring(CommaIndex2 + 1, CommaIndex3 - CommaIndex2 - 1);
            Component4 = str.Substring(CommaIndex3 + 1, str.Length - CommaIndex3 - 1);

            Color clr = new Color(byte.Parse(Component1), byte.Parse(Component2), byte.Parse(Component3), byte.Parse(Component4));

            return clr;
        }

        /// <summary>
        /// Returns the substring inside two quotations.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static String ParseToFileName(String str)
        {
            int Quote1 = str.IndexOf("\"");
            int Quote2 = str.IndexOf("\"", Quote1 + 1);

            String Name = str.Substring(Quote1 + 1, Quote2 - Quote1 - 1);
            return Name;
        }

        /// <summary>
        /// Increases the number of phsx steps taken per frame.
        /// </summary>
        public static void IncrPhsxSpeed()
        {
            Tools.PhsxSpeed += 1;
            if (Tools.PhsxSpeed > 3) Tools.PhsxSpeed = 0;
        }

        /// <summary>
        /// Returns the number of elements in an enumeration.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int Length<T>()
        {
            return GetValues<T>().Count();
        }

        public static IEnumerable<T> GetValues<T>()
        {
            return (from x in typeof(T).GetFields(BindingFlags.Static | BindingFlags.Public)
                    select (T)x.GetValue(null));
        }

        public static byte FloatToByte(float x)
        {
            if (x <= 0) return (byte)0;
            if (x >= 1) return (byte)255;
            return (byte)(255 * x);
        }


        /// <summary>
        /// Takes in world coordinates and returns screen coordinates.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="cam"></param>
        /// <returns></returns>
        public static Vector2 ToScreenCoordinates(Vector2 pos, Camera cam)
        {
            Vector2 loc = Vector2.Zero;
            loc.X = (pos.X - cam.Data.Position.X) / cam.AspectRatio * cam.Zoom.X;
            loc.Y = (pos.Y - cam.Data.Position.Y) * cam.Zoom.Y;

            loc.X *= cam.ScreenWidth / 2;
            loc.Y *= -cam.ScreenHeight / 2;

            loc.X += cam.ScreenWidth / 2;
            loc.Y += cam.ScreenHeight / 2;

            return loc;
        }

        /// <summary>
        /// Takes in screen coordinates and returns world coordinates.
        /// (0,0) corresponds to the top left corner of the screen.
        /// </summary>
        public static Vector2 ToGUICoordinates(Vector2 pos, Camera cam)
        {
            return ToWorldCoordinates(pos, cam, new Vector2(.001f, .001f));
        }
        public static Vector2 ToGUICoordinates(Vector2 pos, Camera cam, Vector2 zoom)
        {
            return ToWorldCoordinates(pos, cam, zoom);
        }

        /// <summary>
        /// Takes in screen coordinates and returns world coordinates.
        /// (0,0) corresponds to the top left corner of the screen.
        /// </summary>
        public static Vector2 ToWorldCoordinates(Vector2 pos, Camera cam)
        {
            return ToWorldCoordinates(pos, cam, cam.Zoom);
        }
        public static Vector2 ToWorldCoordinates(Vector2 pos, Camera cam, Vector2 zoom)
        {
            pos.X -= cam.ScreenWidth / 2;
            pos.Y -= cam.ScreenHeight / 2;

            pos.X /= cam.ScreenWidth / 2;
            pos.Y /= -cam.ScreenHeight / 2;

            pos.X = pos.X * cam.AspectRatio / zoom.X + cam.Data.Position.X;
            pos.Y = pos.Y / zoom.Y + cam.Data.Position.Y;

            return pos;
        }

        /// <summary>
        /// Starts the SpriteBatch if it isn't started already. The quad drawer is flushed first.
        /// </summary>
        public static void StartSpriteBatch()
        {
            if (!UsingSpriteBatch)
            {
                Tools.QDrawer.Flush();
                Tools.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
                UsingSpriteBatch = true;
            }
        }

        /// <summary>
        /// Core wrapper for drawing text. Assumes SpriteBatch is started.
        /// </summary>
        public static void DrawText(Vector2 pos, Camera cam, string str, SpriteFont font)
        {
            Vector2 loc = ToScreenCoordinates(pos, cam);

            Tools.spriteBatch.DrawString(font, str, loc, Color.Azure, 0, Vector2.Zero, new Vector2(.5f, .5f), SpriteEffects.None, 0);
        }

        /// <summary>
        /// Sets the standard render states.
        /// </summary>
        public static void SetStandardRenderStates()
        {
            Tools.QDrawer.SetAddressMode(true, true);
            // All these renderstates need to be ported to XNA 4.0
            /*
            Tools.Device.RenderState.DepthBufferEnable = true;

            Tools.Device.RenderState.AlphaBlendEnable = true;
            Tools.Device.RenderState.CullMode = CullMode.None;
            Tools.Device.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            Tools.Device.RenderState.SourceBlend = Blend.SourceAlpha;

            Tools.Device.RenderState.AlphaSourceBlend = Blend.One;
            Tools.Device.RenderState.AlphaDestinationBlend = Blend.One;
             * */

            Tools.Device.RasterizerState = RasterizerState.CullNone;
            //Tools.Device.BlendState = BlendState.NonPremultiplied;
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

        /// <summary>
        /// Premultiply a color's alpha against its RGB components.
        /// </summary>
        /// <param name="color">The normal, non-premultiplied color.</param>
        public static Color PremultiplyAlpha(Color color)
        {
            return new Color(color.R, color.G, color.B) * (color.A / 255f);
        }
        /// <summary>
        /// Premultiply a color's alpha against its RGB components.
        /// </summary>
        /// <param name="color">The normal, non-premultiplied color.</param>
        /// <param name="BlendAddRatio">When 0 blending is normal, when 1 blending is additive.</param>
        public static Color PremultiplyAlpha(Color color, float BlendAddRatio)
        {
            Color NewColor = PremultiplyAlpha(color);
            NewColor.A = (byte)(NewColor.A * (1 - BlendAddRatio));

            return NewColor;
        }


        public static bool DebugConvenience;// = true;

        /// <summary>
        /// Set a player's controller to vibrate.
        /// </summary>
        /// <param name="Index">The index of the player.</param>
        /// <param name="LeftMotor">The intensity of the left motor vibration (from 0.0 to 1.0)</param>
        /// <param name="RightMotor">The intensity of the left motor vibration (from 0.0 to 1.0)</param>
        /// <param name="Duration">The number of frames the vibration should persist.</param>
        public static void SetVibration(PlayerIndex Index, float LeftMotor, float RightMotor, int Duration)
        {
            if (DebugConvenience) return;

            VibrateTimes[(int)Index] = Duration;
            XnaInput.GamePad.SetVibration(Index, LeftMotor, RightMotor);
        }

        public static void UpdateVibrations()
        {
            for (int i = 0; i < 4; i++)
            {
                if (VibrateTimes[i] > 0)
                {
                    VibrateTimes[i]--;
                    if (VibrateTimes[i] <= 0)
                        SetVibration((PlayerIndex)i, 0, 0, 0);
                }
            }
        }

        public static Vector2 Reciprocal(Vector2 v)
        {
            return new Vector2(v.Y, -v.X);
        }

        public static void PointyAxisTo(ref BasePoint Base, Vector2 dir)
        {
            PointxAxisTo(ref Base, Reciprocal(dir));
        }
        public static void PointxAxisTo(ref BasePoint Base, Vector2 dir)
        {
            PointxAxisTo(ref Base.e1, ref Base.e2, dir);
        }
        public static void PointxAxisTo(ref Vector2 e1, ref Vector2 e2, Vector2 dir)
        {
            if (dir.Length() < .0001f) return;

            dir.Normalize();

            float l = e1.Length();
            e1 = dir * l;

            l = e2.Length();
            e2.X = -e1.Y;
            e2.Y = e1.X;
            e2.Normalize();
            e2 *= l;
        }
        public static void PointxAxisToAngle(ref BasePoint Base, float angle)
        {
            PointxAxisTo(ref Base, AngleToDir(angle));
        }
        /// <summary>
        /// Encode a float as an RGBA color.
        /// </summary>
        public static Vector3 EncodeFloatRGBA(float v)
        {
            const float max24int = 256 * 256 * 256 - 1;
            Vector3 color = new Vector3((float)Math.Floor(v * max24int / (256 * 256)),
                                        (float)Math.Floor(v * max24int / 256),
                                        (float)Math.Floor(v * max24int));

            color.Z -= color.Y * 256f;
            color.Y -= color.X * 256f;

            return color / 255f;
        }

        /// <summary>
        /// Decode an RGBA color into a float (assuming the RGBA was an encoding of a float to start with)
        /// </summary>
        public static float DecodeFloatRGBA(Vector4 rgba)
        {
            return rgba.X * 1f + rgba.Y / 255f + rgba.Z / 65025f + rgba.W / 160581375f;
        }
    }
}