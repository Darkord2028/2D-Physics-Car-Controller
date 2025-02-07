using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private CarController carController;
    private PlayerInput playerInput;

    #region Input Flags

    public int moveAmout { get; private set; }
    public bool BoostInput { get; private set; }

    #endregion

    #region Unity Callback Functions

    private void Start()
    {
        carController = GetComponent<CarController>();
        playerInput = GetComponent<PlayerInput>();
    }

    #endregion

    #region Input Action

    public void OnAccelerateInput(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            moveAmout = 1;
            carController.canApplyImpulse = false;
        }
        else if(context.canceled)
        {
            moveAmout = 0;
            carController.canApplyImpulse = true;
        }
    }

    public void OnDeaccelerateInput(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            moveAmout = -1;
            carController.canApplyImpulse = false;
        }
        else if(context.canceled)
        {
            moveAmout = 0;
            carController.canApplyImpulse = true;
        }
    }

    public void OnBoostInput(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            BoostInput = true;
        }
    }

    #endregion

    #region Use Input

    public void UseBoostInput() => BoostInput = false;

    #endregion

    #region Set Functions

    public void SetPlayerInput(bool Enable)
    {
        playerInput.enabled = Enable;
    }

    #endregion

}
