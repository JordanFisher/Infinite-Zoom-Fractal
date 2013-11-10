using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

using FractalGpu.Core;

namespace FractalGpu
{
    enum FractalTypes { Julia, Mandelbrot, GoldenMean, Newton };

    public class FractalGame : Game
    {
        int CorrectionOrder = 1;

        const int MaxIt = 1500;
        Complex[,] Corner = new Complex[5, MaxIt];

        double Far = (double)10e10f;

        Complex CamPos = new Complex((double)0, (double)0);
        double CamZoom = (double)(.001);

		EzEffect Fx;

		double AspectRatio;

		int[] indexData;
		VertexPositionColorTexture[] vertexData;

		const int TOP_LEFT = 0;
		const int TOP_RIGHT = 1;
		const int BOTTOM_RIGHT = 2;
		const int BOTTOM_LEFT = 3;

        public FractalGame()
        {        
            Tools.DeviceManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Tools.TheGame = this;
        }

        protected override void Initialize()
        {
            Tools.DeviceManager.PreferredBackBufferWidth = 1280;
			Tools.DeviceManager.PreferredBackBufferHeight = 720;
            //graphics.IsFullScreen = true;
			AspectRatio = Tools.DeviceManager.PreferredBackBufferWidth / Tools.DeviceManager.PreferredBackBufferHeight;

			Tools.DeviceManager.ApplyChanges();
            Window.Title = "Fractal Dactyl";

			SetupVertices(Color.White);
			SetupIndices();

            base.Initialize();
        }

		private void SetupVertices(Color color)
		{
			const float HALF_SIDE = 1.0f;
			const float Z = 0.0f;

			vertexData = new VertexPositionColorTexture[4];
			vertexData[TOP_LEFT] = new VertexPositionColorTexture(new Vector3(-HALF_SIDE, HALF_SIDE, Z), color, new Vector2(0, 0));
			vertexData[TOP_RIGHT] = new VertexPositionColorTexture(new Vector3(HALF_SIDE, HALF_SIDE, Z), color, new Vector2(1, 0));
			vertexData[BOTTOM_RIGHT] = new VertexPositionColorTexture(new Vector3(HALF_SIDE, -HALF_SIDE, Z), color, new Vector2(1, 1));
			vertexData[BOTTOM_LEFT] = new VertexPositionColorTexture(new Vector3(-HALF_SIDE, -HALF_SIDE, Z), color, new Vector2(0, 1));
		}

		private void SetupIndices()
		{
			indexData = new int[6];
			indexData[0] = TOP_LEFT;
			indexData[1] = BOTTOM_RIGHT;
			indexData[2] = BOTTOM_LEFT;

			indexData[3] = TOP_LEFT;
			indexData[4] = TOP_RIGHT;
			indexData[5] = BOTTOM_RIGHT;
		}

