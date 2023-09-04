using UnityEngine;
using UnityEngine.UI;

public class NotifOptManager : MonoBehaviour
{
    GameObject notifOps;
    bool currentShow = false;
    private void Start()
    {
        notifOps = this.gameObject.GetComponentInParent<AudioSource>().gameObject.GetComponentInChildren<MeshRenderer>(true).gameObject;
        this.gameObject.GetComponent<Button>().onClick.AddListener(ShowHideNotifOps);
    }
    public void ShowHideNotifOps()
    {
        Debug.Log("ShowHideNotifOps: " + notifOps.name);
        if (!currentShow)
        {
            currentShow = true;
            NotifOptExecutor[] executors = notifOps.GetComponentsInChildren<NotifOptExecutor>(true);
            foreach (NotifOptExecutor exec in executors)
            {
                exec.On();
            }
        }
        else
        {
            currentShow = false;
            NotifOptExecutor[] executors = notifOps.GetComponentsInChildren<NotifOptExecutor>(true);
            foreach (NotifOptExecutor exec in executors)
            {
                exec.Off();
            }
        }
    }
}
