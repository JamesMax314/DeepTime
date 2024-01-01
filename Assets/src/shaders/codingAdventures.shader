Shader "Custom/Terrain" {
    Properties {
        _GrassColour ("Grass Colour", Color) = (0,1,0,1)
        _RockColour ("Rock Colour", Color) = (1,1,1,1)
        _GrassSlopeThreshold ("Grass Slope Threshold", Range(0,1)) = .5
        _GrassBlendAmount ("Grass Blend Amount", Range(0,1)) = .5
        _BumpMap ("Bumpmap", 2D) = "bump" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        struct Input {
            float2 uv_BumpMap;
            float3 worldPos;
            float3 worldNormal;
            float4 color : COLOR;
            //INTERNAL_DATA
        };

        half _MaxHeight;
        half _GrassSlopeThreshold;
        half _GrassBlendAmount;
        fixed4 _GrassColour;
        fixed4 _RockColour;

        sampler2D _MainTex;
        sampler2D _BumpMap;

        void surf (Input IN, inout SurfaceOutputStandard o) {
            float slope = 1-IN.worldNormal.y; // slope = 0 when terrain is completely flat
            float grassBlendHeight = _GrassSlopeThreshold * (1-_GrassBlendAmount);
            float grassWeight = 1-saturate((slope-grassBlendHeight)/(_GrassSlopeThreshold-grassBlendHeight));
            o.Albedo = _GrassColour * grassWeight + _RockColour * (1-grassWeight);
            if (IN.color.r != 0 || IN.color.g != 0 || IN.color.b != 0)
            {
                o.Albedo = IN.color;
            }
            //o.Normal = tex2D(_BumpMap, IN.uv_BumpMap);
        }
        ENDCG
    }
}