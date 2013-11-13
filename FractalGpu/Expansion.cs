using System;

using Microsoft.Xna.Framework;

namespace FractalGpu
{
    public struct Expansion
    {
		public Complex h1, h2, h3, h4, Center, Corner;

		public Expansion(Complex CamPos, Complex Size)
        {
			h1 = h2 = h3 = h4 = Complex.Zero;
			
			Center = CamPos;
			Corner = CamPos + Size;
        }

        public override string ToString()
        {
            return string.Format("h * {0} + h^2 * {1}   at  {2}", h1, h2, Center);
        }
    }
}