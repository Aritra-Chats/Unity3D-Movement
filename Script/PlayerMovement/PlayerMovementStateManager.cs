using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

public class PlayerMovementStateManager : MonoBehaviour
{
    #region Variables
    //public objects
    public Transform Camera;
    public Transform DownLineStart;
    public Transform DownLineEnd;
    public Transform PlayerSubParent;
    public LayerMask LedgeLayer;
    public Animator Animator;
    public SwordFightingStateMachine SwordFighting;
    //public floats
    public float WalkAcceleration=2f;
    public float RunAcceleration=5f;
    public float MaxWalkVelocity=2f;
    public float MaxRunVelocity=5f;
    public float SmoothTurnTime = 0.2f;
    public float AppliedGravity = 29.43f;
    public float MaxJumpHeight = 1.5f;
    public float MaxJumpTime = 0.75f;
    public float GroundedGravity = 2f;
    public float HangYOffset = 0.31f;
    public float HangZOffset = 1.5f;
    //public animator floats
    public float AnimatorAcceleration;
    public float AnimatorDeceleration;
    public float MaxAnimatorWalkVelocity=0.5f;
    public float MaxAnimatorRunVelocity=2f;
    [Range(0f, 1f)] public float CrouchStepDeductor=0.4f;
    [HideInInspector] public bool IsJumping = false;
    //private objects
    CharacterController _controller;
    PlayerInput _inputs;
    //State Variables
    BaseState _currentState;
    PlayerMovementStateFactory _states;
    //private floats
    float _currentMaxAcceleration;
    float _currentMaxVelocity;
    float _smoothTurnVelocity;
    float _temp_gravity;
    float _velocity = 0f;
    float _initialJumpVelocity;
    float _jump_gravity;
    float _lookAngle;
    //private animator floats
    Vector3 _animatorVelocity;
    float _currentMaxAnimatorVelocity;
    //private vectors
    Vector2 _currentMovementInput;
    Vector3 _currentMovement;
    Vector3 _movement;
    Vector3 _appliedMovement;
    //private bool
    bool _isMovementPressed;
    bool _isForwardPressed;
    bool _isBackwardPressed;
    bool _isLeftPressed;
    bool _isRightPressed;
    bool _isRunPressed;
    bool _isJumpPressed;
    bool _requireNewJumpPress=false;
    bool _isCrouching = false;
    bool _isHanging;
    #endregion

    #region Getters & Setters
    //Objects
    public CharacterController Controller { get { return _controller; } }
    public BaseState CurrentState {  get { return _currentState; } set { _currentState = value; } }
    public Vector3 CurrentMovement { get { return _currentMovement; } }
    public float PlayerVelocity { get { return _velocity; } }
    public Vector3 AnimatorVelocity { get { return _animatorVelocity; } }
    public float InitialJumpVelocity { get { return _initialJumpVelocity; } }
    public float LookAngle { get { return _lookAngle; } }
    public float MovementX { get { return _movement.x; } set { _movement.x = value; } }
    public float MovementY { get { return _movement.y; } set { _movement.y = value; } }
    public float MovementZ { get { return _movement.z; } set { _movement.z = value; } }
    public float AppliedMovementX { get { return _appliedMovement.x; } set { _appliedMovement.x = value; } }
    public float AppliedMovementY { get { return _appliedMovement.y; } set { _appliedMovement.y = value; } }
    public float AppliedMovementZ { get { return _appliedMovement.z; } set { _appliedMovement.z = value; } }
    public bool IsMovementPressed { get {  return _isMovementPressed; } }
    public bool IsRunPressed { get { return _isRunPressed; } }
    public bool IsCrouching { get {  return _isCrouching; } }
    public bool IsJumpPressed {  get { return _isJumpPressed; } }
    public bool RequireNewJumpPress { get { return _requireNewJumpPress; } set { _requireNewJumpPress = value; } }
    public bool IsHanging { get { return _isHanging; } set { _isHanging = value; } }
    #endregion

    private void Awake()
    {
        //Defining Objects
        _controller = GetComponent<CharacterController>();
        _inputs = new PlayerInput();

        //setup state
        _states = new PlayerMovementStateFactory(this);
        _currentState = _states.Grounded();
        _currentState.EnterState();

        _inputs.CharacterControls.Move.started += OnMovementInput;
        _inputs.CharacterControls.Move.performed += OnMovementInput;
        _inputs.CharacterControls.Move.canceled += OnMovementInput;

        _inputs.CharacterControls.Jump.started += OnJump;
        _inputs.CharacterControls.Jump.canceled += OnJump;

        _inputs.CharacterControls.Crouch.started += OnCrouch;
        _inputs.CharacterControls.Crouch.canceled += OnCrouch;

        _inputs.CharacterControls.Sprint.started += OnSprint;
        _inputs.CharacterControls.Sprint.canceled += OnSprint;

        _temp_gravity = AppliedGravity;
        SetupJumpVariables();
    }

    void SetupJumpVariables()
    {
        float TimeToApex = MaxJumpTime / 2;
        _jump_gravity = (2 * MaxJumpHeight) / Mathf.Pow(TimeToApex, 2);
        _initialJumpVelocity = (2 * MaxJumpHeight) / TimeToApex;
    }

    #region Input System
    private void OnEnable()
    {
        _inputs.CharacterControls.Enable();
    }

    private void OnDisable()
    {
        _inputs.CharacterControls.Disable();
    }

