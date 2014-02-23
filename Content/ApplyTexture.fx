float4x4 World;
float4x4 Projection;

float4 Tint = {1, 1, 1, 1};

Texture DiffuseMap;

bool TextureEnabled = true;

sampler diffuse_sampler = sampler_state
{
    Texture = <DiffuseMap>;
    
	AddressU = WRAP;
	AddressV = WRAP;
	AddressW = WRAP; 

    MipFilter = ANISOTROPIC;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
	
    output.Position = mul(worldPosition, Projection);
    output.Color = input.Color;
	output.TexCoord = input.TexCoord;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float4 result = 
		TextureEnabled ? 
			tex2D(diffuse_sampler, input.TexCoord) :
			input.Color;
    
    return result * Tint;
}

technique Main
{
    pass P1
    {
        VertexShader = compile vs_1_1 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
