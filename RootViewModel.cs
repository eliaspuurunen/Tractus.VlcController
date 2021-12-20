using Elias.Wpf.Common;
using Elias.Wpf.Common.Commands;
using LibVLCSharp.Shared;
using LibVLCSharp.WPF;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace Tractus.VlcController;

public class RootViewModel : ViewModelBase
{
    public ObservableCollection<MediaItem> Playlist { get; }
         = new ObservableCollection<MediaItem>();

    private System.Windows.Media.ImageSource holdingSlide;
    private bool showHoldingImage;
    private bool showBlackout;
    private bool autoplay;
    private ScreenHelpers.DEVMODE selectedScreen;
    private TimeSpan fileLength;
    private MediaItem playingFile;
    private int height = 720;
    private int width = 1280;
    private MediaPlayer mediaPlayer;
    private MediaItem selectedFile;
    private MediaWindow mediaWindow;

    public VLCState State
    {
        get
        {
            return this.MediaPlayer?.State ?? VLCState.Stopped;
        }
    }


    public bool ShowBlackout
    {
        get => this.showBlackout;
        set
        {
            if (this.showBlackout == value)
                return;
            this.showBlackout = value;

            if (this.showBlackout)
            {
                this.ShowHoldingImage = false;
            }
            this.OnPropertyChanged();
        }
    }


    public bool ShowHoldingImage
    {
        get => this.showHoldingImage;
        set
        {
            if (value)
            {
                if (this.HoldingSlide is null)
                {
                    this.ChooseHoldingImageFromFile();
                    if (this.HoldingSlide is null)
                    {
                        return;
                    }
                }
            }

            if (this.showHoldingImage == value)
                return;
            this.showHoldingImage = value;

            if (this.showHoldingImage)
            {
                this.ShowBlackout = false;
            }
            this.OnPropertyChanged();
        }
    }


    public bool Autoplay
    {
        get => this.autoplay;
        set
        {
            if (this.autoplay == value)
                return;
            this.autoplay = value;
            this.OnPropertyChanged();
        }
    }


    public MediaItem SelectedFile
    {
        get => this.selectedFile;
        set
        {
            if (this.selectedFile == value)
                return;
            this.selectedFile = value;
            this.OnPropertyChanged();
        }
    }

    public MediaItem PlayingFile
    {
        get => this.playingFile;
        set
        {
            if (this.playingFile == value)
                return;
            this.playingFile = value;
            this.OnPropertyChanged();
        }
    }


    public TimeSpan FileLength
    {
        get => this.fileLength;
        set
        {
            if (this.fileLength == value)
                return;
            this.fileLength = value;
            this.OnPropertyChanged();
        }
    }


    public MediaPlayer MediaPlayer
    {
        get => this.mediaPlayer;
        set
        {
            if (this.mediaPlayer == value)
                return;

            if (this.mediaPlayer != null)
            {
                this.mediaPlayer.LengthChanged -= this.OnMediaLengthChanged;
                this.mediaPlayer.PositionChanged -= this.OnMediaPositionChanged;
                this.mediaPlayer.VolumeChanged -= this.OnVolumeChanged;
                this.mediaPlayer.Stopped -= this.OnStopped;
            }

            this.mediaPlayer = value;

            if (this.mediaPlayer != null)
            {
                this.mediaPlayer.LengthChanged += this.OnMediaLengthChanged;
                this.mediaPlayer.PositionChanged += this.OnMediaPositionChanged;
                this.mediaPlayer.VolumeChanged += this.OnVolumeChanged;
                this.mediaPlayer.Stopped += this.OnStopped;
            }

            this.OnPropertyChanged();
        }
    }

