using System;
using System.Collections;
using System.Collections.Generic;



namespace Dropio.Core
{
	public class Job {
	
		public static bool Convert( string jobType, List<Hashtable> inputs, List<Hashtable> outputs, string plugin, string pingbackUrl )
		{
			return ServiceProxy.Instance.Convert( jobType, inputs, outputs, plugin, pingbackUrl );
		}
		
		public static bool Convert( string jobType, Hashtable inputs, Hashtable outputs, string plugin, string pingbackUrl )
		{
			// put Hashtable's into List<Hashtable>'s
			List<Hashtable> inputsHashList = new List<Hashtable>();
			List<Hashtable> outputsHashList = new List<Hashtable>();
			inputsHashList.Add( inputs );
			outputsHashList.Add( outputs );
			
			return ServiceProxy.Instance.Convert( jobType, inputsHashList, outputsHashList, plugin, pingbackUrl );
		}
	}
}