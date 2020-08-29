﻿// Copyright (C) 2019-2020 Alexander Bogarsukov. All rights reserved.
// See the LICENSE.md file in the project root for more information.

// Renders outline based on a texture produced with 'UnityF/OutlineColor'.
// Modified version of 'Custom/Post Outline' shader taken from https://willweissman.wordpress.com/tutorials/shaders/unity-shaderlab-object-outlines/.
Shader "Hidden/UnityFx/Outline"
{
	Properties
	{
		_Width("Outline thickness (in pixels)", Range(1, 32)) = 5
		_Intensity("Outline intensity", Range(0.1, 100)) = 2
		_Color("Outline color", Color) = (1, 0, 0, 1)
	}

	HLSLINCLUDE

		#include "UnityCG.cginc"

		CBUFFER_START(UnityPerMaterial)
			float _Intensity;
			int _Width;
			float4 _Color;
		CBUFFER_END

		UNITY_DECLARE_SCREENSPACE_TEXTURE(_MaskTex);
		UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
		float2 _MainTex_TexelSize;
		float _GaussSamples[32];

#if SHADER_TARGET < 35 || _USE_DRAWMESH

		v2f_img vert(appdata_img v)
		{
			v2f_img o;
			UNITY_INITIALIZE_OUTPUT(v2f_img, o);
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

			o.pos = float4(v.vertex.xy, UNITY_NEAR_CLIP_VALUE, 1);
			o.uv = ComputeScreenPos(o.pos);

			return o;
		}

#else

		struct appdata_vid
		{
			uint vertexID : SV_VertexID;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		float4 GetFullScreenTriangleVertexPosition(uint vertexID, float z)
		{
			// Generates a triangle in homogeneous clip space, s.t.
			// v0 = (-1, -1, 1), v1 = (3, -1, 1), v2 = (-1, 3, 1).
			float2 uv = float2((vertexID << 1) & 2, vertexID & 2);
			return float4(uv * 2 - 1, z, 1);
		}

		v2f_img vert(appdata_vid v)
		{
			v2f_img o;
			UNITY_INITIALIZE_OUTPUT(v2f_img, o);
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

			o.pos = GetFullScreenTriangleVertexPosition(v.vertexID, UNITY_NEAR_CLIP_VALUE);
			o.uv = ComputeScreenPos(o.pos);

			return o;
		}

#endif

		float CalcIntensity(float2 uv, float2 offset)
		{
			float intensity = 0;

			// Accumulates horizontal or vertical blur intensity for the specified texture position.
			// Set offset = (tx, 0) for horizontal sampling and offset = (0, ty) for vertical.
			for (int k = -_Width; k <= _Width; ++k)
			{
				intensity += UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, uv + k * offset).r * _GaussSamples[abs(k)];
			}

			return intensity;
		}

		float4 frag_h(v2f_img i) : SV_Target
		{
			float intensity = CalcIntensity(i.uv, float2(_MainTex_TexelSize.x, 0));
			return float4(intensity, intensity, intensity, 1);
		}

		float4 frag_v(v2f_img i) : SV_Target
		{
			if (UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MaskTex, i.uv).r > 0)
			{
				// TODO: Avoid discard/clip to improve performance on mobiles.
				discard;
			}

			float intensity = CalcIntensity(i.uv, float2(0, _MainTex_TexelSize.y));
			intensity = _Intensity > 99 ? step(0.01, intensity) : intensity * _Intensity;
			return float4(_Color.rgb, saturate(_Color.a * intensity));
		}

	ENDHLSL

	// SM3.5+
	SubShader
	{
		Cull Off
		ZWrite Off
		ZTest Always
		Lighting Off

		// 0) HPass
		Pass
		{
			Name "HPass"

			HLSLPROGRAM

			#pragma target 3.5
			#pragma shader_feature_local _USE_DRAWMESH
			#pragma vertex vert
			#pragma fragment frag_h

			ENDHLSL
		}

		// 1) VPass
		Pass
		{
			Name "VPassBlend"
			Blend SrcAlpha OneMinusSrcAlpha

			HLSLPROGRAM

			#pragma target 3.5
			#pragma shader_feature_local _USE_DRAWMESH
			#pragma vertex vert
			#pragma fragment frag_v

			ENDHLSL
		}
	}

	// SM2.0
	SubShader
	{
		Cull Off
		ZWrite Off
		ZTest Always
		Lighting Off

		// 0) HPass
		Pass
		{
			Name "HPass"

			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag_h

			ENDHLSL
		}

		// 1) VPass
		Pass
		{
			Name "VPassBlend"
			Blend SrcAlpha OneMinusSrcAlpha

			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag_v

			ENDHLSL
		}
	}
}
