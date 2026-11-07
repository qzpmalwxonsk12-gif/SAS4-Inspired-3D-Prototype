using UnityEngine;
using UnityEditor;
public static class HelicopterMenuScene
{
    static Transform root;static Material metal,cloth,armor,black,skin,glass;
    static Material Mat(string n,Color c,float smooth=.25f){string path="Assets/Coop/Art/"+n+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.color=c;m.SetFloat("_Smoothness",smooth);return m;}
    static Transform Part(string n,Transform parent,Vector3 p,Vector3 s,Material m,PrimitiveType type=PrimitiveType.Cube){var g=GameObject.CreatePrimitive(type);g.name=n;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=s;g.GetComponent<Renderer>().sharedMaterial=m;Object.DestroyImmediate(g.GetComponent<Collider>());return g.transform;}
    static void Limb(string n,Transform t,Vector3 a,Vector3 b,float r,Material m){var p=Part(n,t,(a+b)/2,new Vector3(r,(b-a).magnitude/2,r),m,PrimitiveType.Capsule);p.rotation=t.rotation*Quaternion.FromToRotation(Vector3.up,(b-a).normalized);}
    public static RenderTexture Build()
    {
        System.IO.Directory.CreateDirectory("Assets/Coop/Art");AssetDatabase.Refresh();
        var old=GameObject.Find("Helicopter briefing diorama");if(old)Object.DestroyImmediate(old);
        root=new GameObject("Helicopter briefing diorama").transform;root.position=new Vector3(0,100,0);
        var motion=root.gameObject.AddComponent<SASCoop.SquadDiorama>();
        metal=Mat("Cabin gunmetal",new Color(.17f,.21f,.22f));cloth=Mat("Operator fatigues",new Color(.18f,.23f,.18f));armor=Mat("Plate carrier",new Color(.095f,.12f,.13f));black=Mat("Rubber and gun finish",new Color(.025f,.034f,.038f));skin=Mat("Skin",new Color(.47f,.3f,.2f));glass=Mat("Visor",new Color(.08f,.24f,.28f),.85f);
        Part("Deck",root,new Vector3(0,-.12f,0),new Vector3(8,.2f,5),metal);Part("Rear bulkhead",root,new Vector3(0,1.6f,.9f),new Vector3(8,3.5f,.15f),metal);
        for(int i=0;i<9;i++){Part("Bulkhead rib",root,new Vector3(-4+i,1.6f,.75f),new Vector3(.055f,3.4f,.15f),black);Part("Deck anti slip rail",root,new Vector3(-4+i,0,-1),new Vector3(.04f,.025f,4),black);}
        Part("Bench",root,new Vector3(0,.7f,.12f),new Vector3(7.4f,.2f,.7f),black);
        var lamp=Mat("Cabin luminous panels",new Color(.7f,.8f,.82f));lamp.EnableKeyword("_EMISSION");lamp.SetColor("_EmissionColor",new Color(.6f,.8f,1)*2);
        var night=Mat("Night beyond cabin",new Color(.025f,.07f,.11f),.7f);
        for(int i=0;i<4;i++)
        {
            float x=-2.7f+i*1.8f;
            Part("Recessed window frame",root,new Vector3(x,2.05f,.78f),new Vector3(1.45f,1.05f,.2f),black);
            Part("Night window",root,new Vector3(x,2.05f,.65f),new Vector3(1.27f,.87f,.035f),night);
            Part("Overhead strip light",root,new Vector3(x,2.78f,.1f),new Vector3(.9f,.04f,.15f),lamp);
            Part("Bench support",root,new Vector3(x,.34f,.1f),new Vector3(.07f,.6f,.5f),metal);
            Part("Ceiling rib",root,new Vector3(x,2.95f,-.5f),new Vector3(.09f,.13f,3),black);
        }
        Part("Overhead panel",root,new Vector3(0,3.1f,-.3f),new Vector3(8,.15f,3),metal);
        for(int s=-1;s<=1;s+=2){Part("Cabin side frame",root,new Vector3(s*3.9f,1.4f,-.2f),new Vector3(.15f,3,2.3f),metal);Part("Cabin grab rail",root,new Vector3(s*3.7f,1.55f,-.8f),new Vector3(.06f,2.3f,.06f),black);}
        for(int i=0;i<4;i++)
        {
            var t=new GameObject(SASCoop.RoleLoadout.Names[i]+" seated operator").transform;t.SetParent(root,false);t.localPosition=new Vector3(-2.7f+i*1.8f,0,0);
            float bulk=i==3?1.15f:1;var accent=Mat("Role "+i,i==0?new Color(.78f,.34f,.08f):i==1?new Color(.13f,.54f,.57f):i==2?new Color(.58f,.56f,.38f):new Color(.48f,.24f,.15f));
            Part("Seat back",t,new Vector3(0,1.25f,.55f),new Vector3(1.15f,1.1f,.16f),black);
            motion.operators[i]=t;
            Part("Torso",t,new Vector3(0,1.42f,0),new Vector3(.64f*bulk,.39f,.4f),cloth,PrimitiveType.Capsule);
            Part("Ballistic chest plate",t,new Vector3(0,1.43f,-.22f),new Vector3(.62f*bulk,.57f,.15f),armor);
            Part("Pelvis",t,new Vector3(0,.87f,-.03f),new Vector3(.62f,.29f,.45f),cloth);
            Part("Neck",t,new Vector3(0,1.91f,0),new Vector3(.19f,.2f,.2f),skin,PrimitiveType.Cylinder);
            Part("Head",t,new Vector3(0,2.1f,-.01f),new Vector3(.37f,.44f,.36f),skin,PrimitiveType.Sphere);
            Part("Helmet",t,new Vector3(0,2.23f,.01f),new Vector3(.46f,.3f,.45f),armor,PrimitiveType.Sphere);
            Part("Helmet lip",t,new Vector3(0,2.17f,-.08f),new Vector3(.46f,.055f,.45f),armor);
            Part("Goggles",t,new Vector3(0,2.12f,-.191f),new Vector3(.32f,.095f,.05f),glass);
            Part("Face respirator",t,new Vector3(0,2.0f,-.17f),new Vector3(.23f,.15f,.13f),black);
            if(i==2)Part("Breacher face shield",t,new Vector3(0,2.07f,-.24f),new Vector3(.38f,.29f,.04f),glass);
            for(int s=-1;s<=1;s+=2)
            {
                Limb("Upper arm",t,new Vector3(s*.38f,1.7f,0),new Vector3(s*.49f,1.2f,-.14f),.23f,cloth);
                Part("Shoulder plate",t,new Vector3(s*.39f,1.65f,-.02f),new Vector3(.24f,.27f,.36f),armor);
                Part("Unit patch",t,new Vector3(s*.4f,1.65f,-.21f),new Vector3(.15f,.1f,.035f),accent);
                Limb("Forearm",t,new Vector3(s*.49f,1.2f,-.14f),new Vector3(s*.23f,1.05f,-.53f),.19f,cloth);
                Part("Glove",t,new Vector3(s*.23f,1.05f,-.53f),new Vector3(.18f,.14f,.2f),black,PrimitiveType.Sphere);
                Limb("Thigh",t,new Vector3(s*.19f,.83f,-.05f),new Vector3(s*.26f,.75f,-.65f),.28f,cloth);
                Part("Knee protection",t,new Vector3(s*.26f,.76f,-.79f),new Vector3(.24f,.22f,.09f),armor);
                Limb("Shin",t,new Vector3(s*.26f,.68f,-.65f),new Vector3(s*.28f,.22f,-.67f),.22f,cloth);
                Part("Boot",t,new Vector3(s*.28f,.13f,-.78f),new Vector3(.26f,.24f,.44f),black);
            }
            for(int j=0;j<3;j++)Part("Magazine pouch",t,new Vector3(-.2f+j*.2f,1.32f,-.34f),new Vector3(.16f,.23f,.12f),i==0?accent:cloth);
            for(int s=-1;s<=1;s+=2){Part("Harness webbing",t,new Vector3(s*.22f,1.5f,-.325f),new Vector3(.065f,.56f,.025f),black);Part("Harness buckle",t,new Vector3(s*.22f,1.56f,-.35f),new Vector3(.09f,.09f,.03f),metal);}
            var gun=new GameObject("Held weapon").transform;gun.SetParent(t,false);gun.localPosition=new Vector3(-.45f,1.05f,-.62f);gun.localRotation=Quaternion.Euler(0,0,-8);
            float length=i==0?.65f:i==2?1.1f:.9f;
            Part("Receiver",gun,new Vector3(.35f,0,0),new Vector3(.52f,.12f,.11f),black);Part("Stock",gun,new Vector3(.02f,-.02f,0),new Vector3(.25f,.16f,.1f),armor);
            Part("Barrel",gun,new Vector3(.52f+length/2,0,0),new Vector3(length,.045f,.045f),black);Part("Magazine",gun,new Vector3(.36f,-.12f,0),new Vector3(.11f,i==3?.3f:.19f,.09f),metal);
            if(i==2)Part("Pump grip",gun,new Vector3(.71f,-.035f,0),new Vector3(.24f,.12f,.12f),cloth);
            Part("Overhead harness",t,new Vector3(0,2.75f,.65f),new Vector3(.9f,.06f,.1f),black);
        }
        for(int i=0;i<3;i++){var l=new GameObject("Cabin practical light").AddComponent<Light>();l.transform.SetParent(root,false);l.transform.localPosition=new Vector3(-3+i*3,2.8f,-1.8f);l.type=LightType.Point;l.range=8;l.intensity=i==0?5:4;l.color=i==0?new Color(1,.18f,.08f):i==2?new Color(.28f,.6f,1):new Color(.8f,.85f,1);l.shadows=LightShadows.None;}
        var cam=new GameObject("Squad portrait camera").AddComponent<Camera>();cam.transform.SetParent(root,false);cam.transform.localPosition=new Vector3(0,1.55f,-5.8f);cam.transform.LookAt(root.position+new Vector3(0,1.35f,0));cam.fieldOfView=27;cam.nearClipPlane=.1f;cam.farClipPlane=15;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.025f,.035f,.045f);motion.portraitCamera=cam.transform;
        string rtPath="Assets/Coop/Art/SquadPortrait.renderTexture";var rt=AssetDatabase.LoadAssetAtPath<RenderTexture>(rtPath);if(!rt){rt=new RenderTexture(1416,420,24);AssetDatabase.CreateAsset(rt,rtPath);}else{rt.Release();rt.width=1416;rt.height=420;EditorUtility.SetDirty(rt);}cam.targetTexture=rt;AssetDatabase.SaveAssets();return rt;
    }
}
