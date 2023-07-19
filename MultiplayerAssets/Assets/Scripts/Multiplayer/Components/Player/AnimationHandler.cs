using UnityEngine;

namespace Multiplayer.Editor.Components.Player
{
    public class AnimationHandler : MonoBehaviour
    {
        [SerializeField]
        private float animSpeed = 1.5f;
        [SerializeField]
        private Animator animator;

        private AnimatorStateInfo currentBaseState;
        private bool shouldJump;

        private static readonly int idleState = Animator.StringToHash("Base Layer.Idle");
        private static readonly int locoState = Animator.StringToHash("Base Layer.Locomotion");
        private static readonly int jumpState = Animator.StringToHash("Base Layer.Jump");
        private static readonly int restState = Animator.StringToHash("Base Layer.Rest");
        private static readonly int jump = Animator.StringToHash("Jump");
        private static readonly int rest = Animator.StringToHash("Rest");
        private static readonly int direction = Animator.StringToHash("Direction");
        private static readonly int speed = Animator.StringToHash("Speed");

        private void Update()
        {
            animator.speed = animSpeed;
            currentBaseState = animator.GetCurrentAnimatorStateInfo(0);
            if (currentBaseState.fullPathHash == locoState)
            {
                if (!shouldJump || animator.IsInTransition(0))
                    return;
                animator.SetBool(jump, true);
                shouldJump = false;
            }
            else if (currentBaseState.fullPathHash == jumpState)
            {
                if (animator.IsInTransition(0))
                    return;
                animator.SetBool(jump, false);
            }
        }

        public void Jump()
        {
            shouldJump = true;
        }

        public void SetSpeed(float s)
        {
            animator.SetFloat(speed, s);
        }
    }
}
