Shader "CtR/Board/Tile (Projected Icon)" {
	Properties {
		_Active ("Active", Range(0, 1)) = 0
		_Flip ("Flip", Range(0, 1)) = 0
		[ToggleUI] _Mirror ("Mirror", Float) = 0
		_BaseColor ("Color", Vector) = (1,1,1,1)
		_TargetColor ("Target Color", Vector) = (0,0,0,0)
		[NoScaleOffset] _Icon ("Icon", 2D) = "black" {}
		[NoScaleOffset] _TargetIcon ("Target Icon", 2D) = "black" {}
		[NoScaleOffset] _Matcap ("Matcap", 2D) = "white" {}
		[NoScaleOffset] _EffectMask ("Glow Mask", 2D) = "white" {}
		_Orientation ("Orientation", Float) = 45
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