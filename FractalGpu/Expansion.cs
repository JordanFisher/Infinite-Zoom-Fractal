using System;

using Microsoft.Xna.Framework;

namespace FractalGpu
{
    public struct Expansion
    {
		public Complex a1, a2, a3, a4, a0, Corner, Size;
		public int IterationCount;

		public Expansion(Complex CamPos, Complex Size)
        {
			a1 = a2 = a3 = a4 = Complex.Zero;

			a0 = CamPos;
			this.Size = Size;
			Corner = CamPos + Size;

			IterationCount = 0;
        }

		public void ShiftCenter(Complex Shift)
		{
			Complex s1 = Shift, s2 = s1 * s1, s3 = s2 * s1, s4 = s3 * s1;

			a0 = a0 + 1 * a1 * s1 + 1 * a2 * s2 + 1 * a3 * s3 + 1 * a4 * s4;
			a1 =	  1 * a1	  + 2 * a2 * s1 + 3 * a3 * s2 + 4 * a4 * s3;
			a2 =					1 * a2		+ 3 * a3 * s1 + 6 * a4 * s2;
			a3 =								  1 * a3	  + 4 * a4 * s1;
		}

		public void Normalize()
		{
			//double Length = a1.Length();
			double Length = ZoomPiece.OneOver_ZoomThreshold;
			
			a1 /= Length;
			a2 /= Length * Length;
			a3 /= Length * Length * Length;
			a4 /= Length * Length * Length * Length;

			Size *= Length;
		}

        public override string ToString()
        {
            return string.Format("h * {0} + h^2 * {1}   at  {2}", a1, a2, a0);
        }
    }
}