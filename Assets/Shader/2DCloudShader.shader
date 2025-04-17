// This shader fills the mesh shape with a color predefined in the code.
Shader "Universal Render Pipeline/2D/Custom/2DCloudShader"
{
    // The properties block of the Unity shader. In this example this block is empty
    // because the output color is predefined in the fragment shader code.
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _FlowMap("Flow Map", 2D) = "white" {}
        _FlowIntensity("Flow Intensity", Range(0, 1)) = 0.1
        _TimeSpeed("Time Speed", Range(0, 10)) = 0.1
        _MainColor("Color", Color) = (1, 1, 1, 1)
        _Opacity("Opacity", Range(0, 20)) = 1
    }

    // The SubShader block containing the Shader code. 
    SubShader
    {
        // SubShader Tags define when and under which conditions a SubShader block or
        // a pass is executed.
        Tags { "RenderType" = "Transparent"
                "Queue" = "Transparent"
                // "PreviewType" = "Plane"
                // "IgnoreProjector"="True"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            
            // The HLSL code block. Unity SRP uses the HLSL language.
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // The Core.hlsl file contains definitions of frequently used HLSL
            // macros and functions, and also contains #include references to other
            // HLSL files (for example, Common.hlsl, SpaceTransforms.hlsl, etc.).
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"            

            // The structure definition defines which variables it contains.
            // This example uses the Attributes structure as an input structure in
            // the vertex shader.
            struct Attributes
            {
                // The positionOS variable contains the vertex positions in object
                // space.
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                // The positions in this struct must have the SV_POSITION semantic.
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
            };            

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            half4 _MainTex_ST;

            TEXTURE2D(_FlowMap);
            SAMPLER(sampler_FlowMap);

            float _FlowIntensity;
            float _TimeSpeed;
            
            half4 _MainColor;

            float _Opacity;
            
            // The vertex shader definition with properties defined in the Varyings 
            // structure. The type of the vert function must match the type (struct)
            // that it returns.
            Varyings vert(Attributes IN)
            {
                // Declaring the output object (OUT) with the Varyings struct.
                Varyings OUT;
                // The TransformObjectToHClip function transforms vertex positions
                // from object space to homogenous space
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv; // TRANSFORM_TEX(IN.uv, _MainTex);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 flowDir = SAMPLE_TEXTURE2D(_FlowMap, sampler_FlowMap, IN.uv) * 2.0 - 1.0;
                flowDir *= _FlowIntensity;
                // half4 flowTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv - flowDir.xy * _Time.x);

                // 构造两个相位相差半个周期的波形
                float phase0 = frac(_Time.x * _TimeSpeed);
                float phase1 = frac(_Time.x * _TimeSpeed  + 0.5);

                float3 flowTex0 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv - flowDir.xy * phase0); // uv - dir * time
                float3 flowTex1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv - flowDir.xy * phase1);

                float flowLerp = abs(0.5 - phase0) / 0.5;
                float3 finalFlowTex = lerp(flowTex0, flowTex1, flowLerp);
                
                // Rotation
                /*float angle = _Time.x * _Speed;
                float2x2 rotation = float2x2(cos(angle), -sin(angle)
                                            , sin(angle), cos(angle));

                half2 centeredUV = IN.uv - 0.5; // move origin to center
                half2 rotatedUV = mul(rotation, centeredUV);
                IN.uv = rotatedUV + 0.5; // move back 
                */
                
                // half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half fogAlpha = finalFlowTex.r * _Opacity;
                fogAlpha = saturate(fogAlpha);
                
                half4 finalColor = half4(finalFlowTex.rgb * _MainColor.rgb, fogAlpha);
                return finalColor;
            }
            ENDHLSL
        }
    }
}