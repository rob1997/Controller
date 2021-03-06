// Source: http://wiki.unity3d.com/index.php/VertexColorUnlit
Shader "UMotion Editor/Vertex Color Unlit"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	 
	Category
	{
		Tags { "RenderType"="Opaque" "IgnoreProjector"="True" }
		Lighting Off

		BindChannels 
		{
			Bind "Color", color
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
		}
	 
		SubShader
		{
			Pass
			{
				SetTexture [_MainTex]
				{
					Combine texture * primary DOUBLE
				}
			}
		}
	}
}