        protected override void LoadContent()
        {
			Tools.Device = Tools.DeviceManager.GraphicsDevice;

			Fx = new EzEffect(Content, "Shaders\\Standard");

            Tools.spriteBatch = new SpriteBatch(GraphicsDevice);

            Tools.padState = new GamePadState[4];
            Tools.PrevpadState = new GamePadState[4];            

			Tools.SetStandardRenderStates();
        }


        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }


        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        protected void HandleInput()
        {
            if (Tools.PrevKeyboardState == null) Tools.PrevKeyboardState = Tools.keybState;

            Tools.PrevKeyboardState = Tools.keybState;
            Tools.PrevMouseState = Tools.CurMouseState;
    
            for (int i = 0; i < 4; i++)
                if (Tools.PrevpadState[i] == null) Tools.PrevpadState[i] = Tools.padState[i];
     
            for (int i = 0; i < 4; i++)
                Tools.PrevpadState[i] = Tools.padState[i];

            if (!Tools.StepControl || (Tools.keybState.IsKeyDown(Keys.Enter) && !Tools.PrevKeyboardState.IsKeyDown(Keys.Enter)))
            {
                Vector2 dir = ButtonCheck.GetDir(-1);

                double scale = (double)(.03 * .001) / CamZoom;

                CamPos += scale * (Complex)dir;

                double ZoomRate = .95;

                if (ButtonCheck.State(ControllerButtons.B, -1).Down)
                {
                    CamZoom *= ZoomRate;
                }

                if (ButtonCheck.State(ControllerButtons.A, -1).Down)
                {
                    CamZoom /= ZoomRate;
                }
            }
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
            if (!this.IsActive)
            {
                this.IsMouseVisible = true;
                return;
            }
            else
                this.IsMouseVisible = false;

            Tools.keybState = Keyboard.GetState();
            Tools.CurMouseState = Mouse.GetState();
            Tools.padState[0] = GamePad.GetState(PlayerIndex.One);
            Tools.padState[1] = GamePad.GetState(PlayerIndex.Two);
            Tools.padState[2] = GamePad.GetState(PlayerIndex.Three);
            Tools.padState[3] = GamePad.GetState(PlayerIndex.Four);

            Tools.gameTime = gameTime;

            float new_t = (float)gameTime.TotalGameTime.TotalSeconds;
            Tools.dt = new_t - Tools.t;
            Tools.t = new_t;

            HandleInput();

			// Set fractal parameters
            FractalTypes FractalType = FractalTypes.GoldenMean;
            //FractalTypes FractalType = FractalTypes.Julia;

            Complex C = (Complex)0;
            
			if (FractalType == FractalTypes.Julia)
                C = new Complex(.4, -.35); // Favorite Julia
                //C = new Complex(.4 + Math.Sin(Step), -.35 + Math.Cos(1.11f * Step)); // Favorite Julia
			if (FractalType == FractalTypes.GoldenMean)
				//C = FractalFunc.GoldenMean_rho = new Complex(-0.74405117795419151, -0.66812262690690249);
				C = FractalFunc.GoldenMean_rho = new Complex(-0.75905117795419151, -0.65812262690690249);
				//C = FractalFunc.GoldenMean_rho = new Complex(-0.74405117795419151 + 0.01 * Math.Cos(Tools.t / 10), -0.66812262690690249 + 0.01 * Math.Sin(Tools.t / 13));
			Fx.c.SetValue(C.ToVector2());

            // Calculate high precision orbit of four corners
            double zoom = .001 / CamZoom;
            double a = AspectRatio;
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
				MinPoints[i] = .6f * Tools.AngleToDir(2 * Math.PI / NumMinPoints * i + Tools.t) * (float)Math.Sin(2 * Tools.t);
				MinPoints[i].X += .9f * (float)Math.Cos(1.5f * Tools.t);
                MinDist[i] = 10000;
            }

			

			// CPU trace
			//Complex z = CamPos + new Complex(.01, .01);
			//for (int i = 0; i < 730; i++)
			//{
			//    z = FractalFunc.GoldenMean(z);
			//}
			//Console.WriteLine(z);



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
            }

            Console.WriteLine("Depth: {0}", count);

            Fx.MinDist.SetValue(MinDist);
            Fx.MinPoints.SetValue(MinPoints);
            Fx.NumMinPoints.SetValue(NumMinPoints);

            Fx.Rotate.SetValue(h.ToVector2());
            Fx.h2.SetValue(h2.ToVector2());
            Fx.h3.SetValue(h3.ToVector2());
            Fx.h4.SetValue(h4.ToVector2());
            Fx.Center.SetValue(Center.ToVector2());
            Fx.Count.SetValue(count);
            Fx.D.SetValue(D);

            Fx.CamPos.SetValue(CamPos.ToVector2());

			Tools.Device.SetRenderTarget(null);
			Tools.SetStandardRenderStates();
			GraphicsDevice.Clear(Color.Black);

			Fx.Set(Vector2.Zero, 1, (float)AspectRatio);
			vertexData[TOP_RIGHT].TextureCoordinate		= new Vector2( (float)size.X*(float)AspectRatio,  (float)size.Y);
			vertexData[BOTTOM_LEFT].TextureCoordinate	= new Vector2(-(float)size.X*(float)AspectRatio, -(float)size.Y);
			vertexData[TOP_LEFT].TextureCoordinate		= new Vector2(-(float)size.X*(float)AspectRatio,  (float)size.Y);
			vertexData[BOTTOM_RIGHT].TextureCoordinate	= new Vector2( (float)size.X*(float)AspectRatio, -(float)size.Y);

			GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexData, 0, 4, indexData, 0, 2);

            base.Draw(gameTime);
        }
    }
}
