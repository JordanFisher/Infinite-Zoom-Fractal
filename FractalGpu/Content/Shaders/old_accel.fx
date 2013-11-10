#include"RootEffect.fx"

float2 Center, Rotate, h2, Count;
float D;



float2 Z[100];

float2 c;
float2 z;

int Reps = 75;
float CutOff = 10e8;

float2 Mult(float2 z, float2 w)
{
	return float2(z.x * w.x - z.y * w.y, z.x * w.y + z.y * w.x);
}

float MagSquared(float2 z)
{
	return z.x * z.x + z.y * z.y;
}

float RelativeFractal(float2 pos, float2 c)
{
	bool Relative = true;

	float2 Actual;
	float xtemp, d = 0, avg_d = 0;
	int count;
	for (count = 0; count < Reps && d < CutOff; count++)
    {
		//if (Relative)
		{
			float2 z = c;//Z[count];

			pos = 2 * Mult(z, pos) + Mult(pos, pos);
			
			Actual = c;//Z[count+1] + pos;
			d = Actual.x * Actual.x + Actual.y * Actual.y;
			
			//if (d > 10e3)
			//	Relative = false;
		}
	/*	else
		{
			pos = Mult(pos, pos) + c;
			
			d = (pos.x * pos.x + pos.y * pos.y);
			
		}*/
    }

    return Reps - count;
}

float MinDist[3];
float2 MinPoints[3];
float NumMinPoints;

float Fractal(float2 pos, float2 c)
{
	float fancy_d = D;
	float xtemp, d = 0, avg_d = 0;
	int count;
	
	float Min[3];
	for (int i = 0; i < NumMinPoints; i++)
		Min[i] = MinDist[i];
	
	float prev_d = 0;
	//for (count = Count - 1; count < 3; count++)
	for (count = Count; count < Reps && d < CutOff; count++)
    {
		prev_d = d;
		
		xtemp = pos.x;
		pos.x = pos.x * pos.x - pos.y * pos.y + c.x;
		pos.y = 2 * xtemp * pos.y + c.y;
		
		d = (pos.x * pos.x + pos.y * pos.y);
		
		//fancy_d += .75 * log(2*log(2*log(d+2)+2)+2);
		//fancy_d += .75 * log(.8*log(d+1)*(1 + .75 * sin(t/15)) + 1);
		fancy_d += log(log(d+1)+1);
		
		for (int i = 0; i < NumMinPoints; i++)
			Min[i] = min(Min[i], MagSquared(pos - MinPoints[i]));
    }
    
    //return Min[0] + Min[1] + Min[2];
    
    //float mod = Min[0] + Min[1] + Min[2];
    float mod = Min[0];
    //mod += fancy_d / count;
    
    float alpha = log(log(CutOff) / log(prev_d)) / log(2.0);
    return cos(2*pow(log(mod+1),.5)) * 15 * (Reps - count - alpha) / float(Reps);
    
    
    
    
    // Accurate escape time
    //float alpha = log(log(CutOff) / log(prev_d)) / log(2.0);
    //return 15 * (Reps - count - alpha) / float(Reps);


    
    
    
    
    return 1.5 * (1 + cos(fancy_d)) / 2;
    
    return fancy_d;
    return fancy_d / count;
    
    return cos(fancy_d) + 1;
    
    //int M = 300;
    //return .02f * fancy_d * (M - count - Count) / M;
    
    //return d;
    
    
    return log(log(d)+1);
    //return fancy_d;

	//if (count < Reps) return 0; else return 1;

    //return (Reps - count - Count) / (1.0 + Reps + Count);
    
    //int M = 300;
    //return (M - count - Count) / M;
}

PixelToFrame FractalPixelShader(VertexToPixel PSIn)
{
    PixelToFrame Output = (PixelToFrame)0;
    
    float4 baseColor = float4(1, 1, 1, 1);
    
    float2 pos = PSIn.TexCoords;
    
    pos = Mult(pos, Rotate) + Mult(pos, Mult(pos, h2)) + Center;
    

    float d = Fractal(pos, c); // Julia
    //float d = Fractal(c, pos); // Mandelbrot
    
    baseColor.rgb *= 0;
    float pi = 3.14159;
    
    if (d < 1) { baseColor.r = cos(pi*d/2); baseColor.g = sin(pi*d/2); }
    else if (d < 2) { d -= 1; baseColor.g = cos(pi*d/2); baseColor.b = sin(pi*d/2); }
    else { d -= 2; baseColor.b = cos(pi*d/2); baseColor.r = sin(pi*d/2); }
        
    Output.Color = baseColor;

    return Output;
}

technique Simplest
{
    pass Pass0
    {
        VertexShader = compile vs_3_0 SimplestVertexShader();
        PixelShader = compile ps_3_0 FractalPixelShader();
    }
}