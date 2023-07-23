Shader "Custom/FillBack"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,0.5) // 裏面ベースカラー
        [HDR] _Emission ("Emission", Color) = (0,0,0,1) // 裏面発光色
        _MainTex ("Albedo (RGB)", 2D) = "white" {} // 裏面テクスチャ
        _MaxDist ("Appear distance", Range(0,10)) = 2 // 表示開始距離[m]
        _Gain ("Alpha Gain", Range(0,1)) = 0 // 表示強調度合い
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        CULL Front
        Blend SrcAlpha OneMinusSrcAlpha 

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alpha:fade

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        half _MaxDist;
        half _Gain;
        fixed4 _Color;
        fixed4 _Emission;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float dist = length(_WorldSpaceCameraPos - IN.worldPos);
            clip(_MaxDist-dist); // 最大距離以降はカット

            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Emission = _Emission.rgb*_Emission.a;

            float alpha01 = saturate(1-length(_WorldSpaceCameraPos - IN.worldPos)/_MaxDist);
            o.Alpha = _Color.a * pow(alpha01,((1-_Gain)*0.7+0.3));
        }
        ENDCG
    }
    FallBack "Diffuse"
}
