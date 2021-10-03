Shader "Custom/1Pass"
{
    Properties
    { 
    }

    SubShader{
        Tags { 
            "Queue"="Transparent+1000"
            "RenderType"="Opaque"
            }

        UsePass "Hidden/AlphaOn/AlphaOn"
    }
    FallBack "Diffuse"
}
