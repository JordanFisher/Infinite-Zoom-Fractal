//#undef PC_DEBUG
using System.Reflection;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
#if PC_VERSION
#else
using Microsoft.Xna.Framework.GamerServices;
#endif
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

using Drawing;

using FractalGpu;
namespace FractalGpu
{
    public struct PhsxData
    {
        public Vector2 Position, Velocity, Acceleration;

        public void UpdatePosition() { Position += Velocity; }

        public void Integrate()
        {
            Velocity += Acceleration;
            Position += Velocity;
        }
    }

    public class LockableBool
    {
        bool _val = false;
        public bool val
        {
            get
            {
                return _val;
            }
            set
            {
                _val = value;
            }
        }
    }



    enum FractalTypes { Julia, Mandelbrot, GoldenMean, Newton };

    public struct IntVector2
    {
        public int X, Y;
        public IntVector2(int X, int Y) { this.X = X; this.Y = Y; }
        public IntVector2(float X, float Y) { this.X = (int)X; this.Y = (int)Y; }

        public static IntVector2 operator +(IntVector2 A, IntVector2 B)
        {
            return new IntVector2(A.X + B.X, A.Y + B.Y);
        }
        public static IntVector2 operator *(IntVector2 A, IntVector2 B)
        {
            return new IntVector2(A.X * B.X, A.Y * B.Y);
        }
        public static IntVector2 operator *(IntVector2 A, Vector2 B)
        {
            return new IntVector2(A.X * B.X, A.Y * B.Y);
        }
    }

    public struct ResolutionGroup
    {
        public IntVector2 Backbuffer, Bob;

        public Vector2 TextOrigin;
        public float LineHeightMod;

        public void CopyTo(ref ResolutionGroup dest, Vector2 scale)
        {
            dest.Backbuffer = Backbuffer * scale;
            dest.Bob = Bob * scale;

            dest.TextOrigin = TextOrigin - scale;
            dest.LineHeightMod = LineHeightMod * scale.Y;
        }
    }

    public class Game1 : Microsoft.Xna.Framework.Game
    {
        int CorrectionOrder = 1;

        const int MaxIt = 500;
        Complex[,] Corner = new Complex[5, MaxIt];

        double Far = (double)10e10f;

        Complex CamPos = new Complex((double)0, (double)0);
        double CamZoom = (double)(.001);
        bool Down = false;

        double SaveZoom; Complex SavePos;

        public ResolutionGroup Resolution;
        public ResolutionGroup[] Resolutions = new ResolutionGroup[3];

        bool ShowFPS = true;
        float fps;
        int Count = 0;

        public int DrawCount, PhsxCount;

#if PC_VERSION
        QuadClass MousePointer;
#endif

        QuadClass FullScreen;
        RenderTarget2D FullScreenRenderTarget;
        EzTexture FullScreenTexture;

        QuadDrawer QDrawer;
        EzEffectWad EffectWad;
        EzTextureWad TextureWad;
 
        VertexDeclaration vertexDeclaration;

        GraphicsDeviceManager graphics;

        RenderTarget2D ScreenShotRenderTarget;

        GraphicsDevice device;
        int screenWidth;
        int screenHeight;

        public Camera MainCamera;

        
        public Game1()
        {        
#if PC_VERSION
#else
            Components.Add(new GamerServicesComponent(this));
#endif

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Tools.TheGame = this;
        }

        protected override void Initialize()
        {
            Tools.LoadEffects(Content, true);
            EffectWad = Tools.EffectWad;

            // Set the possible resolutions
            Resolutions[0] = new ResolutionGroup();
            Resolutions[0].Backbuffer = new IntVector2(1280, 720);
            Resolutions[0].Bob = new IntVector2(135, 0);
            //Resolutions[0].Bob = new IntVector2(270, 0);
            Resolutions[0].TextOrigin = Vector2.Zero;
            Resolutions[0].LineHeightMod = 1f;

            Resolutions[0].CopyTo(ref Resolutions[1], new Vector2(.5f, .5f));
            Resolutions[1].TextOrigin = new Vector2(-.5f, 0f);

            Resolutions[2] = new ResolutionGroup();
            Resolutions[2].Backbuffer = new IntVector2(640, 480);
            Resolutions[2].Bob = new IntVector2(135, 0);
            Resolutions[2].TextOrigin = new Vector2(-.5f, -.5f);
            Resolutions[2].LineHeightMod = .5f;

            // Set the default resolution
            Resolution = Resolutions[0];

            graphics.PreferredBackBufferWidth = Resolution.Backbuffer.X;
            graphics.PreferredBackBufferHeight = Resolution.Backbuffer.Y;

            //graphics.IsFullScreen = true;
            graphics.ApplyChanges();
            Window.Title = "Windowed SMB";

            fps = 0;

            base.Initialize();
        }


