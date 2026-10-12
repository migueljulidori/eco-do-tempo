using UnityEngine;

namespace EcoDoTempo
{
    // Only works in the past; its effect (open gates) carries over to the future.
    public class Lever : MonoBehaviour, IInteractable
    {
        bool pulled;
        Vector2 cell;

        public static void Create(Transform root, Vector2 pos)
        {
            var go = ArtCatalog.Create("Lever", root, pos, new Vector2(0.15f, 0.7f),
                SpriteFactory.Square, Palette.Lever, 3, "Props/lever", Vector2.one * 0.9f);
            if (ArtCatalog.Has("Props/lever"))
                ArtCatalog.SnapBottomTo(go, ArtCatalog.FloorTop(pos));
            else
                go.transform.rotation = Quaternion.Euler(0f, 0f, 30f);
            go.AddComponent<BoxCollider2D>().isTrigger = true;
            var lever = go.AddComponent<Lever>();
            lever.cell = pos;
            TimeObject.Add(go, EraPresence.Both, Palette.Lever, Palette.FutureLever, solid: false);
        }

        public string Prompt(Era era) => pulled ? "A alavanca já foi puxada" : "E: puxar alavanca";

        public void Interact(Era era)
        {
            var gm = GameManager.Instance;
            if (pulled) return;
            if (era == Era.Future)
            {
                gm.ShowMessage("A alavanca está travada pela ferrugem. Ela precisava ter sido puxada há muito tempo...");
                gm.Synth.PlayBlocked();
                return;
            }
            pulled = true;
            if (ArtCatalog.Has("Props/lever_pulled"))
            {
                ArtCatalog.TryApply(gameObject, "Props/lever_pulled", Vector2.one * 0.9f);
                ArtCatalog.SnapBottomTo(gameObject, ArtCatalog.FloorTop(transform.position + Vector3.up * 0.5f));
            }
            else if (!ArtCatalog.Has("Props/lever"))
                transform.rotation = Quaternion.Euler(0f, 0f, -30f);
            gm.OpenGates();
            gm.Synth.PlayNote(220f, 0.4f);
            gm.ShowMessage("Algo pesado se move ao longe. A comporta se abriu... em todas as eras.");
        }
    }
}
