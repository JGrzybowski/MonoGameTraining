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
float fogStart = 1;
float fogEnd = 100;
float4 fogColor = float4(1.0f, 1.0f, 1.0f, 1.0f);
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

struct PixelShaderOut {
	float4 Color : COLOR0;
};


//----------------
//Functions
//----------------
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

float4 ApplyPointLight(float4 inColor, float4 light, float4 normal, float4 view, int lightN)
{
	if(lightN == 1)
		return inColor + CalculateColor(light, normal, view, L1DColor, L1SColor.xyz, (int)(L1SColor.w), L1Range);
	if(lightN == 2)
		return inColor + CalculateColor(light, normal, view, L2DColor, L2SColor.xyz, (int)(L2SColor.w), L2Range);
}

//position => WPosition
float4 ApplyFog(float4 inColor, float4 position) 
{
	float fogFactor = saturate((fogEnd - (xCameraPosition - position)) / (fogEnd - fogStart));
	float4 fog = fogFactor + (1.0 - fogFactor) * fogColor;

	return inColor * fog;
}

float4 ApplyTexture(float inColor, int texN, int samplerN, float2 texCoordinates)
{
	texture2D tex; 
	if (texN == 1)
		tex = tex1;
	if (texN == 2)
		tex = tex2;

	sampler sam;
	if (samplerN == 1)
		sam = TextureSampler1;
	if (samplerN == 2)
		sam = TextureSampler2;

	float4 tColor = tex.Sample(sam, texCoordinates);
	return float4((1 - tColor.w)*inColor + tColor.xyz*tColor.w, 1);
}

VertexColorShaderOut VSmain(VertexColorShaderIn input) 
{
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

PixelShaderOut PSmain(VertexColorShaderOut input)
{
	PixelShaderOut output = (PixelShaderOut)0;

	float4 normal = normalize(input.WNormal);
	float4 v = normalize(xCameraPosition - input.WPosition);
	float4 l1 = normalize(L1Position - input.WPosition);
	float4 l2 = normalize(L2Position - input.WPosition);

	output.Color = input.Color * AmbientColor;
	if(L1On)
		output.Color = ApplyPointLight(output.Color, l1, normal, v, 1);
	if (L2On)
		output.Color = ApplyPointLight(output.Color, l2, normal, v, 2);
	return output;
}



//---------------SIMPLE TEXTURED
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

//----------------TEXTURED WITH LIGHT SHADER
VertexTextureShaderOut VSLightMain(VertexTextureShaderIn input) {
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

PixelShaderOut PSLightMain(VertexTextureShaderOut input)
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

//----------------
//Techniques
//----------------

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

technique SinglePointLight
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 VSmain();
		PixelShader = compile ps_4_0 PSmain();
	}
}

technique TexturedPointLight 
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 VSLightMain();
		PixelShader = compile ps_4_0 PSLightMain();
	}
};