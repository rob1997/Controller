Shader "UMotion Editor/Unlit Line"
{ 
    Properties 
    {
		_Color("Line Color (RGB) Trans (A)", color) = (0, 0, 0, 1)
		_Thickness("Line Thikness", Range(0, 4)) = 0.9
    }

    SubShader 
    {
		Tags {"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
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

			struct vInput
			{
				float4 vertex : POSITION;
				half4 texcoord : TEXCOORD0;
			};

			struct vOutput
			{
				float4 pos : SV_POSITION;
				float uv_y : TEXCOORD0;
			};

			vOutput vert(vInput i)
			{
				vOutput o;

				o.pos = UnityObjectToClipPos(i.vertex);
				o.uv_y = i.texcoord.y;

				return o;
			}

			fixed4 frag(vOutput i) : SV_Target 
			{
				half width = abs(ddx(i.uv_y)) + abs(ddy(i.uv_y));
				half alpha = smoothstep(0, width * _Thickness, i.uv_y);

				return fixed4(_Color.x, _Color.y, _Color.z, 1 - alpha);
			}

			ENDCG
    	} //Pass
    } //SubShader
} //Shader
