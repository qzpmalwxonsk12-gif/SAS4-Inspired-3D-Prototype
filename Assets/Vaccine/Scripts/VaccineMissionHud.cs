using UnityEngine;
using ShipHUD;

namespace Vaccine
{
    [DefaultExecutionOrder(300)]
    public sealed class VaccineMissionHud : MonoBehaviour
    {
        public VaccineMission mission;public HudMapData surfaceMap,undergroundMap;public HudMapGraphic mapGraphic;
        HudMapData liveMap;float next;HudMapData source;
        void Start(){liveMap=Instantiate(surfaceMap);mission.hud.radar.data=liveMap;mapGraphic.data=liveMap;mission.hud.radar.maximumHeightDifference=4;}
        void LateUpdate()
        {
            var p=mission.player;
            if(!p.Captured)p.prompt.text="";
            if(mission.CombatActive&&!mission.Paused&&p.Captured&&!p.MapVisible&&Physics.Raycast(p.eye.transform.position,p.eye.transform.forward,out var hit,4,Physics.DefaultRaycastLayers,QueryTriggerInteraction.Ignore))
            {
                var interact=hit.collider.GetComponentInParent<VaccineInteractable>();if(interact){p.prompt.text=interact.Prompt;if(p.Actions["Interact"].WasPressedThisFrame())interact.Use();}
                else{var door=hit.collider.GetComponentInParent<VaccineBreakableDoor>();if(door&&!door.Broken)p.prompt.text="SHOOT TO BREACH / "+Mathf.CeilToInt(door.health)+" HP";}
            }
            if(Time.unscaledTime<next)return;next=Time.unscaledTime+.2f;var selected=p.transform.position.y<-2?undergroundMap:surfaceMap;
            if(source!=selected){source=selected;liveMap.floorVertices=source.floorVertices;liveMap.floorTriangles=source.floorTriangles;liveMap.wallQuads=source.wallQuads;}
            var center=new Vector2(p.transform.position.x,p.transform.position.z);liveMap.minimum=center-Vector2.one*35;liveMap.maximum=center+Vector2.one*35;mapGraphic.SetVerticesDirty();mission.hud.radar.RefreshNow();
        }
        void OnDestroy(){if(liveMap)Destroy(liveMap);}
    }
}
