using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallState : BaseState, IRootState
{
    public FallState(PlayerMovementStateManager context, PlayerMovementStateFactory factory) : base(context, factory)
    {
        IsRootState = true;
    }

    public override void EnterState()
    {
        InitializeSubState();
        CTX.Animator.SetBool("isFalling", true);
    }
    public override void UpdateState()
    {
        HandleGravity();
        CheckSwitchState();
    }
    public override void ExitState()
    {
        CTX.Animator.SetBool("isFalling", false);
    }

    public override void InitializeSubState()
    {
        SetSubState(Factory.Idle());
    }

    public override void CheckSwitchState()
    {
        if(CTX.Controller.isGrounded) SwitchState(Factory.Grounded());
    }
    public void HandleGravity()
    {
        float previousYVelocity = CTX.MovementY;
        CTX.MovementY = previousYVelocity - (CTX.AppliedGravity * Time.deltaTime);
        float NewYVelocity = (previousYVelocity + CTX.MovementY) * 0.5f;
        CTX.AppliedMovementY = NewYVelocity;
    }
}
