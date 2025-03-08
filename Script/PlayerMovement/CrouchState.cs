using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrouchState : BaseState, IRootState
{
    public CrouchState(PlayerMovementStateManager context, PlayerMovementStateFactory factory) : base(context, factory)
    {
        IsRootState = true;
    }

    public override void EnterState()
    {
        InitializeSubState();
        CTX.Animator.SetBool("isCrouching", true);
        HandleGravity();
    }
    public override void UpdateState()
    {
        CTX.Animator.SetFloat("Crouch Velocity", CTX.AnimatorVelocity.magnitude);
        CheckSwitchState();
    }
    public override void ExitState()
    {
        CTX.Animator.SetFloat("Crouch Velocity", 0f);
        CTX.Animator.SetBool("isCrouching", false);
    }

    public override void InitializeSubState()
    {
        if (CTX.CurrentMovement.magnitude <= 0f) SetSubState(Factory.Idle());
        else if (CTX.CurrentMovement.magnitude > 0f && !CTX.IsRunPressed) SetSubState(Factory.Walking());
        else if (CTX.CurrentMovement.magnitude > 0f && CTX.IsRunPressed) SetSubState(Factory.Running());
    }

    public override void CheckSwitchState()
    {
        if (!CTX.IsCrouching) SwitchState(Factory.Grounded());
        else if(!CTX.Controller.isGrounded) SwitchState(Factory.Falling());;
    }
    public void HandleGravity()
    {
        CTX.MovementY = -CTX.GroundedGravity;
        CTX.AppliedMovementY = -CTX.GroundedGravity;
    }
}
