using System.Linq;
using UnityEngine;

namespace EcoDoTempo
{
    // A placed level prefab: background and objects live in the asset, not in the text map.
    public static class Level3Layout
    {
        public const string ResourcePath = "Levels/Level3";

        public static bool Available => Resources.Load<GameObject>(ResourcePath) != null;

        public static string PathFor(int index) => "Levels/Level" + (index + 1);

        public static bool HasPrefab(int index) => Resources.Load<GameObject>(PathFor(index)) != null;

        public static GameObject Create(
            int index,
            LevelDefinition level,
            out PlayerController player,
            out LevelBackground background,
            out int fragments,
            out Vector2Int size,
            out float floorTop)
        {
            var prefab = Resources.Load<GameObject>(PathFor(index));
            var root = Object.Instantiate(prefab);
            root.name = "Level " + (index + 1);

            player = root.GetComponentInChildren<PlayerController>(true);
            if (player != null) player.EnsureBound();

            background = root.GetComponentInChildren<LevelBackground>(true);
            background?.UsePlacedSprite();

            var gm = GameManager.Instance;
            foreach (var obj in root.GetComponentsInChildren<TimeObject>(true))
                gm.Register(obj);

            var gates = root.GetComponentsInChildren<Gate>(true)
                .OrderBy(g => g.transform.position.x).ToArray();
            for (int i = 0; i < gates.Length; i++)
            {
                gates[i].Bind(gates[i].GetComponent<TimeObject>(), i);
                gm.RegisterGate(gates[i]);
            }

            var levers = root.GetComponentsInChildren<Lever>(true)
                .OrderBy(l => l.transform.position.x).ToArray();
            for (int i = 0; i < levers.Length; i++)
                levers[i].BindChannel(i);

            var inspects = root.GetComponentsInChildren<Inspectable>(true)
                .OrderBy(n => n.transform.position.x).ToArray();
            for (int i = 0; i < inspects.Length; i++)
            {
                var note = i < level.Notes.Length
                    ? level.Notes[i]
                    : new InspectNote("Nada de especial.", "Nada de especial.");
                inspects[i].Bind(note);
            }

            foreach (var door in root.GetComponentsInChildren<ExitDoor>(true))
                door.EnsureBound();

            fragments = root.GetComponentsInChildren<MelodyFragment>(true).Length;
            size = new Vector2Int(58, 16);
            var spawned = player != null ? (Vector2)player.transform.position : Vector2.zero;
            floorTop = spawned.y - 0.45f;
            return root;
        }
    }
}
