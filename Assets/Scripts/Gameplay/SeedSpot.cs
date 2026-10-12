using UnityEngine;

namespace EcoDoTempo
{
    // A seed planted in the past becomes a tree (platform) in the future.
    public class SeedSpot : MonoBehaviour, IInteractable
    {
        const int TreeHeight = 2;

        bool planted;
        TimeObject sapling;
        readonly TimeObject[] trunk = new TimeObject[TreeHeight];

        public static void Create(Transform root, Vector2 pos)
        {
            var soil = LevelBuilder.Block(root, pos, EraPresence.Both, Palette.PastSoil, Palette.FutureSoil);
            ArtCatalog.TryApply(soil.gameObject, "Tiles/soil", Vector2.one);
            var spot = soil.gameObject.AddComponent<SeedSpot>();

            var saplingGo = ArtCatalog.Create("Sapling", root, pos + Vector2.up, new Vector2(0.25f, 0.5f),
                SpriteFactory.Triangle, Palette.Sapling, 2, "Props/sapling", Vector2.one * 0.85f);
            if (ArtCatalog.Has("Props/sapling"))
                ArtCatalog.SnapBottomTo(saplingGo, ArtCatalog.FloorTop(pos + Vector2.up));
            spot.sapling = TimeObject.Add(saplingGo, EraPresence.PastOnly, Palette.Sapling, Palette.Sapling, solid: false);
            spot.sapling.ForceHidden = true;

            for (int i = 0; i < TreeHeight; i++)
            {
                spot.trunk[i] = LevelBuilder.Block(root, pos + Vector2.up * (i + 1), EraPresence.FutureOnly, Palette.Tree, Palette.Tree);
                spot.trunk[i].ForceHidden = true;
            }
        }

        public string Prompt(Era era) =>
            planted ? "Uma muda cresce aqui" : era == Era.Past ? "E: plantar semente" : "E: examinar a terra";

        public void Interact(Era era)
        {
            var gm = GameManager.Instance;
            if (planted) return;
            if (era == Era.Future)
            {
                gm.ShowMessage("A terra está seca e dura. Nada cresceria aqui agora... mas e antes?");
                return;
            }
            planted = true;
            sapling.ForceHidden = false;
            foreach (var block in trunk) block.ForceHidden = false;
            gm.ApplyEra();
            gm.Synth.PlayNote(330f, 0.5f);
            gm.ShowMessage("Você plantou a semente do avô. O tempo cuidará do resto.");
        }
    }
}
