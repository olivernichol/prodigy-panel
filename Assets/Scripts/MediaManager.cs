using UnityEngine;

public class MediaManager : MonoBehaviour
{
    public void launchVLC()
    {
        string bundleId = "org.videolan.vlc";
        AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject ca = up.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject packageManager = ca.Call<AndroidJavaObject>("getPackageManager");
        AndroidJavaObject launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", bundleId);
        ca.Call("startActivity", launchIntent);
    }
}
