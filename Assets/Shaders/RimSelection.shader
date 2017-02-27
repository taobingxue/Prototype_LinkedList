Shader "Custom/RimSelection" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_RimPower ("Rim Power", Range(0.5,8.0)) = 3.5
		_RimColor ("Rim Color", Color) = (1,1,1,1)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		struct Input {
			float2 uv_MainTex;
			float3 viewDir;
		};
		sampler2D _MainTex;
		float4 _RimColor;
		float _RimPower;
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutput o) {
			o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
			half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
			o.Emission = _RimColor.rgb * pow(rim, _RimPower);
		}
		ENDCG
	}
	FallBack "Diffuse"
}
