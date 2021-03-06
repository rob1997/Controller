Shader "UMotion Editor/Unlit Dashed Line"
{ 
    Properties 
    {
		_Color("Line Color (RGB) Trans (A)", color) = (0, 0, 0, 1)
		_Thickness("Line Thikness", Range(0, 4)) = 0.9
		_DashFrequency("Dash Frequency", Range(0, 150)) = 100
    }

    SubShader 
    {
		Tags {"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "DisableBatching"="True" }
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
		Cull Off

		Pass
	    {
            CGPROGRAM 
		    #pragma vertex vert
	    	#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"

			fixed4 _Color;
			half _Thickness;
			half _DashFrequency;

			struct vInput
			{
				float4 vertex : POSITION;
				half4 texcoord : TEXCOORD0;
			};

			struct vOutput
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			vOutput vert(vInput i)
			{
				vOutput o;

				o.pos = UnityObjectToClipPos(i.vertex);
				
				o.uv = i.texcoord.xy;
				o.uv.x *= length(float3(unity_ObjectToWorld[0].z, unity_ObjectToWorld[1].z, unity_ObjectToWorld[2].z));

				return o;
			}

			fixed4 frag(vOutput i) : SV_Target 
			{
				half2 mass = half2(sin(i.uv.x * _DashFrequency), i.uv.y);

				half2 width = abs(ddx(mass)) + abs(ddy(mass));
				half2 smoothed = smoothstep(half2(0, 0), width * _Thickness, mass.xy);
				half alpha = max(smoothed.x, smoothed.y);

				return fixed4(_Color.x, _Color.y, _Color.z, 1 - alpha);
			}

			ENDCG
    	} //Pass
    } //SubShader
} //Shader
