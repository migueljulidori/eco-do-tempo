using UnityEngine;

namespace EcoDoTempo
{
    // Opens once every melody fragment of the level has been collected.
    public class ExitDoor : MonoBehaviour
    {
        SpriteRenderer sr;

        bool useArt;
        bool showingOpen;
        Vector2 baseCell;

        public static void Create(Transform root, Vector2 pos)
        {
            bool art = ArtCatalog.Has("Props/door") || ArtCatalog.Has("Props/door_open");
            var go = ArtCatalog.Create("Door", root, pos, new Vector2(0.9f, 1.9f),
                SpriteFactory.Square, Palette.DoorLocked, 3, "Props/door", new Vector2(1.5f, 0f));
            if (art) ArtCatalog.SnapBottomTo(go, ArtCatalog.FloorTop(pos), 0f);
            go.AddComponent<BoxCollider2D>().isTrigger = true;
            var door = go.AddComponent<ExitDoor>();
            door.sr = go.GetComponent<SpriteRenderer>();
            door.useArt = art;
            door.baseCell = pos;
        }

        void Update()
        {
            bool open = GameManager.Instance.AllFragmentsCollected;
            if (useArt)
            {
                if (open != showingOpen)
                {
                    showingOpen = open;
                    if (open && ArtCatalog.Has("Props/door_open"))
                        ArtCatalog.TryApply(gameObject, "Props/door_open", new Vector2(1.5f, 0f));
                    else if (ArtCatalog.Has("Props/door"))
                        ArtCatalog.TryApply(gameObject, "Props/door", new Vector2(1.5f, 0f));
                    ArtCatalog.SnapBottomTo(gameObject, ArtCatalog.FloorTop(baseCell));
                }
                sr.color = open
                    ? Color.Lerp(Color.white, new Color(1f, 0.95f, 0.7f), Mathf.PingPong(Time.time, 0.5f))
                    : Color.white;
                return;
            }

            sr.color = open
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
