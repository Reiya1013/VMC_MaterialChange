// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "AlphaOn" {
    Properties {
    }

    SubShader {
        Tags { 
            "Queue"="Transparent+1000"
            "RenderType"="Opaque"
            "LightMode" = "ForwardBase" 
            }

        Pass {
            Name "AlphaOn"
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha, DstAlpha SrcAlpha
            ZWrite On
            //ZTest Always
            ColorMask A

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma multi_compile_fwdbase
            #include "UnityCG.cginc"
            
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                //offset方向の計算
                float3 normal = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal));
                float2 offset = TransformViewToProjection(normal.xy);

                //OutLineとしてレンダリングされるポリゴン
                v2f o;  
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.vertex.xy = o.vertex.xy + offset * 1;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
               fixed4 o = fixed4(1,1,1,1);
               UNITY_APPLY_FOG(i.fogCoord, o);
               return o;
            }
            ENDCG
        }

   //     Pass {
   //         Name "2Pass"
   //         Cull Off
   //         Blend SrcAlpha One, SrcAlpha One
   //         ZWrite Off
   //         ColorMask A

   ////         CGPROGRAM
   ////         #pragma vertex vert
   ////         #pragma fragment frag
			////#pragma multi_compile_fwdbase
   ////         #include "UnityCG.cginc"
            
            
   ////         struct appdata
   ////         {
   ////             float4 vertex : POSITION;
   ////             float2 uv : TEXCOORD0;
   ////         };

   ////         struct v2f
   ////         {
   ////             UNITY_FOG_COORDS(1)
   ////             float4 vertex : SV_POSITION;
   ////         };

   ////         v2f vert (appdata v)
   ////         {
   ////             v2f o;
   ////             o.vertex = UnityObjectToClipPos(v.vertex);
   ////             UNITY_TRANSFER_FOG(o,o.vertex);
   ////             return o;
   ////         }

   ////         fixed4 frag (v2f i) : SV_Target
   ////         {
   ////            fixed4 o = fixed4(1,1,1,1);
   ////            return o;
   ////         }
   ////         ENDCG



   //         //Blend SrcAlpha SrcAlpha//, OneMinusDstAlpha SrcAlpha
   //         ////ZWrite Off
   //         ////ColorMask A

   //         CGPROGRAM
   //         #pragma vertex vert
   //         #pragma fragment frag
   //         #include "UnityCG.cginc"
            
            
   //         struct appdata
   //         {
   //             float4 vertex : POSITION;
   //             float3 normal : NORMAL;
   //             float2 uv : TEXCOORD0;
   //         };

   //         struct v2f
   //         {
   //             UNITY_FOG_COORDS(1)
   //             float4 vertex : SV_POSITION;
   //         };

   //         v2f vert (appdata v)
   //         {
   //             //offset方向の計算
   //             float3 normal = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal));
   //             float2 offset = TransformViewToProjection(normal.xy);

   //             //OutLineとしてレンダリングされるポリゴン
   //             v2f o;  
   //             o.vertex = UnityObjectToClipPos(v.vertex);
   //             o.vertex.xy = o.vertex.xy + offset * 1;
   //             UNITY_TRANSFER_FOG(o,o.vertex);
   //             return o;
   //         }

   //         fixed4 frag (v2f i) : SV_Target
   //         {
   //            fixed4 o = fixed4(1,1,1,1);
   //            UNITY_APPLY_FOG(i.fogCoord, o);
   //            return o;
   //         }
   //         ENDCG
        
   //    }
    }
    FallBack "Diffuse"
}