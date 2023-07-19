using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Player))]
public class PlayerInputController : MonoBehaviour
{
    [SerializeField]
    private Player m_player;
    
    private CustomPlayerInput m_playerInput;

    private void Awake()
    {
        m_playerInput = new CustomPlayerInput();
    }

    private void OnEnable()
    {
        m_playerInput.Enable();

        m_playerInput.Movement.Movement.performed += OnMovementPerformed;
    }

    private void OnDisable()
    {
        m_playerInput.Disable();
        
        m_playerInput.Movement.Movement.performed -= OnMovementPerformed;
    }

    public void SetTarget(Player target)
    {
        m_player = target;
    }
    
    private void OnMovementPerformed(InputAction.CallbackContext value)
    {
        Vector2 inputDir = value.ReadValue<Vector2>();

        m_player.SetMoveDirection(new Vector3(inputDir.x, 0, inputDir.y));
    }
}
