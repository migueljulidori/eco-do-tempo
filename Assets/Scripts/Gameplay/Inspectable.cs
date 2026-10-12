using UnityEngine;

namespace EcoDoTempo
{
    // Cube Escape style: the same object tells a different story in each era.
    public class Inspectable : MonoBehaviour, IInteractable
    {
        InspectNote note;

        public static void Create(Transform root, Vector2 pos, InspectNote note)
        {
            var place = ArtCatalog.Has("Props/inspect") ? ArtCatalog.FloorPos(pos) : pos + Vector2.down * 0.2f;
            var go = ArtCatalog.Create("Inspectable", root, place, new Vector2(0.5f, 0.6f),
                SpriteFactory.Square, Palette.Inspectable, 3, "Props/inspect", Vector2.one * 0.9f);
            go.AddComponent<BoxCollider2D>().isTrigger = true;
            go.AddComponent<Inspectable>().note = note;
            TimeObject.Add(go, EraPresence.Both, Palette.Inspectable, Palette.FutureInspectable, solid: false);
        }

        public string Prompt(Era era) => "E: examinar";

        public void Interact(Era era) => GameManager.Instance.Inspect(era == Era.Past ? note.Past : note.Future);
    }
}
