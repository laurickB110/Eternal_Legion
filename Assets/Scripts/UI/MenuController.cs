using UnityEngine;
using UnityEngine.SceneManagement;

namespace EternalLegion.UI
{
    public class MenuController : MonoBehaviour
    {
        public void OnPlay()
        {
            SceneManager.LoadScene("Main scene - Versus");
        }

        public void OnOptions()
        {
            Debug.Log("Options not implemented");
        }

        public void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}

