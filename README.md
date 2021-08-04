MANGOPAY .NET SDK [![Build Status](https://travis-ci.org/Mangopay/mangopay2-net-sdk.svg?branch=master)](https://travis-ci.org/Mangopay/mangopay2-net-sdk)
=================================================
MangopaySDK is a Microsoft .NET client library to work with
[Mangopay REST API](http://docs.mangopay.com/api-references/).


Installation and package dependencies
-------------------------------------------------
SDK has been written in C# 6.0, for .NET Framework 4.5 and has 3 dependencies on external packages. These dependencies are:
- Common.Logging library (version 3.4.1)
- Newtonsoft.Json (version 13.0.1)
- RestSharp (version 106.12.0)

The installation is as easy as downloading the SDK package and storing it under any location that will be available for referencing by your project (see examples below). You can also install from the .Net Package Manager Console:

	Install-Package mangopay2-sdk

License
-------------------------------------------------
MangopaySDK is distributed under MIT license, see LICENSE file.


Unit Tests
-------------------------------------------------
Tests are placed in MangoPay.SDK.Tests project in solution.


Contact
-------------------------------------------------
Report bugs or suggest features using [issue tracker at GitHub](https://github.com/MangoPay/mangopay2-net-sdk/issues).


Account creation
-------------------------------------------------
You can get yourself a free sandbox account or sign up for a production account by [registering on the Mangopay site](https://www.mangopay.com/start/) (note that validation of your production account involves several steps, so think about doing it in advance of when you actually want to go live).


Configuration
-------------------------------------------------
Using the credential info from the signup process above, you should then set `api.Config.ClientId` to your MANGOPAY Client ID and `api.Config.ClientPassword` to your apiKey.

`api.Config.BaseUrl` is set to sandbox environment by default. To enable production environment, set it to `https://api.mangopay.com`:

    api.Config.BaseUrl = "https://api.mangopay.com";

The `ClientId`, `ClientPassword` and `BaseUrl` properties are mandatory. Optionally, you can set the logger instance, setting `LoggerFactoryAdapter` property to an instance of `ILoggerFactoryAdapter`.
By default, there is `NoOpLoggerFactoryAdapter` used, what means there won't be any logs emitted anywhere. For more details please refer to the [Common.Logging library documentation](http://netcommon.sourceforge.net/docs/2.1.0/reference/html/ch01.html#logging-adapters).

Below is the example showing how to configure SDK:

    MangoPayApi api = new MangoPayApi();

    // configure client credentails..
    api.Config.ClientId = "your-client-id";
    api.Config.ClientPassword = "your-client-api-key";
    api.Config.BaseUrl = "https://api.sandbox.mangopay.com";
	
	// ..and optionally, set the logger you want (here, the console logger is used)
	api.Config.LoggerFactoryAdapter = new Common.Logging.Simple.ConsoleOutLoggerFactoryAdapter();

    // now you're ready to call API methods, i.e.:
    var users = api.Users.GetAll();


Sample usage (get, update and save an entity)
-------------------------------------------------

    MangoPayApi api = new MangoPayApi();

    // configuration
    api.Config.ClientId = "your-client-id";
    api.Config.ClientPassword = "your-client-api-key";

    // get some Natural user
    UserNaturalDTO user = api.Users.GetNatural(someUserId);

	// create update entity
	UserNaturalPutDTO userPut = new UserNaturalPutDTO
    {
        Tag = user.Tag,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName + " - CHANGED",
        Address = user.Address,
        Birthday = user.Birthday,
        Nationality = user.Nationality,
        CountryOfResidence = user.CountryOfResidence,
        Occupation = user.Occupation,
        IncomeRange = user.IncomeRange
    };
	
	// save updated user
	UserNaturalDTO userSaved = api.Users.UpdateNatural(userPut, user.Id);

	// get his bank accounts
    Pagination pagination = new Pagination(2, 10); // get 2nd page, 10 items per page
    ListPaginated<BankAccountDTO> accounts = api.Users.GetBankAccounts(user.Id, pagination);
	
    // get all users (with pagination)
    Pagination pagination = new Pagination(1, 8); // get 1st page, 8 items per page
    ListPaginated<UserDTO> users = api.Users.GetAll(pagination);

	
