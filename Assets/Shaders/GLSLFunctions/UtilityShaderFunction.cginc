#ifndef UTILITY_SHADER_FUNCTION
	#define UTILITY_SHADER_FUNCTION

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

	void RenderTypeBranch_float(float type, float3 standard, float3 heatmap, float3 heatload, out float3 Out)
	{
		if(type == 0)
		{
			Out = standard;
		}
		else if(type == 1)
		{
			Out = heatmap;
		}
		else
		{
			Out = heatload;
		}
	}

#endif