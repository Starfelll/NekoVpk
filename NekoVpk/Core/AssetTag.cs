using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoVpk.Core
{
    public class AssetTag
    {
        public string Name { get; set; }

        public string Color { get; set; }

        public bool Enable { get; set; }

        public AssetTag(string name, string color = "", bool enable = true) { 
            Name = name; Color = color; Enable = enable;
        }

    }
}
