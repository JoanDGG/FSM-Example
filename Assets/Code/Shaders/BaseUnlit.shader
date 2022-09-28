//
//  "InspectorPath/shaderName"
//   Refers to the place where we will select our shader to apply it to a material. This selection is made through the Unity Inspector
//
Shader "Custom/BaseUnlit"
{
    // Properties that the shader can control
    // Properties allow you to expose GUI elements in a material's Inspector tab
    // These are written in shaderlab declarative language
    Properties
    {
        // _Name of the property ("Name in the inspector", Type of the property) = Default value
        // Type of properties: Color, Vector, 2D - Texture, Range(min, max), Float, Int
        [Header(Base properties)]
        [Space(10)]
        // Textures
        _MainTex ("Texture", 2D) = "white" {}

        // Numbers
        _Specular ("Specular", Range(0.0, 1.1)) = 0.3
        _Factor ("Color Factor", Float) = 0.3
        _Cid ("Color id", Int) = 2

        // Vectors 
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _VPos ("Vertex Position", Vector) = (0, 0, 0, 1)

        // Cubemap: This type of texture is quite useful for generating reflection maps, e.g., reflections in our character’s armor or to metallic elements in general.
        _Reflection ("Reflection", Cube) = "black" {}

        // Drawers allow us to define custom properties in the unity inspector.
        // There are 7 drawers: toggle, enum, keywordenum, intrange, space, header, powerslider

        [Header(Drawers)]
        [Space(10)]
        // We do not have boolean type properties, but we have the toggle that does the same thing.
        [Toggle] _Enable ("Enable color?", Float) = 0

        [Toggle] _Flip_Texture("Flip texture?", Float) = 0

        [KeywordEnum(Off, Red, Blue)] _Options ("Options", Float) = 0

        // This drawer is very similar to the KeywordEnum with the difference that it can define a “value/id” as an argument and pass this property to a command in our shader to change its functionality dynamically from the inspector.
        [Enum(Off, 0, Front, 1, Back, 2)] _Face("Face Culling", Float) = 0

        // PowerSlider and IntRange are quite useful when working with numerical ranges and precision.
        // PowerSlider allow us to generate a non-linear slider with curve control
        [PowerSlider(3.0)] _Brightness("Brightness", Range(0.01, 1)) = 0.08

        // IntRange adds a numerical range of integer values.
        [IntRange] _Samples ("Samples", Range (0, 255)) = 100

        // Space and Header help in organization tasks. 
        // Space allows us to add space between one property and another, while header adds a header in the unity inspector.
    }

    // Each shader is made up of at least one SubShader for the perfect loading of the program. When there is more than one SubShader, Unity will process each of them and take the most  suitable according to the hardware characteristics, starting from the first to the last on the list.
    SubShader
    {
        // SubShader configuration

        // Tags are labels that show how and when our shaders are processed.
        // Unity has a processing queue called “Render Queue” which allows us to modify the processing order of objects on the GPU.

        
        // RENDER TYPE we can change from one state to another in the SubShader, adding an effect on any material that matches a given Type.
        // Opaque. Default.
        // Transparent.
        // TransparentCutout.
        // Background.
        // Overlay.
        // TreeOpaque.
        // TreeTransparentCutout.
        // TreeBillboard.
        // Grass.
        // GrassBillboard.

        // QUEUE allows us to modify the processing order of objects on the GPU
        //Background is used mainly for elements that are very far from the camera (e.g. skybox).
        // Geometry is the default value in the Queue and is used for opaque objects in the scene (e.g. primitives and objects in general).
        // AlphaTest is used on semi-transparent objects that must be in front of an opaque object, but behind a transparent object (e.g. glass, grass or vegetation).
        // Transparent is used for transparent elements that must be in front of others.
        // Overlay corresponds to those elements that are foremost in the scene (e.g. UI images).
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        // “Blend” is a function that requires two values called “factors” for its operation and based on an equation it will be the final color that we will obtain on-screen. 
        // The most common types of blending are the following:
        // • Blend SrcAlpha OneMinusSrcAlpha Common transparent blending
        // • Blend One One Additive blending color
        // • Blend OneMinusDstColor One Mild additive blending color
        // • Blend DstColor Zero Multiplicative blending color
        // • Blend DstColor SrcColor Multiplicative blending x2
        // • Blend SrcColor One Blending overlay
        // • Blend OneMinusSrcColor One Soft light blending
        // • Blend Zero OneMinusSrcColor Negative color blending
        Blend SrcAlpha OneMinusSrcAlpha
        
        // Each draw call that the shader performs. There can be multiple passes in a shader. Each Pass renders one object at a time, that is, if we have two passes in our shader, the object will be rendered twice on the GPU and the equivalent of that would be two draw calls. Ideally, we should use the least amount of passes possible.
        Pass
        {
            // HLSL or Cg Program
            CGPROGRAM

            // The #pragma vertex vert allows the vertex shader stage called vert to be compiled to the GPU as a vertex shader
            #pragma vertex vert

            // The #pragma fragment frag directive serves the same function as the pragma vertex, with the difference that it allows the fragment shader stage called “frag” to compile in the code as a fragment shader.
            #pragma fragment frag

            // Needed so that the toggle works. These are declared as constant. If the property is called "_Enable", this constant will be called "_ENABLE".
            #pragma shader_feature _ENABLE_ON

            #pragma shader_feature _FLIP_TEXTURE_ON

            // Used to link the KeywordEnum
            #pragma multi_compile _OPTIONS_OFF _OPTIONS_RED _OPTIONS_BLUE

            // make fog work
            #pragma multi_compile_fog

            // The directive “.cginc” (Cg include) contains several files that can be used in our shader to bring in predefined variables and auxiliary functions.
            #include "UnityCG.cginc"

            // We will use structs to define both inputs and outputs in our shader.
            // Appdata corresponds to the “vertex input” and v2f refers to the “vertex output”.
            // name : Semantic
            // “SEMANTIC” corresponds to the semantics that we will pass as input or output.
            // The most common semantics that we use are:
            // POSITION[n].
            // TEXCOORD[n].
            // TANGENT[n].
            // NORMAL[n].
            // COLOR[n].
            struct appdata
            {
                // the position of the vertices of the object in object-space
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                // SV_POSITION semantic, which fulfills the same function as POSITION[n], but has the prefix “SV_” (System Value).
                float4 vertex : SV_POSITION;
            };

            // Connection variables are used to connect the shader variables to the shaderlab variables so that they can be used. They must have the same name in both sections so that the program recognizes them.

            // “Sampler” refers to the sampling state of a texture. Within this type of data, we can store a texture and its UV coordinates.
            sampler2D _MainTex;

            // “Float” (Cg and HLSL) is a high-precision data type, it is 32 bit and generally used in calculating positions in world-space, texture coordinates (UV), or scalar calculations involving complex functions such as trigonometry
            float4 _Color;

            // For the powerSlider and intRange we need connection variables
            // “Half” (Cg and HLSL) is half-precision, is 16 bit and is mostly used in the calculation of low magnitude vectors, directions, object-space positions, and high dynamic range colors.
            half _Brightness;
            int _Samples;

            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                // vertices are later transformed to clip-space in the vertex shader stage through the UnityObjectToClipPos(v.vertex) function.
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // sample the texture
                #if _FLIP_TEXTURE_ON
                    i.uv.y *= -1;
                #endif

                // there are three Built-in Shader Variables that we will frequently use in animating properties for our effects. These refer to _Time, _SinTime, and _CosTime.
                // i.uv.x += _Time.y;

                // sampler the texture in UV coordinates using the function tex2D().
                float4 col = tex2D(_MainTex, i.uv);

                #if _ENABLE_ON
                    return col * _Color;
                #else
                    #if _OPTIONS_OFF
                        return col;
                    #elif _OPTIONS_RED
                        return col * float4(1, 0, 0, 1);
                    #elif _OPTIONS_BLUE
                        return col * float4(0, 0, 1, 1);
                    #endif
                #endif
            }
            ENDCG
        }
    }
}
