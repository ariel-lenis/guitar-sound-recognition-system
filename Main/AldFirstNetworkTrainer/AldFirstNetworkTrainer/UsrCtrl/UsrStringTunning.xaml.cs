using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AldFirstNetworkTrainer
{
	/// <summary>
	/// Interaction logic for UsrStringTunning.xaml
	/// </summary>
	public partial class UsrStringTunning : UserControl
	{
		public UsrStringTunning()
		{
			this.InitializeComponent();
		}
        public string Information
        {
            get { return txtInformation.Text; }
            set { txtInformation.Text = value; }
        }

        public float Distance {
            set {
                if (Math.Abs(value) < 0.1f)
                {
                    txtDistance.Background = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    txtDistance.Background = new SolidColorBrush(Colors.Red);
                }
                string sign = (value >= 0) ? "+" : "";
                txtDistance.Text = sign+value.ToString("00.00") + " Trastes";
            }
        }

        public float Frequency {
            set {
                this.txtFrequency.Text = value.ToString("000.000") + "hz";
            }
        }

        public float Cents
        {
            set
            {
                this.txtCents.Text = value.ToString("000.000") + "cents";
            }
        }

        public bool Activate
        {
            set {
                Thickness thickness;
                SolidColorBrush borderbrush;
                if (value)
                {
                    borderbrush = new SolidColorBrush(Color.FromArgb(0xFF,0xE7,0xFF,0x00));
                    thickness = new Thickness(0, 5, 0, 5);
                    this.txtDistance.Visibility = System.Windows.Visibility.Visible;
                    this.txtFrequency.Visibility = System.Windows.Visibility.Visible;
                    this.txtCents.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    borderbrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x5F, 0x27, 0x00));
                    thickness = new Thickness(0, 2.5, 0, 2.5);
                    this.txtDistance.Visibility = System.Windows.Visibility.Hidden;
                    this.txtFrequency.Visibility = System.Windows.Visibility.Hidden;
                    this.txtCents.Visibility = System.Windows.Visibility.Hidden;
                }
                


                this.BorderThickness = thickness;
                this.BorderBrush = borderbrush;
            }
        }
	}
}