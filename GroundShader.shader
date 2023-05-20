Shader "Custom/GroundShader"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        _AltColor("Alt Color", Color) = (0, 0, 0, 1)
        _Flag("Flag", Range(0, 1)) = 0
        _MainTex("Main Tex", 2D) = "white"
        _AltTex("Alt Tex", 2D) = "white"
    }

        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID // use this to access instanced properties in the fragment shader.
            };

            sampler2D _MainTex;
            sampler2D _AltTex;

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
                UNITY_DEFINE_INSTANCED_PROP(float4, _AltColor)
                UNITY_DEFINE_INSTANCED_PROP(float, _Flag)
            UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
             UNITY_SETUP_INSTANCE_ID(i);
            float f = UNITY_ACCESS_INSTANCED_PROP(Props, _Flag);

            if (f == 0.0) {
                return tex2D(_MainTex, i.uv);
            }
            else {
                return tex2D(_AltTex, i.uv);

            }

            }
            ENDCG
        }
    }
}
