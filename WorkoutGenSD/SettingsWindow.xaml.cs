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
using System.IO;

namespace WorkoutGenSD
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public bool OK = false;
        public bool Cancel = true; //default to cancel if window simply closed
        
        public int InitialWorkoutLength; //set in main window class
        string outputDirectoryPath="";
        
        public SettingsWindow()
        {
            InitializeComponent();
            
          

        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Cancel = true;
            OK = false;
            this.Close();
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            Cancel = false;
            OK = true;
            if (!Directory.Exists(outputDirectoryPath))
            {

                if (MessageBox.Show(outputDirectoryPath + " does not exist.  Create it now?"
                    +" Select NO if you wish to create and use a different output directory later."
                    , "Create Output Directory?"
                    , MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {


                    try
                    {
                        Directory.CreateDirectory(outputDirectoryPath);



                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "CreateDirectory() Exception");


                    }
                    
                }

            }
            if (InitialWorkoutLength!=System.Convert.ToInt32(this.workoutLengthBox.Text))
            { //confirm with user that he understands all data will be lost
                MessageBoxResult result = MessageBox.Show("Workout Length has been changed.  This will require re-initializing the application, clearing all incline and speed data to 0.  Are you sure?", "Program Re-Initialization Confirmation", MessageBoxButton.YesNoCancel);
                if (result != MessageBoxResult.Yes)
                {
                    OK = false;
                    Cancel = true;
                }

            }


            
            this.Close();
        }

        private void defaultsButton_Click(object sender, RoutedEventArgs e)
        {
            this.includeShowProgressGraphics.IsChecked = true;
            this.includeShowInitialSpeed.IsChecked = true;
            this.includeShowInitialIncline.IsChecked = true;
            this.includePausePriorToStart.IsChecked = true;
            this.includeMaxRunTime.IsChecked = true;
            this.includeBlock17.IsChecked = true;
            this.includeBlock16.IsChecked = true;
            this.includeBlock15.IsChecked = true;
            this.includeBlock14.IsChecked = true;
            this.includeBlock13.IsChecked = true;
            this.includeBlock12.IsChecked = true;
            this.includeBlock11.IsChecked = true;
            this.includeBlock10.IsChecked = true;
            this.includeBlock08.IsChecked = true;
            this.includeBlock02.IsChecked = true;
            this.pausePriorToStartExp1.Text = "0xff";
            this.pausePriorToStartExp2.Text = "0xfb";
            this.pausePriorToStartExp3.Text = "0x01";
            this.showProgressGraphicsExp1.Text = "0x09";
            this.setMaxRunTimeExp1.Text = "0x00";
            this.showInitialSpeedExp1.Text = "0x00";
            
            
            this.unknownBlock02Exp1.Text = "0x03";
            this.unknownBlock08Exp1.Text = "0x00";
            this.unknownBlock08Exp2.Text = "0x00";
            this.unknownBlock10Exp1.Text = "0x01";
            this.unknownBlock11Exp1.Text = "0x01";
            this.unknownBlock12Exp1.Text = "0x02";
            this.unknownBlock13Exp1.Text = "0x01";
            this.unknownBlock14Exp1.Text = "0x01";
            this.unknownBlock15Exp1.Text = "0x14";
            this.unknownBlock15Exp2.Text = "0x00";
            this.unknownBlock16Exp1.Text = "0x00";
            this.unknownBlock17Exp1.Text = "0x00";
            this.userWeightTextBox.Text = "150";


        }

        private void outputDirectoryBox_MouseEnter(object sender, MouseEventArgs e)
        {
            this.outputDirectoryInformationLabel.Content="Enter the output directory, e.g. g:\\";
        }

        private void outputDirectoryBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = this.outputDirectoryBox.Text;

            if (text.Length >= 39)
            {
                text = text.Substring(0, 17) + "..." + text.Substring(text.Length - 17, 17);

            }
            
            string contentString = "Generated files will be in: "
                + text + "\\iFit\\Tread\\";
            
            this.outputDirectoryInformationLabel.Content = contentString;
            this.outputDirectoryPath = outputDirectoryBox.Text + "\\iFit\\Tread\\";
        }

        private void outputDirectoryBox_MouseLeave(object sender, MouseEventArgs e)
        {
            outputDirectoryBox_TextChanged(null, null);
        }

        private void workoutLengthBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }
    }
}
