using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using SASCoop;
public static class MenuPolish
{
    static void Place(Component c,float x,float y,float w,float h){var r=c.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);}
    static GameObject Box(string name,Transform parent,float x,float y,float w,float h,Color color){var g=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Image));g.transform.SetParent(parent,false);Place(g.transform,x,y,w,h);g.GetComponent<UnityEngine.UI.Image>().color=color;g.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;return g;}
    static void Label(string value,Transform parent,float x,float y,float w,float h,int size,Color color){var g=new GameObject(value,typeof(RectTransform),typeof(TextMeshProUGUI));g.transform.SetParent(parent,false);Place(g.transform,x,y,w,h);var t=g.GetComponent<TextMeshProUGUI>();t.font=TMP_Settings.defaultFontAsset;t.text=value;t.fontSize=size;t.color=color;t.raycastTarget=false;}
    public static void Apply()
    {
        var scene=EditorSceneManager.OpenScene("Assets/Coop/Scenes/MainMenu.unity");var m=Object.FindAnyObjectByType<MainMenu>();var root=m.transform.Find("Menu content");
        if(!root){var frame=new GameObject("Menu content",typeof(RectTransform));root=frame.transform;var children=new System.Collections.Generic.List<Transform>();foreach(Transform child in m.transform)children.Add(child);root.SetParent(m.transform,false);var r=(RectTransform)root;r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.sizeDelta=new Vector2(1366,768);foreach(var child in children)child.SetParent(root,false);}
        foreach(var t in root.GetComponentsInChildren<Transform>(true))if(t.name=="Helicopter squad illustration"||t.name=="Specialist divider"||t.name=="01 / SELECT OPERATION"||t.name=="Header rule"||t.name.StartsWith("Operator hotspot "))Object.DestroyImmediate(t.gameObject);
        m.GetComponent<UnityEngine.UI.CanvasScaler>().screenMatchMode=UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand;
        var ink=new Color(.025f,.035f,.045f);var panel=new Color(.045f,.065f,.075f);var amber=new Color(.94f,.64f,.27f);var muted=new Color(.53f,.64f,.65f);
        root.Find("Background").GetComponent<UnityEngine.UI.Image>().color=ink;
        Place(root.Find("Accent"),40,36,4,80);root.Find("Accent").GetComponent<UnityEngine.UI.Image>().color=amber;
        Place(root.Find("SAS 4 3D"),62,31,700,77);root.Find("SAS 4 3D").GetComponent<TMP_Text>().fontStyle=FontStyles.Bold;
        var sub=System.Linq.Enumerable.First(root.GetComponentsInChildren<TMP_Text>(),t=>t.name=="SPECIAL ASSAULT SQUAD  /  CAMPAIGN").transform;Place(sub,65,108,800,25);sub.GetComponent<TMP_Text>().text="SPECIAL ASSAULT SQUAD     /     OPERATIONS COMMAND";sub.GetComponent<TMP_Text>().characterSpacing=2;
        root.Find("CO-OP SURVIVAL").GetComponent<TMP_Text>().text="SURVIVAL CAMPAIGN";
        Place(m.single,40,157,260,47);Place(m.multi,310,157,260,47);
        Place(m.selectionPanel.transform,40,224,530,335);m.selectionPanel.GetComponent<UnityEngine.UI.Image>().color=panel;
        Place(m.missions[0],20,60,235,67);Place(m.missions[1],275,60,235,67);Place(m.missionLabel,22,151,480,145);m.missionLabel.fontSize=23;m.missionLabel.lineSpacing=12;
        var right=root.Find("Squad selection");Place(right,594,157,732,402);right.GetComponent<UnityEngine.UI.Image>().color=panel;
        var art=Box("Helicopter squad illustration",right,12,40,708,210,Color.white);art.transform.SetSiblingIndex(1);
        Object.DestroyImmediate(art.GetComponent<UnityEngine.UI.Image>());var viewport=art.AddComponent<UnityEngine.UI.RawImage>();viewport.raycastTarget=false;viewport.texture=HelicopterMenuScene.Build();
        for(int i=0;i<4;i++){Place(m.roles[i],12+i*180,229,168,33);m.roles[i].image.color=ink;var t=m.roles[i].GetComponentInChildren<TMP_Text>();Place(t,10,2,148,29);t.fontSize=17;t.fontStyle=FontStyles.Bold;t.alignment=TextAlignmentOptions.Center;
            var hot=Box("Operator hotspot "+i,right,57+i*155,62,132,166,new Color(0,0,0,0));hot.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;var h=hot.AddComponent<SASCoop.OperatorHotspot>();h.role=i;h.menu=m;}
        Place(m.roleDetail,22,278,330,111);m.roleDetail.fontSize=18;m.roleDetail.lineSpacing=3;
        Place(m.squad,377,278,335,111);m.squad.fontSize=15;m.squad.lineSpacing=2;
        Box("Specialist divider",right,357,282,1,104,new Color(.2f,.3f,.31f));
        Label("01 / SELECT OPERATION",m.selectionPanel.transform,22,292,480,24,13,muted);
        Place(m.onlinePanel.transform,40,577,920,79);Place(m.readyPanel.transform,40,577,920,79);Place(m.deploy,980,577,346,79);m.deploy.image.color=amber;m.deployLabel.color=ink;m.deployLabel.fontStyle=FontStyles.Bold;
        Place(m.status,40,673,1070,40);m.status.fontSize=16;Place(m.leave,1180,672,146,40);Place(m.quit,1180,724,146,32);
        foreach(var b in m.GetComponentsInChildren<UnityEngine.UI.Button>(true)){var c=b.colors;c.highlightedColor=new Color(1,.85f,.6f);c.pressedColor=new Color(.65f,.8f,.8f);c.fadeDuration=.1f;b.colors=c;}
        Box("Header rule",root,40,140,1286,1,new Color(.18f,.25f,.27f));
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
    }
}
