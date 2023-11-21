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
			

			v2f vp(VertexData v){
				v2f i;

				float shellHeight = (float)_ShellIndex / (float)_ShellCount;
				
				v.vertex.xyz += v.normal.xyz * _ShellLength * shellHeight;

				i.normal = normalize(UnityObjectToWorldNormal(v.normal));

				i.worldPos = mul(unity_ObjectToWorld, v.vertex);
				i.pos = UnityObjectToClipPos(v.vertex); 
				i.uv = v.uv;
				
				return i;
			}

			float4 fp(v2f i) : SV_TARGET{
				
				fixed4 baseColor = tex2D(_MainTexture, i.uv);

				float greyscaleValue = baseColor.r + baseColor.g + baseColor.b;
				
				if(greyscaleValue < (_Threshold * _ShellIndex)) discard;
			
				
				return baseColor;
			}

			ENDCG
		}
	}
}