using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingState : BaseState
{
    public WalkingState(PlayerMovementStateManager context, PlayerMovementStateFactory factory) : base(context, factory)
    {
    }

    public override void EnterState()
    {
    }
    public override void UpdateState()
    {
        HandleMovement();
        CheckSwitchState();
    }
    public override void ExitState()
    {
    }

    public override void InitializeSubState()
    {
    }

    public override void CheckSwitchState()
    {
        if (CTX.CurrentMovement.magnitude <= 0f) SwitchState(Factory.Idle());
        else if (CTX.CurrentMovement.magnitude > 0f && CTX.IsRunPressed) SwitchState(Factory.Running());
    }
    
    void HandleMovement()
    {
        float targetMoveAngle = Mathf.Atan2(CTX.CurrentMovement.x, CTX.CurrentMovement.z) * Mathf.Rad2Deg + CTX.Camera.eulerAngles.y;
        Vector3 moveDir;
        if (CTX.IsCrouching) moveDir = Quaternion.Euler(0f, targetMoveAngle, 0f) * Vector3.forward * CTX.PlayerVelocity * CTX.CrouchStepDeductor;
        else moveDir = Quaternion.Euler(0f, targetMoveAngle, 0f) * Vector3.forward * CTX.PlayerVelocity;
        CTX.AppliedMovementX = moveDir.x;
        CTX.AppliedMovementZ = moveDir.z;
    }
}
