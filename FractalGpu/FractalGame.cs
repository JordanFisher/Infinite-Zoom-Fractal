
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using FractalGpu.Core;

namespace FractalGpu
{
	public struct ZoomPiece
	{
		public Complex CamPos;
		public double CamZoom;

		public Expansion T;

		public const double ZoomThreshold = 1e-12;
		public const double OneOver_ZoomThreshold = 1 / ZoomThreshold;

		public bool RequiresUpdate;

		public void Initialize()
		{
			CamPos = Complex.Zero;
			CamZoom = 1;

			RequiresUpdate = true;
		}
	}

    public class FractalGame : Game
    {
		const int MaxZoomPieces = 10000;
		ZoomPiece[] Pieces = new ZoomPiece[MaxZoomPieces];

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

			for (int i = 0; i < MaxZoomPieces; i++)
				Pieces[i].Initialize();

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

				int i = 0;
				while (i < MaxZoomPieces)
				{
					if (Pieces[i].CamZoom > ZoomPiece.ZoomThreshold) break;
					i++;
				}

				if (i > 0 && Pieces[i].CamZoom > 1) i--;

				for (int j = MaxZoomPieces - 1; j >= i + 1; j--)
				{
					Pieces[j-1].CamPos += Pieces[j].CamPos * Pieces[j-1].CamZoom;

					Pieces[j].CamPos = Complex.Zero;
					Pieces[j].CamZoom = 1;

					Pieces[j].RequiresUpdate = true;
				}

				// Update the current ZoomPiece
				Pieces[i].CamPos += .03 * Pieces[i].CamZoom * (Complex)dir;

                double ZoomRate = .95;

                if (ButtonCheck.State(ControllerButtons.B, -1).Down)
                {
					Pieces[i].CamZoom /= ZoomRate;
                }

                if (ButtonCheck.State(ControllerButtons.A, -1).Down)
                {
					Pieces[i].CamZoom *= ZoomRate;
                }

				//if (i > 0 && Pieces[i].CamPos.Length() * Pieces[i - 1].CamZoom > Pieces[i - 1].T.Size.Length() / 8)
				//{
				//    Pieces[i - 1].CamPos += Pieces[i].CamPos * Pieces[i - 1].CamZoom;
				//    Pieces[i].CamPos = Complex.Zero;

				//    Pieces[i - 1].RequiresUpdate = true;
				//}

				Pieces[i].RequiresUpdate = true;
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

			bool BottomReached = false;

			int i = 0;
			while (i < MaxZoomPieces && !BottomReached)
			{
				double zoom = Pieces[i].CamZoom;

				if (zoom > ZoomPiece.ZoomThreshold)
				{
					BottomReached = true;
				}

				if (Pieces[i].RequiresUpdate)
				{
					if (i == 0)
					{
						Pieces[i].T = CurFractal.InitializeExpansion(Pieces[i].CamPos, new Complex(AspectRatio * zoom, zoom));
					}
					else
					{
						Pieces[i].T = Pieces[i - 1].T;
						Pieces[i].T.Normalize();
						Pieces[i].T.ShiftCenter(Pieces[i].CamPos);
						Pieces[i].T.Size = new Complex(AspectRatio * zoom, zoom);
						Pieces[i].T.Corner = Pieces[i].T.a0 + Pieces[i].T.Size;
					}

					// We know that Pieces[i].T is valid starting out.
					// We store it for future use, because we may need to back-track if we transform Pieces[i].T into something invalid.
					Expansion ValidExpansion = Pieces[i].T;

					while (true)
					{
						// This checks to the see if the expansion is invalid by seeing if the distance from center to corner is too large.
						// It appears we don't need this, and the Taylor series error approximation is sufficient.
						//if ((Pieces[i].T.Corner - Pieces[i].T.a0).Length() > .05f) break;
						
						if ((Pieces[i].T.a2 * Pieces[i].T.Size * Pieces[i].T.Size).Length() > .001f)
						{
							// The expansion has become invalid. Backtrack one step and exit the iteration loop.
							Pieces[i].T = ValidExpansion;
							break;
						}

						ValidExpansion = Pieces[i].T;

						CurFractal.IterateExpansion(ref Pieces[i].T);

						Pieces[i].T.a0 = CurFractal.Iterate(Pieces[i].T.a0);
						Pieces[i].T.Corner = CurFractal.Iterate(Pieces[i].T.Corner);

						Pieces[i].T.IterationCount++;
					}
				}

				Pieces[i].RequiresUpdate = false;

				if (!BottomReached) i++;
			}

			Expansion ex = Pieces[i].T;
			CurFractal.SetGpuParameters(ReferenceFractal, ex, Complex.Zero /* warning wtf */, AspectRatio);

			Tools.Device.SetRenderTarget(null);
			Tools.SetStandardRenderStates();
			GraphicsDevice.Clear(Color.Black);

			vertexData[TOP_RIGHT].TextureCoordinate		= new Vector2( (float)ex.Size.X,  (float)ex.Size.Y);
			vertexData[BOTTOM_LEFT].TextureCoordinate	= new Vector2(-(float)ex.Size.X, -(float)ex.Size.Y);
			vertexData[TOP_LEFT].TextureCoordinate		= new Vector2(-(float)ex.Size.X,  (float)ex.Size.Y);
			vertexData[BOTTOM_RIGHT].TextureCoordinate	= new Vector2( (float)ex.Size.X, -(float)ex.Size.Y);

			GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexData, 0, 4, indexData, 0, 2);

            base.Draw(gameTime);
        }
    }
}
