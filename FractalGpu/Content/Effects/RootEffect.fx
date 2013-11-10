float4 xCameraPos;
float xCameraAngle;
float xCameraAspect;
bool xFlip;
float2 FlipCenter, Pivot;
float t;

struct VertexToPixel
{
    float4 Position     : POSITION0;    
    float4 Color		: COLOR0;
    float2 TexCoords    : TEXCOORD0;
    //float2 Position2D   : TEXCOORD2;
};

struct PixelToFrame
{
    float4 Color        : COLOR0;
};

VertexToPixel SimplestVertexShader( float2 inPos : POSITION0, float2 inTexCoords : TEXCOORD0, float4 inColor : COLOR0)//, float inDepth : POSITION1)
{
    VertexToPixel Output = (VertexToPixel)0;    

    Output.Position.xy = inPos;
    Output.Position.w = 1;
    if (xFlip) inPos.x = FlipCenter.x - (inPos.x - FlipCenter.x);
    Output.Position.x = (inPos.x - xCameraPos.x) / xCameraAspect * xCameraPos.z;
    Output.Position.y = (inPos.y - xCameraPos.y) * xCameraPos.w;
    
    Output.TexCoords = inTexCoords;
    //Output.Position2D = inPos.xy;
    Output.Color = inColor;
    
    return Output;
}