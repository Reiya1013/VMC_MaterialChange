Shader "Custom/4Pass"
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
        UsePass "Hidden/AlphaOn/2Pass"
        UsePass "Hidden/AlphaOn/2Pass"
        UsePass "Hidden/AlphaOn/2Pass"
        UsePass "Hidden/AlphaOn/2Pass"
        UsePass "Hidden/AlphaOn/2Pass"
        UsePass "Hidden/AlphaOn/2Pass"
        UsePass "Hidden/AlphaOn/2Pass"
        UsePass "Hidden/AlphaOn/2Pass"
        UsePass "Hidden/AlphaOn/2Pass"
        UsePass "Hidden/AlphaOn/2Pass"
        UsePass "Hidden/AlphaOn/2Pass"
    }
    FallBack "Diffuse"
}
