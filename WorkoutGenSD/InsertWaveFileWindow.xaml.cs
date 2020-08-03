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
    /// Interaction logic for InsertWaveFileWindow.xaml
    /// </summary>
    public partial class InsertWaveFileWindow : Window
    {
        public bool ResultOK = false;
        
        public InsertWaveFileWindow()
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            ResultOK = true;
            Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            ResultOK = false;
            Close();
        }
    }
}
