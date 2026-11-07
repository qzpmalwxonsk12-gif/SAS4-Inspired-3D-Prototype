using UnityEngine;
namespace SASCoop
{
    // A small live scene: no video or generated image is used by the menu.
    public sealed class SquadDiorama : MonoBehaviour
    {
        public Transform[] operators=new Transform[4];
        public Transform portraitCamera;
        Vector3 cameraOrigin;
        void Start(){if(portraitCamera)cameraOrigin=portraitCamera.localPosition;}
        void Update()
        {
            float t=Time.unscaledTime;
            for(int i=0;i<operators.Length;i++)if(operators[i])
                operators[i].localRotation=Quaternion.Euler(Mathf.Sin(t*.8f+i)*.4f,Mathf.Sin(t*.3f+i)*.7f,0);
            if(portraitCamera)portraitCamera.localPosition=cameraOrigin+new Vector3(Mathf.Sin(t*.21f)*.018f,Mathf.Sin(t*.6f)*.012f,0);
        }
    }
}
