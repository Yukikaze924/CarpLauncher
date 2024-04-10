using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ProjBobcat.Class.Model;

namespace CarpLauncher.Core;

public partial class ProfileManager : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<VersionInfo>? _profiles;
}
