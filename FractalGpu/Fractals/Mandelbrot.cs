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

		public override void InitializeExpansion(Complex CamPos, ref Complex h, ref Complex h2, ref Complex h3, ref Complex h4, ref Complex Center)
		{
			h =  (Complex)0;
			h2 = (Complex)0;
			h3 = (Complex)0;
			h4 = (Complex)0;
			Center = c;
		}

		public override void IterateExpansion(Complex Center, ref Complex h, ref Complex h2, ref Complex h3, ref Complex h4)
		{
			if (CorrectionOrder >= 4) h4 = 2f * Center * h4 + h2 * h2;
			if (CorrectionOrder >= 3) h3 = 2f * Center * h3 + 2f * h * h2;
			if (CorrectionOrder >= 2) h2 = 2f * Center * h2 + h * h;
			if (CorrectionOrder >= 1) h  = 2f * Center * h  + (Complex)1;
		}
	}
}
