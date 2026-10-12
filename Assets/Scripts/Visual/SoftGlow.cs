using UnityEngine;

namespace EcoDoTempo
{
    // Soft pulsing halo behind a pickup or inspectable.
    public class SoftGlow : MonoBehaviour
    {
        public Color Tint = new Color(1f, 0.86f, 0.4f, 0.65f);
        SpriteRenderer sr;
        SpriteRenderer host;

        public static void Attach(GameObject hostGo, Color tint, float worldSize, int sortingOrder)
        {
            var hostSr = hostGo.GetComponent<SpriteRenderer>();
            var go = new GameObject("Glow");
            go.transform.SetParent(hostGo.transform, false);
            if (hostSr != null && hostSr.sprite != null)
                go.transform.position = hostSr.bounds.center;
            else
                go.transform.localPosition = Vector3.zero;

            var glow = go.AddComponent<SoftGlow>();
            glow.Tint = tint;
            glow.host = hostSr;
            glow.sr = SpriteFactory.AddRenderer(go, SpriteFactory.SoftCircle, tint, sortingOrder);

            float parentScale = Mathf.Max(0.01f, hostGo.transform.lossyScale.x);
            go.transform.localScale = Vector3.one * (worldSize / parentScale);
        }

        void Update()
        {
            if (sr == null) return;
            float pulse = 0.75f + Mathf.Sin(Time.time * 2.4f) * 0.25f;
            float hostAlpha = host != null && host.enabled ? host.color.a : 0f;
            var color = Tint;
            color.a *= pulse * hostAlpha;
            sr.color = color;
            sr.enabled = host == null || host.enabled;
        }
    }
}
