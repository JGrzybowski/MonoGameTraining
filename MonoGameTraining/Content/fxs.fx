//Constants
float4x4 xWorld, xView, xProjection, WorldIT;
float4 xCameraPosition;
//PointLight1
float4 Light1Position;
float4 Light1Color;
float Light1Range;
float4 AmbientColor, Diffuse1Color;
float4 Specular1Color;


struct VertexShaderIn {
	float4 Position : SV_Position0;
	float4 Color : COLOR0;
	float4 Normal : NORMAL0;
};

struct VertexShaderOut {
	float4 Position : SV_Position;
	float4 WPosition : TEXCOORD0;
	float4 Color : COLOR0;
	float4 WNormal : TEXCOORD1;
};

struct PixelShaderOut {
	float4 Color : COLOR0;
};

VertexShaderOut VSmain (VertexShaderIn i) {
	VertexShaderOut o = (VertexShaderOut)0;
	
	o.WPosition = mul(i.Position, xWorld);
	o.WNormal = normalize(mul(i.Normal, xWorld));
	o.Position = mul(o.WPosition, xView);
	o.Position = mul(o.WPosition, xProjection);
	o.Color = i.Color; 

	return o;
}

PixelShaderOut PSmain(VertexShaderOut i)
{
	PixelShaderOut o = (PixelShaderOut) 0;
	float4 v = normalize(xCameraPosition- i.WPosition);

	float4 l1 = normalize(Light1Position - i.WPosition);
	float4 r1 = reflect(l1, i.WNormal);
	//float4 c1 = dot(i.WNormal, l1) * Diffuse1Color + pow(dot(i.WNormal, r1), Specular1Color.w) * Specular1Color;
		
	//o.color = ambient + c1;
	return o;
}

technique FirstTechnique
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 VSmain();
		PixelShader = compile ps_4_0 PSmain();
	}
}

//-------------

struct VertexToPixel
{
	float4 Position     : POSITION;
	float4 Color		: COLOR0;
	float3 Normal        : TEXCOORD1;
	float3 Position3D    : TEXCOORD2;
};

struct PixelToFrame
{
	float4 Color        : COLOR0;
};


VertexToPixel SimplestVertexShader(VertexShaderIn i)
{
	VertexToPixel Output = (VertexToPixel)0;

	Output.Position = mul(i.Position, xWorld);
	Output.Position = mul(Output.Position, xView);
	Output.Position = mul(Output.Position, xProjection);

	Output.Color = i.Color;
	Output.Normal = normalize(mul(i.Normal, (float3x3)xWorld));
	Output.Position3D = mul(i.Position, xWorld);

	return Output;
}

float DotProduct(float3 lightPos, float3 pos3D, float3 normal)
{
	float3 lightDir = normalize(pos3D - lightPos);
	return dot(-lightDir, normal);
}

PixelToFrame OurFirstPixelShader(VertexToPixel PSIn)
{
	PixelToFrame Output = (PixelToFrame)0;

	float diffuseLightingFactor = DotProduct(Light1Position, PSIn.Position3D, PSIn.Normal);
	diffuseLightingFactor = saturate(diffuseLightingFactor);
	diffuseLightingFactor *= Light1Range;

	float4 baseColor = PSIn.Color;
	Output.Color = baseColor*(diffuseLightingFactor + AmbientColor);

	return Output;
}

technique Simplest
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 SimplestVertexShader();
		PixelShader = compile ps_4_0 OurFirstPixelShader();
	}
}


//----------
VertexShaderOut PointVertexShader(VertexShaderIn input)
{
	VertexShaderOut Output = (VertexShaderOut)0;

	Output.Position = mul(input.Position, xWorld);
	Output.Position = mul(Output.Position, xView);
	Output.Position = mul(Output.Position, xProjection);

	Output.Color = input.Color;
	Output.WNormal = normalize(mul(input.Normal, xWorld));
	Output.WPosition = mul(input.Position, xWorld);

	return Output;
}

PixelShaderOut PointPixelShader(VertexShaderOut input)
{
	PixelShaderOut Output = (PixelShaderOut)0;
	float3 normal = normalize(input.WNormal);
	float3 toCameraVector = normalize(xCameraPosition - input.WPosition);

	float3 toLightVector = Light1Position - input.WPosition;

	//Tłumienie
	float attenuation = 1;
	//float attenuation =saturate(1 - dot(toLightVector/ Light1Range,toLightVector/ Light1Range));

	toLightVector = normalize(toLightVector);
	float3 diffuse = AmbientColor;

	float NdotL = saturate(dot(normal, toLightVector));
	diffuse += NdotL * diffuse * attenuation;
	
	float3 binormal = normalize(toCameraVector + toLightVector);
	float NdotH = saturate(dot(normal, binormal));

	float specular =0;
	if(NdotL != 0)
		specular += pow(NdotH, Specular1Color.w) * Specular1Color * attenuation;

	float3 finalColor = float3(0,0,0);
	finalColor += diffuse * Diffuse1Color;
	finalColor += Specular1Color.xyz * specular;
	
	Output.Color = float4(finalColor,1);
	return Output;	
}

technique Point
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 PointVertexShader();
		PixelShader = compile ps_4_0 PointPixelShader();
	}
}









