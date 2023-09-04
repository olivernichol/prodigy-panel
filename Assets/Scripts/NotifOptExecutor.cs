using UnityEngine;
using UnityEngine.UI;

public class NotifOptExecutor : MonoBehaviour
{
    public void On()
    {
        this.gameObject.GetComponent<Image>().enabled = true;
        this.gameObject.GetComponentInChildren<Text>().enabled = true;
    }
    public void Off()
    {
        this.gameObject.GetComponent<Image>().enabled = false;
        this.gameObject.GetComponentInChildren<Text>().enabled = false;
    }
}
