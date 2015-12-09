//----------------
//Constants
//----------------
float4x4 xWorld, xView, xProjection, xWorldIT;
float4 xCameraPosition;
float4 AmbientColor = float4(0.3,0.3,0.3,1);
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

//--Structures----------
struct VSColorInput {
	float4 Position : SV_POSITION0;
	float4 Color : COLOR0;
	float4 Normal : NORMAL0;
};

struct VSColorOutput {
	float4 Position : SV_POSITION0;
	float4 WPosition : TEXCOORD0;
	float4 Color : COLOR0;
	float4 WNormal : TEXCOORD1;
};

struct VSTextureInput {
	float4 Position : SV_POSITION0;
	float4 Normal : NORMAL0;
	float4 TexPosition : TEXCOORD0;
};

struct VSTextureOutput {
	float4 Position : SV_POSITION0;
	float4 WPosition : TEXCOORD0;
	float4 WNormal : TEXCOORD1;
	float4 TexPosition : TEXCOORD2;
};

struct PSOutput {
	float4 Color : COLOR0;
};


//--Functions----------
float4 CalculateColor(float4 light, float4 normal, float4 view, 
						float3 diffuseColor, float3 specularColor, int specularPower, float range, float distance)
{

	float attenuation = saturate((range - distance)/range);
	
	float3 diffuse = dot(normal,light) * diffuseColor;
	diffuse *= attenuation;

	float4 reflection = reflect(-light, normal);
	float3 specular = pow(dot(view, reflection), abs(specularPower)) * specularColor;
	specular *= attenuation;

	return float4(saturate(diffuse) + saturate(specular), 1);
}

//float4 CalculateSpotlightColor(float4 light, float4 normal, float4 view,
//								float3 diffuseColor, float3 specularColor, int specularPower, float range, float direction)
//{
//	float c = saturate(dot(-light, direction));
//	
//	if (c > 0)
//		return CalculateColor(light, normal, view, diffuseColor, specularColor, specularPower, range, );
//	else
//		return float4(0, 0, 0, 0);
//}

float4 ApplyPointLight(float4 inColor, float4 light, float distance, float4 normal, float4 view, int lightN)
{
	if(lightN == 1)
		return inColor + CalculateColor(light, normal, view, L1DColor, L1SColor.xyz, (int)(L1SColor.w), L1Range, distance);
	if(lightN == 2)
		return inColor + CalculateColor(light, normal, view, L2DColor, L2SColor.xyz, (int)(L2SColor.w), L2Range, distance);
}

//position => WPosition
float4 ApplyFog(float4 inColor, float4 position) 
{
	float fogFactor = saturate((fogEnd - (xCameraPosition - position)) / (fogEnd - fogStart));
	float4 fog = fogFactor + (1.0 - fogFactor) * fogColor;

	return inColor * fog;
}

float4 ApplyTexture(float4 inColor, texture2D tex, sampler sam, float2 texCoordinates)
{
	float4 tColor = tex.Sample(sam, texCoordinates);
	return float4((1 - tColor.w)*inColor + tColor.xyz * tColor.w, 1);
}


//--Shaders----------

//--Vertex shaders--------
VSColorOutput VSColor(VSColorInput input) 
{
	VSColorOutput output = (VSColorOutput)0;
	input.Position.w = 1;
	input.Normal.w = 0;

	output.WPosition = mul(input.Position, xWorld);
	output.Position = mul(output.WPosition, xView);
	output.Position = mul(output.Position, xProjection);
	output.WNormal = normalize(mul(input.Normal, xWorld));
	
	output.Color = input.Color;
	return output;
}
VSTextureOutput VSTexture(VSTextureInput input) {
	VSTextureOutput output = (VSTextureOutput)0;
	input.Position.w = 1;
	input.Normal.w = 0;

	output.WPosition = mul(input.Position, xWorld);
	output.Position = mul(output.WPosition, xView);
	output.Position = mul(output.Position, xProjection);
	output.WNormal = normalize(mul(input.Normal, xWorld));

	output.TexPosition = input.TexPosition;
	return output;
}

//--Pixel shaders
PSOutput PSColor(VSColorOutput input)
{
	PSOutput output = (PSOutput)0;

	float4 normal = normalize(input.WNormal);
	float4 v = normalize(xCameraPosition - input.WPosition);
	float4 l1 = normalize(L1Position - input.WPosition);
	float dist1 = length(L1Position - input.WPosition);
	float4 l2 = normalize(L2Position - input.WPosition);
	float dist2 = length(L2Position - input.WPosition);

	output.Color = input.Color * AmbientColor;
	if(L1On)
		output.Color = ApplyPointLight(output.Color, l1, dist1, normal, v, 1);
	if (L2On)
		output.Color = ApplyPointLight(output.Color, l2, dist2, normal, v, 2);
	return output;
}
PSOutput PSTextureOnly(VSTextureOutput input)
{
	PSOutput output = (PSOutput)0;
	output.Color = ApplyTexture(output.Color, tex1, TextureSampler1, input.TexPosition);
	return output;
}
PSOutput PSTwoTextureOnly(VSTextureOutput input)
{
	PSOutput output = (PSOutput)0;
	output.Color = ApplyTexture(output.Color, tex1, TextureSampler1, input.TexPosition);
	output.Color = ApplyTexture(output.Color, tex2, TextureSampler2, input.TexPosition);
	return output;
}
PSOutput PSTextureAndLight(VSTextureOutput input) 
{
	PSOutput output = PSTwoTextureOnly(input);
	
	float4 normal = normalize(input.WNormal);
	float4 v = normalize(xCameraPosition - input.WPosition);
	float4 l1 = normalize(L1Position - input.WPosition);
	float dist1 = length(L1Position - input.WPosition);
	float4 l2 = normalize(L2Position - input.WPosition);
	float dist2 = length(L2Position - input.WPosition);

	output.Color = output.Color * AmbientColor;
	if (L1On)
		output.Color = ApplyPointLight(output.Color, l1, dist1, normal, v, 1);
	if (L2On)
		output.Color = ApplyPointLight(output.Color, l2, dist2, normal, v, 2);
	return output;
}

//--Techniques------------
technique SingleTextured
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 VSTexture();
		PixelShader = compile ps_4_0 PSTextureOnly();
	}
};

technique DoubleTextured
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 VSTexture();
		PixelShader = compile ps_4_0 PSTwoTextureOnly();
	}
};

technique PointLightsOnly
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 VSColor();
		PixelShader = compile ps_4_0 PSColor();
	}
}

technique TexturedPointLight 
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 VSTexture();
		PixelShader = compile ps_4_0 PSTextureAndLight();
	}
};