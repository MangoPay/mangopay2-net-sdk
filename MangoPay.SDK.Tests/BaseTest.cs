using Common.Logging.Simple;
using MangoPay.SDK.Core;
using MangoPay.SDK.Core.Enumerations;
using MangoPay.SDK.Entities;
using MangoPay.SDK.Entities.GET;
using MangoPay.SDK.Entities.POST;
using MangoPay.SDK.Entities.PUT;
using NUnit.Framework;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MangoPay.SDK.Tests
{
    /// <summary>Base abstract class for tests.</summary>
    public abstract class BaseTest
    {
        /// <summary>The MangoPayApi instance.</summary>
        protected MangoPayApi Api;

        private static UserNaturalDTO _john;
        private static UserLegalDTO _matrix;
        private static BankAccountIbanDTO _johnsAccount;
        private static WalletDTO _johnsWallet;
        private static WalletDTO _johnsWalletWithMoney;
        private static PayInCardWebDTO _johnsPayInCardWeb;
        private static PayOutBankWireDTO _johnsPayOutBankWire;
        private static CardRegistrationDTO _johnsCardRegistration;
        private static KycDocumentDTO _johnsKycDocument;
        private static PayOutBankWireDTO _johnsPayOutForCardDirect;
        private static HookDTO _johnsHook;
        private static Dictionary<ReportType, ReportRequestDTO> _johnsReports;

        protected BaseTest()
        {
            this.Api = BuildNewMangoPayApi();
            _johnsReports = new Dictionary<ReportType, ReportRequestDTO>();
        }

        protected static UserLegalOwnerPostDTO CreateUserLegalPost()
        {
            var user = new UserLegalOwnerPostDTO
            {
                Name = "MartixSampleOrg",
                LegalPersonType = LegalPersonType.BUSINESS,
                UserCategory = UserCategory.PAYER,
                TermsAndConditionsAccepted = true,
                LegalRepresentativeFirstName = "JohnUbo",
                LegalRepresentativeLastName = "DoeUbo",
                LegalRepresentativeCountryOfResidence = CountryIso.PL,
                LegalRepresentativeNationality = CountryIso.PL,
                HeadquartersAddress = new Address
                {
                    AddressLine1 = "Address line ubo 1",
                    AddressLine2 = "Address line ubo 2",
                    City = "CityUbo",
                    Country = CountryIso.PL,
                    PostalCode = "11222",
                    Region = "RegionUbo"
                },
                LegalRepresentativeAddress = new Address
                {
                    AddressLine1 = "Address line ubo 1",
                    AddressLine2 = "Address line ubo 2",
                    City = "CityUbo",
                    Country = CountryIso.PL,
                    PostalCode = "11222",
                    Region = "RegionUbo"
                },
                LegalRepresentativeEmail = "john.doe@sample.org",
                LegalRepresentativeBirthday = new DateTime(1975, 12, 21, 0, 0, 0),
                Email = "john.doe@sample.org",
                CompanyNumber = "LU72HN11"
            };

            return user;
        }

        protected MangoPayApi BuildNewMangoPayApi()
        {
            var api = new MangoPayApi
            {
                Config =
                {
                    // use test client credentails
                    ClientId = "sdk-unit-tests",
                    ClientPassword = "cqFfFrWfCcb7UadHNxx2C9Lo6Djw8ZduLi7J9USTmu8bhxxpju",
                    BaseUrl = "https://api.sandbox.mangopay.com",
                    ApiVersion = "v2.01",
                    Timeout = 60000 // increase timeout because sandbox API takes longer than 15s to reply for transactions

                }
            };

            // register storage strategy for tests
            api.OAuthTokenManager.RegisterCustomStorageStrategy(new DefaultStorageStrategyForTests());

            return api;
        }

        protected async Task<UserNaturalDTO> GetJohn(bool recreate = false)
        {
            if (BaseTest._john != null && !recreate) return BaseTest._john;

            var user = new UserNaturalOwnerPostDTO
            {
                Email = "john.doe@sample.org",
                FirstName = "John",
                LastName = "Doe",
                Birthday = new DateTime(1975, 12, 21, 0, 0, 0),
                Nationality = CountryIso.FR,
                CountryOfResidence = CountryIso.FR,
                Occupation = "programmer",
                IncomeRange = 3,
                Address = new Address { AddressLine1 = "Address line 1", AddressLine2 = "Address line 2", City = "City", Country = CountryIso.PL, PostalCode = "11222", Region = "Region" }
            };

            BaseTest._john = await this.Api.Users.CreateOwnerAsync(user);

            BaseTest._johnsWallet = null;
            return BaseTest._john;
        }

        protected async Task<UserNaturalDTO> GetNewJohn(bool termsAccepted = false)
        {
            var user = new UserNaturalOwnerPostDTO
            {
                Email = "john.doe@sample.org",
                FirstName = "John",
                LastName = "Doe",
                Birthday = new DateTime(1975, 12, 21, 0, 0, 0),
                Nationality = CountryIso.FR,
                CountryOfResidence = CountryIso.FR,
                Occupation = "programmer",
                IncomeRange = 3,
                Address = new Address { AddressLine1 = "Address line 1", AddressLine2 = "Address line 2", City = "City", Country = CountryIso.PL, PostalCode = "11222", Region = "Region" },
                TermsAndConditionsAccepted = termsAccepted
            };

            return await this.Api.Users.CreateOwnerAsync(user);
        }

        protected async Task<UserLegalDTO> GetMatrix(bool termsAccepted = false, bool newJohn = false)
        {
            if (BaseTest._matrix != null && !newJohn) return BaseTest._matrix;

            var user = new UserLegalOwnerPostDTO
            {
                Name = "MartixSampleOrg",
                LegalPersonType = LegalPersonType.BUSINESS,
                UserCategory = UserCategory.OWNER,
                TermsAndConditionsAccepted = termsAccepted,
                LegalRepresentativeFirstName = "JohnUbo",
                LegalRepresentativeLastName = "DoeUbo",
                LegalRepresentativeCountryOfResidence = CountryIso.PL,
                LegalRepresentativeNationality = CountryIso.PL,
                HeadquartersAddress = new Address
                {
                    AddressLine1 = "Address line ubo 1",
                    AddressLine2 = "Address line ubo 2",
                    City = "CityUbo",
                    Country = CountryIso.PL,
                    PostalCode = "11222",
                    Region = "RegionUbo"
                },
                LegalRepresentativeAddress = new Address
                {
                    AddressLine1 = "Address line ubo 1",
                    AddressLine2 = "Address line ubo 2",
                    City = "CityUbo",
                    Country = CountryIso.PL,
                    PostalCode = "11222",
                    Region = "RegionUbo"
                },
                LegalRepresentativeEmail = "john.doe@sample.org",
                LegalRepresentativeBirthday = new DateTime(1975, 12, 21, 0, 0, 0),
                Email = "john.doe@sample.org",
                CompanyNumber = "LU72HN11"
            };

            BaseTest._matrix = await this.Api.Users.CreateOwnerAsync(user);

            return BaseTest._matrix;
        }

        protected async Task<BankAccountIbanDTO> GetJohnsAccount(bool recreate = false)
        {
            if (BaseTest._johnsAccount != null && !recreate) return BaseTest._johnsAccount;

            var john = await this.GetJohn();
            var account = new BankAccountIbanPostDTO(john.FirstName + " " + john.LastName, john.Address, "FR7630004000031234567890143")
            {
                UserId = john.Id,
                BIC = "BNPAFRPP"
            };
            BaseTest._johnsAccount =  await this.Api.Users.CreateBankAccountIbanAsync(john.Id, account);

            return BaseTest._johnsAccount;
        }

        protected async Task<WalletDTO> GetJohnsWallet()
        {
            if (BaseTest._johnsWallet != null) return BaseTest._johnsWallet;

            var john = await this.GetJohn();

            var wallet = new WalletPostDTO(new List<string> { john.Id }, "WALLET IN EUR", CurrencyIso.EUR);

            BaseTest._johnsWallet = await this.Api.Wallets.CreateAsync(wallet);

            return BaseTest._johnsWallet;
        }


        protected async Task<WalletDTO> CreateJohnsWallet()
        {
            var john = await this.GetJohn();

            var wallet = new WalletPostDTO(new List<string> { john.Id }, "WALLET IN EUR", CurrencyIso.EUR);

            return await Api.Wallets.CreateAsync(wallet);
        }

        /// <summary>Creates wallet for John, loaded with 10k EUR (John's got lucky) if not created yet, or returns an existing one.</summary>
        /// <returns>Wallet instance loaded with 10k EUR.</returns>
        protected async Task<WalletDTO> GetJohnsWalletWithMoney()
        {
            return await GetJohnsWalletWithMoney(200);
        }

        /// <summary>Creates wallet for John, if not created yet, or returns an existing one.</summary>
        /// <param name="amount">Initial wallet's money amount.</param>
        /// <returns>Wallet entity instance returned from API.</returns>
        protected async Task<WalletDTO> GetJohnsWalletWithMoney(int amount)
        {
            if (BaseTest._johnsWalletWithMoney == null)
                BaseTest._johnsWalletWithMoney = await GetNewJohnsWalletWithMoney(amount);

            return BaseTest._johnsWalletWithMoney;
        }

        /// <summary>Creates new wallet for John.</summary>
        /// <param name="amount">Initial wallet's money amount.</param>
        /// <returns>Wallet entity instance returned from API.</returns>
        protected async Task<WalletDTO> GetNewJohnsWalletWithMoney(int amount, UserNaturalDTO user = null)
        {
            var john = user ?? await this.GetJohn();

            // create wallet with money
            var wallet = new WalletPostDTO(new List<string> { john.Id }, "WALLET IN EUR WITH MONEY", CurrencyIso.EUR);

            var johnsWalletWithMoney = await this.Api.Wallets.CreateAsync(wallet);

            var cardRegistrationPost = new CardRegistrationPostDTO(johnsWalletWithMoney.Owners[0], CurrencyIso.EUR);
            var cardRegistration = await this.Api.CardRegistrations.CreateAsync(cardRegistrationPost);

            var cardRegistrationPut = new CardRegistrationPutDTO
            {
                RegistrationData = await this.GetPaylineCorrectRegistartionData(cardRegistration)
            };
            cardRegistration = await this.Api.CardRegistrations.UpdateAsync(cardRegistrationPut, cardRegistration.Id);

            var card = await this.Api.Cards.GetAsync(cardRegistration.CardId);

            // create pay-in CARD DIRECT
            var payIn = new PayInCardDirectPostDTO(cardRegistration.UserId, cardRegistration.UserId,
                new Money { Amount = amount, Currency = CurrencyIso.EUR }, new Money { Amount = 0, Currency = CurrencyIso.EUR },
                johnsWalletWithMoney.Id, "http://test.com", card.Id)
            {
                CardType = card.CardType
            };

            payIn.BrowserInfo = getBrowserInfo();
            payIn.IpAddress = "2001:0620:0000:0000:0211:24FF:FE80:C12C";

            // create Pay-In
            var result = await this.Api.PayIns.CreateCardDirectAsync(payIn);

            return await this.Api.Wallets.GetAsync(johnsWalletWithMoney.Id);
        }

        protected async Task<Tuple<string, WalletDTO>> GetNewJohnsWalletWithMoneyAndCardId(int amount, UserNaturalDTO user = null)
        {
            var john = user ?? await this.GetJohn();

            // create wallet with money
            var wallet = new WalletPostDTO(new List<string> { john.Id }, "WALLET IN EUR WITH MONEY", CurrencyIso.EUR);

            var johnsWalletWithMoney = await this.Api.Wallets.CreateAsync(wallet);

            var cardRegistrationPost = new CardRegistrationPostDTO(johnsWalletWithMoney.Owners[0], CurrencyIso.EUR);
            var cardRegistration = await this.Api.CardRegistrations.CreateAsync(cardRegistrationPost);

            var cardRegistrationPut = new CardRegistrationPutDTO
            {
                RegistrationData = await this.GetPaylineCorrectRegistartionData(cardRegistration, true)
            };
            cardRegistration = await this.Api.CardRegistrations.UpdateAsync(cardRegistrationPut, cardRegistration.Id);

            var card = await this.Api.Cards.GetAsync(cardRegistration.CardId);

            // create pay-in CARD DIRECT
            var payIn = new PayInCardDirectPostDTO(cardRegistration.UserId, cardRegistration.UserId,
                new Money {Amount = amount, Currency = CurrencyIso.EUR},
                new Money {Amount = 0, Currency = CurrencyIso.EUR},
                johnsWalletWithMoney.Id, "http://www.my-site.com/returnurl", card.Id)
            {
                CardType = card.CardType,
                IpAddress = "2001:0620:0000:0000:0211:24FF:FE80:C12C",
                Requested3DSVersion = "V2_1",
                BrowserInfo = getBrowserInfo()
            };


            // create Pay-In
            await this.Api.PayIns.CreateCardDirectAsync(payIn);

            var createdWallet = await this.Api.Wallets.GetAsync(johnsWalletWithMoney.Id);

            return new Tuple<string, WalletDTO>(card.Id, createdWallet);
        }

        protected async Task<PayInCardWebDTO> GetJohnsPayInCardWeb()
        {
            if (BaseTest._johnsPayInCardWeb != null) return BaseTest._johnsPayInCardWeb;

            var wallet = await this.GetJohnsWallet();
            var user = await this.GetJohn();

            var payIn = new PayInCardWebPostDTO(user.Id, new Money { Amount = 1000, Currency = CurrencyIso.EUR }, new Money { Amount = 0, Currency = CurrencyIso.EUR }, wallet.Id, "https://test.com", CultureCode.FR, CardType.CB_VISA_MASTERCARD);

            BaseTest._johnsPayInCardWeb = await this.Api.PayIns.CreateCardWebAsync(payIn);

            return BaseTest._johnsPayInCardWeb;
        }

        protected async Task<PayInCardWebDTO> GetJohnsNewPayInCardWeb()
        {
            BaseTest._johnsPayInCardWeb = null;

            return await GetJohnsPayInCardWeb();
        }

        protected async Task<PayInCardWebDTO> GetJohnsPayInCardWeb(string walletId)
        {
            if (BaseTest._johnsPayInCardWeb != null) return BaseTest._johnsPayInCardWeb;

            var user = await this.GetJohn();

            var payIn = new PayInCardWebPostDTO(user.Id, new Money { Amount = 1000, Currency = CurrencyIso.EUR }, new Money { Amount = 0, Currency = CurrencyIso.EUR }, walletId, "https://test.com", CultureCode.FR, CardType.CB_VISA_MASTERCARD)
            {
                //Add TemplateURLOptionsCard for tests
                TemplateURLOptionsCard = new TemplateURLOptionsCard { PAYLINEV2 = "https://www.maysite.com/payline_template/" }
            };

            BaseTest._johnsPayInCardWeb = await this.Api.PayIns.CreateCardWebAsync(payIn);

            return BaseTest._johnsPayInCardWeb;
        }

        protected async Task<PayInCardWebDTO> CreateJohnsPayInCardWeb(string walletId)
        {
            var user = await this.GetJohn();

            var payIn = new PayInCardWebPostDTO(user.Id, new Money { Amount = 1000, Currency = CurrencyIso.EUR }, new Money { Amount = 0, Currency = CurrencyIso.EUR }, walletId, "https://test.com", CultureCode.FR, CardType.CB_VISA_MASTERCARD);

            return await this.Api.PayIns.CreateCardWebAsync(payIn);
        }

        protected async Task<PayInCardWebDTO> GetNewPayInCardWeb()
        {
            var wallet = await this.GetJohnsWallet();
            var user = await this.GetJohn();

            var payIn = new PayInCardWebPostDTO(user.Id, new Money { Amount = 1000, Currency = CurrencyIso.EUR }, new Money { Amount = 0, Currency = CurrencyIso.EUR }, wallet.Id, "https://test.com", CultureCode.FR, CardType.CB_VISA_MASTERCARD);

            BaseTest._johnsPayInCardWeb = await this.Api.PayIns.CreateCardWebAsync(payIn);

            return BaseTest._johnsPayInCardWeb;
        }

        protected async Task<PayInCardDirectDTO> GetNewPayInCardDirect()
        {
            return await GetNewPayInCardDirect(null);
        }
        
        protected async Task<PayInMbwayWebDTO> GetNewPayInMbwayWeb()
        {
            PayInMbwayWebPostDTO payIn = await GetPayInMbwayWebPost();
            return await this.Api.PayIns.CreateMbwayWebAsync(payIn);
        }

        protected async Task<PayInGooglePayDirectDTO> GetNewPayInGooglePayDirect()
        {
            PayInGooglePayDirectPostDTO payIn = await GetPayInGooglePayDirectV2Post();
            return await Api.PayIns.CreateGooglePayDirectV2Async(payIn);
        }

        protected async Task<PayInMultibancoWebDTO> GetNewPayInMultibancoWeb()
        {
            PayInMultibancoWebPostDTO payIn = await GetPayInMultibancoWebPost();
            return await this.Api.PayIns.CreateMultibancoWebAsync(payIn);
        }

        protected async Task<PayInSatispayWebDTO> GetNewPayInSatispayWeb()
        {
            PayInSatispayWebPostDTO payIn = await GetPayInSatispayWebPost();
            return await this.Api.PayIns.CreateSatispayWebAsync(payIn);
        }

        protected async Task<PayInBlikWebDTO> GetNewPayInBlikWeb()
        {
            PayInBlikWebPostDTO payIn = await GetPayInBlikWebPost();
            return await this.Api.PayIns.CreateBlikWebAsync(payIn);
        }
        
        protected async Task<PayInKlarnaWebDTO> GetNewPayInKlarnaWeb()
        {
            PayInKlarnaWebPostDTO payIn = await GetPayInKlarnaWebPost();
            return await this.Api.PayIns.CreateKlarnaWebAsync(payIn);
        }
        
        protected async Task<PayInIdealWebDTO> GetNewPayInIdealWeb()
        {
            PayInIdealWebPostDTO payIn = await GetPayInIdealWebPost();
            return await this.Api.PayIns.CreateIdealWebAsync(payIn);
        }
        
        protected async Task<PayInGiropayWebDTO> GetNewPayInGiropayWeb()
        {
            PayInGiropayWebPostDTO payIn = await GetPayInGiropayWebPost();
            return await this.Api.PayIns.CreateGiropayWebAsync(payIn);
        }
        
        protected async Task<PayInBancontactWebDTO> GetNewPayInBancontactWeb()
        {
            PayInBancontactWebPostDTO payIn = await GetPayInBancontactWebPost();
            return await this.Api.PayIns.CreateBancontactWebAsync(payIn);
        }
        
        protected async Task<PayInCardWebDTO> CreateLegacyIdealPayInCardWeb(string walletId)
        {
            var user = await this.GetJohn();

            var payIn = new PayInCardWebPostDTO(
                user.Id, 
                new Money { Amount = 1000, Currency = CurrencyIso.EUR }, 
                new Money { Amount = 0, Currency = CurrencyIso.EUR },
                walletId, 
                "https://test.com", 
                CultureCode.FR, 
                CardType.IDEAL,
                bic: "REVOLT21"
                );

            return await this.Api.PayIns.CreateCardWebAsync(payIn);
        }


        /// <summary>Creates PayIn Card Direct object.</summary>
        /// <param name="userId">User identifier.</param>
        /// <returns>PayIn Card Direct instance returned from API.</returns>
        protected async Task<PayInCardDirectDTO> GetNewPayInCardDirect(string userId, string idempotentKey = null)
        {
            PayInCardDirectPostDTO payIn = await GetPayInCardDirectPost(userId, idempotentKey);
            return await this.Api.PayIns.CreateCardDirectAsync(payIn);
        }

        /// <summary>Creates PayIn Card Direct object with billing details.</summary>
        /// <param name="userId">User identifier.</param>
        /// <returns>PayIn Card Direct instance returned from API.</returns>
        protected async Task<PayInCardDirectDTO> GetNewPayInCardDirectWithBilling(string userId = null, string idempotencyKey = null)
        {
            var address = new Address
            {
                AddressLine1 = "Test address line 1",
                AddressLine2 = "Test address line 2",
                City = "Test city",
                Country = CountryIso.RO,
                PostalCode = "65400"
            };

            var billing = new Billing
            {
                Address = address,
                FirstName = "John",
                LastName = "Doe"
            };

            var payIn = await GetPayInCardDirectPost(userId, idempotencyKey);
            payIn.Billing = billing;

            payIn.PaymentCategory = "ECommerce";

            return await this.Api.PayIns.CreateCardDirectAsync(payIn);
        }

        protected async Task<PayInCardDirectDTO> GetNewPayInCardDirectWithBillingAndShipping(string userId = null, string idempotencyKey = null)
        {
            var address = new Address
            {
                AddressLine1 = "Test address line 1",
                AddressLine2 = "Test address line 2",
                City = "Test city",
                Country = CountryIso.RO,
                PostalCode = "65400"
            };

            var billing = new Billing
            {
                Address = address,
                FirstName = "John",
                LastName = "Doe"
            };

            var shipping = new Shipping
            {
                Address = address,
                FirstName = "John",
                LastName = "Doe"
            };

            var payIn = await GetPayInCardDirectPost(userId, idempotencyKey);
            payIn.Billing = billing;
            payIn.Shipping = shipping;

            return await this.Api.PayIns.CreateCardDirectAsync(payIn);
        }

        protected async Task<PayInCardDirectPostDTO> GetPayInCardDirectPost(string userId, string idempotencyKey)
        {
            var wallet = await this.GetJohnsWalletWithMoney();

            if (userId == null)
            {
                var user = await this.GetJohn();
                userId = user.Id;
            }

            var cardRegistrationPost = new CardRegistrationPostDTO(userId, CurrencyIso.EUR);

            var cardRegistration = await this.Api.CardRegistrations.CreateAsync(cardRegistrationPost, idempotencyKey);

            var cardRegistrationPut = new CardRegistrationPutDTO
            {
                RegistrationData = await this.GetPaylineCorrectRegistartionData(cardRegistration)
            };

            cardRegistration = await this.Api.CardRegistrations.UpdateAsync(cardRegistrationPut, cardRegistration.Id);

            var card = await this.Api.Cards.GetAsync(cardRegistration.CardId);

            // create pay-in CARD DIRECT
            var payIn = new PayInCardDirectPostDTO(cardRegistration.UserId, cardRegistration.UserId,
                    new Money { Amount = 100, Currency = CurrencyIso.EUR }, new Money { Amount = 0, Currency = CurrencyIso.EUR },
                    wallet.Id, "http://test.com", card.Id)
            {
                // payment type as CARD
                CardType = card.CardType
            };

            payIn.BrowserInfo = getBrowserInfo();
            payIn.IpAddress = "2001:0620:0000:0000:0211:24FF:FE80:C12C";

            return payIn;
        }
        
        protected async Task<PayInMbwayWebPostDTO> GetPayInMbwayWebPost()
        {
            var wallet = await this.GetJohnsWalletWithMoney();
            var user = await this.GetJohn();
            
            // create pay-in MBWAY WEB
            var payIn = new PayInMbwayWebPostDTO(
                user.Id,
                new Money { Amount = 100, Currency = CurrencyIso.EUR },
                new Money { Amount = 0, Currency = CurrencyIso.EUR },
                wallet.Id,
                "351#269458236",
                "test"
            );
            
            return payIn;
        }

        protected async Task<PayInGooglePayDirectPostDTO> GetPayInGooglePayDirectV2Post()
        {
            var wallet = await GetJohnsWalletWithMoney();
            var user = await GetJohn();
            
            var billing = new Billing();
            var address = new Address
            {
                City = "Test city",
                AddressLine1 = "Test address line 1",
                AddressLine2 = "Test address line 2",
                Country = CountryIso.RO,
                PostalCode = "65400"
            };
            billing.Address = address;
            billing.FirstName = "Joe";
            billing.LastName = "Doe";
            
            var shipping = new Shipping
            {
                Address = address,
                FirstName = "John",
                LastName = "Doe"
            };

            // create pay-in GooglePay DIRECT
            var payIn = new PayInGooglePayDirectPostDTO(
                user.Id,
                new Money { Amount = 100, Currency = CurrencyIso.EUR },
                new Money { Amount = 0, Currency = CurrencyIso.EUR },
                wallet.Id,
                "https://mangopay.com/docs/please-ignore",
                "https://mangopay.com/docs/please-ignore",
                "159.180.248.187",
                getBrowserInfo(),
                "{\"signature\":\"MEUCIQCLXOan2Y9DobLVSOeD5V64Peayvz0ZAWisdz/1iTdthAIgVFb4Hve4EhtW81k46SiMlnXLIiCn1h2+vVQGjHe+sSo\\u003d\",\"intermediateSigningKey\":{\"signedKey\":\"{\\\"keyValue\\\":\\\"MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEDGRER6R6PH6K39YTIYX+CpDNej6gQgvi/Wx19SOPtiDnkjAl4/LF9pXlvZYe+aJH0Dy095I6BlfY8bNBB5gjPg\\\\u003d\\\\u003d\\\",\\\"keyExpiration\\\":\\\"1688521049102\\\"}\",\"signatures\":[\"MEYCIQDup1B+rkiPAWmpg7RmqY0NfgdGhmdyL8wvAX+6C1aOU2QIhAIZACSDQ/ZexIyEia5KrRlG2B+y3AnKNlhRzumRcnNOR\"]},\"protocolVersion\":\"ECv2\",\"signedMessage\":\"{\\\"encryptedMessage\\\":\\\"YSSGK9yFdKP+mJB5+wAjnOujnThPM1E/KbbJxd3MDzPVI66ip1DBESldvQXYjjeLq6Rf1tKE9oLwwaj6u0/gU7Z9t3g1MoW+9YoEE1bs1IxImif7IQGAosfYjjbBBfDkOaqEs2JJC5qt6xjKO9lQ/E6JPkPFGqF7+OJ1vzmD83Pi3sHWkVge5MhxXQ3yBNhrjus3kV7zUoYA+uqNrciWcWypc1NndF/tiwSkvUTzM6n4dS8X84fkJiSO7PZ65C0yw0mdybRRnyL2fFdWGssve1zZFAvYfzcpNamyuZGGlu/SCoayitojmMsqe5Cu0efD9+WvvDr9PA+Vo1gzuz7LmiZe81SGvdFhRoq62FBAUiwSsi2A3pWinZxM2XbYNph+HJ5FCNspWhz4ur9JG4ZMLemCXuaybvL++W6PWywAtoiE0mQcBIX3vhOq5itv0RkaKVe6nbcAS2UryRz2u/nDCJLKpIv2Wi11NtCUT2mgD8F6qfcXhvVZHyeLqZ1OLgCudTTSdKirzezbgPTg4tQpW++KufeD7bgG+01XhCWt+7/ftqcSf8n//gSRINne8j2G6w+2\\\",\\\"ephemeralPublicKey\\\":\\\"BLY2+R8C0T+BSf/W3HEq305qH63IGmJxMVmbfJ6+x1V7GQg9W9v7eHc3j+8TeypVn+nRlPu98tivuMXECg+rWZs\\\\u003d\\\",\\\"tag\\\":\\\"MmEjNdLfsDNfYd/FRUjoJ4/IfLypNRqx8zgHfa6Ftmo\\\\u003d\\\"}\"}",
                SecureMode.DEFAULT,
                billing,
                shipping,
                "test"
            );

            return payIn;
        }

        protected async Task<PayInMultibancoWebPostDTO> GetPayInMultibancoWebPost()
        {
            var wallet = await GetJohnsWalletWithMoney();
            var user = await GetJohn();

            var payIn = new PayInMultibancoWebPostDTO(
                user.Id,
                new Money { Amount = 100, Currency = CurrencyIso.EUR },
                new Money { Amount = 20, Currency = CurrencyIso.EUR },
                wallet.Id,
                "http://www.my-site.com/returnURL?transactionId=wt_8362acb9-6dbc-4660-b826-b9acb9b850b1",
                "Multibanco",
                "test"
            );

            return payIn;
        }

        protected async Task<PayInSatispayWebPostDTO> GetPayInSatispayWebPost()
        {
            var wallet = await GetJohnsWalletWithMoney();
            var user = await GetJohn();

            var payIn = new PayInSatispayWebPostDTO(
                user.Id,
                new Money { Amount = 100, Currency = CurrencyIso.EUR },
                new Money { Amount = 20, Currency = CurrencyIso.EUR },
                wallet.Id,
                "http://www.my-site.com/returnURL?transactionId=wt_71a08458-b0cc-468d-98f7-1302591fc238",
                "IT",
                "Satispay",
                "Satispay test"
            );

            return payIn;
        }

        protected async Task<PayInBlikWebPostDTO> GetPayInBlikWebPost()
        {
            var user = await GetJohn();
            
            // create wallet with money
            var wallet = new WalletPostDTO(new List<string> { user.Id }, "WALLET IN PLN WITH MONEY", CurrencyIso.PLN);
            var johnsWalletWithMoney = await this.Api.Wallets.CreateAsync(wallet);

            var payIn = new PayInBlikWebPostDTO(
                user.Id,
                new Money { Amount = 100, Currency = CurrencyIso.PLN },
                new Money { Amount = 20, Currency = CurrencyIso.PLN },
                johnsWalletWithMoney.Id,
                "https://example.com",
                "Blik",
                "Blik test"
            );

            return payIn;
        }
        
        protected async Task<PayInKlarnaWebPostDTO> GetPayInKlarnaWebPost()
        {
            var wallet = await GetJohnsWalletWithMoney();
            var user = await GetJohn();
            
            var address = new Address
            {
                AddressLine1 = "Big Street",
                AddressLine2 = "no 2 ap 6",
                City = "Lyon",
                Country = CountryIso.FR,
                PostalCode = "65400",
                Region = "Ile de France"
            };

            var billing = new Billing
            {
                Address = address,
                FirstName = "John",
                LastName = "Doe"
            };
            
            var shipping = new Shipping
            {
                Address = address,
                FirstName = "John",
                LastName = "Doe"
            };

            var payIn = new PayInKlarnaWebPostDTO(
                user.Id,
                new Money { Amount = 100, Currency = CurrencyIso.EUR },
                new Money { Amount = 20, Currency = CurrencyIso.EUR },
                wallet.Id,
                "http://www.my-site.com/returnURL?transactionId=wt_71a08458-b0cc-468d-98f7-1302591fc238",
                new List<LineItem>
                {
                    new LineItem
                    {
                        Name = "test item",
                        Quantity = 1,
                        UnitAmount = 100,
                        TaxAmount = 0,
                        Description = "seller1 ID"
                    }
                },
                "FR",
                "33#607080900",
                "mango@mangopay.com",
                "{}",
                billing,
                "afd48-879d-48fg",
                "FR",
                shipping,
                "Klarna test"
            );

            return payIn;
        }
        
        
        protected async Task<PayInIdealWebPostDTO> GetPayInIdealWebPost()
        {
            var wallet = await GetJohnsWalletWithMoney();
            var user = await GetJohn();

            var payIn = new PayInIdealWebPostDTO(
                user.Id,
                new Money { Amount = 100, Currency = CurrencyIso.EUR },
                new Money { Amount = 20, Currency = CurrencyIso.EUR },
                wallet.Id,
                "http://www.my-site.com/returnURL?transactionId=wt_71a08458-b0cc-468d-98f7-1302591fc238",
                "RBRBNL21",
                "Ideal tag",
                "Ideal test"
            );

            return payIn;
        }
        
        protected async Task<PayInGiropayWebPostDTO> GetPayInGiropayWebPost()
        {
            var wallet = await GetJohnsWalletWithMoney();
            var user = await GetJohn();

            var payIn = new PayInGiropayWebPostDTO(
                user.Id,
                new Money { Amount = 100, Currency = CurrencyIso.EUR },
                new Money { Amount = 20, Currency = CurrencyIso.EUR },
                wallet.Id,
                "http://www.my-site.com/returnURL?transactionId=wt_71a08458-b0cc-468d-98f7-1302591fc238",
                "Giropay tag",
                "test"
                
            );

            return payIn;
        }
        
        protected async Task<PayInBancontactWebPostDTO> GetPayInBancontactWebPost()
        {
            var wallet = await GetJohnsWalletWithMoney();
            var user = await GetJohn();

            var payIn = new PayInBancontactWebPostDTO(
                user.Id,
                new Money { Amount = 100, Currency = CurrencyIso.EUR },
                new Money { Amount = 20, Currency = CurrencyIso.EUR },
                wallet.Id,
                "http://www.my-site.com/returnURL?transactionId=wt_71a08458-b0cc-468d-98f7-1302591fc238",
                null,
                null,
                false,
                CultureCode.NL,
                PaymentFlow.APP
            );

            return payIn;
        }

        protected async Task<PayOutBankWireDTO> GetJohnsPayOutBankWire()
        {
            if (BaseTest._johnsPayOutBankWire != null) return BaseTest._johnsPayOutBankWire;

            var wallet = await this.GetJohnsWalletWithMoney();
            var user = await this.GetJohn();
            var account = await this.GetJohnsAccount();

            var payOut = new PayOutBankWirePostDTO(user.Id, wallet.Id, new Money { Amount = 10, Currency = CurrencyIso.EUR }, new Money { Amount = 5, Currency = CurrencyIso.EUR }, account.Id, "Johns bank wire ref", PayoutModeRequested.STANDARD)
                {
                    Tag = "DefaultTag",
                    CreditedUserId = user.Id
                };

            BaseTest._johnsPayOutBankWire = await this.Api.PayOuts.CreateBankWireAsync(payOut);

            return BaseTest._johnsPayOutBankWire;
        }

        /// <summary>Creates PayOut Bank Wire object.</summary>
        /// <returns>PayOut Bank Wire instance returned from API.</returns>
        protected async Task<PayOutBankWireDTO> GetJohnsPayOutForCardDirect()
        {
            if (BaseTest._johnsPayOutForCardDirect != null) return BaseTest._johnsPayOutForCardDirect;

            var payIn = await this.GetNewPayInCardDirect();
            var account = await this.GetJohnsAccount();

            var payOut = new PayOutBankWirePostDTO(payIn.AuthorId, payIn.CreditedWalletId, new Money { Amount = 10, Currency = CurrencyIso.EUR },
                new Money { Amount = 5, Currency = CurrencyIso.EUR }, account.Id, "Johns bank wire ref", PayoutModeRequested.STANDARD)
            {
                Tag = "DefaultTag",
                CreditedUserId = payIn.AuthorId
            };

            BaseTest._johnsPayOutForCardDirect = await this.Api.PayOuts.CreateBankWireAsync(payOut);

            return BaseTest._johnsPayOutForCardDirect;
        }

        protected async Task<PayOutBankWireDTO> GetJohnsPayoutBankwire()
        {
            if (_johnsPayOutBankWire != null) 
                return _johnsPayOutBankWire;

            var payIn = await this.GetNewPayInCardDirect();
            var account = await this.GetJohnsAccount();

            var payOut = new PayOutBankWirePostDTO(payIn.AuthorId, payIn.CreditedWalletId, new Money { Amount = 10, Currency = CurrencyIso.EUR },
                new Money { Amount = 5, Currency = CurrencyIso.EUR }, account.Id, "Johns bank wire ref", PayoutModeRequested.STANDARD)
            {
                Tag = "DefaultTag",
                CreditedUserId = payIn.AuthorId
            };

            return await this.Api.PayOuts.CreateBankWireAsync(payOut);
        }

        protected async Task<TransferDTO> GetNewTransfer(WalletDTO walletIn = null)
        {
            var walletWithMoney = walletIn ?? await this.GetJohnsWalletWithMoney();

            var user = await this.GetJohn();
            var walletPost = new WalletPostDTO(new List<string> { user.Id }, "WALLET IN EUR FOR TRANSFER", CurrencyIso.EUR);
            var wallet = await this.Api.Wallets.CreateAsync(walletPost);

            var transfer = new TransferPostDTO(user.Id, null, new Money { Amount = 100, Currency = CurrencyIso.EUR }, new Money { Amount = 0, Currency = CurrencyIso.EUR }, walletWithMoney.Id, wallet.Id)
                {
                    Tag = "DefaultTag"
                };

            return await this.Api.Transfers.CreateAsync(transfer);
        }

        /// <summary>Creates refund object for transfer.</summary>
        /// <param name="transfer">Transfer.</param>
        /// <returns>Refund instance returned from API.</returns>
        protected async Task<RefundDTO> GetNewRefundForTransfer(TransferDTO transfer)
        {
            var user = await this.GetJohn();
            var debitedFunds = new Money
            {
                Amount = transfer.DebitedFunds.Amount,
                Currency = transfer.DebitedFunds.Currency
            };
            var fees = new Money
            {
                Amount = transfer.Fees.Amount,
                Currency = transfer.Fees.Currency
            };

            var refund = new RefundTransferPostDTO(user.Id, fees, debitedFunds);

            return await this.Api.Transfers.CreateRefundAsync(transfer.Id, refund);
        }

        /// <summary>Creates partial refund object for Tranfert.</summary>
        /// <param name="transfer">Tranfert entity.</param>
        /// <returns>Refund instance returned from API.</returns>
        protected async Task<RefundDTO> GetPartialRefundForPayIn(TransferDTO transfer)
        {
            var user = await this.GetJohn();

            var debitedFunds = new Money
            {
                Amount = 100,
                Currency = transfer.DebitedFunds.Currency
            };
            var fees = new Money
            {
                Amount = 10,
                Currency = transfer.Fees.Currency
            };

            var refund = new RefundTransferPostDTO(user.Id, fees, debitedFunds);
            return await this.Api.Transfers.CreateRefundAsync(transfer.Id, refund);
        }

        /// <summary>Creates refund object for PayIn.</summary>
        /// <param name="payIn">PayIn entity.</param>
        /// <returns>Refund instance returned from API.</returns>
        protected async Task<RefundDTO> GetNewRefundForPayIn(PayInDTO payIn, string idempotencyKey = null)
        {
            var user = await this.GetJohn();

            var debitedFunds = new Money
            {
                Amount = payIn.DebitedFunds.Amount,
                Currency = payIn.DebitedFunds.Currency
            };
            var fees = new Money
            {
                Amount = payIn.Fees.Amount,
                Currency = payIn.Fees.Currency
            };

            var refund = new RefundPayInPostDTO(user.Id, fees, debitedFunds);
            return await this.Api.PayIns.CreateRefundAsync(payIn.Id, refund, idempotentKey: idempotencyKey);
        }
        
        /// <summary>Creates partial refund object for PayIn.</summary>
        /// <param name="payIn">PayIn entity.</param>
        /// <returns>Refund instance returned from API.</returns>
        protected async Task<RefundDTO> GetPartialRefundForPayIn(PayInDTO payIn, string idempotencyKey = null)
        {
            var user = await this.GetJohn();

            var debitedFunds = new Money
            {
                Amount = 100,
                Currency = payIn.DebitedFunds.Currency
            };
            var fees = new Money
            {
                Amount = 10,
                Currency = payIn.Fees.Currency
            };

            var refund = new RefundPayInPostDTO(user.Id, fees, debitedFunds);
            return await this.Api.PayIns.CreateRefundAsync(payIn.Id, refund, idempotentKey: idempotencyKey);
        }

        /// <summary>Creates card registration object.</summary>
        /// <param name="cardType">Card type.</param>
        /// <returns>CardRegistration instance returned from API.</returns>
        protected async Task<CardRegistrationDTO> GetJohnsCardRegistration(CardType cardType = CardType.CB_VISA_MASTERCARD)
        {
            if (BaseTest._johnsCardRegistration != null) return BaseTest._johnsCardRegistration;

            var user = await this.GetJohn();

            var cardRegistration = new CardRegistrationPostDTO(user.Id, CurrencyIso.EUR, cardType)
            {
                Tag = "DefaultTag"
            };

            BaseTest._johnsCardRegistration = await this.Api.CardRegistrations.CreateAsync(cardRegistration);

            return BaseTest._johnsCardRegistration;
        }

        /// <summary>Creates new card registration object.</summary>
        /// <param name="cardType">Card type.</param>
        /// <returns>CardRegistration instance returned from API.</returns>
        protected async Task<CardRegistrationDTO> GetNewJohnsCardRegistration(CardType cardType = CardType.CB_VISA_MASTERCARD)
        {
            BaseTest._johnsCardRegistration = null;

            return await GetJohnsCardRegistration(cardType);
        }

        /// <summary>Creates card registration object.</summary>
        /// <returns>CardPreAuthorization instance returned from API.</returns>
        protected async Task<CardPreAuthorizationDTO> GetJohnsCardPreAuthorization(string idempotencyKey = null)
        {
            var user = await this.GetJohn();
            var cardPreAuthorization = await GetPreAuthorization(user.Id);

            return await this.Api.CardPreAuthorizations.CreateAsync(cardPreAuthorization, idempotencyKey);
        }

        protected async Task<CardPreAuthorizationPostDTO> GetPreAuthorization(string userId)
        {
            var cardRegistrationPost = new CardRegistrationPostDTO(userId, CurrencyIso.EUR);
            var newCardRegistration = await this.Api.CardRegistrations.CreateAsync(cardRegistrationPost);

            var cardRegistrationPut = new CardRegistrationPutDTO();
            var registrationData = await this.GetPaylineCorrectRegistartionData(newCardRegistration);
            cardRegistrationPut.RegistrationData = registrationData;
            var getCardRegistration = await this.Api.CardRegistrations.UpdateAsync(cardRegistrationPut, newCardRegistration.Id);

            var cardPreAuthorization = new CardPreAuthorizationPostDTO(userId, new Money { Amount = 100, Currency = CurrencyIso.EUR }, SecureMode.DEFAULT, getCardRegistration.CardId, "http://test.com");

            cardPreAuthorization.BrowserInfo = getBrowserInfo();
            cardPreAuthorization.IpAddress = "2001:0620:0000:0000:0211:24FF:FE80:C12C";

            return cardPreAuthorization;
        }

        protected async Task<KycDocumentDTO> GetJohnsKycDocument()
        {
            if (BaseTest._johnsKycDocument != null) return BaseTest._johnsKycDocument;

            var john = await this.GetJohn();

            BaseTest._johnsKycDocument = await this.Api.Users.CreateKycDocumentAsync(john.Id, KycDocumentType.IDENTITY_PROOF);

            return BaseTest._johnsKycDocument;
        }

        protected async Task<KycDocumentDTO> GetNewKycDocument()
        {
            BaseTest._johnsKycDocument = null;
            return await GetJohnsKycDocument();
        }

        /// <summary>Gets registration data from Payline service.</summary>
        /// <param name="cardRegistration">CardRegistration instance.</param>
        /// <returns>Registration data.</returns>
        protected async Task<string> GetPaylineCorrectRegistartionData(CardRegistrationDTO cardRegistration, bool is3DSecure = false)
        {
            var client = new RestClient(cardRegistration.CardRegistrationURL);

            var request = new RestRequest
            {
                Method = Method.Post
            };
            request.AddParameter("data", cardRegistration.PreregistrationData);
            request.AddParameter("accessKeyRef", cardRegistration.AccessKey);
            request.AddParameter("cardNumber",  "4970107111111119");
            request.AddParameter("cardExpirationDate", "1224");
            request.AddParameter("cardCvx", "123");

            // Payline requires TLS
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var response = await client.ExecuteAsync(request);

            var responseString = response.Content;

            if (response.StatusCode == HttpStatusCode.OK)
                return responseString;
            
            throw new Exception(responseString);
        }

        protected async Task<CardRegistrationDTO> GetCardRegistrationForDeposit()
        {
            var user = await this.GetJohn();

            var cardRegistrationPostDto = new CardRegistrationPostDTO(user.Id, CurrencyIso.EUR)
            {
                Tag = "DefaultTag"
            };

            var cardRegistration = await this.Api.CardRegistrations.CreateAsync(cardRegistrationPostDto);
            var cardRegistrationPut = new CardRegistrationPutDTO();
            var registrationData = await this.GetPaylineCorrectRegistartionData(cardRegistration);
            cardRegistrationPut.RegistrationData = registrationData;
            cardRegistrationPut.Tag = "DefaultTag - Updated";

            var getCardRegistration = await this.Api.CardRegistrations.UpdateAsync(cardRegistrationPut, cardRegistration.Id);

            return getCardRegistration;
        }

        protected FileInfo GetFileInfoOfFile(string location)
        {
            var exit = false;
            var fi = new FileInfo(location);
            var directory = Directory.GetParent(location);
            do
            {
                fi = directory.GetFiles("TestKycPageFile.png", SearchOption.AllDirectories).SingleOrDefault();
                if (fi == null)
                {
                    directory = Directory.GetParent(directory.FullName);
                }
                else
                {
                    exit = true;
                }
            } while (exit == false);

            return fi;
        }

        protected async Task<HookDTO> GetJohnsHook()
        {
            if (BaseTest._johnsHook != null) return BaseTest._johnsHook;

            var pagination = new Pagination(1, 1);
            var list = await this.Api.Hooks.GetAllAsync(pagination);

            if (list != null && list.Count > 0 && list[0] != null)
            {
                BaseTest._johnsHook = list[0];
            }
            else
            {
                var hook = new HookPostDTO("http://test.com", EventType.PAYIN_NORMAL_CREATED);
                BaseTest._johnsHook = await this.Api.Hooks.CreateAsync(hook);
            }

            return BaseTest._johnsHook;
        }

        protected async Task<ReportRequestDTO> GetJohnsReport(ReportType reportType)
        {
            if (BaseTest._johnsReports.ContainsKey(reportType)) return BaseTest._johnsReports[reportType];

            var reportPost = new ReportRequestPostDTO(ReportType.TRANSACTIONS);
            var reportRequest = await this.Api.Reports.CreateAsync(reportPost);
            BaseTest._johnsReports.Add(reportType, reportRequest);

            return BaseTest._johnsReports[reportType];
        }

        protected async Task<MandateDTO> GetNewMandate()
        {
            var john = await GetJohnsAccount();
            var bankAccountId = john.Id;
            var returnUrl = "http://test.test";
            var mandatePost = new MandatePostDTO(bankAccountId, CultureCode.EN, returnUrl);
            var mandate = await Api.Mandates.CreateAsync(mandatePost);

            return mandate;
        }

        protected void AssertEqualInputProps<T>(T entity1, T entity2)
        {
            Assert.IsNotNull(entity1);
            Assert.IsNotNull(entity2);

            if (entity1 is UserNaturalDTO && entity2 is UserNaturalDTO)
            {
                Assert.AreEqual((entity1 as UserNaturalDTO).Tag, (entity2 as UserNaturalDTO).Tag);
                Assert.AreEqual((entity1 as UserNaturalDTO).PersonType, (entity2 as UserNaturalDTO).PersonType);
                Assert.AreEqual((entity1 as UserNaturalDTO).FirstName, (entity2 as UserNaturalDTO).FirstName);
                Assert.AreEqual((entity1 as UserNaturalDTO).LastName, (entity2 as UserNaturalDTO).LastName);
                Assert.AreEqual((entity1 as UserNaturalDTO).Email, (entity2 as UserNaturalDTO).Email);
                Assert.AreEqual((entity1 as UserNaturalDTO).Address.AddressLine1, (entity2 as UserNaturalDTO).Address.AddressLine1);
                Assert.AreEqual((entity1 as UserNaturalDTO).Address.AddressLine2, (entity2 as UserNaturalDTO).Address.AddressLine2);
                Assert.AreEqual((entity1 as UserNaturalDTO).Address.City, (entity2 as UserNaturalDTO).Address.City);
                Assert.AreEqual((entity1 as UserNaturalDTO).Address.Country, (entity2 as UserNaturalDTO).Address.Country);
                Assert.AreEqual((entity1 as UserNaturalDTO).Address.PostalCode, (entity2 as UserNaturalDTO).Address.PostalCode);
                Assert.AreEqual((entity1 as UserNaturalDTO).Address.Region, (entity2 as UserNaturalDTO).Address.Region);
                Assert.AreEqual((entity1 as UserNaturalDTO).Birthday, (entity2 as UserNaturalDTO).Birthday);
                Assert.AreEqual((entity1 as UserNaturalDTO).Nationality, (entity2 as UserNaturalDTO).Nationality);
                Assert.AreEqual((entity1 as UserNaturalDTO).CountryOfResidence, (entity2 as UserNaturalDTO).CountryOfResidence);
                Assert.AreEqual((entity1 as UserNaturalDTO).Occupation, (entity2 as UserNaturalDTO).Occupation);
                Assert.AreEqual((entity1 as UserNaturalDTO).IncomeRange, (entity2 as UserNaturalDTO).IncomeRange);
            }
            else if (entity1 is UserLegalDTO && entity2 is UserLegalDTO)
            {
                Assert.AreEqual((entity1 as UserLegalDTO).Tag, (entity2 as UserLegalDTO).Tag);
                Assert.AreEqual((entity1 as UserLegalDTO).PersonType, (entity2 as UserLegalDTO).PersonType);
                Assert.AreEqual((entity1 as UserLegalDTO).Name, (entity2 as UserLegalDTO).Name);
                Assert.AreEqual((entity1 as UserLegalDTO).HeadquartersAddress.AddressLine1, (entity2 as UserLegalDTO).HeadquartersAddress.AddressLine1);
                Assert.AreEqual((entity1 as UserLegalDTO).HeadquartersAddress.AddressLine2, (entity2 as UserLegalDTO).HeadquartersAddress.AddressLine2);
                Assert.AreEqual((entity1 as UserLegalDTO).HeadquartersAddress.City, (entity2 as UserLegalDTO).HeadquartersAddress.City);
                Assert.AreEqual((entity1 as UserLegalDTO).HeadquartersAddress.Country, (entity2 as UserLegalDTO).HeadquartersAddress.Country);
                Assert.AreEqual((entity1 as UserLegalDTO).HeadquartersAddress.PostalCode, (entity2 as UserLegalDTO).HeadquartersAddress.PostalCode);
                Assert.AreEqual((entity1 as UserLegalDTO).HeadquartersAddress.Region, (entity2 as UserLegalDTO).HeadquartersAddress.Region);
                Assert.AreEqual((entity1 as UserLegalDTO).LegalRepresentativeFirstName, (entity2 as UserLegalDTO).LegalRepresentativeFirstName);
                Assert.AreEqual((entity1 as UserLegalDTO).LegalRepresentativeLastName, (entity2 as UserLegalDTO).LegalRepresentativeLastName);
                //Assert.AreEqual("***** TEMPORARY API ISSUE: RETURNED OBJECT MISSES THIS PROP AFTER CREATION *****", (entity1 as UserLegal).LegalRepresentativeAddress, (entity2 as UserLegal).LegalRepresentativeAddress);
                Assert.AreEqual((entity1 as UserLegalDTO).LegalRepresentativeEmail, (entity2 as UserLegalDTO).LegalRepresentativeEmail);
                //Assert.AreEqual("***** TEMPORARY API ISSUE: RETURNED OBJECT HAS THIS PROP CHANGED FROM TIMESTAMP INTO ISO STRING AFTER CREATION *****", (entity1 as UserLegal).LegalRepresentativeBirthday, (entity2 as UserLegal).LegalRepresentativeBirthday);
                Assert.AreEqual((entity1 as UserLegalDTO).LegalRepresentativeBirthday, (entity2 as UserLegalDTO).LegalRepresentativeBirthday);
                Assert.AreEqual((entity1 as UserLegalDTO).LegalRepresentativeNationality, (entity2 as UserLegalDTO).LegalRepresentativeNationality);
                Assert.AreEqual((entity1 as UserLegalDTO).LegalRepresentativeCountryOfResidence, (entity2 as UserLegalDTO).LegalRepresentativeCountryOfResidence);
            }
            else if (entity1 is BankAccountDTO && entity2 is BankAccountDTO)
            {
                Assert.AreEqual((entity1 as BankAccountDTO).Tag, (entity2 as BankAccountDTO).Tag);
                Assert.AreEqual((entity1 as BankAccountDTO).UserId, (entity2 as BankAccountDTO).UserId);
                Assert.AreEqual((entity1 as BankAccountDTO).Type, (entity2 as BankAccountDTO).Type);
                Assert.AreEqual((entity1 as BankAccountDTO).OwnerName, (entity2 as BankAccountDTO).OwnerName);
                Assert.AreEqual((entity1 as BankAccountDTO).OwnerAddress.AddressLine1, (entity2 as BankAccountDTO).OwnerAddress.AddressLine1);
                Assert.AreEqual((entity1 as BankAccountDTO).OwnerAddress.AddressLine2, (entity2 as BankAccountDTO).OwnerAddress.AddressLine2);
                Assert.AreEqual((entity1 as BankAccountDTO).OwnerAddress.City, (entity2 as BankAccountDTO).OwnerAddress.City);
                Assert.AreEqual((entity1 as BankAccountDTO).OwnerAddress.Country, (entity2 as BankAccountDTO).OwnerAddress.Country);
                Assert.AreEqual((entity1 as BankAccountDTO).OwnerAddress.PostalCode, (entity2 as BankAccountDTO).OwnerAddress.PostalCode);
                Assert.AreEqual((entity1 as BankAccountDTO).OwnerAddress.Region, (entity2 as BankAccountDTO).OwnerAddress.Region);
                if ((entity1 as BankAccountDTO).Type == BankAccountType.IBAN)
                {
                    Assert.AreEqual((entity1 as BankAccountIbanDTO).IBAN, (entity2 as BankAccountIbanDTO).IBAN);
                    Assert.AreEqual((entity1 as BankAccountIbanDTO).BIC, (entity2 as BankAccountIbanDTO).BIC);
                }
                else if ((entity1 as BankAccountDTO).Type == BankAccountType.GB)
                {
                    Assert.AreEqual((entity1 as BankAccountGbDTO).AccountNumber, (entity2 as BankAccountGbDTO).AccountNumber);
                    Assert.AreEqual((entity1 as BankAccountGbDTO).SortCode, (entity2 as BankAccountGbDTO).SortCode);
                }
                else if ((entity1 as BankAccountDTO).Type == BankAccountType.US)
                {
                    Assert.AreEqual((entity1 as BankAccountUsDTO).AccountNumber, (entity2 as BankAccountUsDTO).AccountNumber);
                    Assert.AreEqual((entity1 as BankAccountUsDTO).ABA, (entity2 as BankAccountUsDTO).ABA);
                }
                else if ((entity1 as BankAccountDTO).Type == BankAccountType.CA)
                {
                    Assert.AreEqual((entity1 as BankAccountCaDTO).AccountNumber, (entity2 as BankAccountCaDTO).AccountNumber);
                    Assert.AreEqual((entity1 as BankAccountCaDTO).BankName, (entity2 as BankAccountCaDTO).BankName);
                    Assert.AreEqual((entity1 as BankAccountCaDTO).InstitutionNumber, (entity2 as BankAccountCaDTO).InstitutionNumber);
                    Assert.AreEqual((entity1 as BankAccountCaDTO).BranchCode, (entity2 as BankAccountCaDTO).BranchCode);
                }
                else if ((entity1 as BankAccountDTO).Type == BankAccountType.OTHER)
                {
                    Assert.AreEqual((entity1 as BankAccountOtherDTO).AccountNumber, (entity2 as BankAccountOtherDTO).AccountNumber);
                    Assert.AreEqual((entity1 as BankAccountOtherDTO).Type, (entity2 as BankAccountOtherDTO).Type);
                    Assert.AreEqual((entity1 as BankAccountOtherDTO).Country, (entity2 as BankAccountOtherDTO).Country);
                    Assert.AreEqual((entity1 as BankAccountOtherDTO).BIC, (entity2 as BankAccountOtherDTO).BIC);
                }
            }
            else if (entity1 is PayInDTO && entity2 is PayInDTO)
            {
                Assert.AreEqual((entity1 as PayInDTO).Tag, (entity2 as PayInDTO).Tag);
                Assert.AreEqual((entity1 as PayInDTO).AuthorId, (entity2 as PayInDTO).AuthorId);
                Assert.AreEqual((entity1 as PayInDTO).CreditedUserId, (entity2 as PayInDTO).CreditedUserId);

                AssertEqualInputProps((entity1 as PayInDTO).DebitedFunds, (entity2 as PayInDTO).DebitedFunds);
                AssertEqualInputProps((entity1 as PayInDTO).CreditedFunds, (entity2 as PayInDTO).CreditedFunds);
                AssertEqualInputProps((entity1 as PayInDTO).Fees, (entity2 as PayInDTO).Fees);
            }
            else if (typeof(T) == typeof(CardDTO))
            {
                Assert.AreEqual((entity1 as CardDTO).ExpirationDate, (entity2 as CardDTO).ExpirationDate);
                Assert.AreEqual((entity1 as CardDTO).Alias, (entity2 as CardDTO).Alias);
                Assert.AreEqual((entity1 as CardDTO).CardType, (entity2 as CardDTO).CardType);
                Assert.AreEqual((entity1 as CardDTO).Currency, (entity2 as CardDTO).Currency);
            }
            else if (typeof(T) == typeof(PayOutDTO))
            {
                Assert.AreEqual((entity1 as PayOutDTO).Tag, (entity2 as PayOutDTO).Tag);
                Assert.AreEqual((entity1 as PayOutDTO).AuthorId, (entity2 as PayOutDTO).AuthorId);
                Assert.AreEqual((entity1 as PayOutDTO).CreditedUserId, (entity2 as PayOutDTO).CreditedUserId);

                AssertEqualInputProps((entity1 as PayOutDTO).DebitedFunds, (entity2 as PayOutDTO).DebitedFunds);
                AssertEqualInputProps((entity1 as PayOutDTO).CreditedFunds, (entity2 as PayOutDTO).CreditedFunds);
                AssertEqualInputProps((entity1 as PayOutDTO).Fees, (entity2 as PayOutDTO).Fees);
            }
            else if (typeof(T) == typeof(TransferDTO))
            {
                Assert.AreEqual((entity1 as TransferDTO).Tag, (entity2 as TransferDTO).Tag);
                Assert.AreEqual((entity1 as TransferDTO).AuthorId, (entity2 as TransferDTO).AuthorId);
                Assert.AreEqual((entity1 as TransferDTO).CreditedUserId, (entity2 as TransferDTO).CreditedUserId);

                AssertEqualInputProps((entity1 as TransferDTO).DebitedFunds, (entity2 as TransferDTO).DebitedFunds);
                AssertEqualInputProps((entity1 as TransferDTO).CreditedFunds, (entity2 as TransferDTO).CreditedFunds);
                AssertEqualInputProps((entity1 as TransferDTO).Fees, (entity2 as TransferDTO).Fees);
            }
            else if (typeof(T) == typeof(TransactionDTO))
            {
                Assert.AreEqual((entity1 as TransactionDTO).Tag, (entity2 as TransactionDTO).Tag);

                AssertEqualInputProps((entity1 as TransactionDTO).DebitedFunds, (entity2 as TransactionDTO).DebitedFunds);
                AssertEqualInputProps((entity1 as TransactionDTO).Fees, (entity2 as TransactionDTO).Fees);
                AssertEqualInputProps((entity1 as TransactionDTO).CreditedFunds, (entity2 as TransactionDTO).CreditedFunds);

                Assert.AreEqual((entity1 as TransactionDTO).Status, (entity2 as TransactionDTO).Status);
            }
            else if (typeof(T) == typeof(Money))
            {
                Assert.AreEqual((entity1 as Money).Currency, (entity2 as Money).Currency);
                Assert.AreEqual((entity1 as Money).Amount, (entity2 as Money).Amount);
            }
            else
            {
                throw new ArgumentException("Unsupported type.");
            }
        }

        protected BrowserInfo getBrowserInfo()
        {
            return new BrowserInfo
            {
                AcceptHeader = "application/json,text/javascript,*/*;q=0.01<",
                ColorDepth = 32,
                JavaEnabled = true,
                JavascriptEnabled = false,
                Language = "FR-FR",
                ScreenHeight = 1080,
                ScreenWidth = 1920,
                TimeZoneOffset = "+3600",
                UserAgent = "postman"
            };
        }
        
        protected async Task<DepositDTO> CreateNewDeposit()
        {
            var john = await this.GetJohn();
            var cardRegistration = await this.GetCardRegistrationForDeposit();
            
            var debitedFunds = new Money();
            debitedFunds.Amount = 1000;
            debitedFunds.Currency = CurrencyIso.EUR;
            
            var dto = new DepositPostDTO(
                john.Id,
                debitedFunds,
                cardRegistration.CardId,
                "https://lorem",
                "2001:0620:0000:0000:0211:24FF:FE80:C12C",
                getBrowserInfo()
            );

            return await this.Api.Deposits.CreateAsync(dto);
        }
    }
}