        protected override void LoadContent()
        {
            device = graphics.GraphicsDevice;

            Tools.LoadBasicArt(Content);
            //Tools.LoadArt(Content);
            TextureWad = Tools.TextureWad;

            QDrawer = new QuadDrawer(device, 1000);
            QDrawer.DefaultEffect = EffectWad.FindByName("NoTexture");
            QDrawer.DefaultTexture = TextureWad.FindByName("White");

            Tools.QDrawer = QDrawer;
            //Tools.Rnd = new Random(425913504);
            Tools.Rnd = new Random();
            int seed = Tools.Rnd.Next();
            //seed = 1380140069;
            Console.WriteLine("Seed: {0}", seed);
            Tools.Rnd = new Random(seed);
            Tools.EffectWad = EffectWad;
            Tools.TextureWad = TextureWad;
            Tools.Device = device;
            Tools.t = 0;

            Tools.spriteBatch = new SpriteBatch(GraphicsDevice);

            screenWidth = device.PresentationParameters.BackBufferWidth;
            screenHeight = device.PresentationParameters.BackBufferHeight;

            PresentationParameters pp = Tools.Device.PresentationParameters;
            ScreenShotRenderTarget = new RenderTarget2D(Tools.Device,
            pp.BackBufferWidth, pp.BackBufferHeight, false,
                                   pp.BackBufferFormat, pp.DepthStencilFormat, pp.MultiSampleCount,
                                   RenderTargetUsage.DiscardContents);

            

            MainCamera = new Camera(screenWidth, screenHeight);
            MainCamera.Update();

                        //System.Diagnostics.Trace.WriteLine("Start");

                        
                        Tools.LilFont= new EzFont("Fonts/LilFont");
                        Tools.Font_Dylan15 = new EzFont("Fonts/LilFontBold");
                        Tools.Font_Dylan20 = new EzFont("Fonts/Dylan20", "Fonts/DylanThinOutline20", -22, 40);
                        Tools.Font_Dylan24 = new EzFont("Fonts/Dylan24", "Fonts/DylanThinOutline24", -22, 40);
                        Tools.Font_Dylan28 = new EzFont("Fonts/Dylan28", "Fonts/DylanThinOutline28", -22, 40);
                        Tools.Font_Dylan42 = new EzFont("Fonts/Dylan42", "Fonts/DylanOutline42", -63, 40);
                        Tools.Font_Dylan60 = new EzFont("Fonts/Dylan60", "Fonts/DylanOutline60", -75, 40);

                        Tools.Font_DylanThick20 = new EzFont("Fonts/Dylan20", "Fonts/DylanOutline20", -22, 45);
                        Tools.Font_DylanThick24 = new EzFont("Fonts/Dylan24", "Fonts/DylanOutline24", -22, 40);
                        Tools.Font_DylanThick28 = new EzFont("Fonts/Dylan28", "Fonts/DylanOutline28", -22, 40);

                        //System.Diagnostics.Trace.WriteLine("Fonts done...");


            //ParticleEffects.Init();

#if PC_VERSION
            // Mouse pointer
            MousePointer = new QuadClass();
            MousePointer.Init();
            MousePointer.Quad.MyTexture = Tools.TextureWad.FindByName("Hand_Open");
            MousePointer.ScaleToTextureSize();
            MousePointer.Scale(2);
#endif


            
//#if XBOX
            Tools.padState = new GamePadState[4];
            Tools.PrevpadState = new GamePadState[4];            
//#endif

            
            Tools.SetStandardRenderStates();


            FullScreen = new QuadClass();
            FullScreen.Init();
            //FullScreen.EffectName = "Fractal";
            FullScreen.EffectName = "Standard";

            FullScreenRenderTarget =
                new RenderTarget2D(Tools.Device,
            pp.BackBufferWidth, pp.BackBufferHeight, false,
                                   pp.BackBufferFormat, pp.DepthStencilFormat, pp.MultiSampleCount,
                                   RenderTargetUsage.DiscardContents);

            //    new RenderTarget2D(Tools.Device,
            //pp.BackBufferWidth, pp.BackBufferHeight, 1,
            //pp.BackBufferFormat, pp.MultiSampleType, pp.MultiSampleQuality);

            FullScreenTexture = new EzTexture();
        }


        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }


