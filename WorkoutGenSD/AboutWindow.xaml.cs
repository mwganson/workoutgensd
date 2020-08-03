using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WorkoutGenSD
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            this.buttonOkay.Click += new RoutedEventHandler(buttonOkay_Click);
            buttonGoToWebsite.Click += new RoutedEventHandler(buttonGoToWebsite_Click);
            buttonDonate.Click += new RoutedEventHandler(buttonDonate_Click);
        }

        void buttonDonate_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=5698213");
            Close();
        }

        void buttonGoToWebsite_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://mwganson.freeyellow.com/workoutgensd");
            Close();
        }

        void buttonOkay_Click(object sender, RoutedEventArgs e)
        {
           
            Close();
        }
    }
}
