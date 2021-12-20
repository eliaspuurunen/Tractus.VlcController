using System;
using System.Linq;

namespace Tractus.VlcController;

public record MediaItem(string Path)
{
    public string Name => System.IO.Path.GetFileNameWithoutExtension(this.Path);
}
