using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class SaveProjectOnKey : MonoBehaviour
    {
        [MenuItem("File/Save Project (key) %&s")]
        private static void DoSomethingWithAShortcutKey()
        {
            AssetDatabase.SaveAssets();
            Debug.Log("Saved project.");
        }
    }
}
