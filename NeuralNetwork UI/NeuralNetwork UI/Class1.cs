using System.Collections.Generic;
using System;
using System.IO;
using System.Linq; //for maxvalues i arrays
using Microsoft.Office.Interop.Excel;

public class NeuralNetwork
{
    private int[] layers;
    private float[][] neurons;
    private float[][] biases;
    private float[][][] weights;
    private float[][][] weightchanges;//samme struktur som weightschanges
    private float[][] biaschange;
    private float costcounter;
    private Application outputscoreapp;
    private Workbook wb;
    private Worksheet ws;
    private int counter;

    private Random random;
    private float batchsize;

    public NeuralNetwork(int[] layers)
    {
        outputscoreapp = new Application();
        wb = outputscoreapp.Workbooks.Add();
        ws = wb.Worksheets.Add();
        random = new Random();
        trainiterations = new int[2];
        counter = 1;


        batchsize = 100;

        //loader lagstrukturen
        this.layers = new int[layers.Length];
        for (int i = 0; i < layers.Length; i++)
        {
            this.layers[i] = layers[i];
        }

        //initierer datastrukturer
        InitializeNeurons();
        InitializeBiases();
        InitializeWeights();

    }

    private void InitializeNeurons()
    {
        List<float[]> neuronsList = new List<float[]>();

        for (int i = 0; i < layers.Length; i++)
        {
            neuronsList.Add(new float[layers[i]]);

        }
        neurons = neuronsList.ToArray();

    }

    private void InitializeBiases()
    {
        //Biases laves til alle neuroner, selvom de ikke bruges på inputlaget; Disse defineres istedet ved FeedForward-metoden som input.
        List<float[]> biasList = new List<float[]>();
        List<float[]> biaschangeList = new List<float[]>();
        for (int i = 0; i < layers.Length; i++)
        {
            float[] bias = new float[layers[i]];
            float[] biaschange = new float[layers[i]];
            for (int j = 0; j < layers[i]; j++)
            {
                bias[j] = (Single)random.NextDouble();
                biaschange[j] = 0;
            }
            biasList.Add(bias);
            biaschangeList.Add(biaschange);
        }
        biases = biasList.ToArray();
        biaschange = biaschangeList.ToArray();
    }

