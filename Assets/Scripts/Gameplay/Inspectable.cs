using UnityEngine;

namespace EcoDoTempo
{
    // Cube Escape style: the same object tells a different story in each era.
    public class Inspectable : MonoBehaviour, IInteractable
    {
        InspectNote note;

        public static void Create(Transform root, Vector2 pos, InspectNote note, bool gardenProps = false)
        {
            string pastArt = gardenProps && ArtCatalog.Has("Props/diary") ? "Props/diary" : "Props/inspect";
            string futureArt = gardenProps && ArtCatalog.Has("Props/plaque") ? "Props/plaque" : pastArt;
            var go = ArtCatalog.Create("Inspectable", root, pos, Vector2.one,
                SpriteFactory.Square, Palette.Inspectable, 3, pastArt, Vector2.one);
            if (ArtCatalog.Has(pastArt))
                ArtCatalog.SnapBottomTo(go, ArtCatalog.FloorTop(pos));
            ArtCatalog.SetEraArt(go, pastArt, futureArt, Vector2.one);
            go.AddComponent<BoxCollider2D>().isTrigger = true;
            go.AddComponent<Inspectable>().note = note;
            SoftGlow.Attach(go, new Color(1f, 0.92f, 0.55f, 0.6f), 1.15f, 2);
            TimeObject.Add(go, EraPresence.Both, Palette.Inspectable, Palette.FutureInspectable, solid: false);
        }

        public string Prompt(Era era) => "E: examinar";

        public void Interact(Era era) => GameManager.Instance.Inspect(era == Era.Past ? note.Past : note.Future);
    }
}
