#ifndef UNIVERSAL_TESSELLATION_FORWARD_LIT_PASS_INCLUDED
    #define UNIVERSAL_TESSELLATION_FORWARD_LIT_PASS_INCLUDED

    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GeometricTools.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Tessellation.hlsl"
    #include "./CustomFunction.hlsl"

    float4 _VerticalPlane;
    float4 _VerticalNormal; 
    float4 _TopPlane;
    float4 _BottomPlane;
    float _FadeDistance;

    struct Attributes
    {
        float4 positionOS   : POSITION;
        float3 normalOS     : NORMAL;
        float4 tangentOS    : TANGENT;
        float2 texcoord     : TEXCOORD0;
        float2 lightmapUV   : TEXCOORD1;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct VaryingsToDS
    {
        float3 positionWS   : INTERNALTESSPOS;
        float3 normalWS     : NORMAL;
        #ifdef _NORMALMAP
            float4 tangentWS    : TANGENT;
        #endif
        float2 texcoord     : TEXCOORD0;
        float2 lightmapUV   : TEXCOORD1;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct VaryingsToVS
    {
        float3 positionWS   : TEXCOORD0;
        float3 normalWS     : TEXCOORD1;
        #ifdef _NORMALMAP
            float4 tangentWS    : TEXCOORD2;
        #endif
        float2 texcoord     : TEXCOORD4;
        float2 lightmapUV   : TEXCOORD5;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct Varyings
    {
        float2 uv                       : TEXCOORD0;
        DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);

        #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
            float3 positionWS               : TEXCOORD2;
        #endif

        #ifdef _NORMALMAP
            float4 normalWS                 : TEXCOORD3;    // xyz: normal, w: viewDir.x
            float4 tangentWS                : TEXCOORD4;    // xyz: tangent, w: viewDir.y
            float4 bitangentWS              : TEXCOORD5;    // xyz: bitangent, w: viewDir.z
        #else
            float3 normalWS                 : TEXCOORD3;
            float3 viewDirWS                : TEXCOORD4;
        #endif

        half4 fogFactorAndVertexLight   : TEXCOORD6; // x: fogFactor, yzw: vertex light

        #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            float4 shadowCoord              : TEXCOORD7;
        #endif

        float4 screenPos : TEXCOORD8;

        float4 positionCS               : SV_POSITION;
        UNITY_VERTEX_INPUT_INSTANCE_ID
        UNITY_VERTEX_OUTPUT_STEREO
    };

    void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
    {
        inputData = (InputData)0;

        #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
            inputData.positionWS = input.positionWS;
        #endif

        #ifdef _NORMALMAP
            half3 viewDirWS = half3(input.normalWS.w, input.tangentWS.w, input.bitangentWS.w);
            inputData.normalWS = TransformTangentToWorld(normalTS,
            half3x3(input.tangentWS.xyz, input.bitangentWS.xyz, input.normalWS.xyz));
        #else
            half3 viewDirWS = input.viewDirWS;
            inputData.normalWS = input.normalWS;
        #endif

        inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
        viewDirWS = SafeNormalize(viewDirWS);
        inputData.viewDirectionWS = viewDirWS;

        #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            inputData.shadowCoord = input.shadowCoord;
        #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
            inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
        #else
            inputData.shadowCoord = float4(0, 0, 0, 0);
        #endif

        inputData.fogCoord = input.fogFactorAndVertexLight.x;
        inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
        inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);
    }

    ///////////////////////////////////////////////////////////////////////////////
    //                  Vertex and Fragment functions                            //
    ///////////////////////////////////////////////////////////////////////////////

    VaryingsToDS PackAttributesToDS(Attributes input)
    {
        VaryingsToDS output;
        UNITY_TRANSFER_INSTANCE_ID(input, output);

        output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
        output.normalWS = TransformObjectToWorldNormal(input.normalOS);
        #ifdef _NORMALMAP
            output.tangentWS.xyz = TransformObjectToWorldDir(input.tangentOS.xyz);
            output.tangentWS.w = input.tangentOS.w;
        #endif
        output.texcoord = input.texcoord;
        output.lightmapUV = input.lightmapUV;

        return output;
    }

    VaryingsToVS InterpolateWithBaryCoordsToDS(VaryingsToDS input0, VaryingsToDS input1, VaryingsToDS input2, float3 baryCoords)
    {
        VaryingsToVS output;

        TESSELLATION_INTERPOLATE_BARY(positionWS, baryCoords);
        TESSELLATION_INTERPOLATE_BARY(normalWS, baryCoords);
        #ifdef _NORMALMAP
            TESSELLATION_INTERPOLATE_BARY(tangentWS, baryCoords);
        #endif
        TESSELLATION_INTERPOLATE_BARY(texcoord, baryCoords);
        TESSELLATION_INTERPOLATE_BARY(lightmapUV, baryCoords);

        return output;
    }

    // Used in Standard (Physically Based) shader
    Varyings TessellationVertex(VaryingsToVS input)
    {
        Varyings output = (Varyings)0;

        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_TRANSFER_INSTANCE_ID(input, output);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
        
        //Displacement
        #ifdef _NORMALMAP
            //SimpleDisplcaement(input.positionWS);
            FullDisplacement(input.positionWS, input.normalWS, input.tangentWS);
        #endif
        //End Displacement

        float4 positionCS = TransformWorldToHClip(input.positionWS);

        half3 viewDirWS = GetCameraPositionWS() - input.positionWS;
        half3 vertexLight = VertexLighting(input.positionWS, input.normalWS);
        half fogFactor = ComputeFogFactor(positionCS.z);

        output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
        output.screenPos = ComputeScreenPos(positionCS);

        #ifdef _NORMALMAP
            output.normalWS = half4(input.normalWS, viewDirWS.x);
            output.tangentWS = half4(input.tangentWS.xyz, viewDirWS.y);

            half sign = input.tangentWS.w * GetOddNegativeScale();
            half3 bitangentWS = cross(input.normalWS, input.tangentWS.xyz) * sign;
            output.bitangentWS = half4(bitangentWS, viewDirWS.z);
        #else
            output.normalWS = NormalizeNormalPerVertex(input.normalWS);
            output.viewDirWS = viewDirWS;
        #endif

        OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
        OUTPUT_SH(output.normalWS.xyz, output.vertexSH);

        output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

        #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
            output.positionWS = input.positionWS;
        #endif

        #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            VertexPositionInputs vertexInput;
            vertexInput.positionWS = input.positionWS;
            output.shadowCoord = GetShadowCoord(vertexInput);
        #endif

        output.positionCS = positionCS;

        return output;
    }

    float screenSpaceAlpha(float3 worldPos)
    {
        float dist = length(worldPos - _WorldSpaceCameraPos);
        return saturate(dist / _FadeDistance);
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
        float3 jvec1 = worldPos - _VerticalPlane.xyz;
        float3 jvec2 = worldPos - _TopPlane.xyz;
        float3 jvec3 = worldPos - _BottomPlane.xyz;

        float jv1 = dot(jvec1, _VerticalNormal.xyz);
        float jv2 = dot(jvec2, float3(0, 1, 0));
        float jv3 = dot(jvec3, float3(0, -1, 0));

        return step(jv1, 0) * step(jv2, 0) * step(jv2, 0) - 0.01;
    }

    // Used in Standard (Physically Based) shader
    half4 LitPassFragment(Varyings input) : SV_Target
    {
        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

        #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
            clip(custom_discard(input.positionWS));
            clip(stippling(input.positionWS, input.screenPos.xy));
        #endif

        SurfaceData surfaceData;
        InitializeStandardLitSurfaceData(input.uv, surfaceData);

        InputData inputData;
        InitializeInputData(input, surfaceData.normalTS, inputData);

        half4 color = UniversalFragmentPBR(inputData, surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness, surfaceData.occlusion, surfaceData.emission, surfaceData.alpha);

        color.rgb = MixFog(color.rgb, inputData.fogCoord);
        return color;
    }

#endif