using UnityEngine;
public class PlayerAudioController : MonoBehaviour
{
    public float footstepInterval = 0.42f;
    public int waterLevel = 9;

    private Rigidbody _rb;
    private float _footstepTimer;
    private string _currentAmbience = "";

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _footstepTimer = footstepInterval;
    }

    void Update()
    {
        HandleFootsteps();
        HandleAmbience();
    }
    private void HandleFootsteps()
    {
        bool moving = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;
        bool grounded = IsGrounded();
        bool inWater = IsInWater();

        if (moving && (grounded || inWater))
        {
            _footstepTimer -= Time.deltaTime;
            if (_footstepTimer <= 0f)
            {
                string clip = inWater ? "Water" : "Walking";
                AudioManager.Instance?.PlaySFX(clip, 0.6f);
                _footstepTimer = inWater ? footstepInterval * 1.5f : footstepInterval;
            }
        }
        else
        {
            _footstepTimer = footstepInterval * 0.4f;
        }
    }
    private void HandleAmbience()
    {
        string target;

        if (IsUnderwater())
            target = "Underwater_Ambient";
        else if (IsUnderground())
            target = "Cave";
        else
            target = "";

        if (target == _currentAmbience) return;

        _currentAmbience = target;

        if (string.IsNullOrEmpty(target))
            AudioManager.Instance?.StopAmbience();
        else
            AudioManager.Instance?.PlayAmbience(target, 0.5f);
    }
    public void OnJump()
    {
        AudioManager.Instance?.PlaySFX("Jump", 0.8f);
    }
    public void OnEnterWater()
    {
        AudioManager.Instance?.PlaySFX("Swimming", 0.6f);
    }

    private bool IsGrounded()
        => Physics.Raycast(transform.position + Vector3.up * 0.05f, Vector3.down, 1.2f);

    private bool IsInWater()
        => transform.position.y < waterLevel;

    private bool IsUnderwater()
        => transform.position.y < waterLevel - 0.5f;
    private bool IsUnderground()
        => !IsInWater() && Physics.Raycast(transform.position + Vector3.up, Vector3.up, 4f);
}