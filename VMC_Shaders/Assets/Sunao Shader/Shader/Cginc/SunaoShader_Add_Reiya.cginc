//----Gameing
	uniform bool      _GameingEnable;
	uniform float     _GameingSpeed;

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

void HiddenDistance(VOUT IN)
{
	// カメラとオブジェクトの距離(長さ)を取得
	float dist = length(_WorldSpaceCameraPos - IN.posWorld);
    if (dist <= _HiddenDistance)
        clip(-1);
}


//----ランダム関数
float rand(float2 co)
{
    return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
}

//----ディゾルブ
float3 Disolve(fixed4 rgba, VOUT IN,float2 MainUV){
	float3 DisolveEmission     = (float3)0.0f;
	if(_DisolveEnable){
		fixed4 m = rgba;
		half g = m.r * 0.2 + m.g * 0.7 + m.b * 0.1;
		if( g < _DisolveThreshold ){
			clip(-1);
		} 

		//----ディゾルブエミッション
		DisolveEmission    = _DisolveEmission * _DisolveEmissionColor.rgb;
		DisolveEmission   *= tex2D(_DisolveEmissionMap  , IN.euv.xy).rgb * tex2D(_DisolveEmissionMap  , IN.euv.xy).a * IN.eprm.x;


		//----テレポート	
		if (_TeleportEnable)
		{
			float dt = (_DisolveThreshold +1) / (2);
			rgba.a = clamp(rand(MainUV) - dt ,0,1);
			if (rgba.a <= (_DisolveThreshold * (0.2 / _DisolveThreshold)) & _DisolveThreshold != 0)
			{
				clip(-1);
			}
		}
	}
	return DisolveEmission;
}

//----ディゾルブ混合
float3 DisolveAdd(float3 ds){
	return _DisolveEnable * ds;
}

//----ゲーミングカラー追加
fixed3 Gameing(fixed3 OUT)
{
	//OUT.x += _GameingEnable * sin((1-OUT.x) * ((_SinTime.w + 1.0)*0.5) * Speed );
	//OUT.y += _GameingEnable * sin((1-OUT.z) * ((_SinTime.z + 1.0)*0.5) * Speed );
	//OUT.z += _GameingEnable * sin((1-OUT.z) * ((_SinTime.y + 1.0)*0.5) * Speed );
	OUT.x += _GameingEnable * sin((1-OUT.x) * _SinTime.w * _GameingSpeed );
	OUT.y += _GameingEnable * sin((1-OUT.z) * _SinTime.z * _GameingSpeed );
	OUT.z += _GameingEnable * sin((1-OUT.z) * _SinTime.y * _GameingSpeed );

	return OUT;
}

//----エミッション
float3 EmissionSet(float3 Emission,VOUT IN)
{
	if (HiddenTexMode == 1){
		Emission   *= tex2D(_HiddenEmissionMap  , IN.euv.xy).rgb * tex2D(_HiddenEmissionMap  , IN.euv.xy).a * IN.eprm.x;
		Emission   *= tex2D(_HiddenEmissionMap2 , IN.euv.zw).rgb * tex2D(_HiddenEmissionMap2 , IN.euv.zw).a;
	}else {
		Emission   *= tex2D(_EmissionMap  , IN.euv.xy).rgb * tex2D(_EmissionMap  , IN.euv.xy).a * IN.eprm.x;
		Emission   *= tex2D(_EmissionMap2 , IN.euv.zw).rgb * tex2D(_EmissionMap2 , IN.euv.zw).a;
	}

	return Emission;
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

		return o.pos.y - n.y;    
	}

	return o.pos.y;
}