using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using FractalGpu.Core;

namespace FractalGpu
{
	public class Julia : Fractal
	{
		public Julia()
		{
			Fx = new EzEffect(Tools.TheGame.Content, "Shaders\\Standard");
		}

		public override Complex Iterate(Complex z)
		{
			return z * z + c;
		}

		public override void SetTime(float t)
		{
			c = new Complex(.4, -.35);
			//C = new Complex(.4 + Math.Sin(Step), -.35 + Math.Cos(1.11f * Step));
		}

		public override void IterateExpansion(ref Expansion ex)
		{
			if (CorrectionOrder >= 4) ex.a4 = 2f * ex.a0 * ex.a4 + ex.a2 * ex.a2;
			if (CorrectionOrder >= 3) ex.a3 = 2f * ex.a0 * ex.a3 + 2f * ex.a1 * ex.a2;
			if (CorrectionOrder >= 2) ex.a2 = 2f * ex.a0 * ex.a2 + ex.a1 * ex.a1;
			if (CorrectionOrder >= 1) ex.a1 *= 2f * ex.a0;
		}
	}
}
