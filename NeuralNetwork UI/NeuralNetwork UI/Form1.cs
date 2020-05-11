using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;

namespace NeuralNetwork_UI
{
    public partial class Form1 : Form
    {

        NeuralNetwork network;
        FileStream ifs;
        BinaryReader ibr;
        FileStream ibs;
        BinaryReader lbr;
        float counter;
        float success;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            counter = 0;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            network = new NeuralNetwork(new int[] { 784, 36, 10 });
            network.Load("TrainedNNN.txt"); //hardcoded filsti
            MessageBox.Show("Succesfully loaded!");

            ifs = new FileStream("C:\\testimages.idx3-ubyte", FileMode.Open); //loader imagestream, hardcoded filsti
            ibr = new BinaryReader(ifs);

            int aa = ibr.ReadInt32();
            aa = ibr.ReadInt32();
            aa = ibr.ReadInt32();
            aa = ibr.ReadInt32();

            ibs = new FileStream("C:\\testlabels.idx1-ubyte", FileMode.Open); //hardcoded filsti
            lbr = new BinaryReader(ibs);
            int bb = lbr.ReadInt32();
            bb = lbr.ReadInt32();

            imageList1.ImageSize = new Size(28, 28);
            listView1.View = View.LargeIcon;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            byte[] pixels = new byte[28 * 28];
            for (int i = 0; i < 28 * 28; i++)
            {
                pixels[i] = ibr.ReadByte();
            }
            float[] f = new float[784];
            for (int i = 0; i < pixels.Length; i++)
            {
                f[i] = Convert.ToSingle(pixels[i]) / 255;
            }
            byte label = lbr.ReadByte();
            int actuallabel = Convert.ToInt32(label);

            Image image = Image.FromFile(@"C:\\testing\" + counter.ToString() + ".png"); //hardcoded filsti
            pictureBox1.Image = image;
            float[] outputs = network.FeedForward(f);

            string Bestguess = outputs.ToList().IndexOf(outputs.Max()).ToString();

            if (Convert.ToInt32(Bestguess) ==  actuallabel)
            {
                success++;
            }
            label1.Text = "Neural Network guess: " + Bestguess;
            counter++;
            float succesrate = (success / counter) * 100;
            label3.Text = succesrate.ToString() + "%";
            label4.Text = actuallabel.ToString();
            label2.Text = "Confidence: " + outputs.Max().ToString();

            if (Convert.ToInt32(Bestguess) != actuallabel || outputs.Max()<0.4f)
            {
                imageList1.Images.Add(image);


                this.listView1.View = View.LargeIcon;
                this.imageList1.ImageSize = new Size(32, 32);
                this.listView1.LargeImageList = this.imageList1;


                ListViewItem item = new ListViewItem();
                item.ImageIndex = imageList1.Images.Count-1;
                item.Text = outputs.Max().ToString() +" : "+ actuallabel.ToString() + " ! " + Bestguess;
                if (Convert.ToInt32(Bestguess) == actuallabel) //Gættede rigtigt, men med lav konfidens
                {
                    item.BackColor = Color.Green;
                } else
                {
                    item.BackColor = Color.Red;
                }

                this.listView1.Items.Add(item);
            }


        }

        private void button3_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 100; i++)
            {
                button2_Click(null, null);
            }

        }
    }
}
