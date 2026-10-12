using UnityEngine;

namespace EcoDoTempo
{
    // Cube Escape style: the same object tells a different story in each era.
    public class Inspectable : MonoBehaviour, IInteractable
    {
        InspectNote note;

        public static void Create(Transform root, Vector2 pos, InspectNote note)
        {
            var go = ArtCatalog.Create("Inspectable", root, pos, new Vector2(0.5f, 0.6f),
                SpriteFactory.Square, Palette.Inspectable, 3, "Props/inspect", Vector2.one * 0.9f);
            if (ArtCatalog.Has("Props/inspect"))
                ArtCatalog.SnapBottomTo(go, ArtCatalog.FloorTop(pos));
            go.AddComponent<BoxCollider2D>().isTrigger = true;
            go.AddComponent<Inspectable>().note = note;
            SoftGlow.Attach(go, new Color(1f, 0.92f, 0.55f, 0.6f), 1.15f, 2);
            TimeObject.Add(go, EraPresence.Both, Palette.Inspectable, Palette.FutureInspectable, solid: false);
        }

        public string Prompt(Era era) => "E: examinar";

        public void Interact(Era era) => GameManager.Instance.Inspect(era == Era.Past ? note.Past : note.Future);
    }
}
