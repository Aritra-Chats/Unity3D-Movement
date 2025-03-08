using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundedState : BaseState, IRootState
{
    public GroundedState(PlayerMovementStateManager context, PlayerMovementStateFactory factory) : base(context, factory)
    {
        IsRootState = true;
    }

    public override void EnterState()
    {
        InitializeSubState();
        HandleGravity();
    }
    public override void UpdateState()
    {
        CTX.Animator.SetFloat("Run Velocity X", CTX.AnimatorVelocity.x);
        CTX.Animator.SetFloat("Run Velocity Z", CTX.AnimatorVelocity.z);
        CheckSwitchState();
    }
    public override void ExitState()
    {
        CTX.Animator.SetFloat("Run Velocity X", 0f);
        CTX.Animator.SetFloat("Run Velocity Z", 0f);
    }

    public override void CheckSwitchState()
    {
        if (CTX.Controller.isGrounded && CTX.IsJumpPressed && !CTX.RequireNewJumpPress) SwitchState(Factory.Jumping());
        else if (CTX.Controller.isGrounded && CTX.IsCrouching) SwitchState(Factory.Crouching());
        else if (!CTX.Controller.isGrounded && !CTX.IsJumping)
        {
            RaycastHit hit;
            Ray ray = new Ray(CTX.transform.position, -Vector3.up);
            if(Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                if(hit.distance > 1f) SwitchState(Factory.Falling());
            }
        }
    }

    public override void InitializeSubState()
    {
        if (CTX.CurrentMovement.magnitude <= 0f) SetSubState(Factory.Idle());
        else if (CTX.CurrentMovement.magnitude > 0f && !CTX.IsRunPressed) SetSubState(Factory.Walking());
        else if (CTX.CurrentMovement.magnitude > 0f && CTX.IsRunPressed) SetSubState(Factory.Running());
    }

    public void HandleGravity()
    {
        CTX.MovementY = -CTX.GroundedGravity;
        CTX.AppliedMovementY = -CTX.GroundedGravity;
    }
}
