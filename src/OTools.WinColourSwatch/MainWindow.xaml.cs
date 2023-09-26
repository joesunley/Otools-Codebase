using OTools.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OTools.WinColourSwatch
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

            sliderC.ValueChanged += (_, _) => UpdateColours();
            sliderM.ValueChanged += (_, _) => UpdateColours();
            sliderY.ValueChanged += (_, _) => UpdateColours();
            sliderK.ValueChanged += (_, _) => UpdateColours();

            txtC.TextChanged += (_, _) => UpdateSlider();
            txtM.TextChanged += (_, _) => UpdateSlider();
            txtY.TextChanged += (_, _) => UpdateSlider();
            txtK.TextChanged += (_, _) => UpdateSlider();

            _colours = new();

            btnNew.Click += (_, _) => AddNew();

            UpdateColours();
        }

        void UpdateSlider()
        {
            try
            {
                sliderC.Value = txtC.Text.Parse<double>();
                sliderM.Value = txtM.Text.Parse<double>();
                sliderY.Value = txtY.Text.Parse<double>();
                sliderK.Value = txtK.Text.Parse<double>();
            } catch { }
            
        }

        void UpdateColours()
        {
            txtC.Text = sliderC.Value.ToString("F0");
            txtM.Text = sliderM.Value.ToString("F0");
            txtY.Text = sliderY.Value.ToString("F0");
            txtK.Text = sliderK.Value.ToString("F0");

            float
                c = (float)sliderC.Value / 100f,
                m = (float)sliderM.Value / 100f,
                y = (float)sliderY.Value / 100f,
                k = (float)sliderK.Value / 100f;

            float[] colValues = { c, m, y, k };
            Color col = Color.FromValues(colValues, new(profileFileName));

            byte r = (byte)(255 * (1 - c) * (1 - k)),
                 g = (byte)(255 * (1 - m) * (1 - k)),
                 b = (byte)(255 * (1 - y) * (1 - k));

            blConv.Background = new SolidColorBrush(Color.FromRgb(r, g, b));
            blCorr.Background = new SolidColorBrush(col);

            txtCMYK.Text = $"{c*100:0f}, {m*100:0f}, {y*100:0f}, {k*100:0f}";
            txtRGB.Text = $"{col.R}, {col.G}, {col.B}";
        }

        List<Colour> _colours;

        void AddNew()
        {
            float
                c = (float)sliderC.Value / 100f,
                m = (float)sliderM.Value / 100f,
                y = (float)sliderY.Value / 100f,
                k = (float)sliderK.Value / 100f;


            CmykColour col = new(txtColour.Text, c, m, y, k);

            _colours.Add(col);

            Button b = new()
            {
                Content = col.Name,
                Foreground = new SolidColorBrush(Colors.White),
                Background = new SolidColorBrush(Color.FromValues(new[] { c, m, y, k }, new(profileFileName))),
            };

            b.Click += (_, _) =>
            {
                sliderC.Value = col.Cyan * 100;
                sliderM.Value = col.Magenta * 100;
                sliderY.Value = col.Yellow * 100;
                sliderK.Value = col.Key * 100;
            };

            b.MouseDoubleClick += (_, _) =>
            {
                _colours.Remove(col);
                stack.Children.Remove(b);
            };

            stack.Children.Add(b);
        }
    }
}
