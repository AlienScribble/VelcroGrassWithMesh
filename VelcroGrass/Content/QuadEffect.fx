#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1 // use 5_0 to allow more instructions (DX11)
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

sampler TexSampler : register(s0)
{
	Texture = <Texture>;
	AddressU = clamp;
	AddressV = clamp;
	Filter = linear;
};


matrix transform;

struct VSOutput{
	float4 pos : SV_POSITION;
	float4 col : COLOR0;
	float2 uv  : TEXCOORD0;
};

VSOutput MainVS(float4 pos : SV_Position, float4 col : COLOR0, float2 uv : TEXCOORD0)
{
	VSOutput output;	
	output.pos   = mul(pos, transform);
	output.col   = col;
	output.uv    = uv;
	return output;
}

float4 MainPS(VSOutput input) : COLOR
{
	return tex2D(TexSampler, input.uv) * input.col;
}


technique QuadDraw
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader  = compile PS_SHADERMODEL MainPS();
	}
};