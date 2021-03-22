#ifndef CUSTOM_FUNCTION_INCLUDED
    #define CUSTOM_FUNCTION_INCLUDED

    sampler2D _DisplacementMap;
    float4 _DisplacementMap_ST;
    float _DisplacementAmount;

    float _UVOffset;// 重新计算法线的uv偏移量
    float _MaxHeight;// 炉缸残厚部分的最大高度

    float4 GetAngleHeightUV(float3 posWS)
    {
        float angle = (degrees(atan2(posWS.x, posWS.z)) + 360.0) % 360.0;
        return float4(angle / 360.0, posWS.y / _MaxHeight, 0, 0);
    }

    void FullDisplacement(inout float3 positionWS, inout float3 normalWS, inout float4 tangentWS)
    {
        if(dot(normalWS, float3(0,1,0)) > 0.99)
        {
            return;
        }
        float3 dir = normalize(float3(positionWS.x, 0, positionWS.z));
        float3 modifiedPos = positionWS;
        float4 uv = GetAngleHeightUV(modifiedPos);
        modifiedPos += dir * _DisplacementAmount * tex2Dlod(_DisplacementMap, uv).r;

        float3 posTangent = positionWS + normalize(tangentWS.xyz) * _UVOffset;
        uv = GetAngleHeightUV(posTangent);
        posTangent += dir * _DisplacementAmount * tex2Dlod(_DisplacementMap, uv).r;

        float3 bbitangentWS = normalize(cross(normalWS, tangentWS.xyz));
        float3 posBiTangent = positionWS + bbitangentWS * _UVOffset;
        uv = GetAngleHeightUV(posBiTangent);
        posBiTangent += dir * _DisplacementAmount * tex2Dlod(_DisplacementMap, uv).r;

        float3 modifiedTangent = normalize(posTangent - modifiedPos);
        float3 modifiedBitangent = normalize(posBiTangent - modifiedPos);

        tangentWS = float4(modifiedTangent, tangentWS.w);
        normalWS = normalize(cross(modifiedTangent, modifiedBitangent));
        positionWS = modifiedPos;
    }

    void SimpleDisplcaement(inout float3 positionWS, float3 normalWS)
    {
        if(dot(normalWS, float3(0,1,0)) > 0.99)
        {
            return;
        }
        float3 dir = normalize(float3(positionWS.x, 0, positionWS.z));
        float4 uv = GetAngleHeightUV(positionWS);
        positionWS += dir * _DisplacementAmount * tex2Dlod(_DisplacementMap, uv).r;
    }
#endif