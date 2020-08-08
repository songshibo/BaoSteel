#ifndef UTILITY_SHADER_FUNCTION
#define UTILITY_SHADER_FUNCTION

const float4x4 thresholdMatrix =
{
	1, 9, 3, 11,
	13, 5, 15, 7,
	4, 12, 2, 10,
	16, 8, 14, 6
};

void Stippling_float(float3 worldPos, float4 screenPos, float fadeDist, out float Out)
{
	float2 pixelPos = screenPos.xy * _ScreenParams.xy;
	float threshold = thresholdMatrix[pixelPos.x % 4][pixelPos.y % 4] / 17;

	float dist = length(worldPos - _WorldSpaceCameraPos);
	dist = saturate(dist / fadeDist);

	Out = dist - threshold;
}

void CustomDiscard_float(float3 worldPos, float3 verticalPlane, float3 verticalNormal, float3 TopPlane, float3 BottomPlane, out float Out)
{
	float3 jvec1 = worldPos - verticalPlane;
	float3 jvec2 = worldPos - TopPlane;
	float3 jvec3 = worldPos - BottomPlane;

	float jv1 = dot(jvec1, verticalNormal);
	float jv2 = dot(jvec2, float3(0, 1, 0));
	float jv3 = dot(jvec3, float3(0, -1, 0));

	Out = step(jv1, 0) * step(jv2, 0) * step(jv2, 0) - 0.01;
}

#endif