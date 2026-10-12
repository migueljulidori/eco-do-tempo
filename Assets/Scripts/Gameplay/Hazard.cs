using UnityEngine;

namespace EcoDoTempo
{
    public class Hazard : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D other) => Hit(other);

        void OnTriggerStay2D(Collider2D other) => Hit(other);

        void Hit(Collider2D other)
        {
            var player = other.GetComponent<PlayerController>();
            if (player != null) player.Die();
        }
    }
}
