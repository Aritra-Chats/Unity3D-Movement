using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

public class PlayerFootIKSolver : MonoBehaviour
{
    #region Variables
    public CharacterController Controller;
    public LayerMask GroundLayer;
    public bool EnableFeetIk;
    [Range(0f,2f)] public float HeightFromGroundRayCast = 1.14f;
    [Range(0f,2f)] public float rayCastDownDistance = 0.75f;
    public float PelvisOffset;
    [Range(0f, 1f)] public float WalkPelvisUpAndDownSpeed = 0.25f;
    [Range(0f, 1f)] public float RunPelvisUpAndDownSpeed = 0.5f;
    [Range(0f, 1f)] public float FeetToIkPositionSpeed = 0.5f;
    public string LeftFootAnimVariableName = "LeftFootCurve";
    public string RightFootAnimVariableName = "RightFootCurve";

    public bool UseProIkFeatures;
    public bool ShowSolverDebug;

    private Animator animator;
    private PlayerInput _inputs;
    private Vector3 _rightFootPosition, _leftFootPosition, _rightFootIkPosition, _leftFootIkPosition;
    private Quaternion _leftFootIkRotation, _rightFootIkRotation;
    private float _lastPelvisPositionY, _lastLeftFootPostionY, _lastRightFootPostionY;
    private float _pelvisUpAndDownSpeed;
    #endregion

    #region Input System
    private void Awake()
    {
        _inputs = new PlayerInput();

        _inputs.CharacterControls.Sprint.started += OnRun;
        _inputs.CharacterControls.Sprint.canceled += OnRun;
    }
    private void OnEnable()
    {
        _inputs.CharacterControls.Enable();
    }
    private void OnDisable()
    {
        _inputs.CharacterControls.Disable();
    }
    private void OnRun(InputAction.CallbackContext context)
    {
        _pelvisUpAndDownSpeed = context.ReadValueAsButton() ? RunPelvisUpAndDownSpeed : WalkPelvisUpAndDownSpeed;
    }
    #endregion

    private void Start()
    {
        animator = GetComponent<Animator>();
        _pelvisUpAndDownSpeed = WalkPelvisUpAndDownSpeed;
    }

    private void FixedUpdate()
    {
        if (!EnableFeetIk) return;
        if (animator == null) return;
        AdjustFeetTarget(ref _rightFootPosition, HumanBodyBones.RightFoot);
        AdjustFeetTarget(ref _leftFootPosition, HumanBodyBones.LeftFoot);

        //Find and Raycast tot find Positions
        FeetPositionSolver(_rightFootPosition, ref _rightFootIkPosition, ref _rightFootIkRotation);
        FeetPositionSolver(_leftFootPosition, ref _leftFootIkPosition, ref _leftFootIkRotation) ;
    }
    private void OnAnimatorIK(int layerIndex)
    {
        if (!EnableFeetIk || animator == null) return;
        if (Controller.isGrounded) MovePelvisHeight();
        else if (!Controller.isGrounded)
        {
            _lastPelvisPositionY = animator.bodyPosition.y;
            _lastLeftFootPostionY = animator.GetIKPosition(AvatarIKGoal.LeftFoot).y;
            _lastRightFootPostionY = animator.GetIKPosition(AvatarIKGoal.RightFoot).y;
        }

        //Use Pro Features
        if(UseProIkFeatures)
        {
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, animator.GetFloat(RightFootAnimVariableName));
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, animator.GetFloat(LeftFootAnimVariableName));
        }
        //Ik position and rotation
        if (Controller.isGrounded)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);

            MoveFeetToIkPoint(AvatarIKGoal.RightFoot, _rightFootIkPosition, _rightFootIkRotation, ref _lastRightFootPostionY);
            MoveFeetToIkPoint(AvatarIKGoal.LeftFoot, _leftFootIkPosition, _leftFootIkRotation, ref _lastLeftFootPostionY);
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
        }
    }
    void MoveFeetToIkPoint(AvatarIKGoal foot, Vector3 positionIkHolder, Quaternion rotationIkHolder, ref float lastFootPositionY)
    {
        Vector3 targetIkPosition = animator.GetIKPosition(foot);
        if(positionIkHolder != Vector3.zero)
        {
            targetIkPosition = transform.InverseTransformPoint(targetIkPosition);
            positionIkHolder = transform.InverseTransformPoint(positionIkHolder);

            float yVariable = Mathf.Lerp(lastFootPositionY, positionIkHolder.y, FeetToIkPositionSpeed);
            targetIkPosition.y += yVariable;

            lastFootPositionY = yVariable;

            targetIkPosition = transform.TransformPoint(targetIkPosition);

            animator.SetIKRotation(foot, rotationIkHolder);
        }

        animator.SetIKPosition(foot, targetIkPosition);
    }
    void MovePelvisHeight()
    {
        if (_rightFootIkPosition == Vector3.zero || _leftFootIkPosition == Vector3.zero || _lastPelvisPositionY == 0f)
        {
            _lastPelvisPositionY = animator.bodyPosition.y;
            return;
        }

        float lOffsetPosition = _leftFootIkPosition.y - transform.position.y;
        float rOffsetPosition = _rightFootIkPosition.y - transform.position.y;

        float totalOffset = lOffsetPosition < rOffsetPosition ? lOffsetPosition : rOffsetPosition;

        Vector3 newPelvisPosition = animator.bodyPosition + Vector3.up * totalOffset;

        newPelvisPosition.y = Mathf.Lerp(_lastPelvisPositionY, newPelvisPosition.y, _pelvisUpAndDownSpeed);

        animator.bodyPosition = newPelvisPosition;

        _lastPelvisPositionY = animator.bodyPosition.y;
    }
    void FeetPositionSolver(Vector3 fromSkyPosition, ref Vector3 feetIkPositions, ref Quaternion feetIkRotations)
    {
        //Raycast Handling Section
        RaycastHit feetOutHit;

        if(ShowSolverDebug) Debug.DrawLine(fromSkyPosition, fromSkyPosition + Vector3.down * (rayCastDownDistance+HeightFromGroundRayCast), Color.red);

        if(Physics.Raycast(fromSkyPosition, Vector3.down, out  feetOutHit, rayCastDownDistance+HeightFromGroundRayCast, GroundLayer))
        {
            //Finding Feet IK Position from Sky Position
            feetIkPositions = fromSkyPosition;
            feetIkPositions.y = feetOutHit.point.y + PelvisOffset;
            feetIkRotations = Quaternion.FromToRotation(Vector3.up, feetOutHit.normal) * transform.rotation;
            return;
        }

        feetIkPositions = Vector3.zero; //Didn't Work
    }
    void AdjustFeetTarget(ref Vector3 feetPositions, HumanBodyBones foot)
    {
        feetPositions = animator.GetBoneTransform(foot).position;
        feetPositions.y = transform.position.y + HeightFromGroundRayCast;
    }
}
