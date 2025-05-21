using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class JumpState : BaseState, IRootState
{
    public JumpState(PlayerMovementStateManager context, PlayerMovementStateFactory factory) : base(context, factory)
    {
        IsRootState = true;
    }

    public override void EnterState()
    {
        InitializeSubState();
        HandleJump();
    }
    public override void UpdateState()
    {
        if (CTX.IsJumping) HandleJumpAnimation();
        HandleGravity();
        if (CTX.IsHanging)
        {
            if (CTX.IsJumpPressed && !CTX.RequireNewJumpPress) HandleJump();
            if (CTX.IsCrouching)
            {
                CTX.IsHanging = false;
                CTX.Animator.SetBool("isHanging", false);
            }
        }
        CheckSwitchState();
    }
    public override void ExitState()
    {
        CTX.Animator.SetFloat("Jump Velocity", 0);
        CTX.Animator.SetBool("isJumping", false);
        CTX.IsJumping = false;
    }

    public override void InitializeSubState()
    {
        if (!CTX.IsMovementPressed) SetSubState(Factory.Idle());
        else if (CTX.IsMovementPressed && !CTX.IsRunPressed) SetSubState(Factory.Walking());
        else if (CTX.IsMovementPressed && CTX.IsRunPressed) SetSubState(Factory.Running());
    }

    public override void CheckSwitchState()
    {
        if (CTX.Controller.isGrounded) SwitchState(Factory.Grounded());
    }

    void HandleJump()
    {
        if (CTX.IsHanging)
        {
            CTX.IsHanging = false;
            CTX.Animator.SetBool("isHanging", false);
            CTX.Animator.SetBool("isJumping", true);
            HandleJumpAnimation();
            CTX.IsJumping = true;
            CTX.RequireNewJumpPress = true;
            CTX.MovementY = 10f;
            CTX.AppliedMovementY = 10f;
        }
        else
        {
            CTX.Animator.SetBool("isJumping", true);
            HandleJumpAnimation();
            CTX.IsJumping = true;
            CTX.RequireNewJumpPress = true;
            CTX.MovementY = CTX.InitialJumpVelocity;
            CTX.AppliedMovementY = CTX.InitialJumpVelocity;
        }
    }
    public void HandleGravity()
    {
        bool isFalling = CTX.Controller.velocity.y <= 0f;
        float fallMultiplier = 1.5f;
        if (isFalling)
        {
            if(CTX.IsJumpPressed) HandleLedgeGrab();
            float previousYVelocity = CTX.MovementY;
            CTX.MovementY = previousYVelocity - (CTX.AppliedGravity * fallMultiplier * Time.deltaTime);
            float newYVelocity = (previousYVelocity + CTX.MovementY) * 0.5f;
            CTX.AppliedMovementY = newYVelocity;
        }
        else
        {
            float previousYVelocity = CTX.MovementY;
            CTX.MovementY = previousYVelocity - (CTX.AppliedGravity * Time.deltaTime);
            float newYVelocity = (previousYVelocity + CTX.MovementY) * 0.5f;
            CTX.AppliedMovementY = newYVelocity;
        }
    }

    public void HandleJumpAnimation()
    {
        if (!CTX.IsMovementPressed || (CTX.IsMovementPressed && !CTX.IsRunPressed)) CTX.Animator.SetFloat("Jump Velocity", 1f);
        else if (CTX.IsMovementPressed && CTX.IsRunPressed) CTX.Animator.SetFloat("Jump Velocity", 2f);
        else CTX.Animator.SetFloat("Jump Velocity", 0f);
    }

    void HandleLedgeGrab()
    {
        if (!CTX.IsHanging)
        {
            RaycastHit downHit;
            Physics.Linecast(CTX.DownLineStart.position, CTX.DownLineEnd.position, out downHit, CTX.LedgeLayer);
            Debug.DrawLine(CTX.DownLineStart.position, CTX.DownLineEnd.position, Color.red);
            if (downHit.collider != null)
            {
                RaycastHit fwdHitDown;
                RaycastHit fwdHitUp;
                Vector3 lineFwdDownStart = new Vector3(CTX.transform.position.x, downHit.point.y - 0.1f, CTX.transform.position.z);
                Vector3 lineFwdDownEnd = new Vector3(CTX.transform.position.x, downHit.point.y - 0.1f, CTX.transform.position.z) + CTX.transform.forward * 0.4f;
                Vector3 lineFwdUpStart = new Vector3(CTX.transform.position.x, downHit.point.y + 0.02f, CTX.transform.position.z);
                Vector3 lineFwdUpEnd = new Vector3(CTX.transform.position.x, downHit.point.y + 0.02f, CTX.transform.position.z) + CTX.transform.forward * 0.4f;
                Physics.Linecast(lineFwdDownStart, lineFwdDownEnd, out fwdHitDown, CTX.LedgeLayer);
                Physics.Linecast(lineFwdUpStart, lineFwdUpEnd, out fwdHitUp, LayerMask.GetMask("Obstacle"));

                
                if (fwdHitDown.collider != null && fwdHitUp.collider == null)
                {
                    CTX.Animator.SetBool("isHanging", true);
                    CTX.IsHanging = true;
                    CTX.IsJumping = false;
                    Vector3 hangPos = (new Vector3(fwdHitDown.point.x, downHit.point.y, fwdHitDown.point.z - 0.2f));
                    Vector3 offset = new Vector3(0, CTX.HangYOffset, CTX.HangZOffset);
                    CTX.transform.position = hangPos;
                    CTX.PlayerSubParent.position = hangPos + offset;
                    CTX.transform.forward = -fwdHitDown.normal;
                }
            }
        }
    }
}
