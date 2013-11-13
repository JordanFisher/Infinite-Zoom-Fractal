#include "RootEffect.fx"
#include "Util.fx"

Texture xTexture;
sampler TextureSampler : register(s1) = sampler_state { texture = <xTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; AddressU = clamp; AddressV = clamp; };

int Count;
float2 Center, Rotate, h2, h3, h4;
float D;

float2 CamPos;
float2 c;

int Reps = 75;
float CutOff = 10;

float Lookup(float2 pos)
{
	float2 reference_center = float2(0.353025315759074, 0.257824909262513);
	float reference_zoom = .001 / 0.000630249409724609;
	float2 reference_BL = reference_center - float2(reference_zoom, reference_zoom);

	float2 mapped_pos = (pos - reference_BL) / float2(2 * reference_zoom, 2 * reference_zoom);
	mapped_pos.y = 1 - mapped_pos.y;
	return tex2D(TextureSampler, mapped_pos).r;
}

float Fractal(float2 pos, float2 c)
{
	for (int i = 0; i < 30; i++)
		pos = Mult(pos, pos) + Mult(c, pos);
	return Lookup(pos);



	float fancy_d = D;
	float xtemp, d = 0, avg_d = 0;
	int count = 0;
	
	float prev_d = 0;
	//int ExtraReps = 60; // Good enough Mandel
	
	for (count = 0; count < 100 && d < CutOff; count++)
	//for (count = Count; count < Count + ExtraReps && d < CutOff; count++)
	//for (count = 0; count < 50 && d < CutOff; count++)
	//for (count = Count; count < Reps && d < CutOff; count++)	
    {
		prev_d = d;

		// Golden mean
		pos = Mult(pos, pos) + Mult(c, pos);

		/* Julia
		pos = Mult(pos, pos) + c;
		*/

		/* Newtons
		float2 pos2 = Mult(pos, pos);
		//pos -= Div(Mult(pos, pos2) - float2(1,0), 3*pos2);
		d = pos2.x * pos2.x + pos2.y * pos2.y;
		if (d > 0)
			pos -= (pos - float2(pos2.x, -pos2.y) / d) / 3;
		*/		

		/* Julia (inlined)
		xtemp = pos.x;
		pos.x = pos.x * pos.x - pos.y * pos.y + c.x;
		pos.y = 2 * xtemp * pos.y + c.y;
		*/

		d = (pos.x * pos.x + pos.y * pos.y);
		
		//fancy_d += .75 * log(2*log(2*log(d+2)+2)+2);
		//fancy_d += .75 * log(.8*log(d+1)*(1 + .75 * sin(t/15)) + 1);
		//fancy_d += log(log(d+1)+1);
    }

	//return count;
	//return (count < Count + ExtraReps && d < CutOff) ? 0 : d;//log(log(d));

	// Newton zero
	//return pos.x;
   
	//return d;
	//return log(count + Count);
	//return log(count + Count) * cos(t) + sin(t);
	//return log(count + Count) + .75f * sin(t);
    //float alpha = log(log(CutOff) / log(prev_d)) / log(2.0);
    //return 15 * (Reps - count - alpha) / float(Reps);


    /* Normal 
	if (count < Count + ExtraReps / 2)
		return count*.3;
	else
		return 0;
	*/

    /* Normal + Good inside
	float Mod = Min[0] + Min[1] + Min[2];
	if (count < Count + ExtraReps)
		return count*.3 * Mod;
	else
		return .75 * Mod;
	*/ 
   
	/* Good inside    
    return Min[0] + Min[1] + Min[2];
    return Min[0];
	*/

	/* Good inside + outside    
    float mod = Min[0] + Min[1] + Min[2];
    float mod = Min[0];
    float alpha = log(log(CutOff) / log(prev_d)) / log(2.0);
    return cos(2*pow(log(mod+1),.5)) * 15 * (Reps - count - alpha) / float(Reps);
    */

    /* Accurate escape time
    float alpha = log(log(CutOff) / log(prev_d)) / log(2.0);
    return 15 * (Reps - count - alpha) / float(Reps);
	*/

	/* Averaging
    return fancy_d / count;
    return cos(fancy_d) + 1;
    */

    /* Binary */
	return d < CutOff ? 1 : 0;
	//if (count < Count + ExtraReps && d < CutOff) return 0; else return 1;
}

