using System;
using System.Collections.Generic;
using System.Text;

namespace Dropio.Core.Types
{
    public class Link : Asset
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
    }
}
