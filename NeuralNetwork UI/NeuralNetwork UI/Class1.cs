using System.Collections.Generic;
using System;
using System.IO;
using System.Linq; //for maxvalues i arrays


public class NeuralNetwork : IComparable<NeuralNetwork>
{
    private int[] layers;//layers
    private float[][] neurons;//neurons
    private float[][] biases;//biasses
    private float[][] biaschange;
    private float[][][] weights;//weights
    private float[][][] weightchanges;//weightschanges
    private int[] activations;//layers
    private int counter;
    private float costcounter;


    private Random random;


    public NeuralNetwork(int[] layers)
    {



        counter = 1;
        trainiterations = new int[2];
        epochtime = 100;
        random = new Random();
        this.layers = new int[layers.Length];
        for (int i = 0; i < layers.Length; i++)
        {
            this.layers[i] = layers[i];
        }
        InitNeurons();
        InitBiases();
        InitWeights();





    }

    private void InitNeurons()//create empty storage array for the neurons in the network.
    {
        List<float[]> neuronsList = new List<float[]>();


        for (int i = 0; i < layers.Length; i++)
        {
            neuronsList.Add(new float[layers[i]]);

        }
        neurons = neuronsList.ToArray();

    }

    private void InitBiases()//initializes and populates array for the biases being held within the network.
    {
        List<float[]> biasList = new List<float[]>();
        List<float[]> biaschangeList = new List<float[]>();
        for (int i = 0; i < layers.Length; i++)
        {
            float[] bias = new float[layers[i]];
            float[] biaschange = new float[layers[i]];
            for (int j = 0; j < layers[i]; j++)
            {
                bias[j] = Convert.ToSingle(random.NextDouble());
                biaschange[j] = 0;
            }
            biasList.Add(bias);
            biaschangeList.Add(biaschange);
        }
        biases = biasList.ToArray();
        biaschange = biaschangeList.ToArray();
    }

    private void InitWeights()//initializes random array for the weights being held in the network.
    {
        List<float[][]> weightsList = new List<float[][]>();
        List<float[][]> weightschangelist = new List<float[][]>(); //kopiering
        for (int i = 1; i < layers.Length; i++)  //Kører én gang for hvert lag af neuroner (efter første)
        {
            List<float[]> layerWeightsList = new List<float[]>();
            List<float[]> layerweightchangelist = new List<float[]>(); //kopiering
            int neuronsInPreviousLayer = layers[i - 1];
            for (int j = 0; j < neurons[i].Length; j++)  //kører for hver neuron i laget
            {
                float[] neuronWeights = new float[neuronsInPreviousLayer]; //en weight til hver neuron i forrige lag
                float[] neuronchangeWeights = new float[neuronsInPreviousLayer]; //en weight til hver neuron i forrige lag
                for (int k = 0; k < neuronsInPreviousLayer; k++)
                {
                    //float sd = 1f / ((neurons[i].Length + neuronsInPreviousLayer) / 2f);
                    neuronWeights[k] = (Single)random.NextDouble() - 0.5f;
                    neuronchangeWeights[k] = 0;
                }
                layerWeightsList.Add(neuronWeights);
                layerweightchangelist.Add(neuronchangeWeights);

            }
            weightsList.Add(layerWeightsList.ToArray());
            weightschangelist.Add(layerweightchangelist.ToArray());
        }
        weights = weightsList.ToArray();
        weightchanges = weightschangelist.ToArray();
    }

