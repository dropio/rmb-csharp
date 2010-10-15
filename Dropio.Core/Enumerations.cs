using System;
using System.Collections.Generic;
using System.Text;

namespace Dropio.Core
{

	/// <summary>
	/// Enumeration for asset types
	/// </summary>
	public enum AssetType
	{
		Image,
		Other,
		Audio,
		Document,
		Movie,
		Link
	};
	
	
	
	/// <summary>
	/// 
	/// </summary>
//    public enum ExpirationLength
//    {
//		/// <summary>
//		/// 
//		/// </summary>
//        OneDayFromNow,
//		/// <summary>
//		/// 
//		/// </summary>
//        OneWeekFromNow,
//		/// <summary>
//		/// 
//		/// </summary>
//        OneMonthFromNow,
//		/// <summary>
//		/// 
//		/// </summary>
//        OneYearFromNow,
//		/// <summary>
//		/// 
//		/// </summary>
//        OneDayFromLastView,
//		/// <summary>
//		/// 
//		/// </summary>
//        OneWeekFromLastView,
//		/// <summary>
//		/// 
//		/// </summary>
//        OneMonthFromLastView,
//		/// <summary>
//		/// 
//		/// </summary>
//        OneYearFromLastView
//    };
	
	/// <summary>
	/// Flags to use for specifying events for pingback creation
	/// </summary>
	[Flags]
	public enum AssetEvents
	{
		AssetCreated = 0x0,
		AssetUpdated = 0x1,
		AssetDeleted = 0x2,
		JobStarted = 0x4,
		JobProgress = 0x8,
		JobComplete = 0x10
	}
	
	/// <summary>
	/// Enumeration for specifying the order of returned results
	/// </summary>
	public enum Order
	{
		Oldest,
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