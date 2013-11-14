using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using FractalGpu.Core;

namespace FractalGpu
{
	public class GoldenMean : Fractal
	{
		public override Complex ViewWholeFractal_Pos  { get { return new Complex(0.353025315759074, 0.257824909262513); } }
		public override double  ViewWholeFractal_Zoom { get { return 0.000630249409724609; } }

		public GoldenMean()
		{
			Fx = new EzEffect(Tools.TheGame.Content, "Shaders\\Standard");
		}

		public static Complex GoldenMean_rho = new Complex(-0.74405117795419151, -0.66812262690690249);
		public override Complex Iterate(Complex z)
		{
			return z * z + GoldenMean_rho * z;
		}

		public override void SetTime(float t)
		{
			// Standard Golden Mean
			//C = FractalFunc.GoldenMean_rho = new Complex(-0.74405117795419151, -0.66812262690690249);

			// Swirly Golden Mean
			//c = GoldenMean_rho = new Complex(-0.75905117795419151, -0.65812262690690249);
			c = GoldenMean_rho;

			// Animated
			//C = FractalFunc.GoldenMean_rho = new Complex(-0.74405117795419151 + 0.01 * Math.Cos(Tools.t / 10), -0.66812262690690249 + 0.01 * Math.Sin(Tools.t / 13));
		}

		public override void IterateExpansion(ref Expansion ex)
		{
			if (CorrectionOrder >= 4) ex.a4 = (2f * ex.a0 + GoldenMean_rho) * ex.a4 + ex.a2 * ex.a2;
			if (CorrectionOrder >= 3) ex.a3 = (2f * ex.a0 + GoldenMean_rho) * ex.a3 + 2f * ex.a1 * ex.a2;
			if (CorrectionOrder >= 2) ex.a2 = (2f * ex.a0 + GoldenMean_rho) * ex.a2 + ex.a1 * ex.a1;
			if (CorrectionOrder >= 1) ex.a1  = (2f * ex.a0 + GoldenMean_rho) * ex.a1;
		}
	}
}