        protected override void Update(GameTime gameTime)
        {
/*
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            */

            base.Update(gameTime);
        }

        public void BeginVideoCapture()
        {
            if (Tools.ScreenshotMode == false)
            {
                ChangeScreenshotMode();
                Tools.CapturingVideo = true;
            }
        }

        public void EndVideoCapture()
        {
            ChangeScreenshotMode();
            Tools.CapturingVideo = false;
        }

        void ChangeScreenshotMode()
        {
            Tools.ScreenshotMode = !Tools.ScreenshotMode;
            if (Tools.ScreenshotMode) Tools.DestinationRenderTarget = ScreenShotRenderTarget;
            else Tools.DestinationRenderTarget = null;
        }

        protected void PhsxStep()
        {
#if WINDOWS
            if (Tools.PrevKeyboardState == null) Tools.PrevKeyboardState = Tools.keybState;


#if PC_DEBUG

            if (Tools.keybState.IsKeyDown(Keys.F) && !Tools.PrevKeyboardState.IsKeyDown(Keys.F))
                ShowFPS = !ShowFPS;

#endif


#if PC_DEBUG
            if (Tools.keybState.IsKeyDown(Keys.OemComma))//&& !Tools.PrevKeyboardState.IsKeyDown(Keys.OemComma))
                //Tools.TheGame.MainCamera.Zoom *= .95f;// .99f;
                CamZoom *= .95;
            if (Tools.keybState.IsKeyDown(Keys.OemPeriod))//&& !Tools.PrevKeyboardState.IsKeyDown(Keys.OemPeriod))
                //Tools.TheGame.MainCamera.Zoom /= .95f;// .99f;
                CamZoom /= .95;

            if (Tools.keybState.IsKeyDown(Keys.U) && !Tools.PrevKeyboardState.IsKeyDown(Keys.U))
            {
                BeginVideoCapture();
            }

            if (Tools.keybState.IsKeyDown(Keys.I) && !Tools.PrevKeyboardState.IsKeyDown(Keys.I))
            {
                ChangeScreenshotMode();
            }

            if (Tools.FreeCam)
            {
                Vector2 pos = MainCamera.Data.Position;
                float scale = .001f * .3f * .001f / (float)CamZoom;// MainCamera.Zoom.X;
                if (Tools.keybState.IsKeyDown(Keys.Right)) pos.X += 130 * scale;
                if (Tools.keybState.IsKeyDown(Keys.Left)) pos.X -= 130 * scale;
                if (Tools.keybState.IsKeyDown(Keys.Up)) pos.Y += 130 * scale;
                if (Tools.keybState.IsKeyDown(Keys.Down)) pos.Y -= 130 * scale;
                MainCamera.Data.Position = MainCamera.Target = pos;
                MainCamera.Update();
            }

            if (Tools.keybState.IsKeyDown(Keys.C) && !Tools.PrevKeyboardState.IsKeyDown(Keys.C))
            {
                CorrectionOrder++;
                if (CorrectionOrder > 4)
                    CorrectionOrder = 1;
            }

            if (Tools.keybState.IsKeyDown(Keys.P) && !Tools.PrevKeyboardState.IsKeyDown(Keys.P))
                Tools.FreeCam = !Tools.FreeCam;
            if (Tools.keybState.IsKeyDown(Keys.Q) && !Tools.PrevKeyboardState.IsKeyDown(Keys.Q))
                Tools.DrawGraphics = !Tools.DrawGraphics;
            if (Tools.keybState.IsKeyDown(Keys.W) && !Tools.PrevKeyboardState.IsKeyDown(Keys.W))
                Tools.DrawBoxes = !Tools.DrawBoxes;
            if (Tools.keybState.IsKeyDown(Keys.E) && !Tools.PrevKeyboardState.IsKeyDown(Keys.E))
                Tools.StepControl = !Tools.StepControl;
            if (Tools.keybState.IsKeyDown(Keys.R) && !Tools.PrevKeyboardState.IsKeyDown(Keys.R))
            {
                Tools.IncrPhsxSpeed();
            }
#endif


            Tools.DeltaMouse = Tools.ToWorldCoordinates(new Vector2(Tools.CurMouseState.X, Tools.CurMouseState.Y), Tools.TheGame.MainCamera) -
                             Tools.ToWorldCoordinates(new Vector2(Tools.PrevMouseState.X, Tools.PrevMouseState.Y), Tools.TheGame.MainCamera);
            //Tools.DeltaMouse = Tools.ToWorldCoordinates(new Vector2(Tools.CurMouseState.X, Tools.CurMouseState.Y), Tools.TheGame.MainCamera) -
            //                   Tools.ToWorldCoordinates(new Vector2(0, 0), Tools.TheGame.MainCamera);

            Tools.PrevKeyboardState = Tools.keybState;
            Tools.PrevMouseState = Tools.CurMouseState;
            //Mouse.SetPosition(0, 0);

#endif
//#elif XBOX
    
            for (int i = 0; i < 4; i++)
                if (Tools.PrevpadState[i] == null) Tools.PrevpadState[i] = Tools.padState[i];

     
            for (int i = 0; i < 4; i++)
                Tools.PrevpadState[i] = Tools.padState[i];

#if WINDOWS
            if (!Tools.StepControl || (Tools.keybState.IsKeyDown(Keys.Enter) && !Tools.PrevKeyboardState.IsKeyDown(Keys.Enter)))
#else
            if (!Tools.StepControl)
#endif
            {
                Vector2 dir = ButtonCheck.GetDir(-1);
                if (dir.Length() < .1f)
                {
                    dir *= 0;
                    ZeroCount++;
                }
                else
                {
                    ZeroCount = 0;
                    SaveZoom = CamZoom;
                    SavePos = CamPos;
                    Down = false;
                }
                if (ZeroCount > 180)
                {
                    if (Down)
                    {
                        if (CamZoom < SaveZoom)
                            CamZoom /= .9635f;
                        else
                        {
                            Down = false;
                            Step += .01f;
                        }
                    }
                    else
                    {
                        if (CamZoom > .001f)
                            CamZoom *= .9635f;
                        else
                        {
                            Down = true;
                            Step += .01f;
                        }
                    }
                }

                Vector2 pos = MainCamera.Data.Position;
                double scale = (double)(.03 * .001) / CamZoom;// MainCamera.Zoom.X;
                //MainCamera.Data.Position = MainCamera.Target = pos + 130 * scale * dir;
                CamPos += scale * (Complex)dir;
                MainCamera.Update();

                double ZoomRate = .95;

                if (ButtonCheck.State(ControllerButtons.B, -1).Down)
                {
                    CamZoom *= ZoomRate;

                    ZeroCount = 0;
                    SaveZoom = CamZoom;
                    SavePos = CamPos;
                    Down = false;
                }

                if (ButtonCheck.State(ControllerButtons.A, -1).Down)
                {
                    CamZoom /= ZoomRate;

                    ZeroCount = 0;
                    SaveZoom = CamZoom;
                    SavePos = CamPos;
                    Down = false;
                }

                Tools.PhsxCount++;
            }
        }


