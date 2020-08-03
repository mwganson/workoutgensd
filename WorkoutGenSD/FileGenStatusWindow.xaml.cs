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
    /// Interaction logic for FileGenStatusWindow.xaml
    /// </summary>
    public partial class FileGenStatusWindow : Window
    {
        public bool bFileGenerationInProgress = true;
        public FileGenStatusWindow()
        {
            InitializeComponent();
            this.Closing += new System.ComponentModel.CancelEventHandler(FileGenStatusWindow_Closing);
        }

        void FileGenStatusWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (bFileGenerationInProgress == true)
            {
                UpdateStatus("please wait, file generation still in progress");
                e.Cancel = true;
            }
        }

        public void UpdateStatus(string status)
        {

            this.InvalidateVisual();
            statusLabel.Content = status;

        }



    }
}
