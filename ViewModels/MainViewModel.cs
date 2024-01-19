using Emgu.CV.Structure;
using Emgu.CV;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Emgu.CV.CvEnum;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Controls;
using System.Diagnostics;

namespace BlurryPhotoSeeker.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<string> _blurryPhotos;
        private string _consoleOutput;
        private double _varianceThreshold = 200.0;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel()
        {
            BlurryPhotos = new ObservableCollection<string>();
        }

        public ObservableCollection<string> BlurryPhotos
        {
            get { return _blurryPhotos; }
            set
            {
                if (_blurryPhotos != value)
                {
                    _blurryPhotos = value;
                    OnPropertyChanged(nameof(BlurryPhotos));
                }
            }
        }

        public string ConsoleOutput
        {
            get { return _consoleOutput; }
            set
            {
                if (_consoleOutput != value)
                {
                    _consoleOutput = value;
                    OnPropertyChanged(nameof(ConsoleOutput));
                }
            }
        }

        public double VarianceThreshold
        {
            get { return _varianceThreshold; }
            set
            {
                if (_varianceThreshold != value)
                {
                    _varianceThreshold = value;
                    OnPropertyChanged(nameof(VarianceThreshold));
                }
            }
        }

        private void UpdateConsoleOutput(string message)
        {
            Application.Current.Dispatcher.Invoke(() => { ConsoleOutput += message + Environment.NewLine; });
        }

        public ICommand OpenFolderCommand => new RelayCommand(OpenFolder);

        private async void OpenFolder()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select Folder",
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Folder Selection",
                Filter = null,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                RestoreDirectory = true,
                ValidateNames = false,
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // Open the BlurryPhotos folder in File Explorer
                string blurryFolder = Path.Combine(Path.GetDirectoryName(openFileDialog.FileNames.First()), "BlurryPhotos");
                Process.Start("explorer.exe", blurryFolder);

                foreach (string selectedFile in openFileDialog.FileNames)
                {
                    // Check if the selected file has a valid image extension
                    if (IsImageFile(selectedFile))
                    {
                        await Task.Run(() =>
                        {
                            // Execute the long-running operation in the background
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                UpdateConsoleOutput($"Selected Image File: {selectedFile}");
                            });

                            // Create the BlurryPhotos folder if it doesn't exist
                            string blurryFolder = Path.Combine(Path.GetDirectoryName(selectedFile), "BlurryPhotos");
                            Directory.CreateDirectory(blurryFolder);

                            // Check if the photo is blurry
                            if (IsPhotoBlurry(selectedFile))
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    // Update the UI on the main thread
                                    UpdateConsoleOutput($"Blurry picture found!" + " - " + selectedFile);
                                });

                                // Move blurry photos to the BlurryPhotos folder
                                string destinationPath = Path.Combine(blurryFolder, Path.GetFileName(selectedFile));
                                File.Move(selectedFile, destinationPath);

                                // Add to the list of blurry photos
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    // Update the UI on the main thread
                                    BlurryPhotos.Add(destinationPath);
                                });
                            }
                        });
                    }
                }

                // Add "DONE" message after scanning all pictures
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Display a popup with "DONE SCANNING!" message
                    MessageBox.Show("DONE SCANNING!", "SCANNING COMPLETE", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK, MessageBoxOptions.DefaultDesktopOnly);
                });
                
            }
        }

        private bool IsImageFile(string filePath)
        {
            string[] validExtensions = { ".jpg", ".jpeg", ".png" };
            string extension = Path.GetExtension(filePath)?.ToLower();
            return !string.IsNullOrEmpty(extension) && validExtensions.Contains(extension);
        }

        private bool IsPhotoBlurry(string imagePath)
        {
            try
            {
                using (var image = new Image<Bgr, byte>(imagePath))
                {
                    // Resize the image to a smaller resolution
                    var resizedImage = image.Resize(640, 480, interpolationType: Inter.Linear);

                    using (var gray = image.Convert<Gray, byte>())
                    {
                        // Compute the Laplacian of the image
                        var laplacian = gray.Laplace(3).AbsDiff(new Gray(0.5));

                        // Calculate the mean and variance of the Laplacian
                        var mean = laplacian.GetAverage().Intensity;
                        var variance = laplacian.Pow(2).GetAverage().Intensity - Math.Pow(mean, 2);

                        // Adjust this threshold based on your needs                       
                        return variance < VarianceThreshold;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log or display an error message)
                Console.WriteLine($"Error processing image: {ex.Message}");
                return false;
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
