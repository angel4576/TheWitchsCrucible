Shader "Spine/Skeleton" {
	Properties {
		_Cutoff ("Shadow alpha cutoff", Range(0,1)) = 0.1
		[NoScaleOffset] _MainTex ("Main Texture", 2D) = "black" {}
		[Toggle(_STRAIGHT_ALPHA_INPUT)] _StraightAlphaInput("Straight Alpha Texture", Int) = 0
		[HideInInspector] _StencilRef("Stencil Reference", Float) = 1.0
		[HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Float) = 8 // Set to Always as default

		// Outline properties are drawn via custom editor.
		[HideInInspector] _OutlineWidth("Outline Width", Range(0,8)) = 3.0
		[HideInInspector][MaterialToggle(_USE_SCREENSPACE_OUTLINE_WIDTH)] _UseScreenSpaceOutlineWidth("Width in Screen Space", Float) = 0
		[HideInInspector] _OutlineColor("Outline Color", Color) = (1,1,0,1)
		[HideInInspector] _OutlineReferenceTexWidth("Reference Texture Width", Int) = 1024
		[HideInInspector] _ThresholdEnd("Outline Threshold", Range(0,1)) = 0.25
		[HideInInspector] _OutlineSmoothness("Outline Smoothness", Range(0,1)) = 1.0
		[HideInInspector][MaterialToggle(_USE8NEIGHBOURHOOD_ON)] _Use8Neighbourhood("Sample 8 Neighbours", Float) = 1
		[HideInInspector] _OutlineOpaqueAlpha("Opaque Alpha", Range(0,1)) = 1.0
		[HideInInspector] _OutlineMipLevel("Outline Mip Level", Range(0,3)) = 0
	
		_NoiseTex ("Noise Texture", 2D) = "white" {}
		// _NoiseScale ("Noise Scale", Range(0, 100)) = 1
		_DissolveThreshold ("Dissolve Threshold", Range(0, 2)) = 0.5 

		[HDR]_EdgeColor ("Edge Color", Color) = (1,1,1,1)
		_EdgeWidth ("Edge Width", Range(0, 1)) = 0.1
		_UVScale ("UV Scale", Range(0, 1)) = 1
		_UVOffset ("UV Offset", Range(0, 1)) = 0
	}

	SubShader {
		Tags {
			 "Queue"="Transparent" 
			 "IgnoreProjector"="True" 
			 "RenderType"="Transparent" 
			 "PreviewType"="Plane" 
			}

		Fog { Mode Off }
		Cull Off
		ZWrite Off
		//Blend One OneMinusSrcAlpha
		Blend SrcAlpha OneMinusSrcAlpha
		Lighting Off

		Stencil {
			Ref[_StencilRef]
			Comp[_StencilComp]
			Pass Keep
		}

		Pass {
			Name "Normal"

			CGPROGRAM
			#pragma shader_feature _ _STRAIGHT_ALPHA_INPUT
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "CGIncludes/Spine-Common.cginc"
			
			sampler2D _MainTex;

			sampler2D _NoiseTex;
			float4 _NoiseTex_ST;
			// float _NoiseScale;

			fixed4 _EdgeColor;
			fixed _EdgeWidth;

			fixed _DissolveThreshold;
			float _UVScale;
			float _UVOffset;

			struct VertexInput {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 vertexColor : COLOR;
			};

			struct VertexOutput {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				// float2 uvNoise : TEXCOORD1;
				float4 vertexColor : COLOR;
			};

			// Simple random noise 
			float random (float2 input) { 
				return frac(sin(dot(input, float2(12.9898,78.233)))* 43758.5453123);
			}	

			VertexOutput vert (VertexInput v) {
				VertexOutput o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				// o.uvNoise = TRANSFORM_TEX(v.uv, _NoiseTex);
				o.vertexColor = PMAGammaToTargetSpace(v.vertexColor);
				return o;
			}

			float4 frag (VertexOutput i) : SV_Target {
				float4 texColor = tex2D(_MainTex, i.uv);

				// adjust uv for noise
				float2 uvNoise = i.uv * _UVScale + _UVOffset;
				fixed noise = tex2D(_NoiseTex, uvNoise).r;

				float alpha = step(noise, _DissolveThreshold);


				#if defined(_STRAIGHT_ALPHA_INPUT)
					texColor.rgb *= texColor.a;
				#endif

				texColor.rgb = texColor.rgb * i.vertexColor.rgb; 

				fixed dissolveFactor = smoothstep(_DissolveThreshold - _EdgeWidth, _DissolveThreshold, noise);
                fixed3 lerpColor = lerp(texColor.rgb, _EdgeColor.rgb, dissolveFactor);

				float4 finalColor = float4(lerpColor, alpha);

				// return (texColor * i.vertexColor);
				return finalColor;
			}
			ENDCG
		}

		// Pass {
		// 	Name "Caster"
		// 	Tags { "LightMode"="ShadowCaster" }
		// 	Offset 1, 1
		// 	ZWrite On
		// 	ZTest LEqual

		// 	Fog { Mode Off }
		// 	Cull Off
		// 	Lighting Off

		// 	CGPROGRAM
		// 	#pragma vertex vert
		// 	#pragma fragment frag
		// 	#pragma multi_compile_shadowcaster
		// 	#pragma fragmentoption ARB_precision_hint_fastest
		// 	#include "UnityCG.cginc"
		// 	sampler2D _MainTex;
		// 	fixed _Cutoff;

		// 	struct VertexOutput {
		// 		V2F_SHADOW_CASTER;
		// 		float4 uvAndAlpha : TEXCOORD1;
		// 	};

		// 	VertexOutput vert (appdata_base v, float4 vertexColor : COLOR) {
		// 		VertexOutput o;
		// 		o.uvAndAlpha = v.texcoord;
		// 		o.uvAndAlpha.a = vertexColor.a;
		// 		TRANSFER_SHADOW_CASTER(o)
		// 		return o;
		// 	}

		// 	float4 frag (VertexOutput i) : SV_Target {
		// 		fixed4 texcol = tex2D(_MainTex, i.uvAndAlpha.xy);
		// 		clip(texcol.a * i.uvAndAlpha.a - _Cutoff);
		// 		SHADOW_CASTER_FRAGMENT(i)
		// 	}
		// 	ENDCG
		// }
	}
	CustomEditor "SpineShaderWithOutlineGUI"
}
