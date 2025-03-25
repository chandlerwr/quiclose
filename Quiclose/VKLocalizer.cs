using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Quiclose {

    /// <summary>
    /// Normally done with a converter. Can't be bothered.
    /// </summary>
    internal partial class VKLocalizer : ObservableObject {

        private readonly ResourceManager _rm = new(typeof(Localization.Main));

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DisplayString))]
        private VK _key;

        public string DisplayString => _rm.GetString($"VK_Label_{Key}") ?? "UNKNOWN?";

    }
}
