﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Particle"
{
	Properties{
	 _TintColor("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	 _MainTex("Particle Texture", 2D) = "white" {}
	 _FadeDistance("Soft Particles Factor", Range(0.01,3.0)) = 1.0
	 _VerticalNormal("Vertical Clip Plane Normal", Vector) = (0, 0, 0, 0)
	 _VerticalPlane("Vertical Clip Plane Pos", Vector) = (0, 0, 0, 0)
	 _TopPlane("Top Horizontal Clip Plane Pos(norm:(0,1,0))", Vector) = (0, 200, 0, 0)
	 _BottomPlane("Bottom Horizontal Clip Plane Pos(norma:(0,-1,0))", Vector) = (0, -200, 0, 0)
	}

		Category{
			Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
			Blend SrcAlpha One
			AlphaTest Greater .01
			ColorMask RGB
			Cull Off Lighting Off ZWrite Off Fog { Color(0,0,0,0) }
			BindChannels {
				Bind "Color", color
				Bind "Vertex", vertex
				Bind "TexCoord", texcoord
			}

		 // ---- Fragment program cards
		 SubShader {
			 Pass {

				 CGPROGRAM
				 #pragma vertex vert
				 #pragma fragment frag
				 #pragma fragmentoption ARB_precision_hint_fastest
				 #pragma multi_compile_particles

				 #include "UnityCG.cginc"
				 #include "../GLSLFunctions/UtilityShaderFunction.cginc"

				 sampler2D _MainTex;
				 fixed4 _TintColor;
				 fixed4 _VerticalNormal;
				 fixed4 _VerticalPlane;
				 fixed4 _TopPlane;
				 fixed4 _BottomPlane;

				 struct appdata_t {
					 float4 vertex : POSITION;
					 fixed4 color : COLOR;
					 float2 texcoord : TEXCOORD0;
				 };

				 struct v2f {
					 float4 vertex : POSITION;
					 fixed4 color : COLOR;
					 float2 texcoord : TEXCOORD0;
					 #ifdef SOFTPARTICLES_ON
					 float4 projPos : TEXCOORD1;
					 #endif
					 float3 worldPos : TEXCOORD2;
				 };

				 float4 _MainTex_ST;

				 v2f vert(appdata_t v)
				 {
					 v2f o;
					 o.vertex = UnityObjectToClipPos(v.vertex);
					 #ifdef SOFTPARTICLES_ON
					 o.projPos = ComputeScreenPos(o.vertex);
					 COMPUTE_EYEDEPTH(o.projPos.z);
					 #endif
					 o.color = v.color;
					 o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
					 o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
					 return o;
				 }

				 sampler2D _CameraDepthTexture;
				 float _FadeDistance;

				 fixed4 frag(v2f i) : COLOR
				 {
					 float clipping;
					 CustomDiscard_float(i.worldPos, _VerticalPlane.xyz, _VerticalNormal.xyz, _TopPlane.xyz, _BottomPlane.xyz, clipping);
					 clip(clipping);

					 #ifdef SOFTPARTICLES_ON
					 float sceneZ = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos))));
					 float partZ = i.projPos.z;
					 float fade = saturate(_FadeDistance * (sceneZ - partZ));
					 i.color.a *= fade;
					 #endif

					 return 2.0f * i.color * _TintColor * tex2D(_MainTex, i.texcoord);
				 }
				 ENDCG
			 }
		 }

		 // ---- Dual texture cards
		 SubShader {
			 Pass {
				 SetTexture[_MainTex] {
					 constantColor[_TintColor]
					 combine constant * primary
				 }
				 SetTexture[_MainTex] {
					 combine texture * previous DOUBLE
				 }
			 }
		 }

					 // ---- Single texture cards (does not do color tint)
					 SubShader {
						 Pass {
							 SetTexture[_MainTex] {
								 combine texture * primary
							 }
						 }
					 }
	 }
}
