using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using AForge.Imaging.Filters;



namespace ImageProcessor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        //-------------------------------------------------------------------------------------
        bool selectRegionButton = false;
        bool mouseDownIndicator = false;
        bool brightnessIndicator = false;
        float contrast = 0;
        Bitmap picture;
        bool toDistanceDraw = false; // включение возможности вычисления расстояния в пикселях
        Point[] pointsForDistance = new Point[2]; // массив для хранения координат при вычислении расстояния в пикселях
        int index = 0;
        //-------------------------------------------------------------------------------------
        private List<Bitmap> BmList;

        void SaveAction(Bitmap picture)
        {
            BmList.Add(picture);
            index++;
        }
        void Undo()
        {
            try
            {
                if (index >= 0)
                {
                    Bitmap bmp = BmList[--index];
                    pictureBox1.Image = bmp;
                    pictureBox1.Refresh();
                }
            }
            catch { }
        }


        double GetBrightness(Bitmap pic, int x, int y)
        {
            int R = pic.GetPixel(x, y).R;
            int G = pic.GetPixel(x, y).G;
            int B = pic.GetPixel(x, y).B;
            double Y = 0.3 * R + 0.59 * G + 0.11 * B;
            return Y;
        }

        
        void LinearFilter()
        {
            Bitmap picture = (Bitmap)pictureBox1.Image;
            int ProgressBarMax = pictureBox1.Image.Height * pictureBox1.Image.Width;
            progressBar1.Maximum = ProgressBarMax;
            int RSum = 0;
            int GSum = 0;
            int BSum = 0;
            for (int i = 0; i < picture.Width; i++)
            {
                for (int j = 0; j < picture.Height; j++)
                {
                        for (int k = i - 1; k < (i+2); k++) // Вычисляем сумму по каналам для области 3 на 3 пикселя
                        {
                            for (int l = j-1; l < (j + 2); l++)
                            {
                                if ((k < picture.Width) && (l < picture.Height) && (k>=0) && (l>=0))
                                {
                                    int R = picture.GetPixel(k, l).R;
                                    int G = picture.GetPixel(k, l).G;
                                    int B = picture.GetPixel(k, l).B;
                                    RSum += R;
                                    GSum += G;
                                    BSum += B;
                                }
                                
                            }
                        }
                    
                    RSum /= 9;
                    GSum /= 9;
                    BSum /= 9;
                    RSum = (RSum < 0) ? 0 : RSum;
                    GSum = (GSum < 0) ? 0 : GSum;
                    BSum = (BSum < 0) ? 0 : BSum;
                    RSum = (RSum > 255) ? 255 : RSum;
                    GSum = (GSum > 255) ? 255 : GSum;
                    BSum = (BSum > 255) ? 255 : BSum;
                    Color color = Color.FromArgb(RSum, GSum, BSum);
                    if (((i + 1) < picture.Width) && ((j + 1) < picture.Height))
                    {
                        picture.SetPixel(i, j, color);
                    }
                    
                    RSum = 0;
                    GSum = 0;
                    BSum = 0;
                    progressBar1.Value += 1;
                }
            }
            SaveAction(picture);
            progressBar1.Value = 0;

        }

        void ChartsForProfile(Point []points)
        {
            List<Point> list = Line(points[0].X, points[0].Y, points[1].X, points[1].Y);
            int[] R = new int[256];
            int[] G = new int[256];
            int[] B = new int[256];
            int[] Y = new int[256];
            RedChart.Series[0].Points.Clear();
            GreenChart.Series[0].Points.Clear();
            BlueChart.Series[0].Points.Clear();
            BrightnessChart.Series[0].Points.Clear();
            Color color;
            Bitmap bmp = (Bitmap) pictureBox1.Image;
            for (int i = 0; i < list.Count; i++)
            {
                color = bmp.GetPixel(list[i].X,list[i].Y);
                ++R[color.R];
                ++G[color.G];
                ++B[color.B];
                ++Y[(int)(0.3 * color.R + 0.59 * color.G + 0.11 * color.B)];
            }
            for (int i = 0; i < 256; i++)
            {
                RedChart.Series[0].Points.AddY(R[i]);
                GreenChart.Series[0].Points.AddY(G[i]);
                BlueChart.Series[0].Points.AddY(B[i]);
                BrightnessChart.Series[0].Points.AddY(Y[i]);
            }
            
        }

        List<Point> Line(int x1, int y1, int x2, int y2)
        {
            List<Point> NewList = new List<Point>();

            int dx = (x2 - x1 >= 0 ? 1 : -1);
            int dy = (y2 - y1 >= 0 ? 1 : -1);

            int lengthX = Math.Abs(x2 - x1);
            int lengthY = Math.Abs(y2 - y1);

            int length = Math.Max(lengthX, lengthY);

            if (lengthY <= lengthX)
            {
                int x = x1;
                int y = y1;
                int d = -lengthX;
                for (int i = 0; i < length; i++)
                {
                    NewList.Add(new Point(x, y));
                    x += dx;
                    d += 2 * lengthY;
                    if (d > 0)
                    {
                        d -= 2 * lengthX;
                        y += dy;
                    }
                }
            }
            else
            {
                int x = x1;
                int y = y1;
                int d = -lengthY;
                for (int i = 0; i < length; i++)
                {
                    NewList.Add(new Point(x, y));
                    y += dy;
                    d += 2 * lengthX;
                    if (d > 0)
                    {
                        d -= 2 * lengthY;
                        x += dx;
                    }
                }

            }
            return NewList;
        }
        
        void Charts()
        {
            RedChart.Series[0].Points.Clear();
            GreenChart.Series[0].Points.Clear();
            BlueChart.Series[0].Points.Clear();
            BrightnessChart.Series[0].Points.Clear();
            RedChart.Series[0].Color = Color.Red;
            GreenChart.Series[0].Color = Color.Green;
            BlueChart.Series[0].Color = Color.Blue;
            BrightnessChart.Series[0].Color = Color.Black;
            Bitmap picture = (Bitmap)pictureBox1.Image;
            int ProgressBarMax = pictureBox1.Image.Height * pictureBox1.Image.Width;
            progressBar1.Maximum = ProgressBarMax;
            int[] R = new int[256];
            int[] G = new int[256];
            int[] B = new int[256];
            int[] Y = new int[256];
            Color color;
            for (int i = 0; i < pictureBox1.Image.Width; i++)
            {
                for (int j = 0; j < pictureBox1.Image.Height; j++)
                {
                    color = picture.GetPixel(i,j);
                    ++R[color.R];
                    ++G[color.G];
                    ++B[color.B];
                    ++Y[(int)(0.3 * color.R + 0.59 * color.G + 0.11 * color.B)];         
                    progressBar1.Value += 1;
                }

            }
            for (int i = 0; i < 256; i++)
            {
                RedChart.Series[0].Points.AddY(R[i]);
                GreenChart.Series[0].Points.AddY(G[i]);
                BlueChart.Series[0].Points.AddY(B[i]);
                BrightnessChart.Series[0].Points.AddY(Y[i]);
            }
            progressBar1.Value = 0;
                
        }
        void SinNoise(int period, double amplitude, string type)
        {
            int ProgressBarMax = pictureBox1.Image.Height * pictureBox1.Image.Width;
            progressBar1.Maximum = ProgressBarMax;
            Random rand = new Random();
            Bitmap picture = (Bitmap)pictureBox1.Image;
            Color color;
            int counter = 0;
            int picSize = 0;
                for (int i = 0; i < picture.Width; i++)
                {
                    for (int j = 0; j < picture.Height; j++)
                    {
                        if (type == "Вертикальная")
                        {
                            counter = i;
                            picSize = picture.Width;
                        }
                        if (type == "Горизонтальная")
                        {
                            counter = j;
                            picSize = picture.Height;
                        }
                        if (type == "Диагональная(ПН-ЛВ)")
                        {
                            counter = i + j;
                            picSize = picture.Width + picture.Height;
                        }
                        if (type == "Диагональная(ПВ-ЛН)")
                        {
                            counter = i - j;
                            picSize = picture.Width + picture.Height;
                        }
                        
                        int R = picture.GetPixel(i, j).R;
                        int G = picture.GetPixel(i, j).G;
                        int B = picture.GetPixel(i, j).B;
                        R = (int)(R + 255 * amplitude * Math.Sin((2 * Math.PI * counter) / (picSize / period)));
                        G = (int)(G + 255 * amplitude * Math.Sin((2 * Math.PI * counter) / (picSize / period)));
                        B = (int)(B + 255 * amplitude * Math.Sin((2 * Math.PI * counter) / (picSize / period)));
                        R = (R < 0) ? 0 : R;
                        G = (G < 0) ? 0 : G;
                        B = (B < 0) ? 0 : B;
                        R = (R > 255) ? 255 : R;
                        G = (G > 255) ? 255 : G;
                        B = (B > 255) ? 255 : B;
                        color = Color.FromArgb(R, G, B);
                        picture.SetPixel(i, j, color);
                        progressBar1.Value += 1;
                    }
                }
            SaveAction(picture);
            progressBar1.Value = 0;

        }

        void SaltAndPepper(double probability)
        {
            int ProgressBarMax = pictureBox1.Image.Height * pictureBox1.Image.Width;
            progressBar1.Maximum = ProgressBarMax;
            Random rand = new Random();
            Bitmap picture = (Bitmap)pictureBox1.Image;
            Color[] color = { Color.Black, Color.White };
            for (int i = 0; i < picture.Width; i++)
            {
                for (int j = 0; j < picture.Height; j++)
                {
                    if (rand.NextDouble() < probability)
                    {
                        picture.SetPixel(i, j, color[rand.Next(2)]);
                        progressBar1.Value += 1;
                    }
                }
            }
            SaveAction(picture);
            progressBar1.Value = 0;

        }

        double pixelDistance(Point[] pnt) // вычисление длины в пикселях
        {
            double distance = Math.Sqrt(Math.Pow(pnt[1].X - pnt[0].X,2) + Math.Pow(pnt[1].Y - pnt[0].Y,2));      
            return Math.Round(distance, 4);
        }

        void CopyImageFragment(Point[] point)
        {
            Bitmap picture = (Bitmap)pictureBox1.Image;
            Bitmap newPicture = new Bitmap(pictureBox1.Image.Width, pictureBox1.Image.Height);
            if (point[0].X > point[1].X && point[0].Y > point[1].Y)
            {
                for (int i = point[1].X; i < point[0].X; i++)
                {
                    for (int j = point[1].Y; j < point[0].Y; j++)
                    {
                        newPicture.SetPixel(i, j, picture.GetPixel(i, j));
                    }
                }
                if (FragmentPanel.Visible == true)
                {
                    label19.Text = "X: " + point[1].X + " Y: " + point[1].Y; 
                    label20.Text = "X: " + point[0].X + " Y: " + point[0].Y;
                }
            }
            else if (point[0].X < point[1].X && point[0].Y > point[1].Y)
            {
               for (int i = point[0].X; i < point[1].X; i++)
                {
                    for (int j = point[1].Y; j < point[0].Y; j++)
                    {
                        newPicture.SetPixel(i, j, picture.GetPixel(i, j));
                    }
                }
               if (FragmentPanel.Visible == true)
               {
                   label19.Text = "X: " + point[0].X + " Y: " + point[1].Y;
                   label20.Text = "X: " + point[1].X + " Y: " + point[0].Y;
               }
            }
            else if (point[0].X > point[1].X && point[0].Y < point[1].Y)
            {
                for (int i = point[1].X; i < point[0].X; i++)
                {
                    for (int j = point[0].Y; j < point[1].Y; j++)
                    {
                        newPicture.SetPixel(i, j, picture.GetPixel(i, j));
                    }
                }
                if (FragmentPanel.Visible == true)
                {
                    label19.Text = "X: " + point[1].X + " Y: " + point[0].Y;
                    label20.Text = "X: " + point[0].X + " Y: " + point[1].Y;
                }
            }
            else
            {
                for (int i = point[0].X; i < point[1].X; i++)
                {
                    for (int j = point[0].Y; j < point[1].Y; j++)
                    {
                        newPicture.SetPixel(i, j, picture.GetPixel(i, j));
                    }
                }
                if (FragmentPanel.Visible == true)
                {
                    label19.Text = "X: " + point[0].X + " Y: " + point[0].Y;
                    label20.Text = "X: " + point[1].X + " Y: " + point[1].Y;
                }
            }
            SaveAction(picture);
            Clipboard.SetImage(newPicture);
        }
        void binaryColorChange()
        {
            Bitmap picture = (Bitmap)pictureBox1.Image;
            int ProgressBarMax = pictureBox1.Image.Height * pictureBox1.Image.Width;
            progressBar1.Maximum = ProgressBarMax;

            for (int i = 0; i < pictureBox1.Image.Width; i++)
            {
                for (int j = 0; j < pictureBox1.Image.Height; j++)
                {
                    int R = picture.GetPixel(i, j).R;
                    int G = picture.GetPixel(i, j).G;
                    int B = picture.GetPixel(i, j).B;
                    double Y = 0.3 * R + 0.59 * G + 0.11 * B;

                    if (trackBar1.Value < Y)
                    {
                        Color set = colorButton1.BackColor;
                        picture.SetPixel(i, j, set);
                    }
                    else
                    {
                        Color set = colorButton2.BackColor;
                        picture.SetPixel(i, j, set);
                    }
                    progressBar1.Value += 1;
                    
                }
            }
            
            pictureBox1.Refresh();
            SaveAction(picture);
            progressBar1.Value = 0;

        }

        public static Bitmap AdjustBrightness(Bitmap Image, int Value)
        {
            Bitmap TempBitmap = Image;
            float FinalValue = (float)Value / 255.0f;
            Bitmap NewBitmap = new Bitmap(TempBitmap.Width, TempBitmap.Height);
            Graphics NewGraphics = Graphics.FromImage(NewBitmap);
            float[][] FloatColorMatrix = {
                        new float[] {1, 0, 0, 0, 0},
                        new float[] {0, 1, 0, 0, 0},
                        new float[] {0, 0, 1, 0, 0},
                        new float[] {0, 0, 0, 1, 0},
                        new float[] {FinalValue, FinalValue, FinalValue, 1, 1}
                    };

            ColorMatrix NewColorMatrix = new ColorMatrix(FloatColorMatrix);
            ImageAttributes Attributes = new ImageAttributes();
            Attributes.SetColorMatrix(NewColorMatrix);
            NewGraphics.DrawImage(TempBitmap, new Rectangle(0, 0, TempBitmap.Width, TempBitmap.Height), 0, 0, TempBitmap.Width, TempBitmap.Height, GraphicsUnit.Pixel, Attributes);
            Attributes.Dispose();
            NewGraphics.Dispose();
            return NewBitmap;
        }
        //------------------------------------------------------------------------------------


        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Открыть изображение(*.jpg)|*.jpg| Открыть изображение(*.bmp)|*.bmp";
            openFileDialog1.ShowDialog();
            try
            {
                pictureBox1.Image = System.Drawing.Image.FromFile(openFileDialog1.FileName);
                picture = new Bitmap(openFileDialog1.FileName);
                BmList = new List<Bitmap>();
                BmList.Add(new Bitmap(picture));
            }
            catch
            {

            }
            
        }



        private void скопироватьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetImage(pictureBox1.Image);
        }

        private void вставитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = Clipboard.GetImage();
        }

        private void скопироватьToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Clipboard.SetImage(pictureBox2.Image);
        }

        private void вставитьToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = Clipboard.GetImage();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetImage(pictureBox1.Image);
            }
            catch { }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            pictureBox2.Image = Clipboard.GetImage();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            try
            {
                string extension = Path.GetExtension(openFileDialog1.SafeFileName);
                extensionLabel.Text = extension;
                widthLabel.Text = Convert.ToString(pictureBox1.Image.Width);
                heightLabel.Text = Convert.ToString(pictureBox1.Image.Height);

                if (informationPanel.Visible == true)
                {

                    informationPanel.Visible = false;
                }
                else
                {
                    informationPanel.Visible = true;
                }
            }
            catch {
                MessageBox.Show("Загрузите изображение", "Image not found");
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (informationPanel.Visible == true)
            {
                Bitmap picture = (Bitmap)pictureBox1.Image;

                displayCoorLabelX.Text = Convert.ToString(e.X);
                displayCoorLabelY.Text = Convert.ToString(e.Y);
                int R = picture.GetPixel(e.X, e.Y).R;
                int G = picture.GetPixel(e.X, e.Y).G;
                int B = picture.GetPixel(e.X, e.Y).B;
                RColorLabel.Text = Convert.ToString(R);
                GColorLabel.Text = Convert.ToString(G);
                BColorLabel.Text = Convert.ToString(B);
                double Y = 0.3 * R + 0.59 * G + 0.11 * B;
                BrightnessLabel.Text = Convert.ToString(Math.Round(Y));

                if (panel3.Visible == true)
                {
                    if (mouseDownIndicator)
                    {
                        Pen pen = new Pen(Color.Black);
                        pictureBox1.Refresh();
                        pictureBox1.CreateGraphics().DrawLine(pen, pointsForDistance[0].X, pointsForDistance[0].Y, e.X, e.Y);
                    }
                }
            }
            if (selectRegionButton == true)
            {
                if (mouseDownIndicator)
                {
                    Pen pen = new Pen(Color.Black);
                    pictureBox1.Refresh();
                    if (pointsForDistance[0].X > e.X && pointsForDistance[0].Y > e.Y)
                    {
                        pictureBox1.CreateGraphics().DrawRectangle(pen, e.X, e.Y, pointsForDistance[0].X - e.X, pointsForDistance[0].Y - e.Y);
                    }
                    else if (pointsForDistance[0].X < e.X && pointsForDistance[0].Y > e.Y) 
                    {
                        pictureBox1.CreateGraphics().DrawRectangle(pen, pointsForDistance[0].X, e.Y, Math.Abs(e.X - pointsForDistance[0].X), Math.Abs(e.Y - pointsForDistance[0].Y));
                    }
                    else if (pointsForDistance[0].X > e.X && pointsForDistance[0].Y < e.Y) //рисуется правильно вроде
                    {
                        pictureBox1.CreateGraphics().DrawRectangle(pen, e.X, pointsForDistance[0].Y, Math.Abs(e.X - pointsForDistance[0].X), Math.Abs(e.Y - pointsForDistance[0].Y));
                    }
                    else
                    {
                        pictureBox1.CreateGraphics().DrawRectangle(pen, pointsForDistance[0].X, pointsForDistance[0].Y, Math.Abs(e.X - pointsForDistance[0].X), Math.Abs(e.Y - pointsForDistance[0].Y));
                    }
                    label21.Text = Convert.ToString(Math.Abs(pointsForDistance[0].Y - e.Y)); //вычисление ширины выделенного фрагмента
                    label22.Text = Convert.ToString(Math.Abs(pointsForDistance[0].X - e.X)); //вычисение высоты выделенного фрагмента
                }
            }
            if (brightnessIndicator)
            {
                if (mouseDownIndicator)
                {
                    Pen pen = new Pen(Color.Black);
                    pictureBox1.Refresh();
                    pictureBox1.CreateGraphics().DrawLine(pen, pointsForDistance[0].X, pointsForDistance[0].Y, e.X, e.Y);
                }
            }
        }

        private void distanceButton_Click(object sender, EventArgs e)
        {
            if (toDistanceDraw)
            {
                toDistanceDraw = false;
                pictureBox1.Refresh();
            }
            else
                toDistanceDraw = true;

        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (toDistanceDraw)
            {
                mouseDownIndicator = true;
                pointsForDistance[0].X = e.X;
                pointsForDistance[0].Y = e.Y;
            }
            if (selectRegionButton)
            {
                mouseDownIndicator = true;
                pointsForDistance[0].X = e.X;
                pointsForDistance[0].Y = e.Y;
            }
            if (brightnessIndicator)
            {
                mouseDownIndicator = true;
                pointsForDistance[0].X = e.X;
                pointsForDistance[0].Y = e.Y;
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (toDistanceDraw)
            {
                mouseDownIndicator = false;
                pointsForDistance[1].X = e.X;
                pointsForDistance[1].Y = e.Y;

                distanceLabel.Text = Convert.ToString(pixelDistance(pointsForDistance));
            }
            if (selectRegionButton)
            {
                mouseDownIndicator = false;
                pointsForDistance[1].X = e.X;
                pointsForDistance[1].Y = e.Y;
                CopyImageFragment(pointsForDistance);                
            }
            if (brightnessIndicator)
            {
                mouseDownIndicator = false;
                pointsForDistance[1].X = e.X;
                pointsForDistance[1].Y = e.Y;
                ChartsForProfile(pointsForDistance);
            }
        }

        private void selectRegion_Click(object sender, EventArgs e)
        {
            if (selectRegionButton)
            {
                selectRegionButton = false;
                FragmentPanel.Visible = false;
                pictureBox1.Refresh();
            }
            else
            {
                selectRegionButton = true;
                FragmentPanel.BorderStyle = BorderStyle.FixedSingle;
                FragmentPanel.Visible = true;
            }

        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
           
        }

        private void изменитьЦветToolStripMenuItem_Click(object sender, EventArgs e)
        {
            binaryColorChange();
            Charts();
        }

        private void distanceTollButton_Click(object sender, EventArgs e)
        {
            if (panel3.Visible)
            {
                panel3.Visible = false;
            }
            else
            {
                panel3.BorderStyle = BorderStyle.FixedSingle;
                panel3.Visible = true;
            }
        }

        private void colorButton1_Click(object sender, EventArgs e)
        {
            ColorDialog NewColorDialog = new ColorDialog();
            if (NewColorDialog.ShowDialog() == DialogResult.OK)
            {
                colorButton1.BackColor = NewColorDialog.Color;
            }
        }

        private void colorButton2_Click(object sender, EventArgs e)
        {
            ColorDialog NewColorDialog = new ColorDialog();
            if (NewColorDialog.ShowDialog() == DialogResult.OK)
            {
                colorButton2.BackColor = NewColorDialog.Color;
            }
        }

        private void ApplyChangeColorButton_Click(object sender, EventArgs e)
        {
            binaryColorChange();
            Charts();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void ChangeColorMenuButton_Click(object sender, EventArgs e)
        {
            if (ChangeColorPanel.Visible == true)
            {
                ChangeColorPanel.Visible = false;
            }
            else
            {
                ChangeColorPanel.BorderStyle = BorderStyle.FixedSingle;
                ChangeColorPanel.Visible = true;
            }
        }

        private void GrayShadesButton_Click(object sender, EventArgs e)
        {
            Bitmap picture = (Bitmap)pictureBox1.Image;
            int ProgressBarMax = pictureBox1.Image.Height * pictureBox1.Image.Width;
            progressBar1.Maximum = ProgressBarMax;

            for (int i = 0; i < pictureBox1.Image.Width; i++)
            {
                for (int j = 0; j < pictureBox1.Image.Height; j++)
                {
                    int R = picture.GetPixel(i, j).R;
                    int G = picture.GetPixel(i, j).G;
                    int B = picture.GetPixel(i, j).B;
                    double Y = 0.3 * R + 0.59 * G + 0.11 * B;

                    Color set = Color.FromArgb((int)Y, (int)Y, (int)Y);
                    picture.SetPixel(i, j, set);
                    
                    progressBar1.Value += 1;

                }
            }

            pictureBox1.Refresh();
            SaveAction(picture);
            progressBar1.Value = 0;
            Charts();

        }

        private void NegativeColorButton_Click(object sender, EventArgs e)
        {
            Bitmap picture = (Bitmap)pictureBox1.Image;
            int ProgressBarMax = pictureBox1.Image.Height * pictureBox1.Image.Width;
            progressBar1.Maximum = ProgressBarMax;

            for (int i = 0; i < pictureBox1.Image.Width; i++)
            {
                for (int j = 0; j < pictureBox1.Image.Height; j++)
                {
                    int R = picture.GetPixel(i, j).R;
                    int G = picture.GetPixel(i, j).G;
                    int B = picture.GetPixel(i, j).B;
                    Color set = Color.FromArgb(255-R,255-G,255-B);
                    picture.SetPixel(i, j, set);

                    progressBar1.Value += 1;

                }
            }

            pictureBox1.Refresh();
            SaveAction(picture);
            progressBar1.Value = 0;
            Charts();
        }

        private void ContrastTrackBar_Scroll(object sender, EventArgs e)
        {
            contrast = 0.04f * ContrastTrackBar.Value;

            
            Bitmap bmp = new Bitmap(picture.Width, picture.Height);

            Graphics g = Graphics.FromImage(bmp);
            ImageAttributes imageAttr = new ImageAttributes();

            ColorMatrix cm = new ColorMatrix(new float[][] {
                                  new float[]{contrast,0f,0f,0f,0f},
                                  new float[]{0f,contrast,0f,0f,0f},
                                  new float[]{0f,0f,contrast,0f,0f},
                                  new float[]{0f,0f,0f,1f,0f},

                                  new float[]{0.001f,0.001f,0.001f,0f,1f}});

            imageAttr.SetColorMatrix(cm);

            g.DrawImage(picture, new Rectangle(0, 0, picture.Width, picture.Height), 0, 0, picture.Width, picture.Height, GraphicsUnit.Pixel, imageAttr);
            g.Dispose();
            imageAttr.Dispose();
            pictureBox1.Image = bmp;
            SaveAction(bmp);
        }

        private void ChartButton_Click(object sender, EventArgs e)
        {
            if (ChartPanel.Visible)
            {
                ChartPanel.Visible = false;
            }
            else
            {
                ChartPanel.Visible = true;
                ChartPanel.BorderStyle = BorderStyle.FixedSingle;
                Charts();
            }
        }

        private void BritnessButton_Click(object sender, EventArgs e)
        {
            if (BritnessPanel.Visible)
            {
                BritnessPanel.Visible = false;
            }
            else
            {
                BritnessPanel.BorderStyle = BorderStyle.FixedSingle;
                BritnessPanel.Visible = true;
            }
        }

        private void BrightnessTrackBar_Scroll(object sender, EventArgs e)
        {
            pictureBox1.Image = AdjustBrightness(picture, BrightnessTrackBar.Value);
            SaveAction((Bitmap)pictureBox1.Image);
        }

        private void SaltPepperButton_Click(object sender, EventArgs e)
        {
            try
            {
                double probability = Convert.ToDouble(comboBox1.SelectedItem);
                SaltAndPepper(probability);
                pictureBox1.Refresh();
                Charts();
            }
            catch (FormatException error)
            {
                MessageBox.Show(error.Message, "Ошибка");
            }
            catch
            {
                MessageBox.Show("Введите значение","Ошибка");
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (NoisePanel.Visible)
            {
                NoisePanel.Visible = false;
            }
            else
            {
                NoisePanel.BorderStyle = BorderStyle.FixedSingle;
                NoisePanel.Visible = true;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void MedianFilterButton_Click(object sender, EventArgs e)
        {
            Bitmap bmp = (Bitmap)pictureBox1.Image;
            Median filter = new Median();
            filter.ApplyInPlace(bmp);
            SaveAction(bmp);
            pictureBox1.Refresh();
            Charts();
        }

        private void SinNoiseButton_Click(object sender, EventArgs e)
        {
            int period = Convert.ToInt32(comboBox2.SelectedItem);
            double amplitude = Convert.ToDouble(comboBox3.SelectedItem);
            string type = comboBox4.SelectedItem.ToString();
            SinNoise(period,amplitude,type);
            pictureBox1.Refresh();
            SaveAction(picture);
            Charts();
        }

        private void LinearFilterButton_Click(object sender, EventArgs e)
        {
            LinearFilter();
            pictureBox1.Refresh();
            Charts();
        }

        private void BrightnessProfieButton_Click(object sender, EventArgs e)
        {
            if (brightnessIndicator == true)
            {
                brightnessIndicator = false;
                pictureBox1.Refresh();
            }
            else
            {
                brightnessIndicator = true;
            }

        }

        private void методКиршаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            picture = (Bitmap)pictureBox1.Image;
            picture = picture.KirschFilter(true);
            pictureBox1.Image = picture;
            SaveAction(picture);
            pictureBox1.Refresh();
            Charts();
        }

        private void методЛапласаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            picture = (Bitmap)pictureBox1.Image;
            picture = picture.Laplacian3x3Filter(true);
            pictureBox1.Image = picture;
            SaveAction(picture);
            pictureBox1.Refresh();
            Charts();
        }

        private void методРобертсаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            picture = (Bitmap)pictureBox1.Image;
            picture = picture.Laplacian3x3OfGaussian3x3Filter();
            pictureBox1.Image = picture;
            SaveAction(picture);
            pictureBox1.Refresh();
            Charts();
            
        }

        private void методСобелаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            picture = (Bitmap)pictureBox1.Image;
            picture = picture.Sobel3x3Filter();
            pictureBox1.Image = picture;
            SaveAction(picture);
            pictureBox1.Refresh();
            Charts();
        }

        private void методУоллесаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            picture = (Bitmap)pictureBox1.Image;
            picture = picture.LaplacianOfGaussianFilter();
            pictureBox1.Image = picture;
            SaveAction(picture);
            pictureBox1.Refresh();
            Charts();
        }

        private void статистическийМетодToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //StatisticalMethod();
            //pictureBox1.Refresh();
            //Charts();
            picture = (Bitmap)pictureBox1.Image;
            picture = picture.StaticMethodFilter();
            pictureBox1.Image = picture;
            SaveAction(picture);
            pictureBox1.Refresh();
            Charts();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Z && e.Control)
            {
                Undo();
            }
        }

        private void отменаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Undo();
        }

    }
}
