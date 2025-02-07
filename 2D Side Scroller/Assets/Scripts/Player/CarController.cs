using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent (typeof(WheelJoint2D))]
[RequireComponent(typeof(WheelJoint2D))]
public class CarController : MonoBehaviour
{
    #region References

    private InputManager inputManager;
    private Rigidbody2D carRigidBody;

    #endregion

    #region Inspector Runtime Variable

    [Header("Runtime Variable")]
    [SerializeField] bool grounded;
    public bool canApplyImpulse;
    [SerializeField] float currentFuel;

    // Temp Variables
    [SerializeField] Vector2 playerVelocity;
    [SerializeField] float playerSpeed;

    #endregion

    #region UI Variables

    [Header("Fuel UI")]
    [SerializeField] Slider fuelSlider;

    [Header("Game UI")]
    [SerializeField] GameObject retryUI;

    #endregion

    #region Inspector References

    [Header("Car Data")]
    [SerializeField] float accelerationForce;
    [SerializeField] float deaccelerationForce;
    [SerializeField] float maxSpeed;
    [SerializeField] Vector2 groundedImpulse;
    [SerializeField] Vector2 inAirImpulse;

    [Header("Car Fuel Data")]
    [SerializeField] float initialFuel;
    [SerializeField] float maxFuel;
    [SerializeField] [Range(0, 1)] float fuelConsumptionRate;

    [Header("Ground References")]
    [SerializeField] Transform groundCheck;
    [SerializeField] [Range(0, 1)] float groundCheckDistance;
    [SerializeField] LayerMask whatIsGround;

    [Header("Injury References")]
    [SerializeField] Transform headCheck;
    [SerializeField] [Range(0, 1)] float headCheckRadius;

    [Header("Impulse Transforms")]
    [SerializeField] Transform frontImpulse;
    [SerializeField] Transform rearImpulse;

    #endregion

    #region Unity Callback Function

    private void Awake()
    {
        inputManager = GetComponent<InputManager>();
        carRigidBody = GetComponent<Rigidbody2D>();

        retryUI.gameObject.SetActive(false);
    }

    private void Start()
    {
        SetInitialFuel();
    }

    private void Update()
    {
        SetPlayerFuel();

        if(currentFuel <= 0)
        {
            inputManager.SetPlayerInput(false);
            retryUI.gameObject.SetActive(true);
        }
        else if (isDead())
        {
            inputManager.SetPlayerInput(false);
            retryUI.gameObject.SetActive(true);
        }

    }

    private void FixedUpdate()
    {
        playerVelocity = carRigidBody.linearVelocity;
        playerSpeed = Mathf.Abs(carRigidBody.linearVelocityX);

        grounded = isGrounded();

        if (isGrounded())
        {
            SetVehicleMovement();
        }
        else if (!isGrounded())
        {
            if(canApplyImpulse && inputManager.moveAmout == 1)
            {
                ApplyInAirImpulse(true);
            }
            else if (canApplyImpulse && inputManager.moveAmout == -1)
            {
                ApplyInAirImpulse(false);
            }
        }

    }

    #endregion

    #region Set Functions

    private void SetVehicleMovement()
    {
        if(Mathf.Abs(carRigidBody.linearVelocityX) < maxSpeed && inputManager.moveAmout == -1)
        {
            carRigidBody.AddForceX(deaccelerationForce * inputManager.moveAmout * Time.fixedDeltaTime, ForceMode2D.Force);
            if(canApplyImpulse) ApplyImpulse(true);
        }
        else if (Mathf.Abs(carRigidBody.linearVelocityX) < maxSpeed && inputManager.moveAmout == 1)
        {
            carRigidBody.AddForceX(accelerationForce * inputManager.moveAmout * Time.fixedDeltaTime, ForceMode2D.Force);
            if(canApplyImpulse) ApplyImpulse(false);
        }
    }

    public void ApplyImpulse(bool isFront = false)
    {
        if (isFront)
        {
            carRigidBody.AddForceAtPosition(groundedImpulse, frontImpulse.up, ForceMode2D.Impulse);
        }
        else
        {
            carRigidBody.AddForceAtPosition(groundedImpulse, rearImpulse.up, ForceMode2D.Impulse);
        }
    }

    private void ApplyInAirImpulse(bool isFront = false)
    {
        if (isFront)
        {
            carRigidBody.AddForceAtPosition(inAirImpulse, frontImpulse.up, ForceMode2D.Impulse);
        }
        else
        {
            carRigidBody.AddForceAtPosition(inAirImpulse, rearImpulse.up, ForceMode2D.Impulse);
        }
    }

    private void SetInitialFuel()
    {
        fuelSlider.maxValue = maxFuel;
        fuelSlider.value = initialFuel;
        currentFuel = initialFuel;
    }

    private void SetPlayerFuel()
    {
        float speed = Mathf.Abs(carRigidBody.linearVelocityX);
        float currentTime = Time.fixedDeltaTime;

        float Distance = speed * currentTime;
        currentFuel -= Distance * fuelConsumptionRate;

        fuelSlider.value = currentFuel;
    }

    public void ResetScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    #endregion

    #region Item Functions

    public void GainFuel(float amount)
    {
        if(currentFuel > 0)
        {
            currentFuel += amount;

            if(currentFuel > maxFuel)
            {
                currentFuel = maxFuel;
            }

            fuelSlider.value = currentFuel;

        }
    }

    #endregion

    #region Check Functions

    private bool isGrounded()
    {
        return Physics2D.Raycast(groundCheck.position, -groundCheck.transform.up, groundCheckDistance, whatIsGround);
    }

    private bool isDead()
    {
        return Physics2D.OverlapCircle(headCheck.position, headCheckRadius, whatIsGround);
    }

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(groundCheck.position, -groundCheck.transform.up * groundCheckDistance);
        Gizmos.DrawWireSphere(headCheck.position, headCheckRadius);
    }

}
