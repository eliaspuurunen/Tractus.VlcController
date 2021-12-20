using Elias.Wpf.Common;
using System;
using System.Linq;

namespace Tractus.VlcController;

public interface IDataContextAsViewModel<TViewModel> where TViewModel : ViewModelBase
{
    TViewModel VM { get; }
}
