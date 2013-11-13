using System;

using Microsoft.Xna.Framework;

namespace FractalGpu
{
    public struct Expansion
    {
		Complex h, h2, h3, h4, Center, Corner;

		public Expansion(Complex CamPos, Complex Size)
        {
			h = h2 = h3 = h4 = Complex.Zero;
			
			Center = CamPos;
			Corner = CamPos + Size;
        }

        public override string ToString()
        {
            return string.Format("h * {0} + h^2 * {1}   at  {2}", h, h2, Center);
        }
    }
}