using UnityEngine;

namespace EcoDoTempo
{
    public class CameraFollow : MonoBehaviour
    {
        const float Smoothing = 6f;

        Transform target;
        Vector2Int levelSize;
        Camera cam;

        public void SetTarget(Transform newTarget, Vector2Int size)
        {
            target = newTarget;
            levelSize = size;
            cam = GetComponent<Camera>();
            transform.position = Clamp(target.position);
        }

        void LateUpdate()
        {
            if (target == null) return;
            transform.position = Vector3.Lerp(transform.position, Clamp(target.position), Smoothing * Time.deltaTime);
        }

        Vector3 Clamp(Vector3 wanted)
        {
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;
            // Map cells are centered on integer coordinates, so the level spans [-0.5, size - 0.5].
            float minX = -0.5f + halfW, maxX = levelSize.x - 0.5f - halfW;
            float minY = -0.5f + halfH, maxY = levelSize.y - 0.5f - halfH;
            float x = minX > maxX ? (levelSize.x - 1) * 0.5f : Mathf.Clamp(wanted.x, minX, maxX);
            float y = minY > maxY ? (levelSize.y - 1) * 0.5f : Mathf.Clamp(wanted.y, minY, maxY);
            return new Vector3(x, y, -10f);
        }
    }
}
