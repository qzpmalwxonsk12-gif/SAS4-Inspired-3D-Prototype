Shader "Vaccine/NightSky"
{
 Properties { _Zenith("Zenith",Color)=(.008,.017,.04,1) _Horizon("Horizon",Color)=(.07,.1,.14,1) }
 SubShader
 {
  Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" "RenderPipeline"="UniversalPipeline" }
  Cull Off ZWrite Off
  Pass
  {
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   struct A { float4 vertex:POSITION; }; struct V { float4 pos:SV_POSITION;float3 dir:TEXCOORD0; };
   CBUFFER_START(UnityPerMaterial)
   half4 _Zenith, _Horizon;
   CBUFFER_END
   V vert(A i){V o;o.pos=TransformObjectToHClip(i.vertex.xyz);o.dir=i.vertex.xyz;return o;}
   float hash(float3 p){p=frac(p*.1031);p+=dot(p,p.yzx+33.33);return frac((p.x+p.y)*p.z);}
   float noise(float3 p){float3 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(lerp(hash(a),hash(a+float3(1,0,0)),f.x),lerp(hash(a+float3(0,1,0)),hash(a+float3(1,1,0)),f.x),f.y),lerp(lerp(hash(a+float3(0,0,1)),hash(a+float3(1,0,1)),f.x),lerp(hash(a+float3(0,1,1)),hash(a+1),f.x),f.y),f.z);}
   half4 frag(V i):SV_Target
   {
    float3 d=normalize(i.dir);float h=saturate(d.y);
    half3 col=lerp(_Horizon.rgb,_Zenith.rgb,pow(h,.4));
    float3 cell=d*420;float star=step(.9975,hash(floor(cell)))*(1-smoothstep(.04,.23,length(frac(cell)-.5)));
    float clouds=noise(d*5+float3(2,0,1))*.62+noise(d*13)*.25+noise(d*31)*.13;
    float cloud=smoothstep(.46,.75,clouds)*smoothstep(-.02,.2,d.y);
    col+=star*.8*smoothstep(0,.3,d.y)*(1-cloud);
    float moonDot=dot(d,normalize(float3(-.45,.42,.78)));
    float moon=smoothstep(.99955,.9997,moonDot);
    col+=float3(.65,.76,.9)*(moon*.85+pow(saturate(moonDot),200)*.035)*(1-cloud*.85);
    col=lerp(col,col*.28+float3(.025,.035,.055),cloud*.65);
    return half4(col,1);
   }
   ENDHLSL
  }
 }
}
