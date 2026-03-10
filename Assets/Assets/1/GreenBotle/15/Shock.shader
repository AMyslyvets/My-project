// Made with Amplify Shader Editor v1.9.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Shock"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		_Main("Main", 2D) = "white" {}
		_Emission("Emission", Float) = 2
		_Mask("Mask", 2D) = "white" {}
		_Flow("Flow", 2D) = "white" {}
		_DistortionSpeedXYPowerZ("Distortion Speed X Y Power Z", Vector) = (0,0,0,0)
		_Noise("Noise", 2D) = "white" {}
		_NoiseSpeedXYPowerZ("Noise Speed XY Power Z", Vector) = (0,0,1,0)
		_NoiseOpacityLerp("Noise Opacity Lerp", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}


	Category 
	{
		SubShader
		{
		LOD 0

			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
			Cull Off
			Lighting Off 
			ZWrite Off
			ZTest LEqual
			
			Pass {
			
				CGPROGRAM
				
				#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
				#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
				#endif
				
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#pragma multi_compile_instancing
				#pragma multi_compile_particles
				#pragma multi_compile_fog
				#include "UnityShaderVariables.cginc"
				#define ASE_NEEDS_FRAG_COLOR


				#include "UnityCG.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					float4 ase_texcoord1 : TEXCOORD1;
					float4 ase_texcoord2 : TEXCOORD2;
				};

				struct v2f 
				{
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD2;
					#endif
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
					float4 ase_texcoord3 : TEXCOORD3;
					float4 ase_texcoord4 : TEXCOORD4;
				};
				
				
				#if UNITY_VERSION >= 560
				UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
				#else
				uniform sampler2D_float _CameraDepthTexture;
				#endif

				//Don't delete this comment
				// uniform sampler2D_float _CameraDepthTexture;

				uniform sampler2D _MainTex;
				uniform fixed4 _TintColor;
				uniform float4 _MainTex_ST;
				uniform float _InvFade;
				uniform float _Emission;
				uniform sampler2D _Noise;
				uniform float4 _NoiseSpeedXYPowerZ;
				uniform float4 _Noise_ST;
				uniform float _NoiseOpacityLerp;
				uniform sampler2D _Main;
				uniform sampler2D _Flow;
				uniform float4 _DistortionSpeedXYPowerZ;
				uniform float4 _Flow_ST;
				uniform float4 _Main_ST;
				uniform sampler2D _Mask;
				uniform float4 _Mask_ST;


				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					o.ase_texcoord3.xy = v.ase_texcoord1.xy;
					o.ase_texcoord4 = v.ase_texcoord2;
					
					//setting value to unused interpolator channels and avoid initialization warnings
					o.ase_texcoord3.zw = 0;

					v.vertex.xyz +=  float3( 0, 0, 0 ) ;
					o.vertex = UnityObjectToClipPos(v.vertex);
					#ifdef SOFTPARTICLES_ON
						o.projPos = ComputeScreenPos (o.vertex);
						COMPUTE_EYEDEPTH(o.projPos.z);
					#endif
					o.color = v.color;
					o.texcoord = v.texcoord;
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag ( v2f i  ) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID( i );
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i );

					#ifdef SOFTPARTICLES_ON
						float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
						float partZ = i.projPos.z;
						float fade = saturate (_InvFade * (sceneZ-partZ));
						i.color.a *= fade;
					#endif

					float2 appendResult43 = (float2(_NoiseSpeedXYPowerZ.x , _NoiseSpeedXYPowerZ.y));
					float2 uv2_Noise = i.ase_texcoord3.xy * _Noise_ST.xy + _Noise_ST.zw;
					float2 panner40 = ( 1.0 * _Time.y * appendResult43 + uv2_Noise);
					float4 tex2DNode41 = tex2D( _Noise, panner40 );
					float lerpResult48 = lerp( tex2DNode41.r , 1.0 , _NoiseOpacityLerp);
					float temp_output_45_0 = (( 1.0 - _NoiseSpeedXYPowerZ.z ) + (lerpResult48 - 0.0) * (_NoiseSpeedXYPowerZ.z - ( 1.0 - _NoiseSpeedXYPowerZ.z )) / (1.0 - 0.0));
					float4 appendResult18 = (float4(_DistortionSpeedXYPowerZ.x , _DistortionSpeedXYPowerZ.y , 0.0 , 0.0));
					float4 uv3s4_Flow = i.ase_texcoord4;
					uv3s4_Flow.xy = i.ase_texcoord4.xy * _Flow_ST.xy + _Flow_ST.zw;
					float2 panner16 = ( 1.0 * _Time.y * appendResult18.xy + (uv3s4_Flow).xy);
					float Flow19 = _DistortionSpeedXYPowerZ.z;
					float T23 = uv3s4_Flow.w;
					float4 uvs4_Main = i.texcoord;
					uvs4_Main.xy = i.texcoord.xy * _Main_ST.xy + _Main_ST.zw;
					float2 appendResult10 = (float2(uvs4_Main.z , uvs4_Main.w));
					float2 temp_output_11_0 = ( (uvs4_Main).xy + appendResult10 );
					float4 tex2DNode2 = tex2D( _Main, ( ( tex2D( _Flow, panner16 ) * Flow19 * T23 ) + float4( temp_output_11_0, 0.0 , 0.0 ) ).rg );
					float2 uv_Mask = i.texcoord.xy * _Mask_ST.xy + _Mask_ST.zw;
					float2 break25 = temp_output_11_0;
					float ifLocalVar28 = 0;
					if( 1.0 == ceil( break25.x ) )
					ifLocalVar28 = 0.0;
					else
					ifLocalVar28 = 1.0;
					float ifLocalVar29 = 0;
					if( 1.0 == ceil( break25.y ) )
					ifLocalVar29 = 0.0;
					else
					ifLocalVar29 = 1.0;
					float W35 = uv3s4_Flow.z;
					float4 appendResult1 = (float4(( _Emission * ( temp_output_45_0 * tex2DNode2 ) * i.color * tex2DNode41 ).rgb , saturate( ( ( tex2DNode2.a * i.color.a * tex2D( _Mask, uv_Mask ).a * ( 1.0 - max( ifLocalVar28 , ifLocalVar29 ) ) * temp_output_45_0 ) * W35 ) )));
					

					fixed4 col = appendResult1;
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				ENDCG 
			}
		}	
	}
	
	
	Fallback Off
}
/*ASEBEGIN
Version=19200
Node;AmplifyShaderEditor.RangedFloatNode;5;167.8533,-454.3577;Inherit;False;Property;_Emission;Emission;1;0;Create;True;0;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;4;157.9242,0.4520364;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;13;-172.7935,-459.864;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT2;0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;11;-1442.033,-103.3478;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;426.0108,3.54073;Inherit;False;5;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CeilOpNode;27;-1155.804,366.1504;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-1172.349,200.5294;Inherit;False;Constant;_Float0;Float 0;6;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;28;-945.6509,84.35252;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;29;-946.5529,296.9602;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;32;-628.1235,220.7411;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CeilOpNode;26;-1145.873,128.1879;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;12;106.6257,266.3391;Inherit;True;Property;_Mask;Mask;2;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;31;-1184.263,278.6278;Inherit;False;Constant;_Float1;Float 1;6;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;33;-368.9434,228.0821;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;1;1231.579,-175.9875;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;1416.883,-177.4338;Float;False;True;-1;2;;0;11;Shock;0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;False;True;2;5;False;;10;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;True;True;True;True;True;False;0;False;;False;False;False;False;False;False;False;False;False;True;2;False;;True;3;False;;False;True;4;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;0;;0;0;Standard;0;0;1;True;False;;False;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;810.5016,116.6638;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;38;1005.958,119.5088;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;37;592.7433,279.8242;Inherit;False;35;W;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;25;-1399.521,165.9899;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.ComponentMaskNode;9;-1674.797,-104.7648;Inherit;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;10;-1654.306,-20.4421;Inherit;True;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;2;68.08696,-371.0984;Inherit;True;Property;_Main;Main;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;16;-1709.092,-518.9774;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;18;-1887.213,-479.1203;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;19;-1891.033,-312.4488;Inherit;False;Flow;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;24;-1240.798,-257.4084;Inherit;False;23;T;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;21;-1256.229,-346.4376;Inherit;False;19;Flow;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;17;-2230.5,-468.5304;Inherit;False;Property;_DistortionSpeedXYPowerZ;Distortion Speed X Y Power Z;4;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;23;-1923.443,-633.4214;Inherit;False;T;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;22;-1902.989,-831.1122;Inherit;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;35;-1909.433,-717.9614;Inherit;False;W;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-1005.778,-484.7428;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;14;-1484.368,-535.3101;Inherit;True;Property;_Flow;Flow;3;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;8;-1934.182,-88.5891;Inherit;False;0;2;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;15;-2162.733,-728.3096;Inherit;False;2;14;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;41;-755.1758,-1011.449;Inherit;True;Property;_Noise;Noise;5;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;40;-1007.747,-985.7393;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;42;-1356.101,-1062.505;Inherit;False;1;41;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;43;-1225.45,-925.9606;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector4Node;44;-1584.75,-911.7371;Inherit;False;Property;_NoiseSpeedXYPowerZ;Noise Speed XY Power Z;6;0;Create;True;0;0;0;False;0;False;0,0,1,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;46;-911.9004,-740.6663;Inherit;False;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;920.3594,-330.01;Inherit;False;4;4;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;47;468.5549,-276.3553;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;45;-296.0553,-891.3807;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;48;-415.8378,-1118.447;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;49;-752.2189,-1205.991;Inherit;False;Property;_NoiseOpacityLerp;Noise Opacity Lerp;7;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
WireConnection;13;0;20;0
WireConnection;13;1;11;0
WireConnection;11;0;9;0
WireConnection;11;1;10;0
WireConnection;7;0;2;4
WireConnection;7;1;4;4
WireConnection;7;2;12;4
WireConnection;7;3;33;0
WireConnection;7;4;45;0
WireConnection;27;0;25;1
WireConnection;28;0;30;0
WireConnection;28;1;26;0
WireConnection;28;2;30;0
WireConnection;28;3;31;0
WireConnection;28;4;30;0
WireConnection;29;0;30;0
WireConnection;29;1;27;0
WireConnection;29;2;30;0
WireConnection;29;3;31;0
WireConnection;29;4;30;0
WireConnection;32;0;28;0
WireConnection;32;1;29;0
WireConnection;26;0;25;0
WireConnection;33;0;32;0
WireConnection;1;0;6;0
WireConnection;1;3;38;0
WireConnection;0;0;1;0
WireConnection;36;0;7;0
WireConnection;36;1;37;0
WireConnection;38;0;36;0
WireConnection;25;0;11;0
WireConnection;9;0;8;0
WireConnection;10;0;8;3
WireConnection;10;1;8;4
WireConnection;2;1;13;0
WireConnection;16;0;22;0
WireConnection;16;2;18;0
WireConnection;18;0;17;1
WireConnection;18;1;17;2
WireConnection;19;0;17;3
WireConnection;23;0;15;4
WireConnection;22;0;15;0
WireConnection;35;0;15;3
WireConnection;20;0;14;0
WireConnection;20;1;21;0
WireConnection;20;2;24;0
WireConnection;14;1;16;0
WireConnection;41;1;40;0
WireConnection;40;0;42;0
WireConnection;40;2;43;0
WireConnection;43;0;44;1
WireConnection;43;1;44;2
WireConnection;46;1;44;3
WireConnection;6;0;5;0
WireConnection;6;1;47;0
WireConnection;6;2;4;0
WireConnection;6;3;41;0
WireConnection;47;0;45;0
WireConnection;47;1;2;0
WireConnection;45;0;48;0
WireConnection;45;3;46;0
WireConnection;45;4;44;3
WireConnection;48;0;41;1
WireConnection;48;2;49;0
ASEEND*/
//CHKSM=14BE1516A472886B6EE145810215C68D34805A56