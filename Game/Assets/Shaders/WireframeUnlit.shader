// レトロワイヤーフレーム用の Unlit シェーダー(Built-in RP)。
// ジオメトリシェーダーで各三角形の重心座標を付与し、エッジ付近のみ描画する。
// fwidth を使わず固定閾値で判定し、広い環境で確実に動作させる。
Shader "ThreeDimensionShooter/WireframeUnlit"
{
    Properties
    {
        _WireColor ("Wire Color", Color) = (0, 1, 0.6, 1)
        _WireWidth ("Wire Width", Range(0.01, 0.3)) = 0.08
    }
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        Cull Off
        ZWrite On

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #pragma target 4.0
            #include "UnityCG.cginc"

            fixed4 _WireColor;
            float _WireWidth;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2g
            {
                float4 pos : SV_POSITION;
            };

            struct g2f
            {
                float4 pos : SV_POSITION;
                float3 bary : TEXCOORD0;
            };

            v2g vert (appdata v)
            {
                v2g o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            [maxvertexcount(3)]
            void geom(triangle v2g input[3], inout TriangleStream<g2f> triStream)
            {
                g2f o;
                o.pos = input[0].pos; o.bary = float3(1, 0, 0); triStream.Append(o);
                o.pos = input[1].pos; o.bary = float3(0, 1, 0); triStream.Append(o);
                o.pos = input[2].pos; o.bary = float3(0, 0, 1); triStream.Append(o);
                triStream.RestartStrip();
            }

            fixed4 frag (g2f i) : SV_Target
            {
                // 重心座標の最小成分が小さいほどエッジに近い
                float minBary = min(min(i.bary.x, i.bary.y), i.bary.z);
                // 固定閾値でエッジ判定(画面サイズ非依存)
                float wire = step(minBary, _WireWidth);
                if (wire < 0.5) discard; // エッジ以外は描画しない
                return _WireColor;
            }
            ENDCG
        }
    }
    Fallback Off
}
