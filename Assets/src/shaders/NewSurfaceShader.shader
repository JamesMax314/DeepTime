Shader "Unlit/NewUnlitShader"
{
    Properties
    {
        _LevelGrass ("Grass Level", Float) = 0.25
        _LevelMud ("Mud Level", Float) = 0.4
        _LevelStone ("Stone Level", Float) = 0.6
        _LevelSnow ("Snow Level", Float) = 0.75
        _BlendFactor ("BlendFactor", Float) = 0.5
        _PeakHeight ("PeakHeight", Float) = 1

        _Color ("Base Color", Color) = (1,1,1,1)
        _Metallic ("Metallic", Range (0, 1)) = 0.0
        _Glossiness ("Smoothness", Range (0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            // Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct v2f members worldPos)
            #pragma exclude_renderers d3d11
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : WORLD_POSITION;
                float3 normal : WORLD_NORMAL;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normal = mul(unity_ObjectToWorld, v.normal).xyz;
                return o;
            }

            float _LevelGrass;
            float _LevelMud;            
            float _LevelStone;
            float _LevelSnow;
            float _BlendFactor;
            float _PeakHeight;

            float4 _Color;
            float _Metallic;
            float _Glossiness;

            float gaussian(float x, float mean, float std)
            {
                return exp(-((x-mean) * (x-mean)) / (2.0 * std * std)) / (sqrt(2.0 * 3.14159265358979323846) * std);
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float3 colourSnow = float3(255, 255, 255)/255;
                float3 colourStone = float3(127, 131, 134)/255;
                float3 colourMud = float3(136, 104, 6)/255;
                float3 colourGrass = float3(124, 252, 0)/255;

                float rockScale = 1;

                float rockStrength = rockScale*i.worldPos[1]; // Calculate how rocky based on height
                float seaStrength = -rockStrength;

                float x = i.worldPos.y/_PeakHeight;
                float3 col = (colourGrass*gaussian(x, _LevelGrass, _BlendFactor)/2
                + colourMud*gaussian(x, _LevelMud, _BlendFactor)/2 
                + colourStone*gaussian(x, _LevelStone, _BlendFactor)/2 
                + colourSnow*gaussian(x, _LevelSnow, _BlendFactor))/2;

                // Standard Lambertian diffuse lighting
                float3 lightDir = normalize(float3(0, 10000, 0) - i.worldPos);
                float ndotl = max(0.0, dot(i.normal, lightDir));

                // Calculate final color with diffuse and specular components
                col *= (_Color.rgb * ndotl);

                float4 returnColour = float4(col, 1);

                // Apply metallic factor
                returnColour.rgb = lerp(returnColour.rgb, returnColour.rgb * _Color.rgb, _Metallic);
                return returnColour;
            }

            ENDCG
        }
    }
}