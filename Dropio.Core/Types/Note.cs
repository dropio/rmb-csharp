using System;
using System.Collections.Generic;
using System.Text;

namespace Dropio.Core.Types
{
    public class Note : Asset
    {
        public string Title { get; set; }
        public string Contents { get; set; }
    }
}
