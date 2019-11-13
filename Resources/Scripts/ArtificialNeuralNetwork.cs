using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Neural network namespace.
/// </summary>
namespace ArtificialNeuralNetwork
{
    /// <summary>
    /// The activation function types.
    /// </summary>
    public enum ActivationFunctionType
    {
        /// <summary>
        /// None/Linear function type.
        /// </summary>
        None = 0,

        /// <summary>
        /// Sigoid function type.
        /// </summary>
        Sigmoid = 1,

        /// <summary>
        /// LeakyReLU function type.
        /// </summary>
        LeakyReLU = 2
    }

    /// <summary>
    /// The activation functions.
    /// </summary>
    public static class ActivationFunctions
    {
        /// <summary>
        /// None/Linear function.
        /// </summary>
        /// <param name="x">The argument.</param>
        /// <param name="derivative">Derivative of None/Linear function.</param>
        /// <returns>The function result.</returns>
        public static double NoneFunction(double x, bool derivative)
        {
            if (!derivative)
            {
                return x;
            }
            else
            {
                return x;
            }
        }

        /// <summary>
        /// Sigmoid function.
        /// </summary>
        /// <param name="x">The argument.</param>
        /// <param name="derivative">Derivative of Sigmoid function.</param>
        /// <returns>The function result.</returns>
        public static double SigmoidFunction(double x, bool derivative)
        {
            if (!derivative)
            {
                return 1 / (1 + Math.Pow(Math.E, -x));
            }
            else
            {
                return x * (1 - x);
            }
        }

        /// <summary>
        /// LeakyReLU function.
        /// </summary>
        /// <param name="x">The argument.</param>
        /// <param name="derivative">Derivative of LeakyReLU function.</param>
        /// <returns>The function result.</returns>
        public static double LeakyReLU(double x, bool derivative)
        {
            if (!derivative)
            {
                return x < 0 ? 0.01 * x : x;
            }
            else
            {
                return x < 0 ? 0.01 : 1;
            }
        }
    }

    /// <summary>
    /// The learn network.
    /// </summary>
    public static class LearnNetwork
    {
        /// <summary>
        /// Mix two genes by cut.
        /// </summary>
        /// <param name="geneA">The gene A.</param>
        /// <param name="geneB">The gene B.</param>
        /// <returns>Mixed gene.</returns>
        private static double MixTwoGenesByCut(double geneA, double geneB)
        {
            Random rand = new Random(Guid.NewGuid().GetHashCode());

            long geneIntA = BitConverter.DoubleToInt64Bits(geneA);
            long geneIntB = BitConverter.DoubleToInt64Bits(geneB);
            long swap;

            if (rand.Next(0, 2) == 1)
            {
                swap = geneIntA;
                geneIntA = geneIntB;
                geneIntB = swap;
            }

            long mask = 0;
            long newGene;

            int cut = rand.Next(1, 64);

            for (int i = 0; i < cut; i++)
            {
                mask |= 1L << 63 - i;
            }

            newGene = (geneIntA & mask) | (geneIntB & ~mask);
            // newGene = Mutate(newGene);

            return BitConverter.Int64BitsToDouble(newGene);
        }

        /// <summary>
        /// Mutate gene.
        /// </summary>
        /// <param name="gene">The gene.</param>
        /// <returns>Mutated gene.</returns>
        private static long Mutate(long gene)
        {
            // Don't work ?

            Random rand = new Random(Guid.NewGuid().GetHashCode());

            long mask = 0;
            long value = 0;

            for (int i = 0; i < 64; i++)
            {
                if (rand.Next(0, 1001) == 0)
                {
                    mask = 1L << i;
                    value = mask & gene;

                    if (value == 0)
                    {
                        gene |= mask;
                    }
                    else
                    {
                        gene &= ~mask;
                    }
                }
            }

            return gene;
        }

