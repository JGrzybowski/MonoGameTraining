//----------------
//Constants
//----------------
float4x4 xWorld, xView, xProjection, xWorldIT;
float4 xCameraPosition;
float4 AmbientColor;
//PointLights
float4 L1Position, L2Position;
float3 L1DColor, L2DColor;
float4 L1SColor, L2SColor;
float L1Range, L2Range;
bool L1On,L2On;
//Fog

//Textures
Texture2D tex1;
Texture2D tex2;
sampler TextureSampler1 = sampler_state { magfilter = POINT; minfilter = POINT; mipfilter = POINT; AddressU = mirror; AddressV = mirror; };
sampler TextureSampler2 = sampler_state { magfilter = POINT; minfilter = POINT; mipfilter = POINT; AddressU = mirror; AddressV = mirror; };
//----------------

//----------------
//Structures
//----------------
struct VertexColorShaderIn {
	float4 Position : SV_POSITION0;
	float4 Color : COLOR0;
	float4 Normal : NORMAL0;
};

struct VertexColorShaderOut {
	float4 Position : SV_POSITION0;
	float4 WPosition : TEXCOORD0;
	float4 Color : COLOR0;
	float4 WNormal : TEXCOORD1;
};

struct PixelShaderOut {
	float4 Color : COLOR0;
};


//----------------
//Shaders
//----------------
VertexColorShaderOut VSmain (VertexColorShaderIn input) {
	VertexColorShaderOut output = (VertexColorShaderOut)0;
	input.Position.w = 1;
	input.Normal.w = 0;
	output.WPosition = mul(input.Position, xWorld);
	output.Position = mul(output.WPosition, xView);
	output.Position = mul(output.Position, xProjection);
	
	output.WNormal = normalize(mul(input.Normal, xWorld));
	output.Color = input.Color; 
	
	return output;
}

float4 CalculateColor(float4 light, float4 normal, float4 view, 
						float3 diffuseColor, float3 specularColor, int specularPower, float range)
{
	float attenuation = saturate(1 - dot(range, light/ range));
	
	float3 diffuse = dot(normal,light) * diffuseColor;
	//diffuse *= attenuation;

	float4 reflection = reflect(-light, normal);
	float3 specular = pow(dot(view, reflection), abs(specularPower)) * specularColor;
	//specular *= attenuation;

	return float4(saturate(diffuse) + saturate(specular), 1);
}

float4 CalculateSpotlightColor(float4 light, float4 normal, float4 view,
								float3 diffuseColor, float3 specularColor, int specularPower, float range, float direction)
{
	float c = saturate(dot(-light, direction));
	
	if (c > 0)
		return CalculateColor(light, normal, view, diffuseColor, specularColor, specularPower, range);
	else
		return float4(0, 0, 0, 0);
}

PixelShaderOut PSmain(VertexColorShaderOut input)
{
	PixelShaderOut output = (PixelShaderOut)0;

	float4 normal = normalize(input.WNormal);
	float4 v = normalize(xCameraPosition - input.WPosition);
	float4 l1 = normalize(L1Position - input.WPosition);
	float4 l2 = normalize(L2Position - input.WPosition);

	float4 c1 = CalculateColor(l1, normal, v, L1DColor, L1SColor.xyz, (int)(L1SColor.w), L1Range);
	float4 c2 = CalculateColor(l2, normal, v, L2DColor, L2SColor.xyz, (int)(L2SColor.w), L2Range);
	
	output.Color = AmbientColor;
	if(L1On)
		output.Color += c1;
	if(L2On)
		output.Color += c2;
	return output;
}

technique SinglePointLight
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 VSmain();
		PixelShader = compile ps_4_0 PSmain();
	}
}

//---------------SIMPLE TEXTURED
struct VertexTextureShaderIn {
	float4 Position : SV_POSITION0;
	float4 Normal : NORMAL0;
	float4 TexPosition : TEXCOORD0;
};


struct VertexTextureShaderOut {
	float4 Position : SV_POSITION0;
	float4 WPosition : TEXCOORD0;
	float4 WNormal : TEXCOORD1;
	float4 TexPosition : TEXCOORD2;
};


VertexTextureShaderOut VSSimpleTexMain(VertexTextureShaderIn input) {
	VertexTextureShaderOut output = (VertexTextureShaderOut)0;
	input.Position.w = 1;
	input.Normal.w = 0;
	output.WPosition = mul(input.Position, xWorld);
	output.Position = mul(output.WPosition, xView);
	output.Position = mul(output.Position, xProjection);
	output.TexPosition = input.TexPosition;

	return output;
}

PixelShaderOut PSSingleTexMain(VertexTextureShaderOut input)
{
	PixelShaderOut output = (PixelShaderOut)0;
	output.Color = tex1.Sample(TextureSampler1, input.TexPosition);
	return output;
}

PixelShaderOut PSDoubleTexMain(VertexTextureShaderOut input)
{
	PixelShaderOut output = (PixelShaderOut)0;
	float4 t1 = tex1.Sample(TextureSampler1, input.TexPosition);
	float4 t2 = tex2.Sample(TextureSampler2, input.TexPosition);
	output.Color = float4((1 - t2.w)*t1.xyz + t2.xyz*t2.w, 1);
	return output;
}

technique SingleTextured
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 VSSimpleTexMain();
		PixelShader = compile ps_4_0 PSSingleTexMain();
	}
};

technique DoubleTextured 
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 VSSimpleTexMain();
		PixelShader = compile ps_4_0 PSDoubleTexMain();
	}
};


//----------------TEXTURED WITH LIGHT SHADER


//-------------

VertexTextureShaderOut VSTexMain(VertexTextureShaderIn input) {
	VertexTextureShaderOut output = (VertexTextureShaderOut)0;
	input.Position.w = 1;
	input.Normal.w = 0;
	output.WPosition = mul(input.Position, xWorld);
	output.Position = mul(output.WPosition, xView);
	output.Position = mul(output.Position, xProjection);

	output.WNormal = normalize(mul(input.Normal, xWorld));
	output.TexPosition = input.TexPosition;

	return output;
}

PixelShaderOut PSTexMain(VertexTextureShaderOut input)
{
	PixelShaderOut output = (PixelShaderOut)0;

	float4 normal = normalize(input.WNormal);
	float4 v = normalize(xCameraPosition - input.WPosition);
	float4 l1 = normalize(L1Position - input.WPosition);
	float4 l2 = normalize(L2Position - input.WPosition);

	float4 c1 = CalculateColor(l1, normal, v, L1DColor, L1SColor.xyz, (int)(L1SColor.w), L1Range);
	float4 c2 = CalculateColor(l2, normal, v, L2DColor, L2SColor.xyz, (int)(L2SColor.w), L2Range);

	output.Color = AmbientColor;
	if (L1On)
		output.Color += c1;
	if (L2On)
		output.Color += c2;

	
	float4 t1 = output.Color = tex2D(TextureSampler1, input.TexPosition);
	float4 t2 = output.Color = tex2D(TextureSampler2, input.TexPosition);
	output.Color = float4((1 - t2.w)*t1.xyz + t2.xyz*t2.w,1);
	return output;
}


technique TexturedPointLight 
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 VSTexMain();
		PixelShader = compile ps_4_0 PSTexMain();
	}
};