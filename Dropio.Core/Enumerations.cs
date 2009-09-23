using System;
using System.Collections.Generic;
using System.Text;

namespace Dropio.Core
{
    public enum SendTo { Drop, Email, Fax };

    public enum ExpirationLength
    {
        OneDayFromNow,
        OneWeekFromNow,
        OneMonthFromNow,
        OneYearFromNow,
        OneDayFromLastView,
        OneWeekFromLastView,
        OneMonthFromLastView,
        OneYearFromLastView
    };
	
	[Flags]
	public enum AssetEvents
	{
		AssetAdded = 0x0,
		AssetUpdated = 0x1,
		AssetDeleted = 0x2,
		CommentAdded = 0x4,
		CommentUpdated = 0x8,
		CommentDeleted = 0x10,
	}
	
	public enum Order
	{ 
		Oldest, 
		Newest
	};
	
	public enum FaxStatus { None, Pending, Failed, Success };

    public enum ServiceError { NotAuthorized, NotFound, BadRequest, ServerError };

}
