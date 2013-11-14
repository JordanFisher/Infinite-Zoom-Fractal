using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using FractalGpu.Core;

namespace FractalGpu
{
	public class Mandelbrot : Fractal
	{
		public Mandelbrot()
		{
			Fx = new EzEffect(Tools.TheGame.Content, "Shaders\\Standard");
		}

		public override Complex Iterate(Complex z)
		{
			return z * z + c;
		}

		public override void SetTime(float t)
		{
		}

		public virtual Expansion InitializeExpansion(Complex CamPos, Complex Size)
		{
			Expansion expansion = new Expansion(c, Size);
			expansion.a1  = (Complex)0;
			expansion.a2 = (Complex)0;
			expansion.a3 = (Complex)0;
			expansion.a4 = (Complex)0;

			return expansion;
		}

		public override void IterateExpansion(ref Expansion ex)
		{
			// For Mandelbrot
			//if (count == 1) Corner[j, count] = FractalFunc.Fractal(C, Corner[j, 0]);
			//else Corner[j, count] = FractalFunc.Fractal(Corner[j, count - 1], Corner[j, 0]);

			if (CorrectionOrder >= 4) ex.a4 = 2f * ex.a0 * ex.a4 + ex.a2 * ex.a2;
			if (CorrectionOrder >= 3) ex.a3 = 2f * ex.a0 * ex.a3 + 2f * ex.a1 * ex.a2;
			if (CorrectionOrder >= 2) ex.a2 = 2f * ex.a0 * ex.a2 + ex.a1 * ex.a1;
			if (CorrectionOrder >= 1) ex.a1  = 2f * ex.a0 * ex.a1  + (Complex)1;
		}
	}
}
