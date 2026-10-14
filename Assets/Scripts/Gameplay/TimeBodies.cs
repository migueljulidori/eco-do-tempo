using System.Collections.Generic;
using UnityEngine;

namespace EcoDoTempo
{
    // Things with weight: Lia, her echo and iron crates. They press plates and keep gates from closing.
    public static class TimeBodies
    {
        static readonly List<Collider2D> hits = new List<Collider2D>();

        // Includes triggers, so a replaying echo (trigger collider) still counts.
        public static ContactFilter2D AllContacts => new ContactFilter2D().NoFilter();

        public static bool IsBody(Collider2D col) =>
            col != null
            && (col.GetComponentInParent<PlayerController>() != null
                || col.GetComponentInParent<TimeCrate>() != null
                || col.GetComponentInParent<EchoGhost>() != null);

        public static bool AnyBodyIn(Vector2 center, Vector2 size)
        {
            int count = Physics2D.OverlapBox(center, size, 0f, AllContacts, hits);
            for (int i = 0; i < count; i++)
                if (IsBody(hits[i])) return true;
            return false;
        }

        // Small colored diamond that ties a lever, plate, clock and gate of one channel together.
        public static void AddChannelBadge(GameObject host, Vector2 worldPos, int channel)
        {
            var badge = SpriteFactory.CreateObject("Canal", host.transform, worldPos, Vector2.one * 0.22f,
                SpriteFactory.Diamond, Palette.Channel(channel), 6);
            var parentScale = host.transform.lossyScale;
            badge.transform.localScale = new Vector3(
                0.22f / Mathf.Max(0.01f, parentScale.x), 0.22f / Mathf.Max(0.01f, parentScale.y), 1f);
            badge.transform.position = worldPos;
        }
    }
}