PixelToFrame FractalPixelShader(VertexToPixel PSIn)
{
    PixelToFrame Output = (PixelToFrame)0;
    float4 baseColor = float4(1, 1, 1, 1);
    float2 pos = PSIn.TexCoords;
    
    float2 delta = pos;
	//float2 delta2 = Mult(delta, pos);
	//float2 delta3 = Mult(delta2, pos);
	//float2 delta4 = Mult(delta3, pos);
    pos = Center + Mult(delta, Rotate);// + Mult(delta2, h2) + Mult(delta3, h3) + Mult(delta4, h4);

	// Julia
    float d = Fractal(pos, c); 

	// Mandelbrot
    //float d = Fractal(pos, CamPos + PSIn.TexCoords); 
    
    /* Full color wheel
    if (d < 1) { baseColor.r = cos(pi*d/2); baseColor.g = sin(pi*d/2); }
    else if (d < 2) { d -= 1; baseColor.g = cos(pi*d/2); baseColor.b = sin(pi*d/2); }
    else { d -= 2; baseColor.b = cos(pi*d/2); baseColor.r = sin(pi*d/2); }
    */
    
	/* Gray scale bands
	baseColor.rgb = .5 * cos(d) + .5;
	*/

	/* Binary */
	if (d >= 1) baseColor = float4(.8,.2,.2,1);
	else baseColor = float4(0,0,0,1);

	/* Color cycling
	baseColor.r = .25 * cos(d+t) + .2;
	baseColor.g = .35 * cos(d+1+t/2) + .2;
	baseColor.b = .15 * cos(d+2+t*3) + .2;
	*/

	/* Nice color spectrum 
	baseColor.r = .25 * cos(d*t) + .2;
	baseColor.g = .35 * cos(d+1+t) + .2;
	baseColor.b = .15 * cos(d+2) + .2;
	*/
    
	//baseColor = float4(d/3,d/10,d/20,1);

    Output.Color = baseColor;

    return Output;
}

float3 ColorFunc(float d)
{
	float3 color;
	color.r = .25 * cos(d) + .2;
	color.g = .35 * cos(d+1) + .2;
	color.b = .15 * cos(d+2) + .2;
	
	return color;
}

PixelToFrame MultisampleFractalPixelShader(VertexToPixel PSIn)
{
    PixelToFrame Output = (PixelToFrame)0;
    
    float4 baseColor = float4(1, 1, 1, 1);
    
    float2 pos = PSIn.TexCoords;
    
    float2 delta = pos;
	float2 delta2 = Mult(delta, pos);
	//float2 delta3 = Mult(delta2, pos);
	//float2 delta4 = Mult(delta3, pos);
    pos = Center + Mult(delta, Rotate) + Mult(delta2, h2);// + Mult(delta3, h3) + Mult(delta4, h4);

    baseColor.rgb *= 0;

    float d;
    
    d = Fractal(pos, c); // Julia
    //d = Fractal(pos, CamPos + PSIn.TexCoords); // Mandelbrot
	
	float3 color = 4 * ColorFunc(d);   

	float shift = .0021;
    d = Fractal(pos+float2(shift,0), c); // Julia
    //d = Fractal(pos, CamPos + PSIn.TexCoords); // Mandelbrot	
	color += ColorFunc(d);   

    d = Fractal(pos-float2(shift,0), c); // Julia
    //d = Fractal(pos, CamPos + PSIn.TexCoords); // Mandelbrot	
	color += ColorFunc(d);   

    d = Fractal(pos+float2(0,shift), c); // Julia
    //d = Fractal(pos, CamPos + PSIn.TexCoords); // Mandelbrot	
	color += ColorFunc(d);   

    d = Fractal(pos-float2(0,shift), c); // Julia
    //d = Fractal(pos, CamPos + PSIn.TexCoords); // Mandelbrot	
	color += ColorFunc(d);   
	
	shift *= 2;
    d = Fractal(pos+float2(shift,0), c); // Julia
    //d = Fractal(pos, CamPos + PSIn.TexCoords); // Mandelbrot	
	color += ColorFunc(d);   

    d = Fractal(pos-float2(shift,0), c); // Julia
    //d = Fractal(pos, CamPos + PSIn.TexCoords); // Mandelbrot	
	color += ColorFunc(d);   

    d = Fractal(pos+float2(0,shift), c); // Julia
    //d = Fractal(pos, CamPos + PSIn.TexCoords); // Mandelbrot	
	color += ColorFunc(d);   

    d = Fractal(pos-float2(0,shift), c); // Julia
    //d = Fractal(pos, CamPos + PSIn.TexCoords); // Mandelbrot	
	color += ColorFunc(d);   

    Output.Color.rgb = color / 12;
    Output.Color.a = 1;

    return Output;
}

technique Simplest
{
    pass Pass0
    {
        VertexShader = compile vs_3_0 SimplestVertexShader();
        PixelShader = compile ps_3_0 FractalPixelShader();
        //PixelShader = compile ps_3_0 MultisampleFractalPixelShader();
    }
}