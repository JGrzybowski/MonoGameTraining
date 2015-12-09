//----------------
//Constants
//----------------
float4x4 xWorld, xView, xProjection, xWorldIT;
float4 xCameraPosition;
float4 AmbientColor = float4(0.3,0.3,0.3,1);
//PointLights
float4 L1Position, L2Position, L3Position = float4(50, 10, 50, 1);
float3 L1DColor, L2DColor, L3DColor = float4(1,1,0,1);
float4 L1SColor, L2SColor, L3SColor = float4(1,1,1,1);
float L1Range, L2Range, L3Range = 25.0f;
float4 L3Direction = float4(0,-1,0,0);
bool L1On,L2On, L3On = true;
float L3Cone = 0.3f;
//Fog
float fogStart = 1;
float fogEnd = 100;
float4 fogColor = float4(1.0f, 1.0f, 1.0f, 0.8f);
//Textures
Texture2D tex1;
Texture2D tex2;
Texture2D Light3Tex;
sampler TextureSampler1 : register(s1);
sampler TextureSampler2 : register(s2);
sampler TextureSampler3 = sampler_state { texture= <Light3Tex>; magfilter = POINT; minfilter = POINT; mipfilter = POINT; AddressU = clamp; AddressV = clamp; };
//----------------

//--Structures----------
struct VSColorInput 
{
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
float4 MixColors(float4 c1, float4 c2)
{
	return float4((1 - c2.w)*c1+ c2.xyz * c2.w, 1);
}
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

float4 CalculateSpotlightColor(float4 light, float4 normal, float4 view, float4 lightDirection,
								float3 diffuseColor, float3 specularColor, int specularPower, float range, float distance)
{
	float spotScale = saturate(dot(light, normal));
	if ((L3Cone - spotScale) > 0)
		return float4(0, 0, 0, 0);

	float4 c = CalculateColor(light, normal, view, diffuseColor, specularColor, specularPower, range, distance);
	float4 t = Light3Tex.Sample(TextureSampler3, float2(0.5-spotScale, 0.5-spotScale));
	return MixColors(c, t);
}

float4 ApplyLight(float4 inColor, float4 light, float distance, float4 normal, float4 view, int lightN)
{
	if(lightN == 1)
		return inColor + CalculateColor(light, normal, view, L1DColor, L1SColor.xyz, (int)(L1SColor.w), L1Range, distance);
	if(lightN == 2)
		return inColor + CalculateColor(light, normal, view, L2DColor, L2SColor.xyz, (int)(L2SColor.w), L2Range, distance);
	if(lightN == 3)
		return inColor + CalculateSpotlightColor(light, normal, view, L3Direction, L3DColor, L3SColor.xyz, (int)(L3SColor.w), L3Range, distance);
}

float4 ApplyFog(float4 inColor, float4 position) 
{
	float fogFactor = saturate( (length(xCameraPosition - position) - fogStart) / (fogEnd - fogStart));
	float4 fog = fogFactor * fogColor;
	
	return MixColors(inColor, fog);
}

float4 ApplyTexture(float4 inColor, texture2D tex, sampler sam, float2 texCoordinates)
{
	float4 tColor = tex.Sample(sam, texCoordinates);
	return MixColors(inColor, tColor);
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
		output.Color = ApplyLight(output.Color, l1, dist1, normal, v, 1);
	if (L2On)
		output.Color = ApplyLight(output.Color, l2, dist2, normal, v, 2);
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
	float4 l3 = normalize(L3Position - input.WPosition);
	float dist3 = length(L3Position - input.WPosition);

	output.Color = output.Color * AmbientColor;
	if (L1On)
		output.Color = ApplyLight(output.Color, l1, dist1, normal, v, 1);
	if (L2On)
		output.Color = ApplyLight(output.Color, l2, dist2, normal, v, 2);
	if (L3On)
		output.Color = ApplyLight(output.Color, l3, dist3, normal, v, 3);
	return output;
}
PSOutput PSTextureFog(VSTextureOutput input)
{
	PSOutput output = PSTextureOnly(input);
	output.Color = ApplyFog(output.Color, input.WPosition);
	return output;
}
PSOutput PSTextureLightFog(VSTextureOutput input)
{
	PSOutput output = PSTextureAndLight(input);
	output.Color = ApplyFog(output.Color, input.WPosition);
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

technique TextureFog
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 VSTexture();
		PixelShader = compile ps_4_0 PSTextureFog();
	}
};

technique TextureLightFog
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 VSTexture();
		PixelShader = compile ps_4_0 PSTextureLightFog();
	}
};