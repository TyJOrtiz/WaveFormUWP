using System;
using System.Numerics;
using Microsoft.Graphics.Canvas.Effects;
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;
using EF = Microsoft.Toolkit.Uwp.UI.Animations.Expressions.ExpressionFunctions;
using Windows.UI.Xaml;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Composition.Interactions;
using Microsoft.Toolkit.Uwp.UI.Animations.Expressions;
using System.Threading.Tasks;
using Windows.Media.Audio;
using Windows.Storage.Pickers;
using Windows.Storage;
using NAudio.WaveFormRenderer;
using NAudio.Wave;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.FileProperties;
using Windows.UI;
using Windows.Media.MediaProperties;
using Windows.Media.Editing;
using System.Collections.Generic;
using System.IO;
using Microsoft.Toolkit.Uwp.UI;
using System.Linq;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TabbedConcept
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<TabControl> TabControls = new ObservableCollection<TabControl>();
        private double height;
        private AudioGraph audioGraph;
        private AudioFileInputNode fileInputNode;
        private WaveFormRendererSettings settings;
        private WaveFormRenderer waveFormRenderer;
        private double PageWidth;
        private double PageHeight;
        private StorageFile mediafile;
        private MusicProperties props;
        public ObservableCollection<AudioSplit> AudioSplits = new ObservableCollection<AudioSplit>();
        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += (s, o) =>
            {
                PageWidth = this.ActualWidth;
                
            settings = GetRendererSettings();
            waveFormRenderer = new WaveFormRenderer();
            };
        }
        private async Task InitAudioGraph()
        {
            await CreateFileInputNode();
        }
        private async Task CreateFileInputNode()
        {
            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
            filePicker.FileTypeFilter.Add(".mp3");
            filePicker.FileTypeFilter.Add(".wav");
            filePicker.FileTypeFilter.Add(".wma");
            filePicker.FileTypeFilter.Add(".m4a");
            filePicker.ViewMode = PickerViewMode.Thumbnail;
            mediafile = await filePicker.PickSingleFileAsync();

            // File can be null if cancel is hit in the file picker
            if (mediafile == null)
            {
                return;
            }
            RenderWaveform(mediafile);
        }
        private WaveFormRendererSettings GetRendererSettings()
        {
            settings = new WaveFormRendererSettings();
            settings.BackgroundBrush = new SolidColorBrush(Windows.UI.Colors.White);
            settings.TopBarBrush = new SolidColorBrush(Windows.UI.Colors.Black);
            settings.BottomBarBrush = new SolidColorBrush(Windows.UI.Colors.Black);
            settings.TopHeight = (int)200;
            settings.BottomHeight = (int)200;
            settings.Width = (int)1920;
            settings.BlockWidth = (int)5;
            settings.PixelsPerPeak = 2;
            settings.SpacerPixels = 1;
            settings.DecibelScale = true;
            //settings.BackgroundBrush = new SolidColorBrush(Windows.UI.Colors.Blue);
            return settings;
        }
        private WaveFormRendererSettings SaveRendererSettings()
        {
            settings = new WaveFormRendererSettings();
            settings.TopBarBrush = new SolidColorBrush(TopPickerColor.Color);
            settings.BottomBarBrush = new SolidColorBrush(BottomPickerColor.Color);
            settings.BackgroundBrush = new SolidColorBrush(BackgroundPickerColor.Color);
            settings.TopHeight = (int)200;
            settings.BottomHeight = (int)200;
            settings.Width = (int)1920;
            settings.BlockWidth = Convert.ToInt32(BlockWidth.Text);
            settings.PixelsPerPeak = 2;
            settings.SpacerPixels = Convert.ToInt32(SpaceWidth.Text);
            settings.DecibelScale = true;
            //settings.BackgroundBrush = new SolidColorBrush(Windows.UI.Colors.Blue);
            return settings;
        }
        private async void RenderThreadFunc(IPeakProvider peakProvider, StorageFile file, WaveFormRendererSettings settings)
        {
            try
            {
                using (var waveStream = new MediaFoundationReader(file.Path))
                {
                        image = await waveFormRenderer.Render(waveStream, peakProvider, settings);
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        Headrer.Children.Add((StackPanel)image);
                        Headrer.Width = image.Width;
                        LoadingBar.Visibility = Visibility.Collapsed;
                    });
                }
            }
            catch (Exception e)
            {
                //MessageBox.Show(e.Message);
            }
        }
        private IPeakProvider GetPeakProvider()
        {
            return new MaxPeakProvider();
        }
        private async void RenderWaveform(StorageFile mediafile)
        {
            if (mediafile == null) 
                return;
            LoadingBar.Visibility = Visibility.Visible;
            var peakProvider = GetPeakProvider();
            mediafile = await mediafile.CopyAsync(ApplicationData.Current.LocalFolder, mediafile.Name, NameCollisionOption.GenerateUniqueName);
            props = await mediafile.Properties.GetMusicPropertiesAsync();
            settings.Width = (int)(props.Duration.TotalMilliseconds / 100);
            //Blah.Width = settings.Width;
            Headrer.Children.Clear();
            await Task.Factory.StartNew(() => RenderThreadFunc(peakProvider, mediafile, settings));
        }

        public async void Reload()
        {
            settings = SaveRendererSettings();
            RenderWaveform(mediafile);
        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await InitAudioGraph();
        }
        internal static TimeSpan ParseTime(string startTime)
        {
            TimeSpan timeSpan = TimeSpan.Parse("00:00");
            var formats = new[]
            {
                @"mm\:ss",
                @"hh\:mm\:ss",
                @"dd\:hh\:mm\:ss"
            };

            try
            {
                var ts = TimeSpan.ParseExact(startTime, formats, null);
                timeSpan = timeSpan.Add(ts);
            }
            catch
            {
                var ts = TimeSpan.ParseExact("00:00", formats, null);
                timeSpan = timeSpan.Add(ts);
            }
            return timeSpan;
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var derp = MediaSource.CreateFromStorageFile(mediafile);
            var d1 = ParseTime(string.IsNullOrEmpty(StartTime.Text) ? props.Duration.ToString(@"mm\:ss") : StartTime.Text) - ParseTime(string.IsNullOrEmpty(EndTime.Text) ? props.Duration.ToString(@"mm\:ss") : EndTime.Text);
            var d2 = -d1;
            var playbackitem = new MediaPlaybackItem(derp, startTime: ParseTime(string.IsNullOrEmpty(StartTime.Text) ? "00:00" : StartTime.Text), d2);
            
            var d = d2.TotalMilliseconds / 100;
            var Rects = new Grid
            {
                Background = new SolidColorBrush { Color = (Color)Application.Current.Resources["SystemAccentColor"], Opacity = 0.5 },
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush { Color = (Color)Application.Current.Resources["SystemAccentColor"] },
                Margin = new Thickness(playbackitem.StartTime.TotalMilliseconds / 100, 0, 0, 0),
                Height = 100,
                Width = d,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            AudioSplits.Add(new AudioSplit
            {
                EndText = EndTime.Text,
                End = (double)ParseTime(string.IsNullOrEmpty(EndTime.Text) ? props.Duration.ToString(@"mm\:ss") : EndTime.Text).Ticks,
                Start = (double)playbackitem.StartTime.Ticks,
                EndDouble = playbackitem.DurationLimit.Value.TotalMilliseconds / 100,
                StartDouble = playbackitem.StartTime.TotalMilliseconds / 100,
                StartText = StartTime.Text,
                PlayBackItem = playbackitem,
                TrimVisual = Rects,
                Width = (double)settings.Width
            });

            var Rect = new Grid
            {
                Background = new SolidColorBrush { Color = (Color)Application.Current.Resources["SystemAccentColor"], Opacity = 0.5 },
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush { Color = (Color)Application.Current.Resources["SystemAccentColor"] },
                Margin = new Thickness(playbackitem.StartTime.TotalMilliseconds / 100, 0, 0, 0),
                Height = 400,
                Width = d,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            Headrer.Children.Add(Rect); 
            
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var encodingProfile = await MediaEncodingProfile.CreateFromFileAsync(file: mediafile);
            var savePicker = new FolderPicker();
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
            savePicker.ViewMode = PickerViewMode.Thumbnail;

            StorageFolder folder = await savePicker.PickSingleFolderAsync();
            if (folder != null)
            {
                foreach (var item in AudioSplits)
                {
                    var clip = await MediaClip.CreateFromFileAsync(mediafile);

                    // Trim the front and back 25% from the clip
                    clip.TrimTimeFromStart = new TimeSpan((long)(item.Start));
                    clip.TrimTimeFromEnd = new TimeSpan((long)((double)props.Duration.Ticks - item.End));

                    var composition = new MediaComposition();
                    composition.Clips.Add(clip);
                    var file = await folder.CreateFileAsync("trimmedfile" + $"_{AudioSplits.IndexOf(item) + 1}" + Path.GetExtension(mediafile.Path), CreationCollisionOption.GenerateUniqueName);
                    //Save to file using original encoding profile
                    var result = await composition.RenderToFileAsync(file, MediaTrimmingPreference.Precise, encodingProfile);

                    if (result != Windows.Media.Transcoding.TranscodeFailureReason.None)
                    {
                        System.Diagnostics.Debug.WriteLine("Saving was unsuccessful");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Trimmed clip saved to file");
                    }
                }
            }
        }

        private async void SaveWaveFormToPng()
        {
            var rtb = new RenderTargetBitmap();
            await rtb.RenderAsync(image); // Render control to RenderTargetBitmap
            var savePicker = new FileSavePicker();
            savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
            savePicker.SuggestedFileName = mediafile.DisplayName + "_visualizer";
            savePicker.DefaultFileExtension = ".png";
            savePicker.FileTypeChoices.Add("PNG", new List<string>() { ".png" });
            savePicker.FileTypeChoices.Add("JPG", new List<string>() { ".jpg", ".jpeg" });
            savePicker.FileTypeChoices.Add("BMP", new List<string>() { ".bmp" });
            StorageFile file1 = await savePicker.PickSaveFileAsync();
            if (file1 != null)
            {
                // Get pixels from RTB
                IBuffer pixelBuffer = await rtb.GetPixelsAsync();
                byte[] pixels = pixelBuffer.ToArray();

                // Support custom DPI
                DisplayInformation displayInformation = DisplayInformation.GetForCurrentView();

                var stream = new InMemoryRandomAccessStream();
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(GetEncoder(file1), stream);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, // RGB with alpha
                                     BitmapAlphaMode.Premultiplied,
                                     (uint)rtb.PixelWidth,
                                     (uint)rtb.PixelHeight,
                                     displayInformation.RawDpiX,
                                     displayInformation.RawDpiY,
                                     pixels);

                await encoder.FlushAsync(); // Write data to the stream
                stream.Seek(0); // Set cursor to the beginning
                //StorageFile file1 = await ApplicationData.Current.LocalFolder.CreateFileAsync("MyImageFile.png", CreationCollisionOption.GenerateUniqueName);

                using (var fileStream1 = await file1.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await RandomAccessStream.CopyAndCloseAsync(stream.GetInputStreamAt(0), fileStream1.GetOutputStreamAt(0));
                }
                stream.Dispose();
                await Windows.System.Launcher.LaunchFileAsync(file1);
            }
            // Use stream (e.g. save to file)
        }

        private Guid GetEncoder(StorageFile file1)
        {
            Guid guid;
            switch (Path.GetExtension(file1.Path))
            {
                case ".png":
                    guid = BitmapEncoder.PngEncoderId;
                    break;
                case ".jpg":
                    guid = BitmapEncoder.JpegEncoderId;
                    break;
                case ".jpeg":
                    guid = BitmapEncoder.JpegEncoderId;
                    break;
                case ".bmp":
                    guid = BitmapEncoder.BmpEncoderId;
                    break;
            }
            return guid;
        }

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {
            var children = this.FindDescendants();
            var scrolls = children.Where(x => x.GetType() == typeof(ScrollViewer)).Cast<ScrollViewer>().ToList().Except(new[] {sender as ScrollViewer});
            foreach (var scroll in scrolls)
            {
                if (scroll.Name == "HeaderScroll" || scroll.Name == "SplitScroll")
                {
                    scroll.ChangeView(e.FinalView.HorizontalOffset, null, null);
                }
            }
        }

        private void LostAudioScroll(object sender, PointerRoutedEventArgs e)
        {
            //SplitScroll.ViewChanging += ScrollViewer_ViewChanging;
        }

        private void GotAudioScroll(object sender, PointerRoutedEventArgs e)
        {
            //SplitScroll.ViewChanging -= ScrollViewer_ViewChanging;
        }
        private MediaPlayer MediaPlayer = new MediaPlayer();
        private StackPanel image;

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //MediaPlayer.Source = null;
            //var list = new MediaPlaybackList();
            //list.Items.Add(((sender as Button).Tag as MediaPlaybackItem));

            //MediaPlayer.Source = list;
            //MediaPlayer.Play();
        }

        private void SaveWaveForm(object sender, RoutedEventArgs e)
        {
            SaveWaveFormToPng();
        }
    }
}
