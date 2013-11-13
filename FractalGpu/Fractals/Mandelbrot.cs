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
			expansion.h  = (Complex)0;
			expansion.h2 = (Complex)0;
			expansion.h3 = (Complex)0;
			expansion.h4 = (Complex)0;

			return expansion;
		}

		public override void IterateExpansion(ref Expansion ex)
		{
			// For Mandelbrot
			//if (count == 1) Corner[j, count] = FractalFunc.Fractal(C, Corner[j, 0]);
			//else Corner[j, count] = FractalFunc.Fractal(Corner[j, count - 1], Corner[j, 0]);

			if (CorrectionOrder >= 4) ex.h4 = 2f * ex.Center * ex.h4 + ex.h2 * ex.h2;
			if (CorrectionOrder >= 3) ex.h3 = 2f * ex.Center * ex.h3 + 2f * ex.h * ex.h2;
			if (CorrectionOrder >= 2) ex.h2 = 2f * ex.Center * ex.h2 + ex.h * ex.h;
			if (CorrectionOrder >= 1) ex.h  = 2f * ex.Center * ex.h  + (Complex)1;
		}
	}
}
