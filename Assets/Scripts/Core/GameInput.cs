using UnityEngine;
#if !ENABLE_LEGACY_INPUT_MANAGER && ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace EcoDoTempo
{
    // Works with either the legacy Input Manager or the new Input System package.
    public static class GameInput
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        static bool Held(params KeyCode[] keys) { foreach (var k in keys) if (Input.GetKey(k)) return true; return false; }
        static bool Down(params KeyCode[] keys) { foreach (var k in keys) if (Input.GetKeyDown(k)) return true; return false; }
        static bool Up(params KeyCode[] keys) { foreach (var k in keys) if (Input.GetKeyUp(k)) return true; return false; }

        public static float Horizontal =>
            (Held(KeyCode.D, KeyCode.RightArrow) ? 1f : 0f) - (Held(KeyCode.A, KeyCode.LeftArrow) ? 1f : 0f);
        public static bool JumpPressed => Down(KeyCode.Space, KeyCode.W, KeyCode.UpArrow);
        public static bool JumpReleased => Up(KeyCode.Space, KeyCode.W, KeyCode.UpArrow);
        public static bool TimeSwitchPressed => Down(KeyCode.Q, KeyCode.LeftShift);
        public static bool InteractPressed => Down(KeyCode.E);
        public static bool ConfirmPressed => Down(KeyCode.Return, KeyCode.KeypadEnter);
        public static bool RestartPressed => Down(KeyCode.R);
#elif ENABLE_INPUT_SYSTEM
        static bool Held(params Key[] keys) { var kb = Keyboard.current; if (kb == null) return false; foreach (var k in keys) if (kb[k].isPressed) return true; return false; }
        static bool Down(params Key[] keys) { var kb = Keyboard.current; if (kb == null) return false; foreach (var k in keys) if (kb[k].wasPressedThisFrame) return true; return false; }
        static bool Up(params Key[] keys) { var kb = Keyboard.current; if (kb == null) return false; foreach (var k in keys) if (kb[k].wasReleasedThisFrame) return true; return false; }

        public static float Horizontal =>
            (Held(Key.D, Key.RightArrow) ? 1f : 0f) - (Held(Key.A, Key.LeftArrow) ? 1f : 0f);
        public static bool JumpPressed => Down(Key.Space, Key.W, Key.UpArrow);
        public static bool JumpReleased => Up(Key.Space, Key.W, Key.UpArrow);
        public static bool TimeSwitchPressed => Down(Key.Q, Key.LeftShift);
        public static bool InteractPressed => Down(Key.E);
        public static bool ConfirmPressed => Down(Key.Enter, Key.NumpadEnter);
        public static bool RestartPressed => Down(Key.R);
#endif
    }
}
