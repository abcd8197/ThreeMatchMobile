using UnityEngine;
using UnityEditor;

public class PlayerPrefRemover : MonoBehaviour
{
    [MenuItem("Tools/ThreeMatch/Delete PlayerPref")]
    public static void PlayerPrefRemove()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("Completed Remove 'PlayerPref'!");
    }
}
