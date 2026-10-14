using System.Collections.Generic;
using UnityEngine;

namespace EcoDoTempo
{
    // Power "Eco" (F): record a few seconds of Lia, then rewind her to where it began.
    // An echo replays the recording and stays where it ended (one echo at a time).
    public class EchoSystem : MonoBehaviour
    {
        public const float MaxSeconds = 8f;
        const int MinFrames = 8;

        readonly List<EchoFrame> frames = new List<EchoFrame>();
        Vector2 startPosition;
        Era startEra;
        bool pendingInteract;
        EchoGhost ghost;

        public bool Recording { get; private set; }
        public float RecordedSeconds => frames.Count * Time.fixedDeltaTime;
        public float RecordProgress => Mathf.Clamp01(RecordedSeconds / MaxSeconds);
        public bool HasEcho => ghost != null;

        public void Toggle()
        {
            if (Recording) Finish();
            else Begin();
        }

        void Begin()
        {
            var gm = GameManager.Instance;
            if (gm.Player == null) return;
            ClearEcho();
            frames.Clear();
            startPosition = gm.Player.transform.position;
            startEra = gm.CurrentEra;
            pendingInteract = false;
            Recording = true;
            gm.Synth.PlayNote(988f, 0.15f, 0.35f);
            gm.ShowMessage("Gravando o eco... aperte F para voltar no tempo.", 2f);
        }

        public void NoteInteract()
        {
            if (Recording) pendingInteract = true;
        }

        void FixedUpdate()
        {
            if (!Recording) return;
            var gm = GameManager.Instance;
            if (gm == null || gm.State != GameState.Playing) return;
            var player = gm.Player;
            if (player == null)
            {
                Cancel();
                return;
            }

            frames.Add(player.Snapshot(pendingInteract));
            pendingInteract = false;
            if (RecordedSeconds >= MaxSeconds) Finish();
        }

        void Finish()
        {
            var gm = GameManager.Instance;
            Recording = false;
            if (frames.Count < MinFrames || gm.Player == null)
            {
                frames.Clear();
                gm.ShowMessage("O eco foi curto demais para ficar.", 2f);
                return;
            }

            // Time rewinds for Lia: back to the start point and the era she was in.
            gm.Player.TeleportTo(startPosition);
            if (gm.CurrentEra != startEra) gm.TrySwitchEra(silentWhenBlocked: true);

            ghost = EchoGhost.Create(gm.LevelRoot != null ? gm.LevelRoot : transform, frames.ToArray(),
                PlayerController.BodyColliderSize);
            frames.Clear();
            gm.Synth.PlayRewind();
            gm.ShowMessage("O tempo voltou. Seu eco repete o que você fez.", 2.5f);
        }

        public void Cancel()
        {
            Recording = false;
            frames.Clear();
            pendingInteract = false;
        }

        public void ClearEcho()
        {
            if (ghost != null) Destroy(ghost.gameObject);
            ghost = null;
        }

        public void ResetAll()
        {
            Cancel();
            ClearEcho();
        }
    }
}
