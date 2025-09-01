using UnityEngine;

namespace EternalLegion.Utilities
{
    // Runtime safeguard: ensures at least one enabled Camera renders Display 1.
    public class EnsureCamera : MonoBehaviour
    {
        [SerializeField] bool orthographic = true;

        void Awake()
        {
            var cams = Camera.allCamerasCount;
            Camera existing = null;
            if (cams > 0)
            {
                foreach (var c in Camera.allCameras)
                {
                    if (c != null && c.enabled) { existing = c; break; }
                }
            }
            if (existing != null) return;

            var go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            var cam = go.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.07f, 0.07f, 0.07f, 1f);
            cam.orthographic = orthographic;
            cam.orthographicSize = 5f;
            go.tag = "MainCamera";
            go.transform.position = new Vector3(0, 0, -10f);
        }
    }
}

