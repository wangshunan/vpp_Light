Shader "Custom/LightTest" {
	SubShader{
		Tags{ "Queue" = "Transparent" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard alpha:fade 
		#pragma target 3.0

		struct Input {
			float2 uv_MainTex;
		};

		void surf(Input IN, inout SurfaceOutputStandard o) {
			o.Albedo = fixed4( 1, 1, 1, 1);
			o.Alpha = 0.1;
		}
		ENDCG
		}
		FallBack "Diffuse"
}
