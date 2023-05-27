namespace CustomToolkit.StateMachine
{
    public abstract class StateMachine
    {
        private IState m_currentState = null;
        public IState CurrentState => m_currentState;

        public void Update()
        {
            if(m_currentState != null)
                m_currentState.Update();
        }
    
        public void SetState(IState newState)
        {
            IState oldState = m_currentState;
        
            if (oldState != null)
            {
                oldState.OnExit(newState);
            }

            m_currentState = newState;

            m_currentState.OnEnter(oldState);
        }
    }   
}
