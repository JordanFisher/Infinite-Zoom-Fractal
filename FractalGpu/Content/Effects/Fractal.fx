// not used
int Count;
float2 Center, Rotate, h2, h3, h4;
float D;

float2 CamPos;
float2 c;
float MinDist[3];
float2 MinPoints[3];
float NumMinPoints;





#include"RootEffect.fx"

Texture xTexture;
sampler TextureSampler : register(s1) = sampler_state { texture = <xTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; };//AddressU = wrap; AddressV = clamp;};

//int Reps = 10;
//float Div = 20;

int Reps = 15;
//float Div = 300;
float CutOff = 10e4;

float Fractal(float2 pos, float2 c)
{
	float modulate = .2 + .2 * sin(t / 23);

	float xtemp, d = 0, avg_d = 0;
	int count;
	for (count = 0; count < Reps && d < CutOff; count++)
	//for (; count < Reps; count++)
    {
		xtemp = pos.x + cos(pos.x/3) * modulate;
		pos.x = pos.x * pos.x - pos.y * pos.y + c.x + sin(pos.y/3) * modulate;
		
		//pos.y = 2 * pos.x * pos.y + c.y;
		pos.y = 2 * xtemp * pos.y + c.y;
		
		d = (pos.x * pos.x + pos.y * pos.y);
		
		//avg_d += log(log(log(d)*2+1)*2+1);
		//avg_d += log(log(log(d)+1)+1);
		avg_d += log(log(d)*(1 + .75 * sin(t/15)) + 1);
    }
    
    avg_d /= count;
    
    return avg_d / (.5 + cos(.125*log(log(d)+1))); // Best Mandelbrot
    //return .5 + cos(log(log(d)+1));
    //return (.5 + cos(log(log(d)+1))) * log(avg_d*4); // fine and delicate
    //return pow(cos(avg_d), cos(3*log(log(d)+1)+1));
    
    //return log(avg_d); // best
    
    //return 1 - log(log(d) * 10) * .1;
    
    return count - log(log(d)/log(2))/log(2);
    
    float div = 5;
    //div = 5 + cos(t / 10) * 3;
    return (avg_d) / div;
    
    return d / 150;
    

    return count / 10.0;
}

PixelToFrame FractalPixelShader(VertexToPixel PSIn)
{
    PixelToFrame Output = (PixelToFrame)0;
    
    //float4 baseColor = tex2D(TextureSampler, PSIn.TexCoords);
    float4 baseColor = float4(1, 1, 1, 1);
    
    float zoom = 4 + 2.5 + .12 * sin(t / 30);
    float2 pos = zoom * (PSIn.TexCoords - float2(.5, .5));
    pos += .06 * float2(cos(t / 40), sin(t / 50));
    
    //pos *= .80;
    //pos.x -= .38;
    
    float2 c = .5 * float2(cos(t/5), sin(t/8));
    
    /*for (int i = 0; i < 10; i++)
    {
		pos.x = pos.x * pos.x - pos.y * pos.y + c.x;
		pos.y = 2 * pos.x * pos.y + c.y;
    }    
    float d = (pos.x * pos.x + pos.y * pos.y) / 20;*/
    
    //float d = Fractal(pos, c); // Julia
    float d = Fractal(c, pos); // Mandelbrot
    
    /* Julia-Julia
    float mod = Fractal(pos, c);
    float d = Fractal(pos, mod * c); */

    /*float s = .0005;
    d += Fractal(pos + float2(s,s), c);
    d += Fractal(pos - float2(s,s), c);
    d += Fractal(pos + float2(s,-s), c);
    d += Fractal(pos - float2(s,-s), c);
    d += Fractal(pos + 2*float2(s,s), c);
    d += Fractal(pos - 2*float2(s,s), c);
    d += Fractal(pos + 2*float2(s,-s), c);
    d += Fractal(pos - 2*float2(s,-s), c);
    d /= 5;
    */
    
    baseColor.rgb *= 0;
    float pi = 3.14159;
    if (d < .5) { baseColor.r = cos(pi*d); baseColor.g = sin(pi*d); }
    else { d -= .5; baseColor.g = cos(pi*d); baseColor.b = sin(pi*d); }
    
    //baseColor.r = d;
    //baseColor.g = 1 - d;
    //baseColor.b = cos(3.14 * d);
    
    //baseColor.rgb *= d;
    //baseColor.r = abs(pos.x);
    //baseColor.g = abs(pos.y);
    
    Output.Color = baseColor;
    Output.Color *= PSIn.Color;

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