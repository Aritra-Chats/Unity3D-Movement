using System.Collections.Generic;

enum EPlayerMovementStates 
{ 
    Grounded, 
    Fall,
    Jump,
    Crouched,
    Idle,
    Walking,
    Running,
}
public class PlayerMovementStateFactory
{
    PlayerMovementStateManager _context;
    Dictionary<EPlayerMovementStates, BaseState> _states = new Dictionary<EPlayerMovementStates, BaseState>();

    public PlayerMovementStateFactory(PlayerMovementStateManager currentContext)
    {
        _context = currentContext;
        _states[EPlayerMovementStates.Grounded] = new GroundedState(_context, this);
        _states[EPlayerMovementStates.Jump] = new JumpState(_context, this);
        _states[EPlayerMovementStates.Fall] = new FallState(_context, this);
        _states[EPlayerMovementStates.Crouched] = new CrouchState(_context, this);
        _states[EPlayerMovementStates.Idle] = new IdleState(_context, this);
        _states[EPlayerMovementStates.Walking] = new WalkingState(_context, this);
        _states[EPlayerMovementStates.Running] = new RunningState(_context, this);
    }

    public BaseState Grounded()
    {
        return _states[EPlayerMovementStates.Grounded];
    }
    public BaseState Jumping()
    {
        return _states[EPlayerMovementStates.Jump];
    }
    public BaseState Falling()
    {
        return _states[EPlayerMovementStates.Fall];
    }
    public BaseState Crouching()
    {
        return _states[EPlayerMovementStates.Crouched];
    }
    public BaseState Idle()
    {
        return _states[EPlayerMovementStates.Idle];
    }
    public BaseState Walking()
    {
        return _states[EPlayerMovementStates.Walking];
    }
    public BaseState Running()
    {
        return _states[EPlayerMovementStates.Running];
    }
}
