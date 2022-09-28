Shader "Custom/SimpleColor"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Noise ("Texture", 2D) = "white" {}
        _Color ("Texture Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }

        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
                #include "UnityCG.cginc"

                #pragma vertex vert
                #pragma fragment frag

                sampler2D _MainTex;
                sampler2D _Noise;
                float4 _MainTex_ST;
                float4 _Color;

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                v2f vert (appdata v)
                {
                    v2f o;
                    o.vertex =  UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                    return o;
                }

                float4 frag (v2f i) : SV_Target
                {
                    float4 noise = tex2D(_Noise, i.uv);

                    float2 translation = i.uv + float2(0.5, -0.5) * _Time.y * 0.2;

                    translation.x += noise.r;
                    
                    if(noise.g < 0.5)
                        translation.y -= noise.g;
                    else
                        translation.y += noise.g;

                    float4 color = tex2D(_MainTex, translation);
                    return float4(color.r, color.g, color.b, 0.8) * _Color;
                }
            ENDCG
        }
    }
}