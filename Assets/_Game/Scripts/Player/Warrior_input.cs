using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Warrior_input : MonoBehaviour
{
    //Data
    private Vector2 movement_ = Vector2.zero;
    private bool pressJump_ = false;
    private bool pressAttack_ = false;

    public enum stateWarriorGrounded { IsRunning, IsIdle}
    public stateWarriorGrounded stateWarriorGrounded_ = stateWarriorGrounded.IsIdle;

    //Get
    public Vector2 GetMovement_ => this.movement_;
    public bool getPressJump_ => this.pressJump_;
    public bool getPressAttack_ => this.pressAttack_;

    public void OnMoveWarrior(InputAction.CallbackContext action)
    {
        Vector2 _rawMovement = action.ReadValue<Vector2>();

        movement_.x = Mathf.Abs(_rawMovement.x) > 0 ? Mathf.Sign(_rawMovement.x) : 0;

        if (action.performed)
            stateWarriorGrounded_ = stateWarriorGrounded.IsRunning;

        if (action.canceled)
            stateWarriorGrounded_ = stateWarriorGrounded.IsIdle;
    }

    public void OnJumpWarrior(InputAction.CallbackContext action)
    {
        if (action.performed)
            pressJump_ = true;

        if (action.canceled)
            pressJump_ = false;
    }

    public void OnAttackWarrior(InputAction.CallbackContext action)
    {
        if (action.performed)
            pressAttack_ = true;

        if (action.canceled)
            pressAttack_ = false;
    }
}
