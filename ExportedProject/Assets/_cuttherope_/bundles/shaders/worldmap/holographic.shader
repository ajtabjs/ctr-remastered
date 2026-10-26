Shader "CtR/Worldmap/Holographic" {
	Properties {
		[HDR] _BaseColor ("Color (A: Color Mask)", Vector) = (1,1,1,1)
		[NoScaleOffset] _BaseMap ("Albedo (A: Color Mask)", 2D) = "white" {}
		[NoScaleOffset] _Pattern ("Holographic Pattern", 2D) = "white" {}
		_PatternST ("Pattern Tiling and Offset", Vector) = (1,1,0,0)
		_Saturation ("Holographic Saturation", Range(0, 8)) = 2.5
		_Intensity ("Holographic Intensity", Range(0, 2)) = 1
		_FresnelPower ("Holographic Fresnel", Range(0.5, 32)) = 8
		[KeywordEnum(_, ON)] LIGHTMAP ("Lightmap", Float) = 0
		_Emission ("Emission", Vector) = (0.75,0.75,0.75,0)
		[HideInInspector] _QueueOffset ("_QueueOffset", Float) = 0
		[HideInInspector] _QueueControl ("_QueueControl", Float) = -1
		[HideInInspector] [NoScaleOffset] unity_Lightmaps ("unity_Lightmaps", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_LightmapsInd ("unity_LightmapsInd", 2DArray) = "" {}
		[HideInInspector] [NoScaleOffset] unity_ShadowMasks ("unity_ShadowMasks", 2DArray) = "" {}
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
	Fallback "Hidden/Shader Graph/FallbackError"
	//CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
}