    private void InitializeWeights()
    {
        List<float[][]> weightsList = new List<float[][]>();
        List<float[][]> weightschangelist = new List<float[][]>();

        for (int i = 1; i < layers.Length; i++)  //Kører én gang for hvert lag af neuroner (efter første)
        {
            List<float[]> layerWeightsList = new List<float[]>();
            List<float[]> layerweightchangelist = new List<float[]>(); //kopiering
            int neuronsInPreviousLayer = layers[i - 1];
            for (int j = 0; j < neurons[i].Length; j++)  //kører én gang for hver neuron i laget
            {
                float[] neuronWeights = new float[neuronsInPreviousLayer]; //en weight til hver neuron i forrige lag
                float[] neuronchangeWeights = new float[neuronsInPreviousLayer];
                for (int k = 0; k < neuronsInPreviousLayer; k++) //kører en gang for hver neuron i forrige lag
                {
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

    public float[] FeedForward(float[] inputs)
    {

        for (int i = 0; i < inputs.Length; i++) //sætter første lags neuroner lig inputs
        {
            neurons[0][i] = inputs[i];
        }
        for (int i = 1; i < layers.Length; i++) //Kører én gang for hvert lag (efter første)
        {
            int layer = i - 1;
            for (int j = 0; j < neurons[i].Length; j++) //Kører en gang for hver neuron i laget
            {
                float z = 0f;
                for (int k = 0; k < neurons[i - 1].Length; k++) //Kører én gang for hver neuron i forrige lag
                {
                    z += weights[i - 1][j][k] * neurons[i - 1][k];   //weights[i - 1][j][k] vægtene fra forrige lag (i-1), j er vægtene der går ind i 
                }
                //sætter neuronværdier
                neurons[i][j] = activate(z + biases[i][j]);


            }
        }
        return neurons[neurons.Length - 1];
    }

    public float activate(float value) //aktiveringsfunktion
    {
        float a = Convert.ToSingle(1 / (1 + Math.Exp(Convert.ToSingle(-value))));
        return a;
    }
    public float activatem(float value) //differentierede function
    {
        float a = Convert.ToSingle(Math.Exp(-(double)value) / Math.Pow((1 + Math.Exp(-(double)value)), 2));
        return a;
    }
    public float invactive(float value) //invers aktiveringsfunktion
    {

        float a = Convert.ToSingle(Math.Log(value / (1 - value)));

        return a;
    }


    public void BackPropogate(float[] expectedvalues, byte[] input)
    {
        //convert input array fra byte til float
        float[] realinput = new float[784];
        for (int i = 0; i < input.Length; i++)
        {
            realinput[i] = Convert.ToSingle(input[i]) / 255;
        }
        float[] output = new float[10];

        //FeedForward netværket
        output = FeedForward(realinput);
        Bestguess = output.ToList().IndexOf(output.Max()).ToString();



        //compute errors
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
        }
        //statistik
        costcounter += cost;

        /////////////////////////////////////////////////////////////////////BackPropogation/////////////////////////////////////////////////////////////////////////////////////////7

        //første til midterste lag
        for (int j = 0; j < 10; j++)
        {
            //float biasvægt = 2 * (output[j] - expectedvalues[j]) * 1 * activatem(invactive(neurons[2][j]) + biaschange[2][j]);
            //biaschange[2][j] += -biasvægt * 0.01f;
            for (int k = 0; k < 36; k++) //Kører én gang for hver neuron i forrige lag
            {

                //float vægt = 2 * (output[j] - expectedvalues[j]) * (neurons[1][k]) * activatem(invactive(neurons[2][j])+biaschange[2][j]);

                float vægt = 2 * (output[j] - expectedvalues[j]) * (neurons[1][k]) * activatem(invactive(neurons[2][j]));


                weightchanges[1][j][k] += -vægt * 0.01f;

            }

        }


        //for (int j = 0; j < 36; j++)
        //{
        //    float vægt = 0;
        //    float da_dtotal = activatem(invactive(neurons[1][j]));
        //    //float da_dtotal = activatem(invactive(neurons[1][j])+biaschange[1][j]);
        //    for (int k = 0; k < 10; k++) //Kører én gang for hver neuron i forrige lag
        //    {

        //        vægt += 1 * da_dtotal * (weights[1][k][j] + weightchanges[1][k][j]) * activatem(invactive(neurons[2][k]) + biaschange[2][k]) * 2 * (output[k] - expectedvalues[k]);

        //    }
        //    biaschange[1][j] += -vægt * 0.01f;

        //}





        //midterste til bagerste lag : hvor meget skal netop denne vægt ændres i forhold til de 10 costværdier:
        for (int k = 0; k < 784; k++) //Kører én gang for hver neuron i inputlaget
        {

            for (int j = 0; j < 36; j++) //Kører én gang for hver neuron i mellemste lag
            {

                //float d_total = activatem(invactive(neurons[1][j]) + biaschange[1][j]); 

                float d_total = activatem(invactive(neurons[1][j]));
                float vægt = 0;
                for (int l = 0; l < 10; l++)
                {
                    vægt += neurons[0][k] * d_total * (weights[1][l][j] + weightchanges[1][l][j]) * activatem(invactive(neurons[2][l])) * 2 * (output[l] - expectedvalues[l]);

                    //vægt += neurons[0][k] * d_total * (weights[1][l][j] + weightchanges[1][l][j]) * activatem(invactive(neurons[2][l]) + biaschange[2][l]) * 2 * (output[l] - expectedvalues[l]);
                }

                weightchanges[0][j][k] += -vægt * 0.01f;

            }

        }

        trainiterations[0] += 1;
        trainiterations[1]++;

        if (batchsize <= trainiterations[0])
        {

            float succesrate = (float)success / (float)(trainiterations[1]);
            Console.WriteLine(succesrate.ToString() + "#" + (costcounter / trainiterations[0]).ToString());
            costcounter = 0f;
            ws.Cells[counter, 2] = succesrate.ToString();
            counter++;

            success = 0;
            trainiterations[1] = 0;

            //merger weights og weightchanges
            for (int j = 0; j < 10; j++)
            {
                for (int k = 0; k < 36; k++)
                {
                    weights[1][j][k] += weightchanges[1][j][k];
                    weightchanges[1][j][k] = 0f;
                    //biases[2][j] += biaschange[2][j];
                    //biaschange[2][j] = 0f;
                }

            }

            for (int j = 0; j < 36; j++)
            {
                for (int k = 0; k < 784; k++)
                {
                    weights[0][j][k] += weightchanges[0][j][k];
                    weightchanges[0][j][k] = 0f;
                    //biases[1][j] += biaschange[1][j];
                    //biaschange[1][j] = 0f;
                }
            }
            trainiterations[0] = 0;
        }
    }

    public void Load(string path)
    {
        TextReader tr = new StreamReader(path);
        int NumberOfLines = (int)new FileInfo(path).Length;
        string[] ListLines = new string[NumberOfLines];
        int index = 1;
        for (int i = 1; i < NumberOfLines; i++)
        {
            ListLines[i] = tr.ReadLine(); //læser hver enkel linje i TextReaderens path
        }
        tr.Close();
        if (new FileInfo(path).Length > 0)
        {
            //indlæs biases
            for (int i = 0; i < biases.Length; i++)
            {
                for (int j = 0; j < biases[i].Length; j++)
                {
                    biases[i][j] = float.Parse(ListLines[index]);
                    index++;
                }
            }
            //indlæs vægte
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

    public void Save(string path)
    {
        File.Create(path).Close();
        StreamWriter writer = new StreamWriter(path, true);

        for (int i = 0; i < biases.Length; i++)
        {
            for (int j = 0; j < biases[i].Length; j++) //nedskriv bias
            {
                writer.WriteLine(biases[i][j]);
            }
        }

        for (int i = 0; i < weights.Length; i++) //nedskriv vægte
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
    public void savebook()
    {
        wb.SaveAs(success.ToString() + "NNOutput.xlsx");
    }

    public int[] trainiterations { get; set; }

    public int success { get; set; }

    public string Bestguess { get; set; }
}
