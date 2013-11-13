
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using FractalGpu.Core;

namespace FractalGpu
{
    public class FractalGame : Game
    {
        const int MaxIt = 1500;

        Complex CamPos = new Complex(0, 0);
        double CamZoom = .001;

		Fractal CurFractal;
		Texture2D ReferenceFractal;

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

		Texture2D CreateTexture(Complex CamPos, double CamZoom, int Width, int Height)
		{
			Color[] clr = new Color[Width * Height];

            double zoom = .001 / CamZoom;
            double a = AspectRatio;
            Complex size = new Complex(zoom * a, zoom);

            Complex TL	 = new Complex(CamPos.X - size.X, CamPos.Y + size.Y);
            Complex Span = new Complex(2 * size.X, -2 * size.Y);

			for (int i = 0; i < Width; i++)
			{
				for (int j = 0; j < Height; j++)
				{
					var z = TL + new Complex(i * Span.X / Width, j * Span.Y / Height);

					for (int n = 0; n < 150 && z.LengthSquared() < 100; n++)
						z = CurFractal.Iterate(z);

					clr[i + j * Width] = z.Length() > 10 ? Color.Black : Color.White;
				}
			}

			Texture2D Texture = new Texture2D(GraphicsDevice, Width, Height);
			Texture.SetData(clr);

			return Texture;
		}

		protected override void Initialize()
        {
			int Width = 1280, Height = 720;
            Tools.DeviceManager.PreferredBackBufferWidth = Width;
			Tools.DeviceManager.PreferredBackBufferHeight = Height;
            //graphics.IsFullScreen = true;
			AspectRatio = Tools.DeviceManager.PreferredBackBufferWidth / Tools.DeviceManager.PreferredBackBufferHeight;

			Tools.DeviceManager.ApplyChanges();
            Window.Title = "Fractal Dactyl";

			SetupVertices(Color.White);
			SetupIndices();

			CurFractal = new GoldenMean();

			ReferenceFractal = Content.Load<Texture2D>("Fractals\\Fractal");
			//var ReferenceFractal = CreateTexture(CurFractal.ViewWholeFractal_Pos, CurFractal.ViewWholeFractal_Zoom, 2 * Width, 2 * Height);
			//using (var s = new FileStream("C:\\Users\\Ezra\\Desktop\\Fractal.png", FileMode.Create))
			//    ReferenceFractal.SaveAsPng(s, Texture.Width, Texture.Height);

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

            Tools.spriteBatch = new SpriteBatch(GraphicsDevice);

            Tools.padState = new GamePadState[4];
            Tools.PrevpadState = new GamePadState[4];            

			Tools.SetStandardRenderStates();
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        protected void HandleInput()
        {
			Tools.keybState = Keyboard.GetState();
			Tools.CurMouseState = Mouse.GetState();
			Tools.padState[0] = GamePad.GetState(PlayerIndex.One);
			Tools.padState[1] = GamePad.GetState(PlayerIndex.Two);
			Tools.padState[2] = GamePad.GetState(PlayerIndex.Three);
			Tools.padState[3] = GamePad.GetState(PlayerIndex.Four);

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

        protected override void Draw(GameTime gameTime)
        {
            if (!this.IsActive)
            {
                this.IsMouseVisible = true;
                return;
            }
            else
                this.IsMouseVisible = false;

            Tools.gameTime = gameTime;

            float new_t = (float)gameTime.TotalGameTime.TotalSeconds;
            Tools.dt = new_t - Tools.t;
            Tools.t = new_t;

            HandleInput();

			// Set fractal parameters
			CurFractal.SetTime(Tools.t);

            // Calculate high precision orbit of four corners
            double zoom = .001 / CamZoom;
            double a = AspectRatio;
            Complex size = new Complex(zoom * a, zoom);

			Expansion ex = CurFractal.InitializeExpansion(CamPos, size);

            int count = 1;
            for (count = 1; count < MaxIt; count++)
            {
				if ((ex.Corner - ex.Center).Length() > .05f) break;
				if ((ex.h2 * size * size).Length() > .001f) break;

				CurFractal.IterateExpansion(ref ex);

				ex.Center = CurFractal.Iterate(ex.Center);
				ex.Corner = CurFractal.Iterate(ex.Corner);

				float d = (float)ex.Center.LengthSquared();
            }

			CurFractal.SetGpuParameters(ReferenceFractal, ex, count, CamPos, AspectRatio);

			Tools.Device.SetRenderTarget(null);
			Tools.SetStandardRenderStates();
			GraphicsDevice.Clear(Color.Black);
			
			vertexData[TOP_RIGHT].TextureCoordinate		= new Vector2( (float)size.X,  (float)size.Y);
			vertexData[BOTTOM_LEFT].TextureCoordinate	= new Vector2(-(float)size.X, -(float)size.Y);
			vertexData[TOP_LEFT].TextureCoordinate		= new Vector2(-(float)size.X,  (float)size.Y);
			vertexData[BOTTOM_RIGHT].TextureCoordinate	= new Vector2( (float)size.X, -(float)size.Y);

			GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexData, 0, 4, indexData, 0, 2);

            base.Draw(gameTime);
        }
    }
}
