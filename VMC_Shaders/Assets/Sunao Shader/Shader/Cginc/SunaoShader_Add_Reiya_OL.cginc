//----Gameing
	uniform bool      _GameingEnable;

//----Teleport
	uniform bool      _TeleportEnable;

//----Disolve
	uniform bool      _DisolveEnable;
	uniform sampler2D _DisolveTex;
	uniform sampler2D _DisolveEmissionMap;
	uniform float4    _DisolveEmissionColor;
	uniform float     _DisolveEmission;
	uniform float     _DisolveThreshold;
	uniform sampler2D _DisolveTimeTex;
	uniform float     _DisolveStartTime;
	uniform float     _DisolveEndTime;
	

//----Hidden
	uniform uint      _Hidden;
	UNITY_DECLARE_TEX2D(_HiddenMainTex);
	uniform sampler2D _HiddenEmissionMap;
	uniform sampler2D _HiddenEmissionMap2;
	int HiddenTexMode;
    uniform float	  _HiddenDistance;
	
//----HiddenTexMode
void HiddemMode()
{
	//-------------------------------------VRCカメラに映らないようにする(Reiya)
	HiddenTexMode = 0;
	if (_Hidden == 1 && _ScreenParams.x == 1280 && _ScreenParams.y == 720)
        clip(-1);
	else if (_Hidden == 4 && _ScreenParams.x != 1280 && _ScreenParams.y != 720)
        clip(-1);
	else if (_Hidden == 2 && _ScreenParams.x == 1280 && _ScreenParams.y == 720)
		HiddenTexMode = 1;
	else if (_Hidden == 3 && _ScreenParams.x != 1280 && _ScreenParams.y != 720)
		HiddenTexMode = 1;
}

void HiddenDistance(VOUT IN)
{
	// カメラとオブジェクトの距離(長さ)を取得
	float dist = length(_WorldSpaceCameraPos - IN.posWorld);
    if (dist <= _HiddenDistance)
        clip(-1);
}

//----SetMainTex
float4 MainTexSampleTex2D(float2 UV)
{
	if(HiddenTexMode == 1)
	{
		return UNITY_SAMPLE_TEX2D(_HiddenMainTex,UV);
	}
	else
	{
		return UNITY_SAMPLE_TEX2D(_MainTex,UV);
	}
}

float4 MainTexSampleTex2D(UNITY_DECLARE_TEX2D_NOSAMPLER(Map) ,float2 UV)
{
	if(HiddenTexMode == 1)
	{
		return UNITY_SAMPLE_TEX2D_SAMPLER(Map,_HiddenMainTex,UV);
	}
	else
	{
		return UNITY_SAMPLE_TEX2D_SAMPLER(Map,_MainTex,UV);
	}
}




//----ランダム関数
float rand(float2 co)
{
    return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
}

//----ディゾルブ
void Disolve(VOUT IN){
	if(_DisolveEnable){
		fixed4 m = tex2D (_DisolveTex, IN.uv);
		half g = m.r * 0.2 + m.g * 0.7 + m.b * 0.1;
		if( g < _DisolveThreshold ){
			clip(-1);
		} 

		float4 dtime = tex2D(_DisolveTimeTex  , IN.uv);
		if (dtime.y <= 0.2f)
			clip(-1);
	}
}

//----ディゾルブ混合
float3 DisolveAdd(float3 ds){
	return _DisolveEnable * ds;
}

//----ゲーミングカラー追加
fixed3 Gameing(fixed3 OUT)
{
	OUT.x += _GameingEnable * clamp(OUT.x + (_SinTime.w ) ,0,1);
	OUT.y += _GameingEnable * clamp(OUT.y + (_SinTime.z ) ,0,1);
	OUT.z += _GameingEnable * clamp(OUT.z + (_SinTime.y ) ,0,1);

	return OUT;
}

//----vrtxテレポート
float VrtxTelepo(VIN v,VOUT o)
{	
	if (_TeleportEnable & _DisolveEnable)
	{
        float3 n = UnityObjectToWorldNormal(v.normal);

		n =  float4(n * _DisolveThreshold * 100, 0);

		if (n.y < 0)
		{
			n.y = 0;
		}

		return UnityObjectToClipPos (v.vertex).y - n.y;    
	}

	return o.pos.y;
}

//----frgmテレポート
void FrgmTelepo(VOUT IN)
{	
	if (_DisolveEnable & _TeleportEnable)
	{
		float dt = (_DisolveThreshold +1) / (1);
		if (clamp(rand(IN.uv) - dt ,0,1) <= _DisolveThreshold*_DisolveThreshold & _DisolveThreshold != 0)
		{
			clip(-1);
		}
	}
}