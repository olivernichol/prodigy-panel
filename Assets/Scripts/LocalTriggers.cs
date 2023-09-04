using UnityEngine;

public class LocalTriggers : MonoBehaviour
{
    public NotificationManager notifManager;
    public SavedNotifsManager savedNotifsManager;
    public void LocalDelRipTrigger(string reason)
    {
        if (notifManager != null) notifManager.DeleteRipple(this.gameObject, reason);
        else
        {
            Debug.Log("DelRipple for Saved");
            savedNotifsManager.DeleteRipple(this.gameObject);
        }
    }
}
