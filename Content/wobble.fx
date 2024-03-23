#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float time;


float3 HSVtoRGB(float3 HSV)
{
    float3 RGB = 0;
    float C = HSV.z * HSV.y;
    float H = HSV.x * 6;
    float X = C * (1 - abs(fmod(H, 2) - 1));
    if (HSV.y != 0)
    {
        float I = floor(H);
        if (I == 0)
        {
            RGB = float3(C, X, 0);
        }
        else if (I == 1)
        {
            RGB = float3(X, C, 0);
        }
        else if (I == 2)
        {
            RGB = float3(0, C, X);
        }
        else if (I == 3)
        {
            RGB = float3(0, X, C);
        }
        else if (I == 4)
        {
            RGB = float3(X, 0, C);
        }
        else
        {
            RGB = float3(C, 0, X);
        }
    }
    float M = HSV.z - C;
    return RGB + M;
}

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float4 Color : COLOR0;
    float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
    float2 UV : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

    float ix = 0.4f * sin(time + 20 * input.Position.y);
    float iy = 0.2f * sin(time + 10 * input.Position.x);
	
    output.Position = input.Position + float4(ix, iy, 0, 0);
	output.Color = input.Color;
    output.UV = input.UV;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float3 rgb = HSVtoRGB(float3(input.UV.x, 0.6f, 0.8f));
    return float4(rgb, 1);
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};