using System;
using System.Collections.Generic;
using System.Text;

namespace Dropio.Core.Types
{
    public class Movie : Asset
    {
        public int Duration { get; set; }
		public string LargeThumbnailUrl { get; set; }
    }
}
