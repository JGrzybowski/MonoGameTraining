//----------------
//Constants
//----------------
float4x4 xWorld, xView, xProjection, xWorldIT;
float4 xCameraPosition;
float4 AmbientColor;
//PointLight1
float4 L1Position, L2Position;
float3 L1DColor, L2DColor;
float4 L1SColor, L2SColor;
float L1Range, L2Range;
//----------------

//----------------
//Structures
//----------------
struct VertexShaderIn {
	float4 Position : SV_POSITION0;
	float4 Color : COLOR0;
	float4 Normal : NORMAL0;
};

struct VertexShaderOut {
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
VertexShaderOut VSmain (VertexShaderIn input) {
	VertexShaderOut output = (VertexShaderOut)0;
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
						float3 diffuseColor, float3 specularColor, int specularPower)
{
	float3 diffuse = dot(normal,light) * diffuseColor;

	float4 reflection = reflect(-light, normal);
	float3 specular = pow(dot(normal, reflection), abs(specularPower)) * specularColor;

	return float4(saturate(diffuse) + saturate(specular), 1);
}

PixelShaderOut PSmain(VertexShaderOut input)
{
	PixelShaderOut output = (PixelShaderOut)0;

	float4 normal = normalize(input.WNormal);
	float4 v = normalize(xCameraPosition - input.WPosition);
	float4 l1 = normalize(L1Position - input.WPosition);

	float4 c1 = CalculateColor(l1, normal, v, L1DColor, L1SColor.xyz, (int)(L1SColor.w));
		
	output.Color = AmbientColor + c1;
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