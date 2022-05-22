using AnimalClassifier.Classes;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
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

namespace AnimalClassifier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image files (*.png; *.jpg) |*.png;*.jpg;*.jpeg|All Files (*.*) |*.*";
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            if(dialog.ShowDialog() == true)
            {
                string fileName = dialog.FileName;
                selectedImage.Source = new BitmapImage(new Uri(fileName));

                MakePredictionAsync(fileName);
            }
        }

        private async void MakePredictionAsync(string fileName)
        {
            predictionsListLabel.Text = "Processing...";
            //if predictionEndpoint is not set default to empty string
            string url = ConfigurationManager.AppSettings.Get("predictionEndpoint");
            string prediction_key = ConfigurationManager.AppSettings.Get("predictionKey");
            string content_type = "application/octet-stream";
            var file = File.ReadAllBytes(fileName);

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Prediction-Key", prediction_key);
                
                using(var content = new ByteArrayContent(file))
                {
                    try
                    {
                        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(content_type);
                        var response = await client.PostAsync(url, content);
                        var responseString = await response.Content.ReadAsStringAsync();

                        List<Prediction> predictions = JsonConvert.DeserializeObject<CustomVision>(responseString).Predictions;
                        predictionsListView.ItemsSource = predictions;

                        //grab the highest probability tag
                        var result = predictions.OrderByDescending(x => x.Probability).First();
                        if (result.TagName == "cat")
                            predictionsListLabel.Text = "Kitty";
                        else if (result.TagName == "dog")
                            predictionsListLabel.Text = "Doggo";
                    }
                    catch(Exception ex)
                    {
                        predictionsListLabel.Text = "Error";
                        MessageBox.Show("Error requesting data. Please check that the App.config file has been properly set!");
                    }
                }
            }
        }
    }
}
