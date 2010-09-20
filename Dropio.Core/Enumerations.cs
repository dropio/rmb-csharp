using System;
using System.Collections.Generic;
using System.Text;

namespace Dropio.Core
{

	public enum AssetType
	{
		Image,
		Other,
		Audio,
		Document,
		Movie,
		Link
	};

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
		AssetCreated = 0x0,
		AssetUpdated = 0x1,
		AssetDeleted = 0x2,
		JobStarted = 0x4,
		JobProgress = 0x8,
		JobComplete = 0x10,
	}
	
	public enum Order
	{ 
		Oldest, 
		Newest
	};

    public enum ServiceError { NotAuthorized, NotFound, BadRequest, ServerError };

}
