using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomToolkit.Variables
{
    public abstract class Variable : ScriptableObject
    {
        public abstract void ClearListeners();
        public abstract void ResetToDefaultValue();
    }

    public abstract class Variable<T> : Variable
    {
        [SerializeField] private T m_defaultValue;

        private T m_value;

        private List<Action<T>> m_actionListeners = new List<Action<T>>();

        public T Value
        {
            get { return m_value; }
            set
            {
                m_value = value;

                RaiseOnValueChanged(value);
            }
        }

        private void OnEnable()
        {
            ResetToDefaultValue();
        }

        public void RegisterListener(Action<T> action)
        {
            if (!m_actionListeners.Contains(action))
                m_actionListeners.Add(action);
        }

        public void UnregisterListener(Action<T> action)
        {
            if (m_actionListeners.Contains(action))
                m_actionListeners.Remove(action);
        }

        private void RaiseOnValueChanged(T value)
        {
            for (int i = 0; i < m_actionListeners.Count; i++)
                m_actionListeners[i].Invoke(value);
        }

        public override void ClearListeners()
        {
            m_actionListeners.Clear();
        }

        public override void ResetToDefaultValue()
        {
            m_value = m_defaultValue;
        }
    }
}
