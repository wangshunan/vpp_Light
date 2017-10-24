Shader "2D Dynamic Lights/Gradient/ZFighting" {

	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_UVXOffset ("UV X Offset", float) = 0
		_UVYOffset ("UV Y Offset", float) = 0
		_UVXScale ("UV X Scale", float) = 1.0
		_UVYScale ("UV Y Scale", float) = 1.0
		_Offset ("Offset", float) = 0
	}    
	

	SubShader {
		//ZTest always
		//Tags { Queue = Transparent }

		Tags{ "RenderType" = "Opaque" "Queue" = "Transparent-100" }
		LOD 200

		Stencil{
			Ref 1
			Pass Replace
		}
	

		Pass { 

			
			Cull Off			
			
			ZWrite On			
			AlphaTest Off
			Lighting Off
			ColorMask RGBA
			
			Blend SrcAlpha OneMinusSrcAlpha
								
						

			CGPROGRAM
			#pragma target 3.0
			#pragma fragment frag
			#pragma vertex vert	
			#include "UnityCG.cginc"


			uniform fixed4 _Color;
			uniform float _UVXOffset;
			uniform float _UVYOffset;
			uniform float _UVXScale;
			uniform float _UVYScale;
			uniform float _Offset;
		

			struct AppData {
				float4 vertex : POSITION;
				half2 texcoord : TEXCOORD0;

			};

			struct Input {
				float2 uv_MainTex;
				float2 worldPos;
			};

			struct VertexToFragment {
				float4 pos : POSITION;	
				half2 uv : TEXCOORD0;
			};

			//Vertex shader
			VertexToFragment vert(AppData v) {
				VertexToFragment o;
				o.pos = UnityObjectToClipPos(v.vertex);

				o.uv = half2((v.texcoord.x+_UVXOffset)*_UVXScale,(v.texcoord.y+_UVYOffset)*_UVYScale);

				return o;
			}


			fixed4 frag(VertexToFragment i) : COLOR {			
				return fixed4(lerp(_Color,fixed4(_Color.rgb,0),sqrt( (i.uv.x*i.uv.x)+(i.uv.y*i.uv.y) )+_Offset ));
			}

			ENDCG

		}
	}
}