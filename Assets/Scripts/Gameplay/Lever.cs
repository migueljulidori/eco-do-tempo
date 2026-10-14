using UnityEngine;

namespace EcoDoTempo
{
    // Only works in the past; opens gates on the same channel in every era.
    public class Lever : MonoBehaviour, IInteractable
    {
        bool pulled;
        Vector2 cell;
        int channel;

        public static void Create(Transform root, Vector2 pos, int channel = 0)
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
            lever.channel = channel;
            TimeObject.Add(go, EraPresence.Both, Palette.Lever, Palette.FutureLever, solid: false);
        }

        public void BindChannel(int ch)
        {
            channel = ch;
            cell = transform.position;
        }

        public string Prompt(Era era) =>
            pulled ? "A alavanca já foi puxada" : "E: puxar alavanca";

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
                ArtCatalog.SnapBottomTo(gameObject, ArtCatalog.FloorTop(cell));
            }
            else if (!ArtCatalog.Has("Props/lever"))
                transform.rotation = Quaternion.Euler(0f, 0f, -30f);
            gm.OpenGates(channel);
            gm.Synth.PlayNote(220f + channel * 40f, 0.4f);
            string which = channel <= 0 ? "A comporta da oficina"
                : channel == 1 ? "A segunda comporta"
                : "Uma comporta distante";
            gm.ShowMessage($"{which} se abriu... em todas as eras.");
        }
    }
}
