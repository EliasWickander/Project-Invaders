namespace CustomToolkit.StateMachine
{
    public interface IState
    {
        public void OnEnter(IState prevState);
        public void Update();
        public void OnExit(IState nextState);
    }   
}
