public abstract class BaseState
{
    #region Variables
    private bool _isRootState = false;
    private PlayerMovementStateManager _ctx;
    private PlayerMovementStateFactory _factory;
    private BaseState _currentSuperState;
    private BaseState _currentSubState;
    #endregion

    #region Getters & Setters
    public bool IsRootState { get { return _isRootState; } set { _isRootState = value; } }
    public PlayerMovementStateManager CTX { get { return _ctx; } }
    public PlayerMovementStateFactory Factory { get { return _factory; } }
    public BaseState CurrentSuperState { get { return _currentSuperState; } }
    public BaseState CurrentSubState { get { return _currentSubState; } }
    #endregion

    public BaseState(PlayerMovementStateManager currentContext, PlayerMovementStateFactory playerStateFactory)
    {
        _ctx = currentContext;
        _factory = playerStateFactory;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
    public abstract void CheckSwitchState();
    public abstract void InitializeSubState();
    public void UpdateStates()
    {
        UpdateState();
        if (_currentSubState != null) _currentSubState.UpdateStates();
    }
    protected void SwitchState(BaseState newState)
    {
        //current state exits state
        ExitState();

        //new state enters state
        newState.EnterState();

        if (_isRootState)
        {
            //Switch Current State of Context
            _ctx.CurrentState = newState;
        }
        else if (_currentSuperState != null)
        {
            //set the current super state's substate to the new state
            _currentSuperState.SetSubState(newState);
        }
    }
    protected void SetSuperState(BaseState newSuperState)
    {
        _currentSuperState = newSuperState;
    }
    protected void SetSubState(BaseState newSubState)
    {
        _currentSubState = newSubState;
        newSubState.SetSuperState(this);
    }
}