        float Step = 0;
        int ZeroCount = 0;
        void DrawGC()
        {

            Tools.StartSpriteBatch();
           Tools.spriteBatch.DrawString(Tools.Font_Dylan24.Font,
              string.Format("Pos: {0}, Zoom: {1}", CamPos.ToVector2(), (float)CamZoom),
              //string.Format("fps: {0}, correction: {1}", fps, CorrectionOrder),
                  new Vector2(300, 100),
                  Color.Orange);

#if WINDOWS
           Tools.spriteBatch.DrawString(Tools.Font_Dylan24.Font, GC.CollectionCount(0).ToString() + ", " + GC.CollectionCount(1).ToString() + ", " + fps.ToString(), new Vector2(300, 300), Color.Orange);
            //Tools.spriteBatch.DrawString(Font1, Tools.CurLevel.GetPhsxStep().ToString(), new Vector2(300, 300), Color.Azure);
            //Tools.spriteBatch.DrawString(Font1, CurLevel.Bobs[0].blob.Position.ToString(), new Vector2(300, 300), Color.Azure);
#endif

                       Tools.EndSpriteBatch();
        }

#if PC_VERSION
        /// <summary>
        /// Whether the mouse should be allowed to be shown, usually only when a menu is active.
        /// </summary>
        public bool ShowMouse = false;
        
