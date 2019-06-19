Shader "Smkgames/NoisyMask" {
	Properties{
		_MainTex("MainTex", 2D) = "white" {}
		_Thickness("Thickness", Range(0, 1)) = 0.25
		_NoiseRadius("Noise Radius", Range(0, 1)) = 1
		_CircleRadius("Circle Radius", Range(0, 1)) = 0.5
		_Speed("Speed", Float) = 0.5
	}
		SubShader{
			Tags {"Queue" = "Transparent" "IgnoreProjector" = "true" "RenderType" = "Transparent"}
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off

			Pass {
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
				#pragma target 3.0
				uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
				uniform float _Thickness,_NoiseRadius,_CircleRadius,_Speed;

				struct VertexInput {
					float4 vertex : POSITION;
					float2 texcoord0 : TEXCOORD0;
				};
				struct VertexOutput {
					float4 pos : SV_POSITION;
					float2 uv0 : TEXCOORD0;
					float4 posWorld : TEXCOORD1;

				};
				VertexOutput vert(VertexInput v) {
					VertexOutput o = (VertexOutput)0;
					o.uv0 = v.texcoord0;

					o.pos = UnityObjectToClipPos(v.vertex);
					o.posWorld = mul(unity_ObjectToWorld, v.vertex);
					return o;
				}
				float4 frag(VertexOutput i, float facing : VFACE) : COLOR {

					float2 uv = (i.uv0 * 2.0 + -1.0); // Remapping uv from [0,1] to [-1,1]
					float circleMask = step(length(uv),_NoiseRadius); // Making circle by LENGTH of the vector from the pixel to the center
					float circleMiddle = step(length(uv),_CircleRadius); // Making circle by LENGTH of the vector from the pixel to the center
					float2 polaruv = float2(length(uv),((atan2(uv.g,uv.r) / 6.283185) + 0.5)); // Making Polar
					polaruv += _Time.y * _Speed / 10;
					float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(polaruv, _MainTex)); // BackGround Noise
					float Noise = (circleMask * step(_MainTex_var.r,_Thickness)); // Masking Background Noise
					float3 finalColor = float3(Noise,Noise,Noise);
					return fixed4(finalColor + circleMiddle,(finalColor + circleMiddle).r);
				}
				ENDCG
			}
		}
			FallBack "Diffuse"
}