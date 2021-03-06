Shader "UMotion Editor/Camera Lit"
{ 
    Properties 
    {
		_Color("Main Color (RGB)", color) = (1, 1, 1, 1)
		_WireColor("Wire Color (RGB) Trans (A)", color) = (0, 0, 0, 1)	
		_WireSize("Wire Size", Range(0, 4)) = 0.9
    }

    SubShader 
    {
		Tags { "RenderType" = "Opaque" "IgnoreProjector"="True" }
		LOD 100

		Pass
	    {
            CGPROGRAM 
		    #pragma vertex vert
	    	#pragma fragment frag
			#pragma target 3.0
			
		    #include "UnityCG.cginc"

			fixed4 _Color;
			fixed4 _WireColor;
			half _WireSize;

			struct vInput
			{
				float4 vertex : POSITION;
				half4 texcoord : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct vOutput
			{
				float4 pos : SV_POSITION;
				fixed3 wirecoord : TEXCOORD1;
				float3 lambert : TEXCOORD2;		
			};

			vOutput vert(vInput i)
			{
				vOutput o;

				o.pos = UnityObjectToClipPos(i.vertex);

				o.wirecoord = fixed3(floor(i.texcoord.x), frac(i.texcoord.x) * 10, i.texcoord.y);

				float3 viewDir = normalize(WorldSpaceViewDir(i.vertex));
				float3 worldNormal = normalize(UnityObjectToWorldNormal(i.normal));
				o.lambert = saturate(dot(viewDir, worldNormal));

				return o;
			}

			fixed4 frag(vOutput i) : SV_Target 
			{
				fixed4 outColor;
				outColor.rgb = i.lambert * _Color;
				outColor.a = 1.0;
				 
				half3 width = abs(ddx(i.wirecoord.xyz)) + abs(ddy(i.wirecoord.xyz));
				half3 smoothed = smoothstep(half3(0, 0, 0), width * _WireSize, i.wirecoord.xyz);		
	  			half wireAlpha = min(min(smoothed.x, smoothed.y), smoothed.z);	
				
				return lerp(lerp(outColor, _WireColor, _WireColor.a), outColor, wireAlpha);
			}

			ENDCG
    	} //Pass
    } //SubShader
} //Shader
