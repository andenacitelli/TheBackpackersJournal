Shader "Test/ColorAverage"
{
	Properties
	{
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag

			# include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 color : COLOR;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 color: COLOR;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				return o;
			}

			[maxvertexcount(3)]
			void geom(triangle v2f input[3], inout TriangleStream<v2f> OutputStream)
			{
				v2f output = (v2f)0;
				float3 averageColor = (input[0].color + input[1].color + input[2].color) / 3;
				for (int i = 0; i < 3; i++)
				{
					output.color = averageColor;
					output.vertex = input[i].vertex;
					OutputStream.Append(output);
				}
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return fixed4(i.color, 0);
			}
			ENDCG
		}
	}
}
