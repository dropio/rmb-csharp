using System;
using System.Collections.Generic;
using System.Text;

namespace Dropio.Core.Types
{
    public class Audio : Asset
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public int Duration { get; set; }
    }
}
