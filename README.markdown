drop.io API library for C#
============================

Before using the Dropio library, the application must set an API key.  This key will be used for all requests to the server.  To get an API key, go to [http://api.drop.io/](http://api.drop.io/).  Then make sure you set the API key before you use the API:

    ServiceProxy.Instance.ServiceAdapter.ApiKey = "<your_api_key>";

It is *highly* recommended you create a secure API key, otherwise malicious individuals can easily use your API for themselves. When you create a secure key you will have an API Key and an API secret that go together. Set the API Key as above, then set the secret:

	ServiceProxy.Instance.ServiceAdapter.ApiSecret = "<your_api_secret>";

The library will sign all the API requests for you if you set the API Secret.

The Drop object
---------------

To get a 'Drop' object make sure you have your API key (and secret if using one) set. Then to acess an existing drop:

	Drop drop = Drop.Find("<name_of_existing_drop>");

To create a new Drop:

	Drop drop = Drop.Create(); // drop will have a random name assigned to it
	Drop drop = Drop.Create("<drop_name>"); // drop will be created with specified name, or
											// will fail if name exists
