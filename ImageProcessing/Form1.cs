using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageProcessing
{
    public partial class Form1 : Form
    {
        private ImageProccessor ip = new ImageProccessor();
        private FeatureProccessor fp = new FeatureProccessor();
        private List<ImageFeature> features = new List<ImageFeature>();

        public Form1()
        {
            InitializeComponent();
            int[,] m = new int[5, 5]
            {
                { 0,0,1,0,0},
                { 3,0,0,0,0},
                { 0,0,0,1,5},
                { 3,0,0,0,0},
                { 0,0,1,1,0}
            };
            //Console.WriteLine(m);
           
        }

        private void GetRawMatrix()
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Open Image";
                dlg.Multiselect = true;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    // Create a new Bitmap object from the picture file on disk,
                    // and assign that to the PictureBox.Image property
                    //Bitmap img = new Bitmap(dlg.FileName);
                    //Bitmap resizedImg = ip.ResizeImage(img, 5, 5);
                    //Bitmap greyscaledImg = ip.MakeGrayscale3(resizedImg);
                    //Bitmap normalizedImg = ip.ContrastStretch(greyscaledImg);
                    features.Clear();
                    
                    for(int i = 0; i < dlg.FileNames.Length; i++)
                    {
                        Bitmap img = new Bitmap(dlg.FileNames[i]);
                        TextBox textName = new TextBox();
                        textName.Text = dlg.FileNames[i].Split('\\')[dlg.FileNames[i].Split('\\').Length-1].Split('.')[0];
                        textName.Parent = panel1;
                        textName.Location = new Point(3, (29 * i) + 3);
                        textName.Size = new Size(600, 26);
                        textName.Name = "file_name_" + i;
                        int[,] matrix0 = ip.sudut0(ip.grayLevel(img), img);
                        int[,] matrix45 = ip.sudut45(ip.grayLevel(img), img);
                        int[,] matrix90 = ip.sudut90(ip.grayLevel(img), img);
                        int[,] matrix135 = ip.sudut135(ip.grayLevel(img), img);
                        ImageFeature feature = new ImageFeature(matrix0, matrix45, matrix90, matrix135);
                        feature.name = textName.Text;
                        textName.TextChanged += on_fileListNameChanged;
                        features.Add(feature);
                    }
                }
            }
        }

        private void on_fileListNameChanged(object sender, EventArgs e)
        {
            string name = ((TextBox)sender).Name;
            int idx = Convert.ToInt32(name.Split('_')[name.Split('_').Length - 1]);
            features[idx].name = ((TextBox)sender).Text;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Open Image";
                dlg.Multiselect = true;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    // Create a new Bitmap object from the picture file on disk,
                    // and assign that to the PictureBox.Image property
                    Bitmap img = new Bitmap(dlg.FileName);
                    imagePanel.BackgroundImage = img;
                    int[,] matrix0 = ip.sudut0(ip.grayLevel(img), img);
                    int[,] matrix45 = ip.sudut45(ip.grayLevel(img), img);
                    int[,] matrix90 = ip.sudut90(ip.grayLevel(img), img);
                    int[,] matrix135 = ip.sudut135(ip.grayLevel(img), img);
                    FeatureInfo result = fp.CalculateMatrix(matrix0, matrix45, matrix90, matrix135);
                    resultBox.Text = result.Result;
                    SREOut0.Text = result.SRE(0).ToString();
                    LREOut0.Text = result.LRE(0).ToString();
                    RLUOut0.Text = result.RLU(0).ToString();
                    GLUOut0.Text = result.GLU(0).ToString();
                    RPCOut0.Text = result.RPC(0).ToString();
                    SREOut45.Text = result.SRE(45).ToString();
                    LREOut45.Text = result.LRE(45).ToString();
                    RLUOut45.Text = result.RLU(45).ToString();
                    GLUOut45.Text = result.GLU(45).ToString();
                    RPCOut45.Text = result.RPC(45).ToString();
                    SREOut90.Text = result.SRE(90).ToString();
                    LREOut90.Text = result.LRE(90).ToString();
                    RLUOut90.Text = result.RLU(90).ToString();
                    GLUOut90.Text = result.GLU(90).ToString();
                    RPCOut90.Text = result.RPC(90).ToString();
                    SREOut135.Text = result.SRE(135).ToString();
                    LREOut135.Text = result.LRE(135).ToString();
                    RLUOut135.Text = result.RLU(135).ToString();
                    GLUOut135.Text = result.GLU(135).ToString();
                    RPCOut135.Text = result.RPC(135).ToString();

                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            JSONManager jm = new JSONManager();
            jm.Write(features.ToArray());
            panel1.Controls.Clear();
            fp.LoadReferences();
        }
    }
}
