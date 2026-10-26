Shader "SuperSystems/Wireframe-Transparent" {
	Properties {
		_WireThickness ("Wire Thickness", Range(0, 800)) = 100
		_WireSmoothness ("Wire Smoothness", Range(0, 20)) = 3
		_WireColor ("Wire Color", Vector) = (0,1,0,1)
		_BaseColor ("Base Color", Vector) = (0,0,0,0)
		_MaxTriSize ("Max Tri Size", Range(0, 200)) = 25
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4x4 unity_ObjectToWorld;
			float4x4 unity_MatrixVP;

			struct Vertex_Stage_Input
			{
				float4 pos : POSITION;
			};

			struct Vertex_Stage_Output
			{
				float4 pos : SV_POSITION;
			};

			Vertex_Stage_Output vert(Vertex_Stage_Input input)
			{
				Vertex_Stage_Output output;
				output.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, input.pos));
				return output;
			}

			float4 frag(Vertex_Stage_Output input) : SV_TARGET
			{
				return float4(1.0, 1.0, 1.0, 1.0); // RGBA
			}

			ENDHLSL
		}
	}
}