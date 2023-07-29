using JetBrains.Annotations;
using UnityEngine;

namespace Multiplayer.Editor.Components.Player
{
    public class AnimationHandler : MonoBehaviour
    {
        [SerializeField]
        private Animator animator;

        private static readonly int hash_Jump = Animator.StringToHash("Jump");
        private static readonly int hash_Vertical = Animator.StringToHash("Vertical");
        private static readonly int hash_Horizontal = Animator.StringToHash("Horizontal");

        [UsedImplicitly]
        public void SetIsJumping(bool isJumping)
        {
            animator.SetBool(hash_Jump, isJumping);
        }

        [UsedImplicitly]
        public void SetMoveDir(Vector2 moveDir)
        {
            animator.SetFloat(hash_Horizontal, moveDir.x);
            animator.SetFloat(hash_Vertical, moveDir.y);
        }
    }
}
