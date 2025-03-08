using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : BaseState
{
    public IdleState(PlayerMovementStateManager context, PlayerMovementStateFactory factory) : base(context, factory)
    {
    }

    public override void EnterState()
    {
        CTX.AppliedMovementX = 0f;
        CTX.AppliedMovementZ = 0f;
    }
    public override void UpdateState()
    {
        CTX.AppliedMovementX = 0f;
        CTX.AppliedMovementZ = 0f;
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
        if (CTX.CurrentMovement.magnitude > 0f && !CTX.IsRunPressed) SwitchState(Factory.Walking());
        else if(CTX.CurrentMovement.magnitude > 0f && CTX.IsRunPressed) SwitchState(Factory.Running());
    }
}
