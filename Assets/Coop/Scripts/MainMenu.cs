using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
namespace SASCoop
{
    public sealed class MainMenu : MonoBehaviour
    {
        public Button single,multi,host,join,deploy,ready,leave,copy,quit;
        public Button[] missions,roles;
        public TMP_InputField code;
        public TMP_Text status,room,missionLabel,roleDetail,squad,deployLabel;
        public GameObject onlinePanel,readyPanel,selectionPanel;
        bool multiplayer,isReady;CoopSession session;
        void Start()
        {
            session=CoopSession.Instance;if(!session)session=new GameObject("SAS game session").AddComponent<CoopSession>();
            single.onClick.AddListener(()=>Mode(false));multi.onClick.AddListener(()=>Mode(true));host.onClick.AddListener(()=>session.Host());join.onClick.AddListener(()=>session.Join(code.text));
            deploy.onClick.AddListener(()=>session.Deploy());ready.onClick.AddListener(()=>{isReady=!isReady;session.Ready(isReady);});leave.onClick.AddListener(()=>session.Leave());copy.onClick.AddListener(()=>GUIUtility.systemCopyBuffer=session.Code);
            quit.onClick.AddListener(()=>Application.Quit());
            for(int i=0;i<missions.Length;i++){int index=i;missions[i].onClick.AddListener(()=>{if(CoopSession.Online)return;session.Mission=index==0?"Onslaught_Mission":"Vaccine_Blockout";});}
            for(int i=0;i<roles.Length;i++){int index=i;roles[i].onClick.AddListener(()=>{session.LocalRole=index;isReady=false;session.Ready(false);roleDetail.text=RoleLoadout.Descriptions[index];});var hover=roles[i].gameObject.AddComponent<RoleHover>();hover.text=roleDetail;hover.description=RoleLoadout.Descriptions[i];}
            roleDetail.text=RoleLoadout.Descriptions[session.LocalRole];Mode(false);Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
        }
        void Mode(bool value){if(CoopSession.Online)return;multiplayer=value;selectionPanel.SetActive(true);onlinePanel.SetActive(value);}
        void Update()
        {
            if(!session)return;bool connected=CoopSession.Online;
            status.text=session.Status;room.text=connected?"SQUAD CODE   "+session.Code:"HOST A SQUAD  /  JOIN A FRIEND";
            missionLabel.text=session.Mission=="Onslaught_Mission"?"01   ONSLAUGHT\nDefend the Mantis. Survive four minutes, defeat the heavy infected and escape.":"02   VACCINE\nRestore subway power. Survive the train journey, recover the vaccine and extract.";
            readyPanel.SetActive(connected);onlinePanel.SetActive(multiplayer&&!connected);deploy.gameObject.SetActive(!multiplayer||connected&&session.net.IsServer);
            deploy.interactable=!session.Busy;host.interactable=!session.Busy;join.interactable=!session.Busy;copy.gameObject.SetActive(connected);leave.gameObject.SetActive(connected);
            ready.GetComponentInChildren<TMP_Text>().text=isReady?"READY  ✓":"READY UP";deployLabel.text=connected?"DEPLOY SQUAD":"DEPLOY SOLO";
            for(int i=0;i<roles.Length;i++)roles[i].image.color=i==session.LocalRole?new Color(.16f,.38f,.4f):new Color(.10f,.14f,.18f);
            single.image.color=!multiplayer?new Color(.18f,.37f,.37f):new Color(.07f,.1f,.12f);multi.image.color=multiplayer?new Color(.18f,.37f,.37f):new Color(.07f,.1f,.12f);
            for(int i=0;i<missions.Length;i++)missions[i].image.color=(i==0)==(session.Mission=="Onslaught_Mission")?new Color(.18f,.37f,.37f):new Color(.07f,.1f,.12f);
            int count=connected?Mathf.Max(1,session.Members.Count):1;squad.text="SQUAD "+count+" / 4\n"+(count==1?"SOLO OPERATION":((CoopSession.InitialPopulationFor(count)-1)*100).ToString("0")+"% MORE INITIAL  /  +"+(CoopSession.ReinforcementsFor(count)*100).ToString("0")+"% RESERVES")+"\n";
            if(connected)foreach(var member in session.Members.Values)squad.text+="PLAYER "+(member.id+1)+"   "+RoleLoadout.Names[member.role]+(member.ready?"   READY":"   CHOOSING")+"\n";
            else squad.text+="Choose a role. Every role can plant C4 when playing with fewer than four people.";
        }
    }
    public sealed class RoleHover:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
    {public TMP_Text text;public string description;public void OnPointerEnter(PointerEventData e){text.text=description;}public void OnPointerExit(PointerEventData e){text.text=RoleLoadout.Descriptions[RoleLoadout.Selected];}}
}
