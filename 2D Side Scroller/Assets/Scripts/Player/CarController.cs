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

    private ReactToUnity reactToUnity;
    private WorldUIManager UIManager;
    private WorldLevelManager levelManager;
    private WorldCheckPointManager checkPointManager;

    #endregion

    #region Inspector Runtime and Private Variable

    [Header("Runtime Variable")]
    [SerializeField] bool grounded;
    public bool canApplyImpulse;
    //[SerializeField] int currentFuel;

    private bool isDead = false;

    private float fuelTimer = 0;
    private float forwardForce;
    private float distanceTraveled;
    private float totalRotation;
    private float inAirTime = 0;
    private float frontWheelieTime = 0;
    private float rearWheelieTime = 0;

    private AudioSource coinAudioSource;

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
    [SerializeField] float minAirTime;
    [SerializeField] float minWheelieTime;

    [Header("Car Particle Data")]
    [SerializeField] ParticleSystem frontDirtParticle;
    [SerializeField] ParticleSystem rearDirtParticle;
    [SerializeField] float maxSpeedForParticle;

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
        coinAudioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        reactToUnity = ReactToUnity.instance;
        UIManager = WorldUIManager.instance;
        levelManager = WorldLevelManager.instance;
        checkPointManager = WorldCheckPointManager.instance;

        SetInitialFuel();
    }

    private void Update()
    {
        grounded = CheckIfGrounded();
        SetPlayerFuel();
        CheckInAirTime();
        //CheckForWheelie();
        CheckForDirtParticleEffect();

        if (CheckIfDead())
        {
            int fuelLoss;
            fuelLoss = (fuelLossPercentageOnDeath * maxFuel) / 100;
            fuelLoss = Mathf.Min(fuelLoss, maxFuel);

            reactToUnity.UseEnergy_Unity(fuelLoss);

            if (checkPointManager.currentCheckPoint != null)
            {
                RetunToLastCheckPoint();
                inAirTime = 0;
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
        UIManager.SetInitialFuel(reactToUnity._maxEnergy);
    }

    private void SetPlayerFuel()
    {
        float deltaDistance = Mathf.Abs(carRigidBody.linearVelocityX) * Time.deltaTime;
        distanceTraveled += deltaDistance;

        fuelTimer += deltaDistance * fuelConsumptionRate;

        if (fuelTimer >= 1f)
        {
            int fuelToConsume = Mathf.FloorToInt(fuelTimer);
            reactToUnity._Energy = Mathf.Max(reactToUnity._Energy - fuelToConsume, 0);
            fuelTimer -= fuelToConsume;

            reactToUnity.UseEnergy_Unity(fuelToConsume);
            UIManager.UpdateFuelSlider(reactToUnity._Energy);

            if(reactToUnity._Energy <= 0 && inputManager.playerInput.enabled)
            {
                inputManager.SetPlayerInput(false);
            }
            else if (reactToUnity._Energy > 0 && !inputManager.playerInput.enabled)
            {
                inputManager.SetPlayerInput(true);
            }
        }
    }

    public void TogglePlayerDeath(bool died)
    {
        inputManager.SetPlayerInput(died);
    }

    public void RetunToLastCheckPoint()
    {
        WorldCheckPointManager.instance.TranslateToLastCheckpoint(gameObject);
    }

    public void ResetScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    #endregion

    #region Item Functions

    public void GainFuel(int amount)
    {
        reactToUnity.GiveEnergy_Unity(amount);
    }

    public void PlayCoinCollectionSound(AudioClip audioClip)
    {
        coinAudioSource.clip = audioClip;
        coinAudioSource.Play();
    }

    #endregion

    #region Check Functions

    private bool CheckIfGrounded()
    {
        if (CheckIfFrontWheelGrounded())
        {
            return true;
        }
        else if (CheckIfRearWheelGrounded())
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

            reactToUnity.GiveEnergy_Unity(fuelGain);

            totalRotation = 0f;
        }
    }

    private void CheckInAirTime()
    {
        if (!CheckIfFrontWheelGrounded() && !CheckIfRearWheelGrounded())
        {
            inAirTime += Time.deltaTime;
        }
        else if(CheckIfGrounded())
        {
            if(inAirTime > minAirTime)
            {
                float airTime = Mathf.Round(inAirTime * 100f) / 100f;
                UIManager.ShowStuntMessage("Air Time! " + airTime);
            }

            inAirTime = 0f;
        }
    }

    private void CheckForWheelie()
    {
        if (CheckIfFrontWheelGrounded() && !CheckIfRearWheelGrounded())
        {
            frontWheelieTime += Time.deltaTime;
            rearWheelieTime = 0f;
        }
        else if (CheckIfRearWheelGrounded() && !CheckIfFrontWheelGrounded())
        {
            rearWheelieTime += Time.deltaTime;
            frontWheelieTime = 0f;
        }
        else if(!CheckIfFrontWheelGrounded() && !CheckIfRearWheelGrounded())
        {
            if(frontWheelieTime > minWheelieTime)
            {
                float frontWheelie = Mathf.Round(frontWheelieTime * 100) / 100;
                UIManager.ShowStuntMessage("Rear Wheelie " + frontWheelie);
            }
            if(rearWheelieTime > minWheelieTime)
            {
                float rearWheelie = Mathf.Round(rearWheelieTime * 100) / 100;
                UIManager.ShowStuntMessage("Front Wheelie " +  rearWheelie);
            }

            frontWheelieTime = 0f;
            rearWheelieTime = 0f;
        }
    }

    private void CheckForDirtParticleEffect()
    {
        if (CheckIfFrontWheelGrounded() && carRigidBody.linearVelocityX > maxSpeedForParticle)
        {
            frontDirtParticle.gameObject.SetActive(true);
        }
        if (CheckIfRearWheelGrounded() && carRigidBody.linearVelocityX > maxSpeedForParticle)
        {
            rearDirtParticle.gameObject.SetActive(true);
        }
        else if (!CheckIfGrounded() || carRigidBody.linearVelocityX < maxSpeedForParticle)
        {
            frontDirtParticle.gameObject.SetActive(false);
            rearDirtParticle.gameObject.SetActive(false);
        }
        else
        {
            frontDirtParticle.gameObject.SetActive(false);
            rearDirtParticle.gameObject.SetActive(false);
        }
    }

    #endregion

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("LevelSpawner"))
        {
            WorldLevelManager.instance.CreateWorld();
            collision.gameObject.SetActive(false);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(frontWheelGroundCheck.position, groundCheckRadius);
        Gizmos.DrawWireSphere(rearWheelGroundCheck.position, groundCheckRadius);
        Gizmos.DrawWireSphere(headCheck.position, headCheckRadius);
    }

}
