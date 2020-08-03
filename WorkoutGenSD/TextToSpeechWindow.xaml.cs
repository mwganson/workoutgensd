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
using System.Speech.Synthesis;

namespace WorkoutGenSD
{
    /// <summary>
    /// Interaction logic for TextToSpeechWindow.xaml
    /// </summary>
    public partial class TextToSpeechWindow : Window
    {
        SpeechSynthesizer speaker;
        public string PathToWav;
        public bool result = false;
        public List<ListBoxItem> rates = new List<ListBoxItem>(21);
        public List<ListBoxItem> volumes = new List<ListBoxItem>(512);

        public TextToSpeechWindow()
        {
            InitializeComponent();

            speaker = new SpeechSynthesizer();
            //  speaker.Rate = 1;
            for (int ii = -10; ii <= 10; ii++)
            {
                ListBoxItem rateItem = new ListBoxItem();
                rates.Add(rateItem);
                rateItem.Content = ii;
                rateItem.MouseDoubleClick += new MouseButtonEventHandler(rateItem_MouseDoubleClick);
                rateItem.ToolTip = "Double Click me to set the speaker rate to " + ii.ToString() + ".\n";
                rateItem.ToolTip += "Use the Speech Applet in the System Control Panel to set a new default, if desired.";
                rateListBox.Items.Add(rateItem);
            }
            rateListBox.SelectedIndex = speaker.Rate + 10;
            rateListBox.ScrollIntoView(rates[speaker.Rate + 10]);
            rateLabel.Content = "Rate: " + speaker.Rate.ToString();


            {
                for (int ii = 0; ii <= 100; ii++)
                {
                    ListBoxItem volumeItem = new ListBoxItem();
                    volumes.Add(volumeItem);
                    volumeItem.Content = ii;
                    volumeItem.MouseDoubleClick += new MouseButtonEventHandler(volumeItem_MouseDoubleClick);
                    volumeItem.ToolTip = "Double Click me to set the speaker volume to " + ii.ToString() + ".\n";
                    volumeListBox.Items.Add(volumeItem);
                }
                volumeListBox.SelectedIndex = speaker.Volume;
                volumeListBox.ScrollIntoView(volumes[speaker.Volume]);
                volumeLabel.Content = "Volume: " + speaker.Volume.ToString();


            }
          //  speaker.Volume = 100;






        }

        void volumeItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem volumeItem = (ListBoxItem)sender;
            speaker.Volume = (int)volumeItem.Content;
            volumeLabel.Content = "Volume: " + speaker.Volume;
            volumeListBox.SelectedItem = speaker.Volume;
            
        }

        void rateItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem rateItem = (ListBoxItem)sender;
            speaker.Rate = (int)rateItem.Content;
            rateLabel.Content = "Rate: " + speaker.Rate;
            rateListBox.SelectedItem = speaker.Rate;
        }

        private void previewButton_Click(object sender, RoutedEventArgs e)
        {
            speaker.SetOutputToDefaultAudioDevice();
            speaker.Speak(textBox1.Text);
        }

        private void makeWaveButton_Click(object sender, RoutedEventArgs e)
        {
            System.Speech.AudioFormat.SpeechAudioFormatInfo formatInfo = new System.Speech.AudioFormat.SpeechAudioFormatInfo(8000,System.Speech.AudioFormat.AudioBitsPerSample.Eight,System.Speech.AudioFormat.AudioChannel.Mono);
            speaker.SetOutputToWaveFile(PathToWav,formatInfo);

           
          
            speaker.Speak(textBox1.Text);
            speaker.SetOutputToDefaultAudioDevice();
            
            result = true;
            speaker.Dispose();
           
            Close();
           
        
        }

      
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            result = false;
            speaker.Dispose();
            Close();
        }
    }
}
