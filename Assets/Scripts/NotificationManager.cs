using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Messaging;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Text;
using UnityEngine.SceneManagement;

public class NotificationManager : MonoBehaviour
{
    FirebaseApp app;
    public Text ArgonText, NitroText, NexusText, ProdigyText, ErrorTitle, ErrorInfo;
    public GameObject NotifcationTemplate, MainScene, currentDelete, Error, Canvas;
    int currentPos = 800;
    int currentNotification = 1;
    public IDictionary<string, Text> batteryTexts = new Dictionary<string, Text>();

    async void Start()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable) ErrorMessage("No Internet Connection!", "Prodigy requires an internet connection to function. Please check your internet connection and try again.");
        await FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            if (task.Result == Firebase.DependencyStatus.Available) app = Firebase.FirebaseApp.DefaultInstance;
            else
            {
                UnityEngine.Debug.LogError(System.String.Format("Could not resolve all Firebase dependencies: {0}", task.Result));
                Application.Quit();
            }
        });
        batteryTexts.Add("Argon", ArgonText);
        batteryTexts.Add("Nexus", NexusText);
        batteryTexts.Add("Nitro", NitroText);
        loadPersistent();
        StartCoroutine(LoadSaved());
        FirebaseMessaging.TokenReceived += OnTokenReceived;
        FirebaseMessaging.MessageReceived += OnMessageReceived;
        LocalBatt();
        InvokeRepeating("LocalBatt", 60, 60);
    }
    private void Update()
    {
        if ((SoftStorage.Status == 0) && (SoftStorage.TransitionRequested))
        {
            MainScene.SetActive(true);
            SoftStorage.TransitionRequested = false;
        }
    }
    public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
    {
        StartCoroutine(Post("https://olivernichol.xyz:2666/", "{\"type\": \"token\", \"token\": \"" + token.Token + "\"}"));
    }
    public void ChangeScene(int statusNo)
    {
        MainScene.SetActive(false);
        SoftStorage.Status = statusNo;
        SoftStorage.TransitionRequested = true;
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(statusNo));
    }
    private IEnumerator LoadSaved()
    {
        AsyncOperation loadSaved = SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        yield return null;
    }

    public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e) //When a message is sent downstream from the prodigy server.
    {
        var data = e.Message.Data;
        Debug.Log("Message Recieved - Type: " + data["type"]);
        if (data["type"] == "batt") batteryTexts[data["device"]].text = data["device"] + ":\n" + data["level"] + "%";


        else if (data["type"] == "notification") { //Handling notification 
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
            titleBox.text = data["title"] + " on " + data["device"];
            descBox.text = data["message"];
            Davinci.get().load("https://olivernichol.xyz:2666/" + data["app"] + ".png").setFadeTime(1).setCached(true).into(NotifObject.GetComponentInChildren<Image>()).start();
            SaveData.notifs.Add(new NotificationObject(titleBox.text, descBox.text, data["app"], currentPos, currentNotification));
            SaveSystem.Save();
            currentPos -= 100;
            currentNotification += 1;
        }
    }
    void loadPersistent()
    {
        SaveSystem.Load();
        for (int i = 0; i < SaveData.notifs.Count; i++)
        {
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
            titleBox.text = SaveData.notifs[i].titleText;
            descBox.text = SaveData.notifs[i].descText;
            Davinci.get().load("https://olivernichol.xyz:2666/" + SaveData.notifs[i].appID + ".png").setFadeTime(1).setCached(true).into(NotifObject.GetComponentInChildren<Image>()).start();
            SaveData.notifs[i].yPos = currentPos;
            SaveData.notifs[i].notifIdent = currentNotification;
            currentPos -= 100;
            currentNotification += 1;
        }
        SaveSystem.Save();
    }
    private void LocalBatt()
    {
        ProdigyText.text = "Prodigy:\n" + (SystemInfo.batteryLevel * 100).ToString() + "%";
    }
    public void FakeNotif()
    {
        StartCoroutine(Post("https://olivernichol.xyz:2666/", "{\"type\": \"fakenotif\"}"));
    }
    public void DeleteRipple(GameObject delete, string reason)
    {
        currentDelete = delete;
        if (reason == "save")
        {
            delete.GetComponent<Animation>().clip = delete.GetComponent<Animation>().GetClip("SwipeLeft");
            NotificationObject notifData = SaveData.notifs.Find(lookForItem);
            Debug.Log("notifData Grabbed: " + notifData.appID);
            SaveData.savedNotifs.Add(notifData);
            StartCoroutine(playAnim(delete.GetComponent<Animation>(), reason));
        }
        else if (reason == "dismiss")
        {
            delete.GetComponent<Animation>().clip = delete.GetComponent<Animation>().GetClip("SwipeRight");
            StartCoroutine(playAnim(delete.GetComponent<Animation>(), reason));
        }
        else if (reason == "block")
        {
            NotificationObject notifData = SaveData.notifs.Find(lookForItem);
            StartCoroutine(Post("https://olivernichol.xyz:2666/", "{\"type\": \"block\", \"app\": \"" + notifData.appID + "\"}"));
            delete.GetComponent<Animation>().clip = delete.GetComponent<Animation>().GetClip("SwipeRight");
            StartCoroutine(playAnim(delete.GetComponent<Animation>(), reason));
        }
        
    }
    public bool lookForItem(NotificationObject current)
    {
        //Debug.Log("Object Name: " + current.notifIdent.ToString());
        //Debug.Log("Object YPos: " + current.yPos.ToString());
        //Debug.Log("Lookup YPos: " + currentDelete.transform.position.y.ToString());
        //return current.yPos == currentDelete.transform.position.y;
        return current.notifIdent == Int32.Parse(currentDelete.name.Split(' ')[1]);
    }
    IEnumerator Post(string url, string bodyJsonString)
    {
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
        Debug.Log("Status Code: " + request.responseCode);
        if (request.error != null) ErrorMessage("Communication error!", "There was an error communicating with the server - please check client and server software and try again.");
    }
    IEnumerator playAnim(Animation anim, string reason)
    {
        anim.Play();
        while (anim.isPlaying) yield return null;
        FinishDelete(anim.gameObject, reason);
    }
    void ErrorMessage(string Title, string Info)
    {
        ErrorTitle.text = Title;
        ErrorInfo.text = Info;
        Error.SetActive(true);
        this.gameObject.SetActive(false);
    }
    void NotifsDown()
    {
        for (int i = 0; i < SaveData.notifs.Count; i++)
        {
            //Debug.Log("I: " + i.ToString());
            SaveData.notifs[i].yPos -= 100;
            GameObject NotifObject = GameObject.Find("Notification " + SaveData.notifs[i].notifIdent);
            //Debug.Log("Notification Object: " + NotifObject.name);
            NotifObject.transform.position = new Vector3(NotifcationTemplate.transform.position.x, SaveData.notifs[i].yPos, NotifcationTemplate.transform.position.z);
        }
    }
    void NotifsUp()
    {
        for (int i = 0; i < SaveData.notifs.Count; i++)
        {
            //Debug.Log("I: " + i.ToString());
            SaveData.notifs[i].yPos += 100;
            GameObject NotifObject = GameObject.Find("Notification " + SaveData.notifs[i].notifIdent);
            //Debug.Log("Notification Object: " + NotifObject.name);
            NotifObject.transform.position = new Vector3(NotifcationTemplate.transform.position.x, SaveData.notifs[i].yPos, NotifcationTemplate.transform.position.z);
        }
    }
    public void FinishDelete(GameObject delete, string reason)
    {
        currentPos += 100;
        for (int i = SaveData.notifs.FindIndex(lookForItem); i < SaveData.notifs.Count; i++) // Will only move those below item
        {
            //Debug.Log("I: " + i.ToString());
            SaveData.notifs[i].yPos += 100;
            GameObject NotifObject = GameObject.Find("Notification " + SaveData.notifs[i].notifIdent);
            //Debug.Log("Notification Object: " + NotifObject.name);
            NotifObject.transform.position = new Vector3(NotifcationTemplate.transform.position.x, SaveData.notifs[i].yPos, NotifcationTemplate.transform.position.z);
        }
        //Debug.Log("Debug: " + SaveData.notifs[SaveData.notifs.FindIndex(lookForItem)]);
        SaveData.notifs.RemoveAt(SaveData.notifs.FindIndex(lookForItem));
        Debug.Log("Deleting: " + delete.name);
        GameObject.Destroy(delete);
    }
}
