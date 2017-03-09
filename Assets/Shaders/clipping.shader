Shader "Custom/clipping" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Color("Color",color) =(1,1,1,1)
		_BumpMap("Bumpmap", 2D) = "bump" {}
		_ClipPoint("VanishingPoint", float) = 0.
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		Cull Off
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Lambert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpMap;
		float _ClipPoint;
		float4 _Color;

		struct Input {
			float2 uv_MainTex;
			float3 worldPos;
			float2 uv_BumpMap;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			clip(_ClipPoint - IN.worldPos.x);
			o.Albedo = tex2D(_MainTex, IN.uv_MainTex)* _Color .rgb;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
		}
		ENDCG
	}
	FallBack "Diffuse"
}