    public float[] FeedForward(float[] inputs)//feed forward, inputs >==> outputs.
    {
        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }
        for (int i = 1; i < layers.Length; i++)
        {
            int layer = i - 1;
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float value = 0f;
                for (int k = 0; k < neurons[i - 1].Length; k++) //Kører én gang for hver neuron i forrige lag
                {
                    value += weights[i - 1][j][k] * neurons[i - 1][k];   //weights[i - 1][j][k] vægtene fra forrige lag (i-1), j er vægtene der går ind i 
                }
                neurons[i][j] = activate(value + biases[i][j]);

            }
        }
        return neurons[neurons.Length - 1];
    }

    public float activate(float value)
    {

        float a = Convert.ToSingle(1 / (1 + Math.Exp(Convert.ToSingle(-value))));

        return a;
    }
    public float activatem(float value)
    {

        float a = Convert.ToSingle(Math.Exp(-(double)value) / Math.Pow((1 + Math.Exp(-(double)value)), 2));

        return a;
    }
    public float invactive(float value)
    {

        float a = Convert.ToSingle(Math.Log(value / (1 - value)));

        return a;
    }

    private int epochtime;
    public void BackPropogate(float[] expectedvalues, byte[] input, int lag)
    {

        float[] realinput = new float[784];
        for (int i = 0; i < input.Length; i++)
        {
            realinput[i] = Convert.ToSingle(input[i]) / 255;
        }
        float[] output = new float[10];
        output = FeedForward(realinput);
        Bestguess = output.ToList().IndexOf(output.Max()).ToString();
        //Console.WriteLine("Currently thinks this is: " + Bestguess);
        //vi har nu output og expected values



        //compute errors (and total errors)
        float[] errors = new float[10];
        float cost = 0f;
        for (int i = 0; i < output.Length; i++)
        {
            if (expectedvalues[i] > 0f)
            {
                errors[i] = Convert.ToSingle(-Math.Pow(output[i] - expectedvalues[i], 2));
            }
            else
            {
                errors[i] = Convert.ToSingle(Math.Pow(output[i] - expectedvalues[i], 2));
            }

            cost += Convert.ToSingle(Math.Pow(output[i] - expectedvalues[i], 2));

            //Console.WriteLine(i.ToString() + " Error: " + errors[i].ToString());

        }
        costcounter += cost;


        //backpropogate

        for (int j = 0; j < 10; j++)
        {

            for (int k = 0; k < 36; k++) //Kører én gang for hver neuron i forrige lag
            {
                //hvormeget skal

                float vægtvægt = 2 * (output[j] - expectedvalues[j]) * (neurons[1][k]) * activatem(invactive(neurons[2][j]));

                weightchanges[1][j][k] += -vægtvægt * 0.01f;

                float biasvægt = 2 * (output[j] - expectedvalues[j]) * activatem(invactive(neurons[2][j]));
                biaschange[2][j] += 0;
            }

        }

        //første til midterste lag
        for (int k = 0; k < 784; k++)
        {

            for (int j = 0; j < 36; j++) //Kører én gang for hver neuron i inputlaget
            {
                //hvor meget skal netop denne vægt ændres i forhold til de 10 costværdier:
                float da_dtotal = activatem(invactive(neurons[1][j]));
                float vægt = 0;
                for (int l = 0; l < 10; l++)
                {
                    vægt += neurons[0][k] * da_dtotal * (weights[1][l][j] + weightchanges[1][l][j]) * activatem(invactive(neurons[2][l])) * 2 * (output[l] - expectedvalues[l]);
                }

                weightchanges[0][j][k] += -vægt * 0.01f;


            }

        }




        //compute igen
        trainiterations[0] += 1;
        trainiterations[1]++;
        if (epochtime <= trainiterations[0]) //kopieret fra initweights
        {

            float succesrate = (float)success / (float)(trainiterations[1]);
            Console.WriteLine(succesrate.ToString() + "#" + (costcounter / trainiterations[0]).ToString());
            costcounter = 0f;

            counter++;

            success = 0;
            trainiterations[1] = 0;

            int neuronsInPreviousLayer = layers[1];
            for (int j = 0; j < neurons[2].Length; j++)  //kører for hver neuron i laget
            {

                for (int k = 0; k < neuronsInPreviousLayer; k++)
                {
                    //float sd = 1f / ((neurons[i].Length + neuronsInPreviousLayer) / 2f);
                    weights[1][j][k] += weightchanges[1][j][k];
                    weightchanges[1][j][k] = 0f;
                    //biases[2][j] += biaschange[2][j];
                    //biaschange[2][j] = 0f;

                }


            }
            neuronsInPreviousLayer = layers[0];
            for (int j = 0; j < neurons[1].Length; j++)  //kører for hver neuron i laget
            {

                for (int k = 0; k < neuronsInPreviousLayer; k++)
                {
                    //float sd = 1f / ((neurons[i].Length + neuronsInPreviousLayer) / 2f);
                    weights[0][j][k] += weightchanges[0][j][k];
                    weightchanges[0][j][k] = 0f;
                    //biases[2][j] += biaschange[2][j];
                    //biaschange[2][j] = 0f;

                }


            }



            trainiterations[0] = 0;
        }


    }
    public void savebook()
    {

    }



    public int[] trainiterations { get; set; }

    public int success { get; set; }

    public string Bestguess { get; set; }


    public int CompareTo(NeuralNetwork other) //Comparing For NeuralNetworks performance.
    {
        if (other == null) return 1;

        if (fitness > other.fitness)
            return 1;
        else if (fitness < other.fitness)
            return -1;
        else
            return 0;
    }
    public float fitness
    {
        get;
        set;
    }
    public float Errorrate { get; set; }

    public NeuralNetwork copy(NeuralNetwork nn) //For creatinga deep copy, to ensure arrays are serialzed.
    {
        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                nn.biases[i][j] = biases[i][j];
            }
        }
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    nn.weights[i][j][k] = weights[i][j][k];
                }
            }
        }
        return nn;
    }

    public void Load(string path)//this loads the biases and weights from within a file into the neural network.
    {
        TextReader tr = new StreamReader(path);
        int NumberOfLines = (int)new FileInfo(path).Length;
        string[] ListLines = new string[NumberOfLines];
        int index = 1;
        for (int i = 1; i < NumberOfLines; i++)
        {
            ListLines[i] = tr.ReadLine();
        }
        tr.Close();
        if (new FileInfo(path).Length > 0)
        {
            for (int i = 0; i < biases.Length; i++)
            {
                for (int j = 0; j < biases[i].Length; j++)
                {
                    biases[i][j] = float.Parse(ListLines[index]);
                    index++;
                }
            }

            for (int i = 0; i < weights.Length; i++)
            {
                for (int j = 0; j < weights[i].Length; j++)
                {
                    for (int k = 0; k < weights[i][j].Length; k++)
                    {
                        weights[i][j][k] = float.Parse(ListLines[index]); ;
                        index++;
                    }
                }
            }
        }
    }

    public void Save(string path)//this is used for saving the biases and weights within the network to a file.
    {
        File.Create(path).Close();
        StreamWriter writer = new StreamWriter(path, true);

        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++)
            {
                writer.WriteLine(biases[i][j]);
            }
        }

        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    writer.WriteLine(weights[i][j][k]);
                }
            }
        }
        writer.Close();
    }
}
