using System;
using System.Collections.Generic;
using System.Text;

namespace Dropio.Core.Types
{
    public class Document : Asset
    {
        public int Pages { get; set; }

        /// <summary>
        /// Determines whether this instance can fax.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance can fax; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanFax()
        {
            return true;
        }
    }
}
