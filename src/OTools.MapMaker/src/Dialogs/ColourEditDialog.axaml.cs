using Avalonia.Controls;
using Avalonia.Media;
using OTools.Maps;

namespace OTools.MapMaker
{
    public partial class ColourEditDialog : Window
    {
        private RadioButton _active;

        public ColourEditDialog()
        {
            InitializeComponent();

            EventInit();

            SetRadios(RadioButton.RGB);
        }

        private void EventInit()
        {
            
            radRGB.Checked += (_, _) => SetRadios(RadioButton.RGB);
            radCMYK.Checked += (_, _) => SetRadios(RadioButton.CMYK);
            radSpot.Checked += (_, _) => SetRadios(RadioButton.Spot);

            nud1.ValueChanged += (_, _) => SetSample();
            nud2.ValueChanged += (_, _) => SetSample();
            nud3.ValueChanged += (_, _) => SetSample();
            nud4.ValueChanged += (_, _) => SetSample();
        }

        private void SetRadios(RadioButton rB)
        {
            _active = rB;

            switch (rB)
            {
                case RadioButton.RGB:
                    radRGB.IsChecked = true;

                    radCMYK.IsChecked = false;
                    radSpot.IsChecked = false;
                    ShowRGB();
                    break;
                case RadioButton.CMYK:
                    radCMYK.IsChecked = true;

                    radRGB.IsChecked = false;
                    radSpot.IsChecked = false;
                    ShowCMYK();
                    break;
                case RadioButton.Spot:
                    radSpot.IsChecked = true;

                    radRGB.IsChecked = false;
                    radCMYK.IsChecked = false;
                    ShowSpot();
                    break;
            }
        }

        private void ShowRGB()
        {
            txt1.Text = "Red";
            txt2.Text = "Green";
            txt3.Text = "Blue";

            nud1.Value = 0;
            nud2.Value = 0;
            nud3.Value = 0;

            txt1.IsVisible = true;
            txt2.IsVisible = true;
            txt3.IsVisible = true;
            nud1.IsVisible = true;
            nud2.IsVisible = true;
            nud3.IsVisible = true;

            txt4.IsVisible = false;
            nud4.IsVisible = false;

            nud1.Maximum = 255;
            nud2.Maximum = 255;
            nud3.Maximum = 255;

            nud1.FormatString = "{0}";
            nud2.FormatString = "{0}";
            nud3.FormatString = "{0}";

        }
        private void ShowCMYK()
        {
            txt1.Text = "Cyan";
            txt2.Text = "Magenta";
            txt3.Text = "Yellow";
            txt4.Text = "Black";

            nud1.Value = 0;
            nud2.Value = 0;
            nud3.Value = 0;
            nud4.Value = 0;

            txt1.IsVisible = true;
            txt2.IsVisible = true;
            txt3.IsVisible = true;
            txt4.IsVisible = true;
            nud1.IsVisible = true;
            nud2.IsVisible = true;
            nud3.IsVisible = true;
            nud4.IsVisible = true;

            nud1.Maximum = 100;
            nud2.Maximum = 100;
            nud3.Maximum = 100;

            nud1.FormatString = "{0}%";
            nud2.FormatString = "{0}%";
            nud3.FormatString = "{0}%";
            nud4.FormatString = "{0}%";
        }
        private void ShowSpot()
        {
            txt1.IsVisible = false;
            txt2.IsVisible = false;
            txt3.IsVisible = false;
            txt4.IsVisible = false;
            nud1.IsVisible = false;
            nud2.IsVisible = false;
            nud3.IsVisible = false;
            nud4.IsVisible = false;

        }

        private Grid CreateSpotColourGrid()
        {
            Grid g = new()
            {
                ColumnDefinitions = new ColumnDefinitions("*,*"),
                Height = 30,
            };

            ComboBox cb = new();
            cb.SetValue(Grid.ColumnProperty, 0);

            //var spotCols = Manager.Map?.SpotColours.Select(x => x.Name).ToList()
            //    ?? new List<string>();

            //cb.ItemsSource = spotCols;

            NumericUpDown nud = new()
            {
                Margin = new(10, 0, 10, 0),

                Maximum = 100,
                Minimum = 0,
                Value = 0,
                Increment = 1,

                HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,

                FormatString = "{0}%",
            };
            nud.SetValue(Grid.ColumnProperty, 1);

            g.Children.Add(cb);
            g.Children.Add(nud);

            return g;
        }

        private void SetSample()
        {
            var (a, r, g, b) = CreateColour().ToARGB();

            Color col = new(a, r, g, b);
            rctCol.Fill = new SolidColorBrush(col);
        }

        private Colour CreateColour()
        {
            switch (_active)
            {
                case RadioButton.RGB:
                    return new RgbColour(txtName.Text ?? "", 
                        (byte)(nud1.Value ?? 0), 
                        (byte)(nud2.Value ?? 0), 
                        (byte)(nud3.Value ?? 0));
                case RadioButton.CMYK:
                    return new CmykColour(txtName.Text ?? "", 
                        (float)(nud1.Value ?? 0) / 100, 
                        (float)(nud2.Value ?? 0) / 100, 
                        (float)(nud3.Value ?? 0) / 100, 
                        (float)(nud4.Value ?? 0) / 100);
            }

            return Colour.Transparent;
        }

        enum RadioButton { RGB, CMYK, Spot }
    }
}
