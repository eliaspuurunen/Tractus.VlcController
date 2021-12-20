using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Tractus.VlcController;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static LibVLC LibVLCInstance { get; }
        = new LibVLC();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Core.Initialize();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);
        LibVLCInstance.Dispose();
    }
}
