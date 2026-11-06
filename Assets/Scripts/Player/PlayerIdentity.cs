using UnityEngine;
using UnityEngine.InputSystem;

namespace MarioBros.Player
{
    public class PlayerIdentity : MonoBehaviour
    {
        public int slot;
        public static Controls InputFor(Component player) => new Controls(player.GetComponent<PlayerIdentity>()?.slot ?? 0);
        public readonly struct Controls
        {
            private readonly int slot;
            public Controls(int slot) { this.slot = slot; }
            private bool Second => CoopSession.TwoPlayers && slot == 1;
            private bool Split => CoopSession.TwoPlayers;
            private Keyboard K => Keyboard.current;
            public bool Left => K != null && (Second ? K.leftArrowKey.isPressed : Split ? K.aKey.isPressed : MarioInput.Left);
            public bool Right => K != null && (Second ? K.rightArrowKey.isPressed : Split ? K.dKey.isPressed : MarioInput.Right);
            public bool Down => K != null && (Second ? K.downArrowKey.isPressed : Split ? K.sKey.isPressed : MarioInput.Down);
            public bool Up => K != null && (Second ? K.upArrowKey.isPressed : Split ? K.wKey.isPressed : MarioInput.Up);
            public bool Run => K != null && (Second ? K.rightShiftKey.isPressed : Split ? K.leftShiftKey.isPressed || K.xKey.isPressed : MarioInput.Run);
            public bool JumpDown => K != null && (Second ? K.rightCtrlKey.wasPressedThisFrame : MarioInput.JumpDown);
            public bool JumpHeld => K != null && (Second ? K.rightCtrlKey.isPressed : MarioInput.JumpHeld);
            public bool JumpUp => K != null && (Second ? K.rightCtrlKey.wasReleasedThisFrame : MarioInput.JumpUp);
            public bool FireDown => K != null && (Second ? K.rightAltKey.wasPressedThisFrame : K.leftShiftKey.wasPressedThisFrame || K.xKey.wasPressedThisFrame);
        }
    }
}
