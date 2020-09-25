#include "AutoLight.cginc"
#include "UnityPBSLighting.cginc"
#include "UnityCG.cginc"

fixed4 _VPnormal;
fixed4 _VPpos;
fixed4 _TopHPPos;
fixed4 _BottomHPPos;
float _FadeDist;

float screenSpaceAlpha(float3 worldPos)
{
    float dist = length(worldPos - _WorldSpaceCameraPos);
    return saturate(dist / _FadeDist);
}

float stippling(float3 worldPos, float2 scrPos)
{
    const float4x4 thresholdMatrix = 
    {
        1, 9, 3, 11,
        13, 5, 15, 7,
        4, 12, 2, 10,
        16, 8, 14, 6
    };

    float2 pixelPos = scrPos * _ScreenParams.xy;
    float threshold = thresholdMatrix[pixelPos.x % 4][pixelPos.y % 4] / 17;

    return screenSpaceAlpha(worldPos) - threshold;
}

float custom_discard(float3 worldPos)
{
    float3 jvec1 = worldPos - _VPpos.xyz;
    float3 jvec2 = worldPos - _TopHPPos.xyz;
    float3 jvec3 = worldPos - _BottomHPPos.xyz;

    float jv1 = dot(jvec1, _VPnormal.xyz);
    float jv2 = dot(jvec2, float3(0, 1, 0));
    float jv3 = dot(jvec3, float3(0, -1, 0));

    return step(jv1, 0) * step(jv2, 0) * step(jv2, 0) - 0.01;
}

float3 CreateBinormal (float3 normal, float3 tangent, float binormalSign) {
	return cross(normal, tangent.xyz) *
		(binormalSign * unity_WorldTransformParams.w);
}