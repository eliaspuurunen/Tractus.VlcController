using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Tractus.VlcController;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, IDataContextAsViewModel<RootViewModel>
{
    private MediaWindow? mediaWindow;
    private bool closed; 

    public MainWindow()
    {
        this.InitializeComponent();
        this.DataContext = new RootViewModel();
        this.Loaded += this.OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        this.mediaWindow = new MediaWindow(this.VM);
        this.mediaWindow.Closed += this.OnMediaWindowClosed;
        this.mediaWindow.Show();
        this.VM.InitializePlayer(
            this.mediaWindow.VideoView,
            this.mediaWindow);
    }

    private void OnMediaWindowClosed(object? sender, EventArgs e)
    {
        this.mediaWindow = null;

        if (!this.closed)
        {
            this.Close();
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if(this.mediaWindow == null)
        {
            base.OnClosing(e);
            return;
        }

        var result = MessageBox.Show("Are you sure you want to exit?", "Exit?", MessageBoxButton.YesNo);

        if (result == MessageBoxResult.No)
        {
            e.Cancel = true;
            base.OnClosing(e);

            return;
        }

        this.mediaWindow.SkipCloseWarning = true;
        this.closed = true;
        base.OnClosing(e);
        this.mediaWindow.Close();
    }

    public RootViewModel VM => this.DataContext as RootViewModel;

    private void OnDoubleClick(object sender, MouseButtonEventArgs e)
    {
        this.VM.PlayFile.Execute(null);
    }

    private void OnPreviewKeyUp(object sender, KeyEventArgs e)
    {
        var isCtrlPressed = e.KeyboardDevice.Modifiers == ModifierKeys.Control;

        if (isCtrlPressed)
        {
            if (e.Key == Key.O)
            {
                this.VM.AddItemsToPlaylist.Execute(null);
            }
            else if (e.Key == Key.H)
            {
                this.mediaWindow.ToggleBorder();
            }
            else if (e.Key == Key.S)
            {
                this.VM.SnapPlayerToTopLeft.Execute(null);
            }
            else if (e.Key == Key.Up)
            {
                this.VM.AdjustVolume(5);
            }
            else if (e.Key == Key.Down)
            {
                this.VM.AdjustVolume(-5);
            }
        }
        else
        {
            if (e.Key == Key.Space || e.Key == Key.C)
            {
                this.VM.Pause.Execute(null);
            }
            else if (e.Key == Key.B)
            {
                this.VM.PlayNext.Execute(null);
            }
            else if (e.Key == Key.X)
            {
                this.VM.PlayFile.Execute(null);
            }
            else if (e.Key == Key.Delete)
            {
                this.VM.RemoveSelectedPlaylistItem.Execute(null);
            }
            else if (e.Key == Key.R)
            {
                this.VM.ResetProgress.Execute(null);
            }
        }


    }

    private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)
        {
            UseShellExecute = true
        });
        e.Handled = true;
    }
}