    private void OnStopped(object? sender, EventArgs e)
    {
        this.OnPropertyChanged("State");

        if (this.Autoplay)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback((_) =>
            {
                this.PlayNext.Execute(null);
            }));
        }
    }

    public float Position
    {
        get
        {
            if (this.MediaPlayer == null)
            {
                return 0;
            }

            return this.MediaPlayer.Position;
        }
        set
        {
            this.MediaPlayer.Position = value;
        }
    }

    private void OnVolumeChanged(object sender, MediaPlayerVolumeChangedEventArgs e)
    {
        this.OnPropertyChanged("Volume");
    }

    private void OnMediaPositionChanged(object? sender, MediaPlayerPositionChangedEventArgs e)
    {
        this.OnPropertyChanged("Position");
        this.OnPropertyChanged("State");
    }

    private void OnMediaLengthChanged(object sender, MediaPlayerLengthChangedEventArgs e)
    {
        this.FileLength = TimeSpan.FromMilliseconds(this.MediaPlayer.Length);
    }

    public int Width
    {
        get => this.width;
        set
        {
            if (this.width == value)
                return;
            this.width = value;
            this.OnPropertyChanged();
        }
    }

    public int Height
    {
        get => this.height;
        set
        {
            if (this.height == value)
                return;
            this.height = value;
            this.OnPropertyChanged();
        }
    }

    public ICommand PlayFile { get; }
    public ICommand Pause { get; }
    public ICommand PlayNext { get; }
    public ICommand ResetProgress { get; }
    public ICommand RemoveSelectedPlaylistItem { get; }
    public ICommand AddItemsToPlaylist { get; }

    public ICommand SnapPlayerToTopLeft { get; }

    public ICommand HidePlayerDecorations { get; }
    public ICommand ChooseHoldingImage { get; }

    public ObservableCollection<ScreenHelpers.DEVMODE> Screens { get; }
        = new ObservableCollection<ScreenHelpers.DEVMODE>();

    
    public System.Windows.Media.ImageSource HoldingSlide
    {
        get => this.holdingSlide;
        set
        {
            if (this.holdingSlide == value)
                return;
            this.holdingSlide = value;
            this.OnPropertyChanged();
        }
    }
    

    public ScreenHelpers.DEVMODE SelectedScreen
    {
        get => this.selectedScreen;
        set
        {
            this.selectedScreen = value;
            this.OnPropertyChanged();
        }
    }

    public int Volume
    {
        get => this.MediaPlayer?.Volume ?? 0;
        set
        {
            this.MediaPlayer.Volume = value;
        }
    }

    public RootViewModel()
    {
        this.Pause = new ActionCommand(this.PauseMedia);
        this.PlayFile = new ActionCommand(this.PlaySelectedMedia);
        this.PlayNext = new ActionCommand(this.PlayNextFile);
        this.ResetProgress = new ActionCommand(this.ResetProgressToZero);
        this.RemoveSelectedPlaylistItem = new ActionCommand(this.RemoveSelectedItem);
        this.AddItemsToPlaylist = new ActionCommand(this.OpenFileBrowser);
        this.SnapPlayerToTopLeft = new ActionCommand(this.ForceSnapPlayer);
        this.HidePlayerDecorations = new ActionCommand(this.ToggleHidePlayerDecorations);
        this.ChooseHoldingImage = new ActionCommand(this.ChooseHoldingImageFromFile);

        foreach (var screen in System.Windows.Forms.Screen.AllScreens)
        {
            var dm = new ScreenHelpers.DEVMODE();
            dm.dmSize = (short)Marshal.SizeOf(typeof(ScreenHelpers.DEVMODE));
            var settings = ScreenHelpers.EnumDisplaySettings(screen.DeviceName, -1, ref dm);

            this.Screens.Add(dm);
        }
    }

    private void ChooseHoldingImageFromFile()
    {
        var openFileDialog = new OpenFileDialog();

        openFileDialog.Filter = "Image files (*.jpg;*.png;*.bmp)|*.jpg;*.bmp;*.png";

        openFileDialog.Title = "Choose Holding Image...";

        var result = openFileDialog.ShowDialog();

        if(result != true)
        {
            return;
        }

        var source = new System.Windows.Media.Imaging.BitmapImage();
        source.BeginInit(); 

        var uri = new Uri(openFileDialog.FileName);

        source.UriSource = uri;

        source.EndInit();
        source.Freeze();

        this.HoldingSlide = source;
    }

    private void ToggleHidePlayerDecorations()
    {
        this.mediaWindow.ToggleBorder();
    }

    private void ForceSnapPlayer()
    {
        var screen = this.SelectedScreen;

        var top = screen.dmPositionY;
        var left = screen.dmPositionX;

        this.mediaWindow.Top = top;
        this.mediaWindow.Left = left;
        this.mediaWindow.Height = this.Height;
        this.mediaWindow.Width = this.Width;
    }

    public void InitializePlayer(
        VideoView videoView,
        MediaWindow mediaWindow)
    {
        this.mediaWindow = mediaWindow;
        this.MediaPlayer = new MediaPlayer(App.LibVLCInstance);

        videoView.MediaPlayer = this.MediaPlayer;
        this.OnPropertyChanged("Volume");
    }

    private void PlaySelectedMedia()
    {
        if (this.SelectedFile == null)
        {
            return;
        }

        Media oldMedia = null;
        if (this.MediaPlayer.Media != null)
        {
            oldMedia = this.MediaPlayer.Media;
        }

        var media = new Media(App.LibVLCInstance, this.SelectedFile.Path);

        this.MediaPlayer.Play(media);

        this.PlayingFile = this.SelectedFile;
        this.OnPropertyChanged("State");
        this.OnPropertyChanged("Volume");

        if (oldMedia != null)
        {
            oldMedia.Dispose();
        }
    }

    private void PauseMedia()
    {
        this.MediaPlayer.Pause();
        this.OnPropertyChanged("State");
        this.OnPropertyChanged("Volume");
    }

    private void PlayNextFile()
    {
        if (this.Playlist.Count == 0)
        {
            return;
        }

        this.MediaPlayer.Stop();

        if (this.PlayingFile == null)
        {
            if (this.SelectedFile == null)
            {
                this.SelectedFile = this.Playlist.FirstOrDefault();
            }

            this.PlaySelectedMedia();
        }
        else
        {
            var index = this.Playlist.IndexOf(this.PlayingFile);
            if (index == -1)
            {
                index = 0;
            }
            else
            {
                index = (index + 1) % this.Playlist.Count;
            }

            var item = this.Playlist[index];

            this.SelectedFile = item;
            this.PlaySelectedMedia();
        }

        this.OnPropertyChanged("State");
        this.OnPropertyChanged("Volume");
    }

    private void ResetProgressToZero()
    {
        this.MediaPlayer.Position = 0;
    }

    private void RemoveSelectedItem()
    {
        if (this.SelectedFile == null)
        {
            return;
        }

        this.Playlist.Remove(this.SelectedFile);
        this.SelectedFile = null;
    }

    private void OpenFileBrowser()
    {
        var openFileDialog = new OpenFileDialog();

        openFileDialog.Filter = "Media Files (*.mov;*.mp4;*.mkv)|*.mov;*.mp4;*.mkv|All files (*.*)|*.*";
        openFileDialog.Multiselect = true;

        var result = openFileDialog.ShowDialog();

        if (result != true)
        {
            return;
        }

        var files = openFileDialog.FileNames.OrderBy(f => f);

        foreach (var file in files)
        {
            this.Playlist.Add(new MediaItem(file));
        }
    }

    public void AdjustVolume(int magnitude)
    {
        var newVolume = Math.Max(0, Math.Min(200, this.mediaPlayer.Volume + magnitude));

        this.mediaPlayer.Volume = newVolume;
    }
}
