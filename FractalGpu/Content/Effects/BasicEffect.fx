#include"RootEffect.fx"

Texture xTexture;
sampler TextureSampler : register(s1) = sampler_state { texture = <xTexture> ; magfilter = LINEAR; minfilter = LINEAR; mipfilter=LINEAR; };//AddressU = wrap; AddressV = clamp;};

PixelToFrame OurFirstPixelShader(VertexToPixel PSIn)
{
    PixelToFrame Output = (PixelToFrame)0;
    
    float4 baseColor = tex2D(TextureSampler, PSIn.TexCoords);
    
    Output.Color = baseColor;
    Output.Color *= PSIn.Color;

    return Output;
}

technique Simplest
{
    pass Pass0
    {
        VertexShader = compile vs_3_0 SimplestVertexShader();
        PixelShader = compile ps_3_0 OurFirstPixelShader();
    }
}