using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoVpk.ViewModels
{
    public partial class Settings : ViewModelBase
    {
        public string GameDir
        {
            get => NekoSettings.Default.GameDir;
            set
            {
                if (NekoSettings.Default.GameDir != value)
                {
                    NekoSettings.Default.GameDir = value;
                    this.RaisePropertyChanged(nameof(GameDir));
                }
            }
        }

        public short CompressionLevel
        {
            get => (short)(NekoSettings.Default.CompressionLevel - 1);
            set
            {
                if (CompressionLevel != (value))
                {
                    NekoSettings.Default.CompressionLevel = (short)(value+1);
                    this.RaisePropertyChanged(nameof(CompressionLevel));
                }
            }
        }



    }
}
