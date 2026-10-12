using UnityEngine;

namespace EcoDoTempo
{
    // Only works in the past; its effect (open gates) carries over to the future.
    public class Lever : MonoBehaviour, IInteractable
    {
        bool pulled;

        public static void Create(Transform root, Vector2 pos)
        {
            var go = SpriteFactory.CreateObject("Lever", root, pos + Vector2.down * 0.15f, new Vector2(0.15f, 0.7f),
                SpriteFactory.Square, Palette.Lever, 3);
            go.transform.rotation = Quaternion.Euler(0f, 0f, 30f);
            go.AddComponent<BoxCollider2D>().isTrigger = true;
            go.AddComponent<Lever>();
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
            transform.rotation = Quaternion.Euler(0f, 0f, -30f);
            gm.OpenGates();
            gm.Synth.PlayNote(220f, 0.4f);
            gm.ShowMessage("Algo pesado se move ao longe. A comporta se abriu... em todas as eras.");
        }
    }
}
