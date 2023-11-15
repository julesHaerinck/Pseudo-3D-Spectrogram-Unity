Shader "Custom/Spectrogram"
{
		Properties{
			_MainTexture ("Texture", 2D) = "white" {}
		}
	SubShader{

		Tags{
			"LightMode" = "ForwardBase"
			"RenderType"="Opaque"
		}

		Pass{
			Cull Off
			CGPROGRAM

			//#pragma multi_compile_instancing
			#pragma vertex vp
			#pragma fragment fp

			#include "UnityPBSLighting.cginc"
			#include "AutoLight.cginc"

			struct VertexData{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv     : TEXCOORD0;
			};

			struct v2f{
				float4 pos      : SV_POSITION;
				float2 uv       : TEXCOORD0;
				float3 normal   : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
			};

			int    _ShellIndex;
			int    _ShellCount;
			float  _ShellLength;
			float  _Threshold;
			sampler2D _MainTexture;
			//float  _Multiplier;
			//float  _ColorMultiplier;
			//float3 _ShellColor;

			

			

			v2f vp(VertexData v){
				v2f i;

				float shellHeight = (float)_ShellIndex / (float)_ShellCount;
				//shellHeight = pow(shellHeight, _ShellDistanceAttenuation);
				
				v.vertex.xyz += v.normal.xyz * _ShellLength * shellHeight;

				i.normal = normalize(UnityObjectToWorldNormal(v.normal));

				i.worldPos = mul(unity_ObjectToWorld, v.vertex);
				i.pos = UnityObjectToClipPos(v.vertex); 
				i.uv = v.uv;
				
				return i;
			}

			float4 fp(v2f i) : SV_TARGET{
				//float SpecColor = tex2D(_MainTexture, i.uv) * _Multiplier;
				//
				//if(SpecColor > 1.0) SpecColor = 1.0;
				
				fixed4 baseColor = tex2D(_MainTexture, i.uv);
				//float4 baseColor = tex2D(_MainTexture, i.uv) * _Multiplier;
				//float2 newUV = i.uv * 10;
				//float2 localUV = frac(newUV) * 2 - 1;
				
				//float greyscaleValue = 0.2126*baseColor.r + 0.7152*baseColor.g + 0.0722*baseColor.b;
				float greyscaleValue = baseColor.r + baseColor.g + baseColor.b;
				
				if(greyscaleValue < (_Threshold * _ShellIndex)) discard;
			
				
				return baseColor;
			}

			ENDCG
		}
	}
}

/*
fp Code :
				float2 newUV = i.uv * _NoiseDensity;

				float2 localUV = frac(newUV) * 2 - 1;
				

				float localDistanceFromCenter = length(localUV);

				uint2 tid = newUV;
				uint seed = tid.x + 100 * tid.y + 100 * 10;
				
				float shellIndex = _ShellIndex;
				float shellCount = _ShellCount;

				float rand = lerp(_NoiseMin, _NoiseMax, hash(seed));

				float h = shellIndex / shellCount;

				int outsideThickness = (localDistanceFromCenter) > (_Thickness * (rand - h));

				if(outsideThickness && _ShellIndex > 0) discard;

				float ndot1 = DotClamped(i.normal, _WorldSpaceLightPos0) * 0.5f + 0.5f;

				ndot1 = ndot1 * ndot1;

				float ambientOcclusion = pow(h, _Attenuation);
				ambientOcclusion += _OcclusionBias;

				return float4(_ShellColor * ndot1 * ambientOcclusion, 1.0);
*/