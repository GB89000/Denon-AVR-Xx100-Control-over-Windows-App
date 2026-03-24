using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DenonControl;

namespace Denon
{
    /// <summary>
    /// Interaktionslogik für DenonSettings.xaml
    /// </summary>
    public partial class DenonSettings : Window
    {
        public DenonSettings()
        {
            InitializeComponent();
            IP_AddressBox.Text = Properties.Settings.Default.IP_Address;
            PortBox.Text = Convert.ToString(Properties.Settings.Default.Port);
            NameBox.Text = Properties.Settings.Default.Name;
            Number1.Text = Convert.ToString(Properties.Settings.Default.QSelect1);
            Number2.Text = Convert.ToString(Properties.Settings.Default.QSelect2);
            Number3.Text = Convert.ToString(Properties.Settings.Default.QSelect3);
            Number4.Text = Convert.ToString(Properties.Settings.Default.QSelect4);
            Name1.Text = Convert.ToString(Properties.Settings.Default.QSName1);
            Name2.Text = Convert.ToString(Properties.Settings.Default.QSName2);
            Name3.Text = Convert.ToString(Properties.Settings.Default.QSName3);
            Name4.Text = Convert.ToString(Properties.Settings.Default.QSName4);
            SetVol1.Text = Convert.ToString(Properties.Settings.Default.QSetVol1);
            SetVol2.Text = Convert.ToString(Properties.Settings.Default.QSetVol2);
            SetVol3.Text = Convert.ToString(Properties.Settings.Default.QSetVol3);
            SetVol4.Text = Convert.ToString(Properties.Settings.Default.QSetVol4);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.IP_Address = IP_AddressBox.Text;
            Properties.Settings.Default.Port = Int32.Parse(PortBox.Text);
            Properties.Settings.Default.Name = NameBox.Text;
            Properties.Settings.Default.QSelect1 = Int32.Parse(Number1.Text);
            Properties.Settings.Default.QSelect2 = Int32.Parse(Number2.Text);
            Properties.Settings.Default.QSelect3 = Int32.Parse(Number3.Text);
            Properties.Settings.Default.QSelect4 = Int32.Parse(Number4.Text);
            Properties.Settings.Default.QSName1 = Name1.Text;
            Properties.Settings.Default.QSName2 = Name2.Text;
            Properties.Settings.Default.QSName3 = Name3.Text;
            Properties.Settings.Default.QSName4 = Name4.Text;
            Properties.Settings.Default.QSetVol1 = Int32.Parse(SetVol1.Text);
            Properties.Settings.Default.QSetVol2 = Int32.Parse(SetVol2.Text);
            Properties.Settings.Default.QSetVol3 = Int32.Parse(SetVol3.Text);
            Properties.Settings.Default.QSetVol4 = Int32.Parse(SetVol4.Text);
            Properties.Settings.Default.Save();
            
            this.DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }
    }
}
