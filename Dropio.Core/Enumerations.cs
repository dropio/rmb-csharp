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
	
	public enum FaxStatus { None, Pending, Failed, Success };

    public enum ServiceError { NotAuthorized, NotFound, BadRequest, ServerError };

}
