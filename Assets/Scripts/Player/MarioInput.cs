using UnityEngine;
using UnityEngine.InputSystem;

namespace MarioBros
{
    /// <summary>
    /// Sistema de entrada unificado compatible con el New Input System y Old Input Manager.
    /// Soporta teclas flechas, WASD, Espacio/Shift y botones clásicos Z/X de NES.
    /// </summary>
    public static class MarioInput
    {
        public static bool Left
        {
            get
            {
                var kb = Keyboard.current;
                if (kb != null && (kb.aKey.isPressed || kb.leftArrowKey.isPressed)) return true;
                try { return Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow); } catch { return false; }
            }
        }

        public static bool Right
        {
            get
            {
                var kb = Keyboard.current;
                if (kb != null && (kb.dKey.isPressed || kb.rightArrowKey.isPressed)) return true;
                try { return Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow); } catch { return false; }
            }
        }

        public static bool Up
        {
            get
            {
                var kb = Keyboard.current;
                if (kb != null && (kb.wKey.isPressed || kb.upArrowKey.isPressed)) return true;
                try { return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow); } catch { return false; }
            }
        }

        public static bool Down
        {
            get
            {
                var kb = Keyboard.current;
                if (kb != null && (kb.sKey.isPressed || kb.downArrowKey.isPressed)) return true;
                try { return Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow); } catch { return false; }
            }
        }

        public static bool Run
        {
            get
            {
                var kb = Keyboard.current;
                if (kb != null && (kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed || kb.xKey.isPressed)) return true;
                try { return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.X); } catch { return false; }
            }
        }

        public static bool JumpDown
        {
            get
            {
                var kb = Keyboard.current;
                if (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.zKey.wasPressedThisFrame)) return true;
                try { return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Z); } catch { return false; }
            }
        }

        public static bool JumpHeld
        {
            get
            {
                var kb = Keyboard.current;
                if (kb != null && (kb.spaceKey.isPressed || kb.zKey.isPressed)) return true;
                try { return Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Z); } catch { return false; }
            }
        }

        public static bool JumpUp
        {
            get
            {
                var kb = Keyboard.current;
                if (kb != null && (kb.spaceKey.wasReleasedThisFrame || kb.zKey.wasReleasedThisFrame)) return true;
                try { return Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.Z); } catch { return false; }
            }
        }
    }
}
