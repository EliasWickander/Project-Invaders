using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class InputEvent : MonoBehaviour
{
    public enum ActionType
    {
        Performed,
        Started,
        Canceled
    }
    
    public InputActionAsset m_inputAsset;
    public string m_selectedActionGuid;
    public ActionType m_eventType;
    public UnityEvent<InputAction.CallbackContext> m_event;

    private InputAction m_selectedAction;
    private void OnEnable()
    {
        m_selectedAction = m_inputAsset.FindAction(m_selectedActionGuid);
        
        if(m_selectedAction == null)
            return;
        
        m_inputAsset.Enable();
        
        switch (m_eventType)
        {
            case ActionType.Performed:
            {
                m_selectedAction.performed += Trigger;
                break;
            }
            case ActionType.Started:
            {
                m_selectedAction.started += Trigger;
                break;
            }
            case ActionType.Canceled:
            {
                m_selectedAction.canceled += Trigger;
                break;
            }
        }
    }

    private void OnDisable()
    {
        if(m_selectedAction == null)
            return;
        
        m_inputAsset.Disable();
        
        switch (m_eventType)
        {
            case ActionType.Performed:
            {
                m_selectedAction.performed -= Trigger;
                break;
            }
            case ActionType.Started:
            {
                m_selectedAction.started -= Trigger;
                break;
            }
            case ActionType.Canceled:
            {
                m_selectedAction.canceled -= Trigger;
                break;
            }
        }
    }

    private void Trigger(InputAction.CallbackContext context)
    {
        m_event?.Invoke(context);
    }
}
