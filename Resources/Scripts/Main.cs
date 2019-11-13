using ArtificialNeuralNetwork;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Main programm.
/// </summary>
public class Main : MonoBehaviour
{
    /// <summary>
    /// Cars ammount.
    /// </summary>
    public int carsAmmount;

    /// <summary>
    /// Text - cars.
    /// </summary>
    public GameObject textCars;

    /// <summary>
    /// Text - speed.
    /// </summary>
    public GameObject textSpeed;

    /// <summary>
    /// Text - generation.
    /// </summary>
    public GameObject textGeneration;

    /// <summary>
    /// Car prefab.
    /// </summary>
    public GameObject carPrefab;

    /// <summary>
    /// Time scale.
    /// </summary>
    private float timeScale;

    /// <summary>
    /// Time scale max.
    /// </summary>
    private float timeScaleMax;

    /// <summary>
    /// Time scale min.
    /// </summary>
    private float timeScaleMin;

    /// <summary>
    /// Time scale step.
    /// </summary>
    private float timeScaleStep;

    /// <summary>
    /// Generation number.
    /// </summary>
    private int generationNumber;

    /// <summary>
    /// All cars.
    /// </summary>
    private GameObject allCars;

    /// <summary>
    /// The first performed function.
    /// </summary>
    private void Start()
    {
        this.timeScale = 1f;
        this.timeScaleMax = 4f;
        this.timeScaleMin = 1f;
        this.timeScaleStep = 0.2f;
        this.SetTextSpeed();
        this.generationNumber = 1;

        this.allCars = this.gameObject;
        this.SpawnCars(carsAmmount);

        this.textCars.GetComponent<TextMeshPro>().text = "Cars: " + this.carsAmmount;
    }

    /// <summary>
    /// Updates per every frame.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            this.PauseSimulation();
        }

        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            this.timeScale += this.timeScaleStep;

            if (this.timeScale > this.timeScaleMax)
            {
                this.timeScale = this.timeScaleMax;
            }

            this.timeScale = Mathf.Round(this.timeScale * 10) / 10;
            Time.timeScale = this.timeScale;
            this.SetTextSpeed();
        }

        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            this.timeScale -= this.timeScaleStep;

            if (this.timeScale < this.timeScaleMin)
            {
                this.timeScale = this.timeScaleMin;
            }

            this.timeScale = Mathf.Round(this.timeScale * 10) / 10;
            Time.timeScale = this.timeScale;
            this.SetTextSpeed();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            for (int i = 0; i < this.allCars.transform.childCount; i++)
            {
                Destroy(this.allCars.transform.GetChild(i).gameObject);
            }

            this.SpawnCars(carsAmmount);

            if (Time.timeScale == 0)
            {
                this.PauseSimulation();
            }

            this.generationNumber = 1;
            this.SetTextGeneration();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            this.ResetCars();

            if (Time.timeScale == 0)
            {
                this.PauseSimulation();
            }

            this.SetTextGeneration();
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            for (int i = 0; i < this.allCars.transform.childCount; i++)
            {
                this.allCars.transform.GetChild(i).gameObject.GetComponent<Car>().bestCar = false;

                if (this.allCars.transform.GetChild(i).gameObject.GetComponent<Car>().crashedCar)
                {
                    this.allCars.transform.GetChild(i).gameObject.GetComponent<SpriteRenderer>().color = Color.red;
                }
                else
                {
                    this.allCars.transform.GetChild(i).gameObject.GetComponent<SpriteRenderer>().color = Color.yellow;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    /// <summary>
    /// Pause the simulation.
    /// </summary>
    private void PauseSimulation()
    {
        if (Time.timeScale == 0)
        {
            Time.timeScale = this.timeScale;
        }
        else
        {
            Time.timeScale = 0;
        }

        this.SetTextSpeed();
    }

    /// <summary>
    /// Set the text - generation.
    /// </summary>
    private void SetTextGeneration()
    {
        this.textGeneration.GetComponent<TextMeshPro>().text = "Generation = " + this.generationNumber;
    }

    /// <summary>
    /// Set the text - speed.
    /// </summary>
    private void SetTextSpeed()
    {
        this.textSpeed.GetComponent<TextMeshPro>().text = "Speed = " + Time.timeScale;
    }

    /// <summary>
    /// Set position of the car.
    /// </summary>
    /// <param name="car"></param>
    private void SetCar(GameObject car)
    {
        car.transform.position = new Vector3(29f, -6f, 1f);
        car.transform.rotation = new Quaternion(0, 0, 0, 0);
        car.gameObject.GetComponent<Car>().bestCar = false;
        car.gameObject.GetComponent<Car>().crashedCar = false;
        car.gameObject.GetComponent<SpriteRenderer>().color = Color.yellow;
    }

    /// <summary>
    /// Spawn cars.
    /// </summary>
    /// <param name="ammount">Amount of cars.</param>
    private void SpawnCars(int ammount)
    {
        GameObject newCar;

        for (int i = 0; i < ammount; i++)
        {
            newCar = Instantiate(carPrefab);
            newCar.name = "Car1_n" + i;
            newCar.transform.parent = this.allCars.transform;

            this.SetCar(newCar);
        }
    }

    /// <summary>
    /// Reset cars.
    /// </summary>
    private void ResetCars()
    {
        Transform car;

        this.NeuralNetworkLearn();

        for (int i = 0; i < this.allCars.transform.childCount; i++)
        {
            car = this.allCars.transform.GetChild(i);

            this.SetCar(car.gameObject);
        }
    }

    /// <summary>
    /// Learn the new weights of the neural networks.
    /// </summary>
    private void NeuralNetworkLearn()
    {
        List<List<List<double>>> car = null;
        List<List<List<double>>> currentCar = null;
        List<List<List<List<double>>>> bestCars = new List<List<List<List<double>>>>();

        for (int i = 0; i < this.allCars.transform.childCount; i++)
        {
            if (this.allCars.transform.GetChild(i).gameObject.GetComponent<Car>().bestCar)
            {
                car = this.allCars.transform.GetChild(i).gameObject.GetComponent<Car>().network.GetWeights();
                bestCars.Add(car);
                break;
            }
        }

        if(bestCars.Count != 0)
        {
            this.generationNumber += 1;

            foreach (var bestCar in bestCars)
            {
                for (int i = 0; i < this.allCars.transform.childCount; i++)
                {
                    currentCar = this.allCars.transform.GetChild(i).gameObject.GetComponent<Car>().network.GetWeights();
                    car = LearnNetwork.MixGenes(bestCar, currentCar);
                    this.allCars.transform.GetChild(i).gameObject.GetComponent<Car>().network.SetWeights(car);
                }
            }
        }
    }
}