        /// <summary>
        /// Whether the user is using the mouse. False when the mouse hasn't been used since the arrow keys.
        /// </summary>
        public bool MouseInUse = false;

        /// <summary>
        /// Draw the mouse cursor.
        /// </summary>
        void MouseDraw()
        {
            if (!MouseInUse) return;
            if (MousePointer == null) return;

            MousePointer.Pos = Tools.MouseWorldPos() + new Vector2(.88f * MousePointer.Size.X, -.62f * MousePointer.Size.Y);
            MousePointer.Draw();
            Tools.QDrawer.Flush();
        }

        /// <summary>
        /// Update the boolean flag MouseInUse
        /// </summary>
        void UpdateMouseUse()
        {
            if (Tools.keybState.IsKeyDown(Keys.Up) ||
                Tools.keybState.IsKeyDown(Keys.Down) ||
                Tools.keybState.IsKeyDown(Keys.Left) ||
                Tools.keybState.IsKeyDown(Keys.Right))
                MouseInUse = false;

            if (Tools.DeltaMouse != Vector2.Zero ||
                Tools.CurMouseState.LeftButton == ButtonState.Pressed ||
                Tools.CurMouseState.RightButton == ButtonState.Pressed)
                MouseInUse = true;
        }
#endif

        protected override void Draw(GameTime gameTime)
        {
            //double x = (double)5;
            //Console.WriteLine("{0}", x);
            //Console.WriteLine("{0}", (float)x);
            //Console.WriteLine("{0}", (float)(x/2));
            //Console.WriteLine("{0}", (float)(1 / x));
            //Console.WriteLine("{0}", (float)((1/x)*(1/x)));
            //Console.WriteLine("{0}", x);



// fix the grid size h, then any number is just h * BigInteger. x * y is just bigint * bigint and then move over 1/h decimal spots
            //BigInteger x = 10000000;
            //Console.WriteLine("{0}", x);
            //Console.WriteLine("{0}", x*x);
            //Console.WriteLine("{0}", x*x*);

            //double X = new double(5, Complex.spec);
            //double Y = new double(3, Complex.spec);
            //Console.WriteLine("{0}", X);
            //Console.WriteLine("{0}", 2*X);

            //Complex A = new Complex(.000000001, 0);
            //Complex B = new Complex(0, .000000001);
            //Console.WriteLine("{0}", A * A);
            //Console.WriteLine("{0}", A * B);
            //Console.WriteLine("{0}", B * B);
            //Console.WriteLine("{0}", A * A);




#if WINDOWS
            if (!this.IsActive)
            {
                this.IsMouseVisible = true;
                return;
            }
            else
                this.IsMouseVisible = false;
#endif

#if PC_VERSION
            UpdateMouseUse();
#endif

            Tools.DrawCount++;

            bool DrawBool = true;
            
            {
                {
#if WINDOWS
                    Tools.keybState = Keyboard.GetState();
                    Tools.CurMouseState = Mouse.GetState();
#endif
                    Tools.padState[0] = GamePad.GetState(PlayerIndex.One);
                    Tools.padState[1] = GamePad.GetState(PlayerIndex.Two);
                    Tools.padState[2] = GamePad.GetState(PlayerIndex.Three);
                    Tools.padState[3] = GamePad.GetState(PlayerIndex.Four);

                    Tools.UpdateVibrations();
                }

                Tools.gameTime = gameTime;
                DrawCount++;

                float new_t = (float)gameTime.TotalGameTime.TotalSeconds;
                Tools.dt = new_t - Tools.t;
                Tools.t = new_t;
                fps = .3f * fps + .7f * (1000f / (float)Math.Max(.00000231f, gameTime.ElapsedGameTime.Milliseconds));

                int Reps = 0;
                if (Tools.PhsxSpeed == 0 && DrawCount % 2 == 0) Reps = 1;
                else if (Tools.PhsxSpeed == 1) Reps = 1;
                else if (Tools.PhsxSpeed == 2) Reps = 2;
                else if (Tools.PhsxSpeed == 3) Reps = 4;


                for (int i = 0; i < Reps; i++)
                {
                    PhsxCount++;
                    PhsxStep();
                }
            }

            device.SetRenderTarget(Tools.DestinationRenderTarget);

            Vector4 cameraPos = new Vector4(MainCamera.Data.Position.X, MainCamera.Data.Position.Y, MainCamera.Zoom.X, MainCamera.Zoom.Y);//.001f, .001f);

            Tools.SetStandardRenderStates();

            //device.RenderState.AlphaBlendEnable = true;
            //device.RenderState.CullMode = CullMode.None;

            //device.RenderState.DestinationBlend = Blend.One;
            //device.RenderState.SourceBlend = Blend.SourceAlpha;

            //QDrawer.SetAddressUMode(TextureAddressMode.Clamp);
            //QDrawer.SetAddressVMode(TextureAddressMode.Clamp);

        
            GraphicsDevice.Clear(Color.Black);

            //device.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            //device.RenderState.SourceBlend = Blend.SourceAlpha;





            //device.VertexDeclaration = Tools.vertexDeclaration;
            //device.RenderState.AlphaBlendEnable = true;
            //device.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
            //device.RenderState.SourceBlend = Blend.SourceAlpha;
            //device.RenderState.CullMode = CullMode.None;
            foreach (EzEffect fx in EffectWad.EffectList) fx.effect.Parameters["xCameraPos"].SetValue(cameraPos);
            foreach (EzEffect fx in EffectWad.EffectList) fx.effect.Parameters["xCameraAspect"].SetValue(MainCamera.AspectRatio);
            foreach (EzEffect fx in EffectWad.EffectList) fx.effect.CurrentTechnique = fx.effect.Techniques["Simplest"];
            
            //foreach (EzEffect fx in EffectWad.EffectList) fx.effect.Parameters["t"].SetValue(Tools.t);

            float t = (Tools.PhsxCount % 100000)/ 200f;
            foreach (EzEffect fx in EffectWad.EffectList) fx.effect.Parameters["t"].SetValue(t);
            /*
            if (DrawCount < 5)
            {
                FullScreen.FullScreen(MainCamera);
                FullScreen.Scale(4);
            }*/

/*
            // Calculate high precision orbit, starting at camera center
            Complex C = new Complex(.5, .5);

            Complex []Z = new Complex[100];
            Vector2[] z = new Vector2[100];
            Z[0] = MainCamera.Data.Position;

            int count = 1;
            for (count = 1; count < 100; count++)
            {
                Z[count] = FractalFunc.Fractal(Z[count - 1], C);

                if (Z[count].Length() > 10e10)
                    break;
            }

            for (int i = 0; i < count; i++)
                z[i] = Z[i].ToVector2();




            Effect Fx = EffectWad.FindByName("Standard").effect;
            Fx.Parameters["z"].SetValue(MainCamera.Data.Position);
            Fx.Parameters["c"].SetValue(C.ToVector2());
            Fx.Parameters["Z"].SetValue(z);

            float zoom = .001f / MainCamera.Zoom.X;
            float a = MainCamera.AspectRatio;

            FullScreen.FullScreen(MainCamera);

            FullScreen.Quad.UVFromBounds(new Vector2(-zoom / a, -zoom), new Vector2(zoom / a, zoom));
*/


            // Set fractal parameters
            FractalTypes FractalType = FractalTypes.GoldenMean;//.Julia;
            //FractalTypes FractalType = FractalTypes.Julia;

            Complex C = (Complex)0;
            if (FractalType == FractalTypes.Julia)
                C = new Complex(.4, -.35); // Favorite Julia
                //C = new Complex(.4 + Math.Sin(Step), -.35 + Math.Cos(1.11f * Step)); // Favorite Julia


            Effect Fx = EffectWad.FindByName("Standard").effect;
            //Effect Fx = EffectWad.FindByName("Fractal").effect;

            Fx.Parameters["c"].SetValue(C.ToVector2());

            // Calculate high precision orbit of four corners
            double zoom = .001 / CamZoom;
            float a = MainCamera.AspectRatio;
            Complex size = new Complex(zoom * a, zoom);



            Corner[0, 0] = CamPos + size;
            Corner[1, 0] = CamPos - size;
            Corner[2, 0] = new Complex(CamPos.X + size.X, CamPos.Y - size.Y);
            Corner[3, 0] = new Complex(CamPos.X - size.X, CamPos.Y + size.Y);
            Corner[4, 0] = CamPos;

            Complex h = (Complex)1;
            Complex h2 = (Complex)0;
            Complex h3 = (Complex)0;
            Complex h4 = (Complex)0;
            Complex Center = CamPos;

            if (FractalType == FractalTypes.Julia ||
                FractalType == FractalTypes.Newton)
            {
                h = (Complex)1;
                Center = CamPos;
            }
            
            if (FractalType == FractalTypes.Mandelbrot)
            {
                h = (Complex)0;
                Center = C;
            }

            float D = 0;
            int NumMinPoints = 3;
            Vector2[] MinPoints = new Vector2[NumMinPoints];
            float[] MinDist = new float[NumMinPoints];
            for (int i = 0; i < NumMinPoints; i++)
            {
                //MinPoints[i] = .1f * Tools.AngleToDir(2 * Math.PI / NumMinPoints * i);

                MinPoints[i] = .6f*Tools.AngleToDir(2 * Math.PI / NumMinPoints * i + t)*(float)Math.Sin(2*t);
                MinPoints[i].X += .9f*(float)Math.Cos(1.5f * t);
                MinDist[i] = 10000;
            }
            //MinPoints[0] = new Vector2(0, 0);
            //MinPoints[1] = new Vector2(.3f, 0);
            //MinPoints[2] = new Vector2(0, .1f);

            //Console.WriteLine("{0}, {1}", Corner[4, 0], Corner[0, 0]);

            int count = 1;
            for (count = 1; count < MaxIt; count++)
            {
                for (int j = 0; j < 5; j++)
                {
                    if (j != 0 && j != 4)
                        continue;

                    if (FractalType == FractalTypes.Newton)
                        Corner[j, count] = FractalFunc.Newton(Corner[j, count - 1]);
                    if (FractalType == FractalTypes.GoldenMean)
                        Corner[j, count] = FractalFunc.GoldenMean(Corner[j, count - 1]);
                    if (FractalType == FractalTypes.Julia)
                        Corner[j, count] = FractalFunc.Fractal(Corner[j, count - 1], C);
                    if (FractalType == FractalTypes.Mandelbrot)
                    {
                        if (count == 1) Corner[j, count] = FractalFunc.Fractal(C, Corner[j, 0]);
                        else Corner[j, count] = FractalFunc.Fractal(Corner[j, count - 1], Corner[j, 0]);
                    }
                }

                if (Corner[4, count].LengthSquared() > Far)
                    break;
// !!!!!!!!
//stop only when approximation breaks down

                //float cutoff = .05f; // Appears to work with h2 correction
                //float cutoff = .001f; // No higher order correction needed
                float cutoff = .01f; // Good when h2 correction is used
                if ((Corner[0, count] - Corner[4, count]).Length() > cutoff) break;
                //if ((Corner[1, count] - Corner[4, count]).Length() > cutoff) break;
                //if ((Corner[2, count] - Corner[4, count]).Length() > cutoff) break;
                //if ((Corner[3, count] - Corner[4, count]).Length() > cutoff) break;

                if (FractalType == FractalTypes.GoldenMean)
                {
                    if (CorrectionOrder >= 4) h4 = (2f * Center + FractalFunc.GoldenMean_rho) * h4 + h2 * h2;
                    if (CorrectionOrder >= 3) h3 = (2f * Center + FractalFunc.GoldenMean_rho) * h3 + 2f * h * h2;
                    if (CorrectionOrder >= 2) h2 = (2f * Center + FractalFunc.GoldenMean_rho) * h2 + h * h;
                    if (CorrectionOrder >= 1) h = (2f * Center + FractalFunc.GoldenMean_rho) * h;
                }
                else
                {
                    if (CorrectionOrder >= 4) h4 = 2f * Center * h4 + h2 * h2;
                    if (CorrectionOrder >= 3) h3 = 2f * Center * h3 + 2f * h * h2;
                    if (CorrectionOrder >= 2) h2 = 2f * Center * h2 + h * h;
                    if (CorrectionOrder >= 1)
                    {
                        h *= 2f * Center;
                        if (FractalType == FractalTypes.Mandelbrot)
                            h += (Complex)1;
                    }
                }
                Center = Corner[4, count];

                float d = (float)Center.LengthSquared();
                //D += .75f * (float)Math.Log(2*Math.Log(2*Math.Log(d+2)+2)+2);
                //D += .75f * (float)Math.Log(.8f*Math.Log(d+1) * (1 + .75 * Math.Sin(t / 15)) + 1);
                //D += (float)Math.Log(Math.Log(d + 1) + 1);

                //for (int i = 0; i < NumMinPoints; i++)
                    //MinDist[i] = Math.Min(MinDist[i], (Center - (Complex)MinPoints[i]).LengthSquared());
            }

            Console.WriteLine("Depth: {0}", count);
            //Console.WriteLine("h: {0}", h.ToVector2());

            Fx.Parameters["MinDist"].SetValue(MinDist);
            Fx.Parameters["MinPoints"].SetValue(MinPoints);
            Fx.Parameters["NumMinPoints"].SetValue(NumMinPoints);

            Fx.Parameters["Rotate"].SetValue(h.ToVector2());
            Fx.Parameters["h2"].SetValue(h2.ToVector2());
            Fx.Parameters["h3"].SetValue(h3.ToVector2());
            Fx.Parameters["h4"].SetValue(h4.ToVector2());
            Fx.Parameters["Center"].SetValue(Center.ToVector2());
            Fx.Parameters["Count"].SetValue(count);
            Fx.Parameters["D"].SetValue(D);

            Fx.Parameters["CamPos"].SetValue(CamPos.ToVector2());

            FullScreen.FullScreen(MainCamera);

            FullScreen.Quad.UVFromBounds(-size.ToVector2(), size.ToVector2());







            
            
            FullScreen.Draw();
            Tools.QDrawer.Flush();





            //if (ShowFPS || Tools.DebugConvenience)
            //    DrawGC();




#if PC_VERSION
            if (!Tools.ShowLoadingScreen && ShowMouse)
                MouseDraw();
            ShowMouse = false;
#endif

            base.Draw(gameTime);


            // Save screenshot
#if WINDOWS
            if (Tools.ScreenshotMode)
            {
                Tools.Device.SetRenderTarget(null);

                Tools.Screenshot = Tools.DestinationRenderTarget;


                Tools.Screenshots++;

                Stream stream = File.OpenWrite("TestSave_" + Tools.Screenshots.ToString() + ".jpg");
                Tools.Screenshot.SaveAsPng(stream, Tools.Screenshot.Width, Tools.Screenshot.Height);
                stream.Close();

                if (!Tools.CapturingVideo)
                    ChangeScreenshotMode();
            }
#endif
        }
    }
}
