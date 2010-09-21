using System;
using System.Collections.Generic;
using System.Text;

namespace Dropio.Core
{

	/// <summary>
	/// 
	/// </summary>
	public enum AssetType
	{
		/// <summary>
		/// 
		/// </summary>
		Image,
		/// <summary>
		/// 
		/// </summary>
		Other,
		/// <summary>
		/// 
		/// </summary>
		Audio,
		/// <summary>
		/// 
		/// </summary>
		Document,
		/// <summary>
		/// 
		/// </summary>
		Movie,
		/// <summary>
		/// 
		/// </summary>
		Link
	};

	/// <summary>
	/// 
	/// </summary>
    public enum ExpirationLength
    {
		/// <summary>
		/// 
		/// </summary>
        OneDayFromNow,
		/// <summary>
		/// 
		/// </summary>
        OneWeekFromNow,
		/// <summary>
		/// 
		/// </summary>
        OneMonthFromNow,
		/// <summary>
		/// 
		/// </summary>
        OneYearFromNow,
		/// <summary>
		/// 
		/// </summary>
        OneDayFromLastView,
		/// <summary>
		/// 
		/// </summary>
        OneWeekFromLastView,
		/// <summary>
		/// 
		/// </summary>
        OneMonthFromLastView,
		/// <summary>
		/// 
		/// </summary>
        OneYearFromLastView
    };
	
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	public enum AssetEvents
	{
		/// <summary>
		/// 
		/// </summary>
		AssetCreated = 0x0,
		/// <summary>
		/// 
		/// </summary>
		AssetUpdated = 0x1,
		/// <summary>
		/// 
		/// </summary>
		AssetDeleted = 0x2,
		/// <summary>
		/// 
		/// </summary>
		JobStarted = 0x4,
		/// <summary>
		/// 
		/// </summary>
		JobProgress = 0x8,
		/// <summary>
		/// 
		/// </summary>
		JobComplete = 0x10
	}
	
	/// <summary>
	/// 
	/// </summary>
	public enum Order
	{
		/// <summary>
		/// 
		/// </summary>
		Oldest,
		/// <summary>
		/// 
		/// </summary>
		Newest
	};

	/// <summary>
	/// 
	/// </summary>
    public enum ServiceError
	{
		/// <summary>
		/// 
		/// </summary>
		NotAuthorized,
		/// <summary>
		/// 
		/// </summary>
		NotFound,
		/// <summary>
		/// 
		/// </summary>
		BadRequest,
		/// <summary>
		/// 
		/// </summary>
		ServerError
	};
}