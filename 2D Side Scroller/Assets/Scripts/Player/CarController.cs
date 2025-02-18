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

    private bool isDead = false;

    private float fuelTimer = 0;

    private float forwardForce;
    private float distanceTraveled;

    private float totalRotation;

    #endregion

    #region UI Variables

    [Header("Fuel UI")]
    [SerializeField] Slider fuelSlider;

    [Header("Game UI")]
    [SerializeField] GameObject retryUI;

    #endregion

    #region Car Data

    [Header("Car Data")]
    [SerializeField] float accelerationForce;
    [SerializeField] float inAirAccelerationForce;
    [SerializeField] float maxSpeed;
    [SerializeField] float inAirTorque;

    [Header("Car Fuel Data")]
    [SerializeField] int maxFuel;
    [SerializeField][Range(0, 10)] float fuelConsumptionRate;
    [SerializeField][Range(0, 100)] int fuelGainPercentageOnFlip;
    [SerializeField][Range(0, 100)] int fuelLossPercentageOnDeath;

    [Header("Car Stunt Data")]
    [SerializeField] float _360FlipAngle;

    [Header("Ground References")]
    [SerializeField] Transform frontWheelGroundCheck;
    [SerializeField] Transform rearWheelGroundCheck;
    [SerializeField] [Range(0, 1)] float groundCheckRadius;
    [SerializeField] LayerMask whatIsGround;

    [Header("Injury References")]
    [SerializeField] Transform headCheck;
    [SerializeField] [Range(0, 1)] float headCheckRadius;

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
        grounded = CheckIfGrounded();
        SetPlayerFuel();

        if (currentFuel <= 0)
        {
            TogglePlayerDeath(true);
        }
        if (CheckIfDead())
        {
            int fuelLoss;
            fuelLoss = (fuelLossPercentageOnDeath * maxFuel) / 100;
            fuelLoss = Mathf.Min(fuelLoss, maxFuel);
            currentFuel -= fuelLoss;
            if (currentFuel < 0) currentFuel = 0;

            if (WorldCheckPointManager.instance.currentCheckPoint != null)
            {
                RetunToLastCheckPoint();
            }
        }
        if (grounded)
        {
            forwardForce = accelerationForce;
            totalRotation = 0;
        }
        else if(!grounded)
        {
            forwardForce = inAirAccelerationForce;
            CheckIfCarDidFlip();
        }
    }

    private void FixedUpdate()
    {
        if (!isDead)
        {
            SetVehicleMovement();
        }
    }

    #endregion

    #region Set Functions

    private void SetVehicleMovement()
    {
        ApplyInAirImpulse();
        if (Mathf.Abs(carRigidBody.linearVelocityX) < maxSpeed)
        {
            carRigidBody.AddForceX(forwardForce * inputManager.moveAmout * Time.fixedDeltaTime, ForceMode2D.Force);
        }
    }

    private void ApplyInAirImpulse()
    {
        if (grounded) return;
        carRigidBody.AddTorque(inAirTorque * inputManager.moveAmout, ForceMode2D.Impulse);
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

    public void TogglePlayerDeath(bool died)
    {
        retryUI.SetActive(died);
        isDead = died;

        if(!isDead)
        {
            SetInitialFuel();
        }
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
        if (Physics2D.OverlapCircle(rearWheelGroundCheck.position, groundCheckRadius, whatIsGround))
        {
            return true;
        }
        else if (Physics2D.OverlapCircle(frontWheelGroundCheck.position, groundCheckRadius, whatIsGround))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool CheckIfFrontWheelGrounded()
    {
        return Physics2D.OverlapCircle(frontWheelGroundCheck.position, groundCheckRadius, whatIsGround);
    }

    private bool CheckIfRearWheelGrounded()
    {
        return Physics2D.OverlapCircle(rearWheelGroundCheck.position, groundCheckRadius, whatIsGround);
    }

    private bool CheckIfDead()
    {
        return Physics2D.OverlapCircle(headCheck.position, headCheckRadius, whatIsGround);
    }

    private void CheckIfCarDidFlip()
    {
        totalRotation += carRigidBody.angularVelocity * Time.deltaTime;

        if (Mathf.Abs(totalRotation) > _360FlipAngle)
        {
            WorldUIManager.instance.ShowStuntMessage("FLIP!");

            int fuelGain;
            fuelGain = ((fuelGainPercentageOnFlip * maxFuel)/100);
            currentFuel += fuelGain;
            currentFuel = Mathf.Min(currentFuel, maxFuel);

            totalRotation = 0f;
        }
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
        Gizmos.DrawWireSphere(frontWheelGroundCheck.position, groundCheckRadius);
        Gizmos.DrawWireSphere(rearWheelGroundCheck.position, groundCheckRadius);
        Gizmos.DrawWireSphere(headCheck.position, headCheckRadius);
    }

}
