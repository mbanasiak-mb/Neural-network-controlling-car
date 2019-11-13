using ArtificialNeuralNetwork;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Car class.
/// </summary>
public class Car : MonoBehaviour
{
    /// <summary>
    /// Best car.
    /// </summary>
    public bool bestCar;

    /// <summary>
    /// Crashed car.
    /// </summary>
    public bool crashedCar;

    /// <summary>
    /// Acceleration power.
    /// </summary>
    public float accelerationPower;

    /// <summary>
    /// Steering power.
    /// </summary>
    public float steeringPower;

    /// <summary>
    /// Neural network.
    /// </summary>
    public NeuralNetwork network;

    /// <summary>
    /// Distance number 1.
    /// </summary>
    private float distance1;

    /// <summary>
    /// Distance number 2.
    /// </summary>
    private float distance2;

    /// <summary>
    /// Distance number 3.
    /// </summary>
    private float distance3;

    /// <summary>
    /// Car speed magnitude.
    /// </summary>
    private float carSpeedMagnitude;

    /// <summary>
    /// Vertical driving action.
    /// </summary>
    private float verticalAction;

    /// <summary>
    /// Horizontal driving action.
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
    /// Car collider.
    /// </summary>
    private Collider2D cl;

    /// <summary>
    /// Car rigidbody.
    /// </summary>
    private Rigidbody2D rb;

    /// <summary>
    /// Map layer.
    /// </summary>
    private LayerMask mapLayer;

    /// <summary>
    /// The first performed function.
    /// </summary>
    private void Start()
    {
        this.bestCar = false;
        this.crashedCar = false;

        this.rb = GetComponent<Rigidbody2D>();
        this.cl = GetComponent<Collider2D>();
        this.mapLayer = LayerMask.GetMask("MapLayer");

        this.SensorsMeasure();
        this.CreateNetwork();
    }

    /// <summary>
    /// Updates per every frame.
    /// </summary>
    private void FixedUpdate()
    {
        if (this.crashedCar)
        {
            return;
        }

        this.SensorsMeasure();
        this.NetworkDecide();

        this.speed = this.verticalAction * this.accelerationPower;
        this.direction = Mathf.Sign(Vector2.Dot(this.rb.velocity, this.rb.GetRelativeVector(Vector2.up)));

        this.rb.rotation += -this.horizontalAction * this.steeringPower * this.rb.velocity.magnitude * this.direction;
        this.rb.AddRelativeForce(Vector2.up * this.speed);
        this.rb.AddRelativeForce(-Vector2.right * this.rb.velocity.magnitude * this.steeringAmount / 2);
    }

    /// <summary>
    /// On collision.
    /// </summary>
    /// <param name="collision">The collision object.</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Edit -> ProjectSettings... -> Physics2D -> bottom

        this.crashedCar = true;
        this.Crashed();        
    }

    /// <summary>
    /// On click mouse down.
    /// </summary>
    private void OnMouseDown()
    {
        this.bestCar = !this.bestCar;

        if (this.bestCar)
        {
            this.gameObject.GetComponent<SpriteRenderer>().color = Color.green;
        }
        else
        {
            if (this.crashedCar)
            {
                this.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
            }
            else
            {
                this.gameObject.GetComponent<SpriteRenderer>().color = Color.yellow;
            }
        }
    }

    /// <summary>
    /// Create the neural network.
    /// </summary>
    private void CreateNetwork()
    {
        this.network = new NeuralNetwork(4);
        this.network.AddLayer(5, ActivationFunctionType.LeakyReLU);
        this.network.AddLayer(5, ActivationFunctionType.LeakyReLU);
        this.network.AddLayer(4, ActivationFunctionType.Sigmoid);
        this.network.InitializeWeights();
    }

    /// <summary>
    /// Update the input variables to the neural network.
    /// </summary>
    private void SensorsMeasure()
    {
        this.distance1 = this.CheckDistance(Vector2.up);
        this.distance2 = this.CheckDistance(new Vector2(-0.5f, 1f));
        this.distance3 = this.CheckDistance(new Vector2(0.5f, 1f));
        this.carSpeedMagnitude = this.rb.velocity.magnitude;
    }

    /// <summary>
    /// Change the color of the car if is crashed.
    /// </summary>
    private void Crashed()
    {
        if (this.crashedCar)
        {
            if (!this.bestCar)
            {
                this.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
            }
        }
    }

    /// <summary>
    /// Check distance from  the center of car to nearest 2D collider in specific direction.
    /// </summary>
    /// <param name="direction">The Direction.</param>
    /// <returns>The Distance.</returns>
    private float CheckDistance(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(this.gameObject.transform.position, this.transform.rotation * direction, Mathf.Infinity, this.mapLayer);

        if (hit.transform != null)
        {
            return hit.distance;
        }
        else
        {
            return -1;
        }
    }

    /// <summary>
    /// Set the new input to the neural network, calculate the output and decides how the car have to move.
    /// </summary>
    private void NetworkDecide()
    {
        List<double> input = new List<double>() { this.distance1, this.distance2, this.distance3, this.carSpeedMagnitude };
        this.network.SetInput(input);
        this.network.FeedForward();
        List<double> output = this.network.GetOutput();

        if (output[0] > output[1])
        {
            this.horizontalAction = 1f;
        }
        else
        {
            this.horizontalAction = -1f;
        }

        if (output[2] > output[3])
        {
            this.verticalAction = 1f;
        }
        else
        {
            this.verticalAction = -1f;
        }
    }
}