        /// <summary>
        /// Mix all genes in two set of weights.
        /// </summary>
        /// <param name="weights1">First weights set.</param>
        /// <param name="weights2">Second weights set.</param>
        /// <returns>All genes mixed up.</returns>
        public static List<List<List<double>>> MixGenes(List<List<List<double>>> weights1, List<List<List<double>>> weights2)
        {
            if (weights1.Count == weights2.Count)
            {
                for (int nLayer = 0; nLayer < weights1.Count; nLayer++)
                {
                    if (weights1[nLayer].Count == weights2[nLayer].Count)
                    {
                        for (int nNeuron = 0; nNeuron < weights1[nLayer].Count; nNeuron++)
                        {
                            if (weights1[nLayer][nNeuron].Count != weights2[nLayer][nNeuron].Count)
                            {
                                throw new Exception("Miss match of weights count in neuron.");
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Miss match of neuron count in layer.");
                    }
                }
            }
            else
            {
                throw new Exception("Miss match of layers count.");
            }

            int lastLayer;
            int lastNeuron;
            double mixedWeight;
            List<List<List<double>>> mixedWeights = new List<List<List<double>>>();

            for (int nLayer = 0; nLayer < weights1.Count; nLayer++)
            {
                mixedWeights.Add(new List<List<double>>());
                lastLayer = mixedWeights.Count - 1;

                for (int nNeuron = 0; nNeuron < weights1[nLayer].Count; nNeuron++)
                {
                    mixedWeights[lastLayer].Add(new List<double>());
                    lastNeuron = mixedWeights[lastLayer].Count - 1;

                    for (int nWeight = 0; nWeight < weights1[nLayer][nNeuron].Count; nWeight++)
                    {
                        mixedWeight = MixTwoGenesByCut(weights1[nLayer][nNeuron][nWeight], weights2[nLayer][nNeuron][nWeight]);
                        mixedWeights[lastLayer][lastNeuron].Add(mixedWeight);
                    }
                }
            }

            return mixedWeights;
        }
    }

    /// <summary>
    /// The neural network.
    /// </summary>
    public class NeuralNetwork
    {
        private List<Layer> Layers = new List<Layer>();

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="numberInputs">Number of inputs.</param>
        public NeuralNetwork(int numberInputs)
        {
            if (numberInputs < 1)
            {
                string error = String.Format("Too smal amount of inputs, number of inputs = {0} must be greather than 0.", numberInputs);
                throw new Exception(error);
            }

            this.AddLayer(numberInputs, ActivationFunctionType.None);
        }

        /// <summary>
        /// Set input.
        /// </summary>
        /// <param name="input">The input</param>
        public void SetInput(List<double> input)
        {
            if (input.Count != this.Layers[0].Neurons.Count)
            {
                string exception = string.Format("Miss match of inputs ({0} != {1}).", input.Count, Layers[0].Neurons.Count);
                throw new Exception(exception);
            }

            for (int i = 0; i < input.Count; i++)
            {
                this.Layers[0].Neurons[i].input = input[i];
            }
        }

        /// <summary>
        /// Get output.
        /// </summary>
        /// <returns>The output.</returns>
        public List<double> GetOutput()
        {
            List<double> output = new List<double>();

            foreach (Neuron neuron in this.Layers.Last().Neurons)
            {
                output.Add(neuron.outpt);
            }

            return output;
        }

        /// <summary>
        /// Set all weights
        /// </summary>
        /// <param name="weights">New set of weights.</param>
        public void SetWeights(List<List<List<double>>> weights)
        {
            if (this.Layers.Count < 2)
            {
                throw new Exception("Weights don't exist, network is too smal (network have less than two layers).");
            }

            if (weights.Count == this.Layers.Count)
            {
                for (int nLayer = 0; nLayer < weights.Count; nLayer++)
                {
                    if (weights[nLayer].Count == this.Layers[nLayer].Neurons.Count)
                    {
                        for (int nNeuron = 0; nNeuron < weights[nLayer].Count; nNeuron++)
                        {
                            if (weights[nLayer][nNeuron].Count != this.Layers[nLayer].Neurons[nNeuron].connectionInputs.Count)
                            {
                                throw new Exception("Miss match of weights count in neuron");
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Miss match of neurons count in layer");
                    }
                }
            }
            else
            {
                throw new Exception("Miss match of layers count");
            }

            for (int nLayer = 0; nLayer < weights.Count; nLayer++)
            {
                for (int nNeuron = 0; nNeuron < weights[nLayer].Count; nNeuron++)
                {
                    for (int nWeight = 0; nWeight < weights[nLayer][nNeuron].Count; nWeight++)
                    {
                        this.Layers[nLayer].Neurons[nNeuron].connectionInputs[nWeight].weight = weights[nLayer][nNeuron][nWeight];
                    }
                }
            }
        }

        /// <summary>
        /// Get all weights.
        /// </summary>
        /// <returns></returns>
        public List<List<List<double>>> GetWeights()
        {
            if (this.Layers.Count < 2)
            {
                string error = "Weights don't exist, network is too smal.";
                throw new Exception(error);
            }

            int lastLayer;
            int lastNeuron;
            List<List<List<double>>> weights = new List<List<List<double>>>();

            foreach (Layer layer in this.Layers)
            {
                weights.Add(new List<List<double>>());

                foreach (Neuron neuron in layer.Neurons)
                {
                    lastLayer = weights.Count - 1;
                    weights[lastLayer].Add(new List<double>());

                    foreach (Connection connection in neuron.connectionInputs)
                    {
                        lastNeuron = weights[lastLayer].Count - 1;
                        weights[lastLayer][lastNeuron].Add(connection.weight);
                    }
                }
            }

            return weights;
        }

        /// <summary>
        /// Feed forward action.
        /// </summary>
        public void FeedForward()
        {
            foreach (Layer layer in this.Layers)
            {
                foreach (Neuron neuron in layer.Neurons)
                {
                    neuron.CollectInpulses();
                    neuron.AcivateFunction();
                }
            }
        }

        /// <summary>
        /// Add layer.
        /// </summary>
        /// <param name="amountNeurons">Ammount of neurons in layer.</param>
        /// <param name="type">Type of activation functions in neurons.</param>
        public void AddLayer(int amountNeurons, ActivationFunctionType type)
        {
            this.Layers.Add(new Layer(amountNeurons, type));

            if (this.Layers.Count > 1)
            {
                int lastLayer = this.Layers.Count - 1;
                this.ConnectLayers(this.Layers[lastLayer - 1], this.Layers[lastLayer]);
            }
        }

        /// <summary>
        /// Initialize all weights.
        /// </summary>
        public void InitializeWeights()
        {
            Random rand = new Random(Guid.NewGuid().GetHashCode());

            foreach (Layer layer in this.Layers)
            {
                foreach (Neuron neuron in layer.Neurons)
                {
                    foreach (Connection connection in neuron.connectionInputs)
                    {
                        connection.weight = rand.NextDouble() * 2 - 1;
                    }
                }
            }
        }

        /// <summary>
        /// Connect all neurons in two layers.
        /// </summary>
        /// <param name="layerA">The layer A.</param>
        /// <param name="layerB">The layer B.</param>
        private void ConnectLayers(Layer layerA, Layer layerB)
        {
            foreach (Neuron neuronA in layerA.Neurons)
            {
                foreach (Neuron neuronB in layerB.Neurons)
                {
                    Connection connection = new Connection();

                    neuronA.connectionOutputs.Add(connection);
                    neuronB.connectionInputs.Add(connection);
                    connection.neuronA = neuronA;
                    connection.neuronB = neuronB;
                }
            }
        }

        /// <summary>
        /// The layer class.
        /// </summary>
        private class Layer
        {
            public List<Neuron> Neurons = new List<Neuron>();

            public Layer(int amountNeurons, ActivationFunctionType functionType)
            {
                if (amountNeurons < 1)
                {
                    string error = String.Format("Too smal amount of neurons, number of neurons = {0} must be greather than 0.", amountNeurons);
                    throw new Exception(error);
                }

                for (int i = 0; i < amountNeurons; i++)
                {
                    Neurons.Add(new Neuron(functionType));
                }
            }
        }

        /// <summary>
        /// The neuron class.
        /// </summary>
        private class Neuron
        {
            public double input;
            public double outpt;
            public List<Connection> connectionInputs;
            public List<Connection> connectionOutputs;
            private ActivationFunctionDelegate acivationFunction;
            private delegate double ActivationFunctionDelegate(double input, bool derivative);

            public Neuron(ActivationFunctionType type = ActivationFunctionType.None)
            {
                this.input = 0;
                this.outpt = 0;
                this.connectionInputs = new List<Connection>();
                this.connectionOutputs = new List<Connection>();
                this.SetActivationFunction(type);
            }

            public void CollectInpulses()
            {
                if (this.connectionInputs.Count > 0)
                {
                    this.input = 0;

                    foreach (Connection connection in this.connectionInputs)
                    {
                        this.input += connection.neuronA.outpt * connection.weight;
                    }
                }
            }

            public void AcivateFunction(bool derivative = false)
            {
                this.outpt = this.acivationFunction.Invoke(input, derivative);
            }

            public void SetActivationFunction(ActivationFunctionType type)
            {
                switch (type)
                {
                    case ActivationFunctionType.None:
                        this.acivationFunction = new ActivationFunctionDelegate(ActivationFunctions.NoneFunction);
                        break;
                    case ActivationFunctionType.Sigmoid:
                        this.acivationFunction = new ActivationFunctionDelegate(ActivationFunctions.SigmoidFunction);
                        break;
                    case ActivationFunctionType.LeakyReLU:
                        this.acivationFunction = new ActivationFunctionDelegate(ActivationFunctions.LeakyReLU);
                        break;
                    default:
                        throw new Exception("Wrong activation function type.");
                }
            }
        }

        /// <summary>
        /// The connection class.
        /// </summary>
        private class Connection
        {
            public double weight;
            public Neuron neuronA;
            public Neuron neuronB;

            public Connection()
            {
                this.weight = 0;
            }
        }
    }
}
