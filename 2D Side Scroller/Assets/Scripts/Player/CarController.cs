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

    #region Inspector Runtime and Private Variable

    [Header("Runtime Variable")]
    [SerializeField] bool grounded;
    public bool canApplyImpulse;
    [SerializeField] int currentFuel;

    private float distanceTraveled;
    private float fuelTimer = 0;
    private bool isDead = false;

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
    [SerializeField] int maxFuel;
    [SerializeField] [Range(0, 10)] float fuelConsumptionRate;

    [Header("Ground References")]
    [SerializeField] Transform frontWheelGroundCheck;
    [SerializeField] Transform rearWheelGroundCheck;
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

        if(currentFuel <= 0 || CheckIfDead())
        {
            SetPlayerDeath();
        }
    }

    private void FixedUpdate()
    {
        grounded = CheckIfGrounded();

        if (CheckIfGrounded() && !isDead)
        {
            SetVehicleMovement();
        }
        else if (!CheckIfGrounded() && !isDead)
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
        ReactToUnity.instance._maxEnergy = maxFuel;
        ReactToUnity.instance._Energy = ReactToUnity.instance._maxEnergy;
        currentFuel = maxFuel;

        WorldUIManager.instance.SetInitialFuel(maxFuel);
    }

    private void SetPlayerFuel()
    {
        float deltaDistance = Mathf.Abs(carRigidBody.linearVelocityX) * Time.deltaTime;
        distanceTraveled += deltaDistance;

        fuelTimer += deltaDistance * fuelConsumptionRate;

        if (fuelTimer >= 1f)
        {
            int fuelToConsume = Mathf.FloorToInt(fuelTimer);
            currentFuel = Mathf.Max(currentFuel - fuelToConsume, 0);
            fuelTimer -= fuelToConsume;

            ReactToUnity.instance.UseEnergy_Unity(fuelToConsume);
            WorldUIManager.instance.UpdateFuelSlider(currentFuel);
        }
    }

    private void SetPlayerDeath()
    {
        retryUI.SetActive(true);
        isDead = true;
    }

    public void RetunToLastCheckPoint()
    {
        WorldCheckPointManager.instance.TranslateToLastCheckpoint(gameObject);
    }

    public void CheatPlayerFuel()
    {
        currentFuel = maxFuel;

        //Slider
        fuelSlider.maxValue = maxFuel;
        fuelSlider.value = currentFuel;
    }

    public void ResetScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    #endregion

    #region Item Functions

    public void GainFuel(int amount)
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

    private bool CheckIfGrounded()
    {
        if (Physics2D.Raycast(rearWheelGroundCheck.position, -rearWheelGroundCheck.transform.up, groundCheckDistance, whatIsGround))
        {
            return true;
        }
        else if (Physics2D.Raycast(frontWheelGroundCheck.position, -frontWheelGroundCheck.transform.up, groundCheckDistance, whatIsGround))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool CheckIfDead()
    {
        return Physics2D.OverlapCircle(headCheck.position, headCheckRadius, whatIsGround);
    }

    #endregion

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("LevelSpawner"))
        {
            WorldLevelManager.Instance.CreateWorld();
            collision.gameObject.SetActive(false);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(frontWheelGroundCheck.position, -frontWheelGroundCheck.transform.up * groundCheckDistance);
        Gizmos.DrawRay(rearWheelGroundCheck.position, -rearWheelGroundCheck.transform.up * groundCheckDistance);
        Gizmos.DrawWireSphere(headCheck.position, headCheckRadius);
    }

}
