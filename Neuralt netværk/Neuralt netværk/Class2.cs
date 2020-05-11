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

        private Application outputscoreapp;
        private FileStream ifs;
        private FileStream ibs;
        private BinaryReader lbr;
        private BinaryReader ibr;
        private int[] layerss;
        private NeuralNetwork network;

        public void Startup() 
        {

            outputscoreapp = new Application();
            Workbook wb = outputscoreapp.Workbooks.Add();
            Worksheet ws = wb.Worksheets.Add();


            //loader imagestreams, hardcoded, HUSK ADMIN PRIVELEGES
            ifs = new FileStream("C:\\trainimages.idx3-ubyte", FileMode.Open); 
            ibr = new BinaryReader(ifs);

            ibs = new FileStream("C:\\trainlabels.idx1-ubyte", FileMode.Open);
            lbr = new BinaryReader(ibs);

            layerss = new int[] { 784, 36, 10 }; //Strukturen for netværket defineres her

            //Sørger for vi læser hvor end der er labels og billeder (Første 4 4-bytes er headers)
            int aa = ibr.ReadInt32();
            aa = ibr.ReadInt32();
            aa = ibr.ReadInt32();
            aa = ibr.ReadInt32();

            int bb = lbr.ReadInt32();
            bb = lbr.ReadInt32();

            //initiere en instans af NeuralNetwork
            network = new NeuralNetwork(layerss);

            Engage();



        }

        void Engage()
        {
            for (int a = 0; a < 60000; a++)
            {
                //load billedet
                byte[] pixels = new byte[28 * 28];
                for (int i = 0; i < 28 * 28; i++)
                {

                    pixels[i] = ibr.ReadByte();

                }
                Currenttrainimage = pixels;
                Currenttrainlabel = Convert.ToInt32(lbr.ReadByte());

                //compute de perfekte svar
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
                //backpropogate
                network.BackPropogate(expectvalueslist.ToArray(), Currenttrainimage); 

                //tæl successer
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


    }
}


