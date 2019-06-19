Shader "Custom/DissolveBasedOnViewDistance" {
	Properties{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Center("Dissolve Center", Vector) = (0,0,0,0)
		_Interpolation("Dissolve Interpolation", Range(0,5)) = 0.8
		_DissTexture("Dissolve Texture", 2D) = "white" {}
	}

		SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200


			CGPROGRAM

		#pragma surface surf Standard vertex:vert addshadow

		#pragma target 3.0

		struct Input {
			float2 uv_MainTex;
			float2 uv_DissTexture;
			float3 worldPos;
			float viewDist;
		};



		sampler2D _MainTex;
		sampler2D _DissTexture;
		half _Interpolation;
		float4 _Center;


		// Computes world space view direction
		// inline float3 WorldSpaceViewDir( in float4 v )
		// {
		//     return _WorldSpaceCameraPos.xyz - mul(_Object2World, v).xyz;
		// }


		void vert(inout appdata_full v,out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input,o);

		 half3 viewDirW = WorldSpaceViewDir(v.vertex);
		 o.viewDist = length(viewDirW);

		}

		void surf(Input IN, inout SurfaceOutputStandard o) {


			float l = length(_Center - IN.worldPos.xyz);

			clip(saturate(IN.viewDist - l + (tex2D(_DissTexture, IN.uv_DissTexture) * _Interpolation * saturate(IN.viewDist))) - 0.5);

		 o.Albedo = tex2D(_MainTex,IN.uv_MainTex);
		}
		ENDCG
	}
		Fallback "Diffuse"
}