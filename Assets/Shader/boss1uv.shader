Shader "Spine/Skeleton" {
	Properties {
		_Cutoff ("Shadow alpha cutoff", Range(0,1)) = 0.1
		[NoScaleOffset] _MainTex ("Main Texture", 2D) = "black" {}
		[Toggle(_STRAIGHT_ALPHA_INPUT)] _StraightAlphaInput("Straight Alpha Texture", Int) = 0
		[HideInInspector] _StencilRef("Stencil Reference", Float) = 1.0
		[HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Float) = 8 // Set to Always as default

		[NoScaleOffset] _NoiseTex ("R: Noise1 G: Noise2 ", 2D) = "grey" {}
		_Mask ("R: inner flame G: outer flame a: alpha", 2D) = "blue" {}
        _Noise1Params ("X: scale Y: xspeed Z: yspeed W: intensity", vector) = (1, 0.03, 0.03, 1)
        _Noise2Params ("X: scale Y: xspeed Z: yspeed W: intensity", vector) = (1, 0.03, 0.03, 1)
		_DisturbDirection("Disturb Direction x y z", Vector) = (0, 0, 0, 0)
		[HDR]_OuterCol ("Outer Color", Color) = (1,1,1,1)

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
		
		[HideInInspector] _InvulnerabilityColor("Invulnerable Color", Color) = (1.0, 1.0, 1.0, 1.0)

		// uv displacement and distortion


	}

	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }

		Fog { Mode Off }
		Cull Off
		ZWrite Off
		Blend One OneMinusSrcAlpha
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
			#include "../Spine/Runtime/spine-unity/Shaders/CGIncludes/Spine-Common.cginc"
			
			sampler2D _MainTex;
            sampler2D _NoiseTex;
			sampler2D _Mask;

            uniform half3 _Noise1Params;
            uniform half3 _Noise2Params;
			uniform half4 _DisturbDirection;
            uniform float4 _InnerCol;
            uniform float4 _OuterCol;

			fixed4 _InvulnerabilityColor;

			struct VertexInput {
				float4 vertex : POSITION;
				float2 uv0 : TEXCOORD0;
				float4 vertexColor : COLOR;
			};

			struct VertexOutput {
				float4 pos : SV_POSITION;
				float2 uv0 : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float2 uv2 : TEXCOORD2;
				float4 vertexColor : COLOR;
			};

			VertexOutput vert (VertexInput v) {
				VertexOutput o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv0 = v.uv0; // 将原始 UV 传递给片段着色器
				o.vertexColor = PMAGammaToTargetSpace(v.vertexColor);
				return o;
			}

			float4 frag (VertexOutput i) : SV_Target {
				float4 texColor = tex2D(_MainTex, i.uv0);

				#if defined(_STRAIGHT_ALPHA_INPUT)
				texColor.rgb *= texColor.a;
				#endif
			
				// 从 Mask 的 G 通道采样控制强度
				float controlStrength = tex2D(_Mask, i.uv0).g;
			
				// 动态 UV 扰动（在片段着色器中计算）
				float2 noise1Offset = float2(_Time.x * 0.1 * _Noise1Params.y, _Time.x * 0.1 * _Noise1Params.z);
				float2 noise2Offset = float2(_Time.x * 0.1 * _Noise2Params.y, _Time.x * 0.1 * _Noise2Params.z);
			
				float var_Noise1 = tex2D(_NoiseTex, i.uv0 * _Noise1Params.x + noise1Offset).r;
				float var_Noise2 = tex2D(_NoiseTex, i.uv0 * _Noise2Params.x + noise2Offset).g;
			
				float2 disturbDir = normalize(float2(_DisturbDirection.x, _DisturbDirection.y));
				float projectedUV = dot(i.uv0, disturbDir);
				float noiseStrength = saturate(lerp(0, 0.5, projectedUV)) * controlStrength;
			
				float noise = (var_Noise1 * _Noise1Params.z + var_Noise2 * _Noise2Params.z) * noiseStrength;
			
				// 计算扰动后的 UV
				float2 warpUV = i.uv0 + disturbDir * noise;
			
				// 使用扰动后的 UV 采样
				float3 var_Mask = tex2D(_Mask, warpUV).rgb;
				float opacity = var_Mask.b; // A 通道控制不透明度
				float4 finalRGB = float4(var_Mask.r * texColor.rgb, opacity);
			
				// 颜色混合
				finalRGB.rgb *= _InvulnerabilityColor.rgb;
				return (finalRGB * i.vertexColor);
			}
			ENDCG
		}

		Pass {
			Name "Caster"
			Tags { "LightMode"="ShadowCaster" }
			Offset 1, 1
			ZWrite On
			ZTest LEqual

			Fog { Mode Off }
			Cull Off
			Lighting Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			sampler2D _MainTex;
			fixed _Cutoff;

			struct VertexOutput {
				V2F_SHADOW_CASTER;
				float4 uvAndAlpha : TEXCOORD1;
			};

			VertexOutput vert (appdata_base v, float4 vertexColor : COLOR) {
				VertexOutput o;
				o.uvAndAlpha = v.texcoord;
				o.uvAndAlpha.a = vertexColor.a;
				TRANSFER_SHADOW_CASTER(o)
				return o;
			}

			float4 frag (VertexOutput i) : SV_Target {
				fixed4 texcol = tex2D(_MainTex, i.uvAndAlpha.xy);
				clip(texcol.a * i.uvAndAlpha.a - _Cutoff);
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
	}
	CustomEditor "SpineShaderWithOutlineGUI"
}
