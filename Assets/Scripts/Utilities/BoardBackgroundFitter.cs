using UnityEngine;

namespace EternalLegion.Utilities
{
    [ExecuteAlways]
    public class BoardBackgroundFitter : MonoBehaviour
    {
        public Camera targetCamera;
        [Tooltip("Distance from camera along forward direction")] public float distanceFromCamera = 20f;
        [Tooltip("Automatically place near far clip to avoid occluding gameplay")] public bool autoDistance = true;
        public bool lockToCamera = true;

        void OnEnable()
        {
            if (targetCamera == null) targetCamera = Camera.main;
            Fit();
        }

        void Update()
        {
            Fit();
        }

        public void Fit()
        {
            if (targetCamera == null) return;

            float dist = distanceFromCamera;
            if (autoDistance)
            {
                dist = Mathf.Clamp(targetCamera.farClipPlane - 5f, targetCamera.nearClipPlane + 0.5f, 10000f);
                distanceFromCamera = dist;
            }

            if (lockToCamera)
            {
                if (transform.parent != targetCamera.transform)
                    transform.SetParent(targetCamera.transform, false);
                transform.localPosition = new Vector3(0, 0, dist);
                transform.localRotation = Quaternion.identity;
            }
            else
            {
                transform.position = targetCamera.transform.position + targetCamera.transform.forward * dist;
                transform.rotation = Quaternion.LookRotation(-targetCamera.transform.forward, targetCamera.transform.up);
            }

            float height, width;
            if (targetCamera.orthographic)
            {
                height = targetCamera.orthographicSize * 2f;
                width = height * targetCamera.aspect;
            }
            else
            {
                height = 2f * Mathf.Tan(targetCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) * dist;
                width = height * targetCamera.aspect;
            }
            transform.localScale = new Vector3(width, height, 1f);
        }
    }
}

