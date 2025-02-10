Shader "UI/SmoothCircleMask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (1,1,1,1)
        _GlowPower ("Glow Power", Range(0,10)) = 2
        _GlowWidth ("Glow Width", Range(0,1)) = 0.05
    }
    SubShader
    {
        Tags {"Queue"="Overlay" "IgnoreProjector"="True" "RenderType"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off

        Pass    
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            fixed4 _GlowColor;
            float _GlowPower;
            float _GlowWidth;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv - float2(0.5, 0.5);
                float dist = length(uv);    // 중앙으로부터 uv의 거리리
                
                float alpha = smoothstep(0.5, 0.48, dist);
                
                // 글로우 효과 (바깥쪽으로 희미해지는 그라데이션)
                float glow = smoothstep(0.5, 0.5 + _GlowWidth, dist) * 
                            step(0.48, dist); // 원 내부 제외
                glow = pow(1.0 - glow, _GlowPower) * 3.0;
                
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // 원 외곽에서 바깥쪽으로 글로우 적용
                float isEdge = step(0.5, dist) * smoothstep(0.5 + _GlowWidth + 0.1, 0.5, dist);
                col.rgb = lerp(col.rgb, _GlowColor.rgb, saturate(glow * isEdge) * _GlowColor.a);
                col.a *= alpha;
                return col;
            }
            ENDCG
        }
    }
}
