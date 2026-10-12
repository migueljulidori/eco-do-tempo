using UnityEngine;

namespace EcoDoTempo
{
    // Opens once every melody fragment of the level has been collected.
    public class ExitDoor : MonoBehaviour
    {
        SpriteRenderer sr;

        public static void Create(Transform root, Vector2 pos)
        {
            var go = SpriteFactory.CreateObject("Door", root, pos + Vector2.up * 0.45f, new Vector2(0.9f, 1.9f),
                SpriteFactory.Square, Palette.DoorLocked, 1);
            go.AddComponent<BoxCollider2D>().isTrigger = true;
            go.AddComponent<ExitDoor>().sr = go.GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            sr.color = GameManager.Instance.AllFragmentsCollected
                ? Color.Lerp(Palette.DoorOpen, Color.white, Mathf.PingPong(Time.time, 0.5f))
                : Palette.DoorLocked;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerController>() != null) GameManager.Instance.OnPlayerReachedDoor();
        }

        void OnTriggerStay2D(Collider2D other)
        {
            if (GameManager.Instance.AllFragmentsCollected && other.GetComponent<PlayerController>() != null)
                GameManager.Instance.OnPlayerReachedDoor();
        }
    }
}
