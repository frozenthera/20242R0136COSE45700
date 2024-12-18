Shader "Custom/DashedOutline"
{
    Properties
    {
        _Color ("Outline Color", Color) = (0, 0, 0, 1)
        _DashSize ("Dash Size", Float) = 0.1
        _OutlineWidth ("Outline Width", Float) = 0.05
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Transparent"}
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite ON
        Cull Off
        ColorMask RGB

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2g
            {
                float4 pos : POSITION;
                float3 worldPos : TEXCOORD0;
            };

            struct g2f
            {
                float4 pos : POSITION;
                float dashPattern : TEXCOORD0;
            };

            float _DashSize;
            float _OutlineWidth;
            fixed4 _Color;

            v2g vert(appdata v)
            {
                v2g o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            // Geometry shader to create quads for thick lines
            [maxvertexcount(6)]
            void geom(line v2g p[2], inout TriangleStream<g2f> triStream)
{
                // Calculate edge direction and normal
                float3 edgeDir = normalize(p[1].worldPos - p[0].worldPos);
                float3 up = float3(0, 1, 0); // Up vector for 2D meshes; adjust if needed
                float3 normal = normalize(cross(edgeDir, up)) * _OutlineWidth;

                // Setup quad vertices
                g2f o1, o2, o3, o4;
                o1.pos = UnityWorldToClipPos(p[0].worldPos + normal);
                o2.pos = UnityWorldToClipPos(p[1].worldPos + normal);
                o3.pos = UnityWorldToClipPos(p[1].worldPos - normal);
                o4.pos = UnityWorldToClipPos(p[0].worldPos - normal);

                // Pass dash pattern info based on length along the edge
                o1.dashPattern = 0.0;
                o2.dashPattern = length(p[1].worldPos - p[0].worldPos);
                o3.dashPattern = o2.dashPattern;
                o4.dashPattern = 0.0;

                // Emit vertices for two triangles
                triStream.Append(o1);
                triStream.Append(o2);
                triStream.Append(o3);
                triStream.Append(o3);
                triStream.Append(o4);
                triStream.Append(o1);
}

            fixed4 frag(g2f i) : SV_Target
            {
                // Create dashed pattern based on distance along the edge
                float dash = fmod(i.dashPattern, _DashSize * 2.0) < _DashSize ? 1.0 : 0.0;
                return dash * _Color;
            }
            ENDCG
        }
    }
}
