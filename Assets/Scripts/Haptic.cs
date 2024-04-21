///FROM https://forum.unity.com/threads/guide-haptic-feedback-on-android-with-no-plugin.382384/
using UnityEngine;
public class Haptics : MonoBehaviour
{
    private class HapticFeedbackManager
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        private int HapticFeedbackConstantsKey;
        private AndroidJavaObject UnityPlayer;
#endif

        public HapticFeedbackManager(string ID)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            HapticFeedbackConstantsKey=new AndroidJavaClass("android.view.HapticFeedbackConstants").GetStatic<int>(ID);
            UnityPlayer=new AndroidJavaClass ("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity").Get<AndroidJavaObject>("mUnityPlayer");
#endif
        }

        public bool Execute()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return UnityPlayer.Call<bool> ("performHapticFeedback",HapticFeedbackConstantsKey);
#endif
            return false;
        }
    }

    //Cache the Manager for performance
    private static HapticFeedbackManager mHapticFeedbackManager;

    public static bool HapticFeedback(string ID)
    {
        if (mHapticFeedbackManager == null)
        {
            mHapticFeedbackManager = new HapticFeedbackManager(ID);
        }
        return mHapticFeedbackManager.Execute();
    }
}