using UnityEngine;

namespace EcoDoTempo
{
    public class CameraFollow : MonoBehaviour
    {
        const float Smoothing = 6f;
        public const float DefaultSize = 6f;
        // How much world below the walkable floor stays on screen (spikes / pit lip).
        const float UnderFloorVisible = 1.75f;

        Transform target;
        Vector2Int levelSize;
        float floorTopY;
        Camera cam;

        public void SetTarget(Transform newTarget, Vector2Int size, float floorTop = 0f)
        {
            target = newTarget;
            levelSize = size;
            floorTopY = floorTop;
            cam = GetComponent<Camera>();
            FitToLevel(cam, size.x);
            transform.position = Clamp(target.position);
        }

        public void Snap()
        {
            if (target != null) transform.position = Clamp(target.position);
        }

        // On very wide windows, zoom in just enough that nothing outside the level shows.
        public static void FitToLevel(Camera camera, int levelWidth)
        {
            if (camera == null || levelWidth <= 0) return;
            float aspect = Mathf.Max(0.1f, camera.aspect);
            camera.orthographicSize = Mathf.Min(DefaultSize, levelWidth / (2f * aspect));
        }

        void LateUpdate()
        {
            if (target == null) return;
            FitToLevel(cam, levelSize.x);
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

            float y;
            if (minY > maxY)
            {
                // Short level: rest the floor near the bottom of the screen so the
                // painted masonry under it is cropped. If Lia drops into a pit or
                // cellar, follow her down; the resting frame comes back when she climbs.
                if (floorTopY > 0f)
                {
                    float pinned = floorTopY - UnderFloorVisible + halfH;
                    float followDown = wanted.y + halfH - 2.2f;
                    y = Mathf.Min(pinned, followDown);
                }
                else y = (levelSize.y - 1) * 0.5f;
            }
            else
            {
                y = Mathf.Clamp(wanted.y, minY, maxY);
            }

            return new Vector3(x, y, -10f);
        }
    }
}
