using UnityEngine;

/// <summary>
/// Test car class.
/// </summary>
public class CarManual : MonoBehaviour
{
    /// <summary>
    /// Acceleration power.
    /// </summary>
    public float accelerationPower;

    /// <summary>
    /// Steering power.
    /// </summary>
    public float steeringPower;

    /// <summary>
    /// Vertical driving action.
    /// </summary>
    private float verticalAction;

    /// <summary>
    /// Horiziontal driving action.
    /// </summary>
    private float horizontalAction;

    /// <summary>
    /// Driving speed.
    /// </summary>
    private float speed;

    /// <summary>
    /// Steering direction.
    /// </summary>
    private float direction;

    /// <summary>
    /// Steering ammount.
    /// </summary>
    private readonly float steeringAmount;

    /// <summary>
    /// Car rigidbody
    /// </summary>
    private Rigidbody2D rb;

    /// <summary>
    /// The first performed function.
    /// </summary>
    void Start()
    {
        this.rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// Updates per every frame.
    /// </summary>
    void FixedUpdate()
    {
        this.verticalAction = Input.GetAxis("Vertical");
        this.horizontalAction = Input.GetAxis("Horizontal");

        this.speed = this.verticalAction * this.accelerationPower;
        this.direction = Mathf.Sign(Vector2.Dot(this.rb.velocity, this.rb.GetRelativeVector(Vector2.up)));

        this.rb.rotation += -this.horizontalAction * this.steeringPower * this.rb.velocity.magnitude * this.direction;
        this.rb.AddRelativeForce(Vector2.up * this.speed);
        this.rb.AddRelativeForce(-Vector2.right * this.rb.velocity.magnitude * this.steeringAmount / 2);
    }
}
