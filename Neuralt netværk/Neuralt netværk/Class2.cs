using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;

namespace Neuralt_netværk
{
    class Manager
    {
        /// <summary>
        /// Fix vægt i forrige lag
        /// ellers solid 70% succesrate!
        /// </summary>

        private Application outputscoreapp;
        private FileStream ifs;
        private FileStream ibs;
        private BinaryReader lbr;
        private BinaryReader ibr;
        private int[] layerss;
        private int counterGeneration;
        private bool started;
        private NeuralNetwork network;
        public void Startup() //string[] args
        {


            outputscoreapp = new Application();
            Workbook wb = outputscoreapp.Workbooks.Add();
            Worksheet ws = wb.Worksheets.Add();



            ifs = new FileStream("C:\\trainimages.idx3-ubyte", FileMode.Open); //loader imagestream
            ibr = new BinaryReader(ifs);

            ibs = new FileStream("C:\\trainlabels.idx1-ubyte", FileMode.Open);
            lbr = new BinaryReader(ibs);

            layerss = new int[] { 784, 36, 10 }; //ændret

            //Sørger for vi læser hvor end der er labels og billeder
            int aa = ibr.ReadInt32();
            aa = ibr.ReadInt32();
            aa = ibr.ReadInt32();
            aa = ibr.ReadInt32();

            int bb = lbr.ReadInt32();
            bb = lbr.ReadInt32();

            network = new NeuralNetwork(layerss);

            Engage();



        }
        //void EngageTest()
        //{
            
        //}
        void Engage()
        {
            for (int a = 0; a < 60000; a++)
            {


                byte[] pixels = new byte[28 * 28];
                for (int i = 0; i < 28 * 28; i++)
                {

                    pixels[i] = ibr.ReadByte();

                }
                Currenttrainimage = pixels;

                byte label = lbr.ReadByte();
                Currenttrainlabel = Convert.ToInt32(label);
                //Console.WriteLine("Current image showing: " + Currenttrainlabel.ToString());








                List<float> expectvalueslist = new List<float>();
                for (int i = 0; i < 10; i++)
                {
                    if (i == Currenttrainlabel)
                    {
                        expectvalueslist.Add(1f);

                    }
                    else
                    {
                        expectvalueslist.Add(0f);
                    }
                }
                network.BackPropogate(expectvalueslist.ToArray(), Currenttrainimage, 0);
                //Check sucessrate
                if (network.Bestguess == Currenttrainlabel.ToString())
                {
                    network.success++;
                }

            }



            network.savebook();
            network.Save("TrainedNN.txt");


            Environment.Exit(1);
        }
        byte[] Currenttrainimage
        {
            get;
            set;
        }
        int Currenttrainlabel
        {
            get;
            set;
        }
        public float[] OutputNN(NeuralNetwork n, byte[] input)
        {
            float[] f = new float[784];
            for (int i = 0; i < input.Length; i++)
            {
                f[i] = Convert.ToSingle(input[i]) / 255;


            }
            float[] outputs = n.FeedForward(f);
            return outputs;

        }

    }
}


