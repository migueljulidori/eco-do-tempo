using UnityEngine;

namespace EcoDoTempo
{
    public class MelodyFragment : MonoBehaviour
    {
        public int Order;
        public bool UseCustomArt;
        Vector3 basePosition;

        void Start() => basePosition = transform.position;

        void Update()
        {
            transform.position = basePosition + Vector3.up * Mathf.Sin(Time.time * 3f + Order) * 0.1f;
            if (!UseCustomArt) transform.Rotate(0f, 0f, 60f * Time.deltaTime);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerController>() == null) return;
            GameManager.Instance.CollectFragment(this);
            Destroy(gameObject);
        }
    }
}
