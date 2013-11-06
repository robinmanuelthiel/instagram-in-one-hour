﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using InstagramInOneHour.Resources;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Imaging;
using Nokia.Graphics.Imaging;
using Nokia.InteropServices.WindowsRuntime;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Media.PhoneExtensions;
using Windows.Storage.Streams;

namespace InstagramInOneHour
{
    public partial class MainPage : PhoneApplicationPage    
    {
        // This is the pure image without any filters
        // It is staic which makes it accessible from everywhere
        public static WriteableBitmap ImageToFilter;

        // To edit a picture with the Nokia Imaging SDK we need an EditingSession which we define here
        private EditingSession editingSession;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            // Every time we navigate to the MainPage we check if a filter has been selected on the FilterView page
            // If so, we apply this filter to the PreviewImage
            if (FilterSelectorView.SelectedFilter != null)
            {
                await ApplyFilter(FilterSelectorView.SelectedFilter, PreviewPicture);
            }
        }

        private async Task ApplyFilter(ImageFilter imageFilter, Image image)
        {
            // Here we create a new EditingSession based on our selected image and add the selected filter to it
            // After the picture gets rendered to our delivered image
            editingSession = new EditingSession(ImageToFilter.AsBitmap());
            editingSession.AddFilter(imageFilter.Filter);
            await editingSession.RenderToImageAsync(image, OutputOption.PreserveAspectRatio);
        }

        private void AddPictureButton_Click(object sender, RoutedEventArgs e)
        {
            // Select a picture from the picture library or camera and crop it a size of 500px x 500px
            PhotoChooserTask photoChooser = new PhotoChooserTask();            
            photoChooser.Completed += photoChooser_Completed;
            photoChooser.ShowCamera = true;
            photoChooser.PixelHeight = 500;
            photoChooser.PixelWidth = 500;
            photoChooser.Show();
        }

        void photoChooser_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                // The selected picture is delivered as a BitmapImage
                BitmapImage bitmap = new BitmapImage();
                bitmap.SetSource(e.ChosenPhoto);

                // Now we set our PreviewImage and pure image that we want to filter later to that BitmapImage
                PreviewPicture.Source = bitmap;               
                ImageToFilter = new WriteableBitmap(bitmap);

                // After that we can hide the AddPictureButton and activate the Application Bar
                AddPictureButton.Visibility = Visibility.Collapsed;
                ApplicationBar.IsVisible = true;
            }
        }

        private async void AppBarSaveButton_Click(object sender, EventArgs e)
        {
            // Call the save Image method and inform the user
            await SaveImage();
            MessageBox.Show("The image has been successfully saved to the media library", "Image saved", MessageBoxButton.OK);
        }

        private async Task<string> SaveImage()
        {
            // Create a unique file name
            string fileName = "FilteredImage - " + DateTime.Now.ToString();

            // To save an image to the media library we need it as a steam
            IBuffer jpegOut = await editingSession.RenderToJpegAsync();

            // After initializing a MediaLibrary object we can access the library to save the picture
            MediaLibrary library = new MediaLibrary();
            Picture picture = library.SavePicture(fileName, jpegOut.AsStream());

            // Return the path to the saved picture in case that we need it later
            return picture.GetPath();
        }

        private async void AppBarShareButton_Click(object sender, EventArgs e)
        {
            // To share an image through the ShareMediaTask it need to be saved locally
            // Here we need the path of the saved image to deliver it to the MediaShareTask
            ShareMediaTask shareMediaTask = new ShareMediaTask();
            shareMediaTask.FilePath = await SaveImage();
            shareMediaTask.Show();
        }

        private void AppBarApplyFilterButton_Click(object sender, EventArgs e)
        {
            // To apply a filter we need to select one first
            NavigationService.Navigate(new Uri("/FilterSelectorView.xaml", UriKind.Relative));
        }        
    }
}