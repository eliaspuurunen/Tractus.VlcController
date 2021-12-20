using LibVLCSharp.WPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Tractus.VlcController;

/// <summary>
/// Interaction logic for MediaWindow.xaml
/// </summary>
public partial class MediaWindow : Window, IDataContextAsViewModel<RootViewModel>
{
    public MediaWindow(RootViewModel viewModel)
    {
        this.InitializeComponent();
        this.DataContext = viewModel;
    }

    public VideoView VideoView => this.videoView;

    public bool SkipCloseWarning { get; set; }

    public RootViewModel VM => this.DataContext as RootViewModel;

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Space)
        {
            return;
        }

        this.ToggleBorder();

    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (this.SkipCloseWarning)
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

        base.OnClosing(e);
    }

    public void ToggleBorder()
    {
        if (this.WindowStyle == WindowStyle.SingleBorderWindow)
        {
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
        }
        else
        {
            this.WindowStyle = WindowStyle.SingleBorderWindow;
            this.ResizeMode = ResizeMode.CanResizeWithGrip;
        }
    }
}