    void OnMovementInput(InputAction.CallbackContext context)
    {
        _currentMovementInput = context.ReadValue<Vector2>();
        _currentMovement.x = _currentMovementInput.x;
        _currentMovement.z = _currentMovementInput.y;
        _isMovementPressed = _currentMovementInput.x != 0f || _currentMovementInput.y != 0f;
        _isForwardPressed = _currentMovementInput.y > 0f;
        _isBackwardPressed = _currentMovementInput.y < 0f;
        _isRightPressed = _currentMovementInput.x > 0f;
        _isLeftPressed = _currentMovementInput.x < 0f;
    }

    void OnJump(InputAction.CallbackContext context)
    {
        _isJumpPressed = context.ReadValueAsButton();
        _requireNewJumpPress = false;
    }

    void OnCrouch(InputAction.CallbackContext context)
    {
        _isCrouching = context.ReadValueAsButton();
    }
    
    void OnSprint(InputAction.CallbackContext context)
    {
        _isRunPressed = context.ReadValueAsButton();
    }
    #endregion

    private void Update()
    {
        _currentMaxAcceleration = _isRunPressed ? RunAcceleration : WalkAcceleration;
        _currentMaxVelocity = _isRunPressed ? MaxRunVelocity : MaxWalkVelocity;
        _currentMaxAnimatorVelocity = _isRunPressed ? MaxAnimatorRunVelocity : MaxAnimatorWalkVelocity;
        AppliedGravity = _isJumpPressed ? _jump_gravity : _temp_gravity;

        //Function Calling
        if (!_isHanging)
        {
            HandleRotation();
            HandleVelocity();
            HandleAnimatorVelocity();
        }

        //Updating States
        _currentState.UpdateStates();

        //Applying Movement
        if(!_isHanging) _controller.Move(_appliedMovement * Time.deltaTime);
        else if(_isHanging)
        {
            _appliedMovement = Vector3.zero;
            _controller.Move(_appliedMovement);
        }
    }

    //Controls Direction of player during movement;
    void HandleRotation()
    {
        if (_currentMovement.magnitude > 0.1f)
        {
            float targetLookAngle = Mathf.Atan2(_currentMovement.x, _currentMovement.z) * Mathf.Rad2Deg + Camera.eulerAngles.y;
            _lookAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetLookAngle, ref _smoothTurnVelocity, SmoothTurnTime);
            transform.rotation = Quaternion.Euler(0f, _lookAngle, 0f);
        }
    }

    //Controls velocity of player
    void HandleVelocity()
    {
        //Increase
        if (_currentMovement.magnitude > 0.1f && _velocity < _currentMaxVelocity)
        {
            _velocity += _currentMaxAcceleration * Time.deltaTime;
        }
        //Increase Locking
        else if (_currentMovement.magnitude > 0.1f && _velocity > _currentMaxVelocity) _velocity = _currentMaxVelocity;
        //Decrease locking
        else if (_currentMovement.magnitude <= 0.1f) _velocity = 0f;
    }

    //Controls velocity of animator
    void HandleAnimatorVelocity()
    {
        if (SwordFighting != null && SwordFighting.IsAttackModeEnabled) 
        {
            //Forward and Backwards
            if (_isForwardPressed && !_isBackwardPressed && _animatorVelocity.z < _currentMaxAnimatorVelocity) _animatorVelocity.z += AnimatorAcceleration * Time.deltaTime;
            else if (_isForwardPressed && !_isBackwardPressed && _animatorVelocity.z > _currentMaxAnimatorVelocity) _animatorVelocity.z = _currentMaxAnimatorVelocity;
            else if (_isBackwardPressed && !_isForwardPressed && _animatorVelocity.z > -_currentMaxAnimatorVelocity) _animatorVelocity.z -= AnimatorAcceleration * Time.deltaTime;
            else if (_isBackwardPressed && !_isForwardPressed && _animatorVelocity.z < -_currentMaxAnimatorVelocity) _animatorVelocity.z = -_currentMaxAnimatorVelocity;
            else if ((_isForwardPressed && _isBackwardPressed) || (!_isForwardPressed && !_isBackwardPressed)) _animatorVelocity.z = 0f;
            //Left and Right
            if (_isRightPressed && !_isLeftPressed && _animatorVelocity.x < _currentMaxAnimatorVelocity) _animatorVelocity.x += AnimatorAcceleration * Time.deltaTime;
            else if (_isRightPressed && !_isLeftPressed && _animatorVelocity.x > _currentMaxAnimatorVelocity) _animatorVelocity.x = _currentMaxAnimatorVelocity;
            else if (_isLeftPressed && !_isRightPressed && _animatorVelocity.x > -_currentMaxAnimatorVelocity) _animatorVelocity.x -= AnimatorAcceleration * Time.deltaTime;
            else if (_isLeftPressed && !_isRightPressed && _animatorVelocity.x < -_currentMaxAnimatorVelocity) _animatorVelocity.x = -_currentMaxAnimatorVelocity;
            else if ((_isLeftPressed && _isRightPressed) || (!_isLeftPressed && !_isRightPressed)) _animatorVelocity.x = 0f;
        }
        else
        {
            _animatorVelocity.x = 0f;
            if (_isMovementPressed)
            {
                if (_animatorVelocity.z < _currentMaxAnimatorVelocity) _animatorVelocity.z += AnimatorAcceleration * Time.deltaTime;
                else if (_animatorVelocity.z > _currentMaxAnimatorVelocity) _animatorVelocity.z = _currentMaxAnimatorVelocity;
            }
            else if (!_isMovementPressed)
            {
                if (_animatorVelocity.z > 0f) _animatorVelocity.z -= AnimatorDeceleration * Time.deltaTime;
                else if (_animatorVelocity.z < 0f) _animatorVelocity.z = 0f;
            }
        }
    }
}
