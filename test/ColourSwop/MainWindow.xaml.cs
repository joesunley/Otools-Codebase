using Microsoft.Win32;
using Sunley.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace ColourSwop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string profileFileName = @"C:\Program Files (x86)\Purple Pen\USWebCoatedSWOP.icc";

        public MainWindow()
        {
            InitializeComponent();

            cText.TextChanged += (_, _) => OnUpdate();
            mText.TextChanged += (_, _) => OnUpdate();
            yText.TextChanged += (_, _) => OnUpdate();
            kText.TextChanged += (_, _) => OnUpdate();

            btnBrowse.Click += (_, _) => BtnBrow();
            btnSave.Click += (_, _) => SaveToFile();
        }

        void OnUpdate()
        {
            float c = 0, m = 0, y = 0, k = 0;
            Color color;

            try
            {
                c = cText.Text.Parse<int>() / 100f;
                m = mText.Text.Parse<int>() / 100f;
                y = yText.Text.Parse<int>() / 100f;
                k = kText.Text.Parse<int>() / 100f;

                Uri profileUri = new Uri(profileFileName);

                float[] colourValues = { c, m, y, k };
                color = Color.FromValues(colourValues, profileUri);

            }
            catch
            {
                color = Color.FromRgb(255, 255, 255);
            }

            byte r = (byte)(255 * (1 - c) * (1 - k)),
                g = (byte)(255 * (1 - m) * (1 - k)),
                b = (byte)(255 * (1 - y) * (1 - k));


            rawCol.Text = $"{r}, {g}, {b}";
            profCol.Text = $"{color.R}, {color.G}, {color.B}";

            rawView.Background = new SolidColorBrush(Color.FromRgb(r, g, b));
            profView.Background = new SolidColorBrush(Color.FromRgb(color.R, color.G, color.B));
        }

        void BtnBrow()
        {
            OpenFileDialog dial = new();

            dial.Filter = "Colour Profiles (.icc)|*.icc";

            dial.ShowDialog();

            profileFileName = dial.FileName;

            OnUpdate();
        }

        void SaveToFile()
        {
            SaveFileDialog dial = new();

            dial.Filter = "Text documents (.txt)|*.txt";

            dial.ShowDialog();

            progressBar1.Maximum = 101 * 101 * 101 * 101;


            Thread th = new(() =>
            {
                DateTime start = DateTime.Now;
                var lines = GetFile();
                TimeSpan t = DateTime.Now - start;

                MessageBox.Show(t.TotalSeconds.ToString());

                File.WriteAllLines(dial.FileName, lines);

            });

            th.Start();
        }

        string[] GetFile()
        {

            List<string> lines = new();

            for (int c = 0; c <= 100; c++)
            for (int m = 0; m <= 100; m++)
            for (int y = 0; y <= 100; y++)
            for (int k = 0; k <= 100; k++)
            {
                float[] colourValues = { c / 100f, m / 100f, y / 100f, k / 100f };
                Color color = Color.FromValues(colourValues, new(profileFileName));

                string line = $"{colourValues[0]}, {colourValues[1]}, {colourValues[2]}, {colourValues[3]} : {color.R}, {color.G}, {color.B}";
                lines.Add(line);

                Application.Current.Dispatcher.Invoke(() => { progressBar1.Value++; text.Text = progressBar1.Value.ToString(); });
            }

            
            return lines.ToArray();
        }
    }
}
