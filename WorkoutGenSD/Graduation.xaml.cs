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
    /// Interaction logic for Graduation.xaml
    /// </summary>
    public partial class Graduation : Window
    {
        public int numWorkouts { get; set; }
        public bool bResult = false;
        
        
        public Graduation()
        {
            InitializeComponent();
            speedGradientListBox.SelectionChanged += new SelectionChangedEventHandler(speedGradientListBox_SelectionChanged);
            speedGradientPlusListBox.SelectionChanged+=new SelectionChangedEventHandler(speedGradientListBox_SelectionChanged);
        }

        void speedGradientListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            double grad = System.Convert.ToDouble(speedGradientListBox.SelectedItem);
            double plus = System.Convert.ToDouble(speedGradientPlusListBox.SelectedItem);
            speedGradientLabel.Content = "Speed Gradient: " + (grad+plus).ToString();

        }

        private void templateListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int idx = templateListBox.SelectedIndex;
            int maxNumGrads = templateListBox.Items.Count - 1 - idx;

            numGraduatesListBox.Items.Clear();
            for (int ii = 0; ii < maxNumGrads; ii++)
            {
                numGraduatesListBox.Items.Add((ii+1).ToString());
            }
            numGraduatesListBox.SelectedIndex = numGraduatesListBox.Items.Count - 1;

        }

        private void okayButton_Click(object sender, RoutedEventArgs e)
        {
            bResult = true;
            Close();

        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
           
            Close(); //bResult is already false by default, so no need to set it thus
        }
    }
}
