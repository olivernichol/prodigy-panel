using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Firebase;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SavedNotifsManager : MonoBehaviour
{
    FirebaseApp app;
    public GameObject currentDelete, SavedNotifs, NotifcationTemplate, Error, Canvas;
    public Text ErrorTitle, ErrorInfo;
    int currentPos = 800;
    int currentNotification = 1;
    public IDictionary<string, Text> batteryTexts = new Dictionary<string, Text>();

    void Start()
    {
        loadPersistent();
    }
    private void Update()
    {
        if ((SoftStorage.Status == 1) && (SoftStorage.TransitionRequested))
        {
            SavedNotifs.SetActive(true);
            SoftStorage.TransitionRequested = false;
            loadPersistent();
        }
    }
    void loadPersistent()
    {
        for (int i = 0; i < SaveData.savedNotifs.Count; i++)
        {
            GameObject.Destroy(GameObject.Find("Notification " + currentNotification.ToString()));
            GameObject NotifObject = Instantiate<GameObject>(NotifcationTemplate, Canvas.transform);
            NotifObject.SetActive(true);
            NotifObject.name = "Notification " + currentNotification.ToString();
            NotifObject.transform.position = new Vector3(NotifcationTemplate.transform.position.x, currentPos, NotifcationTemplate.transform.position.z);
            Text titleBox, descBox;
            Component[] textBoxes = NotifObject.GetComponentsInChildren(typeof(Text), true);
            if (textBoxes[0].gameObject.name == "NotifTitle")
            {
                titleBox = textBoxes[0].GetComponent<Text>();
                descBox = textBoxes[1].GetComponent<Text>();
            }
            else
            {
                descBox = textBoxes[0].GetComponent<Text>();
                titleBox = textBoxes[1].GetComponent<Text>();
            }
            titleBox.text = SaveData.savedNotifs[i].titleText;
            descBox.text = SaveData.savedNotifs[i].descText;
            Davinci.get().load("https://olivernichol.xyz:2666/" + SaveData.savedNotifs[i].appID + ".png").setFadeTime(1).setCached(true).into(NotifObject.GetComponentInChildren<Image>()).start();
            SaveData.savedNotifs[i].yPos = currentPos;
            SaveData.savedNotifs[i].notifIdent = currentNotification;
            currentPos -= 100;
            currentNotification += 1;
        }
        SaveSystem.Save();
    }
    public void DeleteRipple(GameObject delete)
    {
        currentDelete = delete;
        delete.GetComponent<Animation>().clip = delete.GetComponent<Animation>().GetClip("SwipeRight");
        StartCoroutine(playAnim(delete.GetComponent<Animation>()));
    }
    public void ChangeScene(int statusNo)
    {
        SavedNotifs.SetActive(false);
        SoftStorage.Status = statusNo;
        SoftStorage.TransitionRequested = true;
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(statusNo));
    }
    public bool lookForItem(NotificationObject current)
    {
        return current.notifIdent == Int32.Parse(currentDelete.name.Split(' ')[1]);
    }
    IEnumerator playAnim(Animation anim)
    {
        anim.Play();
        while (anim.isPlaying) yield return null;
        FinishDelete(anim.gameObject);
    }
    void ErrorMessage(string Title, string Info)
    {
        ErrorTitle.text = Title;
        ErrorInfo.text = Info;
        Error.SetActive(true);
        this.gameObject.SetActive(false);
    }
    public void FinishDelete(GameObject delete)
    {
        currentPos += 100;
        for (int i = SaveData.savedNotifs.FindIndex(lookForItem); i < SaveData.savedNotifs.Count; i++) // Will only move those below item
        {
            //Debug.Log("I: " + i.ToString());
            SaveData.savedNotifs[i].yPos += 100;
            GameObject NotifObject = GameObject.Find("Notification " + SaveData.savedNotifs[i].notifIdent);
            NotifObject.transform.position = new Vector3(NotifcationTemplate.transform.position.x, SaveData.savedNotifs[i].yPos, NotifcationTemplate.transform.position.z);
        }
        //Debug.Log("Debug: " + SaveData.savedNotifs[SaveData.savedNotifs.FindIndex(lookForItem)]);
        SaveData.savedNotifs.RemoveAt(SaveData.savedNotifs.FindIndex(lookForItem));
        Debug.Log("Deleting: " + delete.name);
        SaveSystem.Save();
        GameObject.Destroy(delete);
    }
}
