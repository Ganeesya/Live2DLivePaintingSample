Shader "Unlit/maskedPen"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
        _MaskTex ("MaskTex", 2D) = "white" {}
        
		_TargetWidth("target width", float)		= 512.0
		_TargetHeight("target height", float)		= 512.0
		
		_FixColor ("Main Color", Color) = (1,.5,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100
		Lighting Off
		ZWrite   Off
		Blend    SrcAlpha OneMinusSrcAlpha , One OneMinusSrcAlpha 

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma alpha:fade

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
//                float2 localPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _MaskTex;
            float4 _MainTex_ST;
            float _TargetWidth;
            float _TargetHeight;
            fixed4 _FixColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
//                o.localPos.x = v.vertex.x / 1.0;
//                o.localPos.y = v.vertex.z / 1.0;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col  = tex2D(_MainTex, i.uv);
                fixed4 mask = tex2D(_MaskTex, float2( i.vertex.x / _TargetWidth, i.vertex.y / _TargetHeight));
                col = col * _FixColor;
                col.a = col.a * mask.a;
                // apply fog
                return col;
            }
            ENDCG
        }
    }
}
