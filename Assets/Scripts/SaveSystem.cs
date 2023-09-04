using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class SaveSystem
{
    public static void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        SerializableSaveData serializableSaveData = new SerializableSaveData();
        FileStream file = File.Open(Application.persistentDataPath + "/save.dat", FileMode.OpenOrCreate);
        bf.Serialize(file, serializableSaveData);
        file.Close();
    }
    public static bool Load()
    {
        if (File.Exists(Application.persistentDataPath + "/save.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/save.dat", FileMode.Open);
            SerializableSaveData serializableSaveData = (SerializableSaveData)bf.Deserialize(file);
            file.Close();
            serializableSaveData.RestoreSaveData();
            return true;
        }
        else
        {
            return false;
        }
    }
}
public class SaveData
{
    public static List<NotificationObject> notifs = new List<NotificationObject>();
    public static List<NotificationObject> savedNotifs = new List<NotificationObject>();
}
[Serializable]
public class SerializableSaveData
{
    private List<NotificationObject> notifs = SaveData.notifs;
    private List<NotificationObject> savedNotifs = SaveData.savedNotifs;

    public void RestoreSaveData()
    {
        SaveData.notifs = notifs;
        SaveData.savedNotifs = savedNotifs;
    }
}
[Serializable]
public class NotificationObject
{
    public string titleText, descText, appID;
    public int notifIdent, yPos;
    public NotificationObject(string titleText, string descText, string appID, int yPos, int notifIdent)
    {
        this.titleText = titleText;
        this.descText = descText;
        this.appID = appID;
        this.yPos = yPos;
        this.notifIdent = notifIdent;
    }
}
public static class SoftStorage
{
    public static int Status = 0;
    public static bool TransitionRequested = false;
}

