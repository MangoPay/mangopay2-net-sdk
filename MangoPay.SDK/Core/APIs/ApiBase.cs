using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MangoPay.SDK.Core.Enumerations;
using MangoPay.SDK.Entities;
using MangoPay.SDK.Entities.GET;

namespace MangoPay.SDK.Core.APIs
{
    /// <summary>Base class for all Api classes.</summary>
    public abstract class ApiBase
    {
        /// <summary>Root/parent instance that holds the OAuthToken and Configuration instance.</summary>
        protected MangoPayApi Root;

        /// <summary>Array with REST URL and request type.</summary>
        private readonly Dictionary<MethodKey, ApiEndPoint> _methods = new Dictionary<MethodKey, ApiEndPoint>
        {
            { MethodKey.AuthenticationBase, new ApiEndPoint("/clients/", RequestType.POST)},
            { MethodKey.AuthenticationOAuth, new ApiEndPoint("/oauth/token", RequestType.POST)},

            { MethodKey.EventsAll, new ApiEndPoint("/events", RequestType.GET)},

            { MethodKey.HooksCreate, new ApiEndPoint("/hooks", RequestType.POST)},
            { MethodKey.HooksAll, new ApiEndPoint("/hooks", RequestType.GET)},
            { MethodKey.HooksGet, new ApiEndPoint("/hooks/{0}", RequestType.GET)},
            { MethodKey.HooksSave, new ApiEndPoint("/hooks/{0}", RequestType.PUT)},

            { MethodKey.CardRegistrationCreate, new ApiEndPoint("/cardregistrations", RequestType.POST)},
            { MethodKey.CardRegistrationGet, new ApiEndPoint("/cardregistrations/{0}", RequestType.GET)},
            { MethodKey.CardRegistrationSave, new ApiEndPoint("/cardregistrations/{0}", RequestType.PUT)},
            { MethodKey.CardByFingerprintGet, new ApiEndPoint("/cards/fingerprints/{0}", RequestType.GET)},

            { MethodKey.PreauthorizationCreate, new ApiEndPoint("/preauthorizations/card/direct", RequestType.POST)},
            { MethodKey.PreauthorizationGet, new ApiEndPoint("/preauthorizations/{0}", RequestType.GET)},
            { MethodKey.PreauthorizationSave, new ApiEndPoint("/preauthorizations/{0}", RequestType.PUT)},
            { MethodKey.PreauthorizationTransactionsGet, new ApiEndPoint("/preauthorizations/{0}/transactions", RequestType.GET)},

            { MethodKey.CardGet, new ApiEndPoint("/cards/{0}", RequestType.GET)},
            { MethodKey.CardSave, new ApiEndPoint("/cards/{0}", RequestType.PUT)},
            { MethodKey.CardPreauthorizations, new ApiEndPoint("/cards/{0}/preauthorizations", RequestType.GET)},
            { MethodKey.CardTransactions, new ApiEndPoint("/cards/{0}/transactions", RequestType.GET)},
            { MethodKey.CardValidation, new ApiEndPoint("/cards/{0}/validation", RequestType.POST) },
            { MethodKey.GetCardValidation, new ApiEndPoint("/cards/{0}/validation/{1}", RequestType.GET) },

            { MethodKey.PayinsCardWebCreate, new ApiEndPoint("/payins/card/web", RequestType.POST)},
            { MethodKey.PayinsCardWebGetCardData, new ApiEndPoint("/payins/card/web/{0}/extended/", RequestType.GET)},
            { MethodKey.PayinsCardDirectCreate, new ApiEndPoint("/payins/card/direct", RequestType.POST)},
            { MethodKey.PayinsGet, new ApiEndPoint("/payins/{0}", RequestType.GET)},
            { MethodKey.PayinsCreateRefunds, new ApiEndPoint("/payins/{0}/refunds", RequestType.POST)},
            { MethodKey.PayinsGetRefunds, new ApiEndPoint("/payins/{0}/refunds", RequestType.GET)},

            { MethodKey.PayinsPayPalCreate, new ApiEndPoint("/payins/paypal/web", RequestType.POST)},
            { MethodKey.PayinsPayPalWebCreate, new ApiEndPoint("/payins/payment-methods/paypal", RequestType.POST)},

            { MethodKey.PayinsPreauthorizedDirectCreate, new ApiEndPoint("/payins/preauthorized/direct", RequestType.POST)},

            { MethodKey.PayinsBankwireDirectCreate, new ApiEndPoint("/payins/bankwire/direct", RequestType.POST)},

            { MethodKey.PayinsPayconiqWebCreate, new ApiEndPoint("/payins/payconiq/web", RequestType.POST)},
            { MethodKey.PayinsPayconiqV2WebCreate, new ApiEndPoint("/payins/payment-methods/payconiq", RequestType.POST)},

            { MethodKey.PayinsDirectDebitCreate, new ApiEndPoint("/payins/directdebit/web", RequestType.POST)},
            { MethodKey.PayinsMandateDirectDebitCreate, new ApiEndPoint("/payins/directdebit/direct", RequestType.POST)},
            
            { MethodKey.ApplePayinsDirectCreate, new ApiEndPoint("/payins/applepay/direct", RequestType.POST)},
            { MethodKey.GooglePayinsDirectCreate, new ApiEndPoint("/payins/googlepay/direct", RequestType.POST)},
            { MethodKey.GooglePayinsDirectCreateV2, new ApiEndPoint("/payins/payment-methods/googlepay", RequestType.POST)},
            
            { MethodKey.PayinsMbwayWebCreate, new ApiEndPoint("/payins/payment-methods/mbway", RequestType.POST)},
            { MethodKey.PayinsMultibancoWebCreate, new ApiEndPoint("/payins/payment-methods/multibanco", RequestType.POST)},
            { MethodKey.PayinsSatispayWebCreate, new ApiEndPoint("/payins/payment-methods/satispay", RequestType.POST)},
            { MethodKey.PayinsBlikWebCreate, new ApiEndPoint("/payins/payment-methods/blik", RequestType.POST)},
            { MethodKey.PayinsKlarnaWebCreate, new ApiEndPoint("/payins/payment-methods/klarna", RequestType.POST)},
            { MethodKey.PayinsIdealWebCreate, new ApiEndPoint("/payins/payment-methods/ideal", RequestType.POST)},
            { MethodKey.PayinsGiropayWebCreate, new ApiEndPoint("/payins/payment-methods/giropay", RequestType.POST)},
            { MethodKey.PayinsBancontactWebCreate, new ApiEndPoint("/payins/payment-methods/bancontact", RequestType.POST)},
            { MethodKey.PayinsSwishWebCreate, new ApiEndPoint("/payins/payment-methods/swish", RequestType.POST)},
            
            { MethodKey.PayinsRecurringRegistration, new ApiEndPoint("/recurringpayinregistrations", RequestType.POST)},
            { MethodKey.PayinsGetRecurringRegistration, new ApiEndPoint("/recurringpayinregistrations/{0}", RequestType.GET)},
            { MethodKey.PayinsPutRecurringRegistration, new ApiEndPoint("/recurringpayinregistrations/{0}", RequestType.PUT)},
            { MethodKey.PayinsRecurringCardDirect, new ApiEndPoint("/payins/recurring/card/direct", RequestType.POST) },
            
            { MethodKey.PayInsCreateCardPreAuthorizedDeposit,new ApiEndPoint("/payins/deposit-preauthorized/direct/full-capture",RequestType.POST)},
            { MethodKey.PayInsAddTrackingInformation,new ApiEndPoint("/payins/{0}/trackings",RequestType.PUT)},
            
            { MethodKey.GetPaymentMethodMetadata, new ApiEndPoint("/payment-methods/metadata", RequestType.POST) },

            { MethodKey.PayoutsBankwireCreate, new ApiEndPoint("/payouts/bankwire", RequestType.POST)},
            { MethodKey.PayoutsBankwireGet, new ApiEndPoint("/payouts/bankwire/{0}", RequestType.GET)},
            { MethodKey.PayoutsGet, new ApiEndPoint("/payouts/{0}", RequestType.GET)},
            { MethodKey.PayoutsGetRefunds, new ApiEndPoint("/payouts/{0}/refunds", RequestType.GET)},
            { MethodKey.PayoutsEligibility, new ApiEndPoint("/payouts/reachability/", RequestType.POST)},

            { MethodKey.RefundsGet, new ApiEndPoint("/refunds/{0}", RequestType.GET)},

            { MethodKey.TransfersCreate, new ApiEndPoint("/transfers", RequestType.POST)},
            { MethodKey.TransfersGet, new ApiEndPoint("/transfers/{0}", RequestType.GET)},
            { MethodKey.TransfersGetRefunds, new ApiEndPoint("/transfers/{0}/refunds", RequestType.GET)},
            { MethodKey.TransfersCreateRefunds, new ApiEndPoint("/transfers/{0}/refunds", RequestType.POST)},

            { MethodKey.UsersAll, new ApiEndPoint("/users", RequestType.GET)},
            { MethodKey.UsersAllWallets, new ApiEndPoint("/users/{0}/wallets", RequestType.GET)},
            { MethodKey.UsersAllBankAccount, new ApiEndPoint("/users/{0}/bankaccounts", RequestType.GET)},
            { MethodKey.UsersAllCards, new ApiEndPoint("/users/{0}/cards", RequestType.GET)},
            { MethodKey.UsersAllTransactions, new ApiEndPoint("/users/{0}/transactions", RequestType.GET)},
            { MethodKey.UsersCreateNaturals, new ApiEndPoint("/users/natural", RequestType.POST)},
            { MethodKey.UsersCreateLegals, new ApiEndPoint("/users/legal", RequestType.POST)},
            { MethodKey.UsersCreateKycDocument, new ApiEndPoint("/users/{0}/KYC/documents", RequestType.POST)},
            { MethodKey.UsersCreateKycPage, new ApiEndPoint("/users/{0}/KYC/documents/{1}/pages", RequestType.POST)},
            { MethodKey.UsersCreateBankAccountsIban, new ApiEndPoint("/users/{0}/bankaccounts/iban", RequestType.POST)},
            { MethodKey.UsersCreateBankAccountsGb, new ApiEndPoint("/users/{0}/bankaccounts/gb", RequestType.POST)},
            { MethodKey.UsersCreateBankAccountsUs, new ApiEndPoint("/users/{0}/bankaccounts/us", RequestType.POST)},
            { MethodKey.UsersCreateBankAccountsCa, new ApiEndPoint("/users/{0}/bankaccounts/ca", RequestType.POST)},
            { MethodKey.UsersCreateBankAccountsOther, new ApiEndPoint("/users/{0}/bankaccounts/other", RequestType.POST)},
            { MethodKey.UsersGet, new ApiEndPoint("/users/{0}", RequestType.GET)},
            { MethodKey.UsersGetNaturals, new ApiEndPoint("/users/natural/{0}", RequestType.GET)},
            { MethodKey.UsersGetLegals, new ApiEndPoint("/users/legal/{0}", RequestType.GET)},
            { MethodKey.UsersPreauthorizations, new ApiEndPoint("/users/{0}/preauthorizations", RequestType.GET)},
            { MethodKey.UsersRegulatory, new ApiEndPoint("/users/{0}/Regulatory", RequestType.GET)},

            { MethodKey.UsersGetKycDocument, new ApiEndPoint("/users/{0}/KYC/documents/{1}", RequestType.GET)},
            { MethodKey.UsersGetKycDocuments, new ApiEndPoint("/users/{0}/KYC/documents", RequestType.GET)},
            { MethodKey.UsersGetBankAccount, new ApiEndPoint("/users/{0}/bankaccounts/{1}", RequestType.GET)},
            { MethodKey.UsersSaveBankAccount, new ApiEndPoint("/users/{0}/bankaccounts/{1}", RequestType.PUT)},
            { MethodKey.UsersSaveNaturals, new ApiEndPoint("/users/natural/{0}", RequestType.PUT)},
            { MethodKey.UsersSaveLegals, new ApiEndPoint("/users/legal/{0}", RequestType.PUT)},
            { MethodKey.UsersSaveKycDocument, new ApiEndPoint("/users/{0}/KYC/documents/{1}", RequestType.PUT)},
            { MethodKey.UsersEmoneyGet, new ApiEndPoint("/users/{0}/emoney", RequestType.GET)},
            { MethodKey.UsersEmoneyYearGet, new ApiEndPoint("/users/{0}/emoney/{1}", RequestType.GET)},
            { MethodKey.UsersEmoneyYearMonthGet, new ApiEndPoint("/users/{0}/emoney/{1}/{2}", RequestType.GET)},
            { MethodKey.UsersEmoneyMonthGet, new ApiEndPoint("/users/{0}/emoney/{1}/{2}", RequestType.GET)},


            { MethodKey.WalletsCreate, new ApiEndPoint("/wallets", RequestType.POST)},
            { MethodKey.WalletsAllTransactions, new ApiEndPoint("/wallets/{0}/transactions", RequestType.GET)},
            { MethodKey.WalletsGet, new ApiEndPoint("/wallets/{0}", RequestType.GET)},
            { MethodKey.WalletsSave, new ApiEndPoint("/wallets/{0}", RequestType.PUT)},
            { MethodKey.BankingAliasCreateIban, new ApiEndPoint("/wallets/{0}/bankingaliases/iban", RequestType.POST)},
            { MethodKey.BankingAliasAll, new ApiEndPoint("/wallets/{0}/bankingaliases", RequestType.GET)},
            { MethodKey.BankingAliasGet, new ApiEndPoint("/bankingaliases/{0}", RequestType.GET)},
            { MethodKey.BankingAliasSave, new ApiEndPoint("/bankingaliases/{0}", RequestType.PUT)},

            { MethodKey.ClientGetKycDocuments, new ApiEndPoint("/KYC/documents", RequestType.GET)},
            { MethodKey.GetKycDocument, new ApiEndPoint("/KYC/documents/{0}", RequestType.GET)},
            { MethodKey.KycDocumentConsult, new ApiEndPoint("/KYC/documents/{0}/consult", RequestType.POST)},

            { MethodKey.ClientGetWalletsDefault, new ApiEndPoint("/clients/wallets", RequestType.GET)},
            { MethodKey.ClientGetWalletsFees, new ApiEndPoint("/clients/wallets/fees", RequestType.GET)},
            { MethodKey.ClientGetWalletsCredit, new ApiEndPoint("/clients/wallets/credit", RequestType.GET)},
            { MethodKey.ClientGetWalletsDefaultWithCurrency, new ApiEndPoint("/clients/wallets/{0}", RequestType.GET)},
            { MethodKey.ClientGetWalletsFeesWithCurrency, new ApiEndPoint("/clients/wallets/fees/{0}", RequestType.GET)},
            { MethodKey.ClientGetWalletsCreditWithCurrency, new ApiEndPoint("/clients/wallets/credit/{0}", RequestType.GET)},

            { MethodKey.ClientGetTransactions, new ApiEndPoint("/clients/transactions", RequestType.GET)},
            { MethodKey.ClientGetWalletTransactions, new ApiEndPoint("/clients/wallets/{0}/{1}/transactions", RequestType.GET)},
            { MethodKey.ClientCreateBankwireDirect, new ApiEndPoint("/clients/payins/bankwire/direct", RequestType.POST)},
            { MethodKey.ClientGet, new ApiEndPoint("/clients", RequestType.GET)},
            { MethodKey.ClientSave, new ApiEndPoint("/clients", RequestType.PUT)},
            { MethodKey.ClientUploadLogo, new ApiEndPoint("/clients/logo", RequestType.PUT)},
            { MethodKey.ClientBankAccount, new ApiEndPoint("/clients/bankaccounts/iban", RequestType.POST)},
            { MethodKey.ClientPayout, new ApiEndPoint("/clients/payouts", RequestType.POST)},

            { MethodKey.DisputesGet, new ApiEndPoint("/disputes/{0}", RequestType.GET)},
            { MethodKey.DisputesSaveTag, new ApiEndPoint("/disputes/{0}", RequestType.PUT)},
            { MethodKey.DisputesSaveContestFunds, new ApiEndPoint("/disputes/{0}/submit", RequestType.PUT)},
            { MethodKey.DisputeSaveClose, new ApiEndPoint("/disputes/{0}/close", RequestType.PUT)},
            { MethodKey.DisputesGetPendingSettlement, new ApiEndPoint("/disputes/pendingsettlement", RequestType.GET)},

            { MethodKey.DisputesGetTransactions, new ApiEndPoint("/disputes/{0}/transactions", RequestType.GET)},

            { MethodKey.DisputesGetAll, new ApiEndPoint("/disputes", RequestType.GET)},
            { MethodKey.DisputesGetForWallet, new ApiEndPoint("/wallets/{0}/disputes", RequestType.GET)},
            { MethodKey.DisputesGetForUser, new ApiEndPoint("/users/{0}/disputes", RequestType.GET)},

            { MethodKey.DisputesDocumentCreate, new ApiEndPoint("/disputes/{0}/documents", RequestType.POST)},
            { MethodKey.DisputesDocumentPageCreate, new ApiEndPoint("/disputes/{0}/documents/{1}/pages", RequestType.POST)},
            { MethodKey.DisputesDocumentSubmit, new ApiEndPoint("/disputes/{0}/documents/{1}", RequestType.PUT)},
            { MethodKey.DisputesDocumentGet, new ApiEndPoint("/dispute-documents/{0}", RequestType.GET)},
            { MethodKey.DisputesDocumentGetForDispute, new ApiEndPoint("/disputes/{0}/documents", RequestType.GET)},
            { MethodKey.DisputesDocumentGetForClient, new ApiEndPoint("/dispute-documents", RequestType.GET)},
            { MethodKey.DisputesDocumentConsult, new ApiEndPoint("/dispute-documents/{0}/consult", RequestType.POST)},

            { MethodKey.DisputesRepudiationGet, new ApiEndPoint("/repudiations/{0}", RequestType.GET)},
            { MethodKey.DisputesRepudiationGetRefunds, new ApiEndPoint("/repudiations/{0}/refunds", RequestType.GET)},

            { MethodKey.DisputesRepudiationCreateSettlement, new ApiEndPoint("/repudiations/{0}/settlementtransfer", RequestType.POST)},
            { MethodKey.SettlementsGet, new ApiEndPoint("/settlements/{0}/", RequestType.GET)},

            { MethodKey.IdempotencyResponseGet, new ApiEndPoint("/responses/{0}/", RequestType.GET)},

            { MethodKey.MandateCreate, new ApiEndPoint("/mandates/directdebit/web", RequestType.POST)},
            { MethodKey.MandateCancel, new ApiEndPoint("/mandates/{0}/cancel/", RequestType.PUT)},
            { MethodKey.MandateGet, new ApiEndPoint("/mandates/{0}/", RequestType.GET)},
            { MethodKey.MandatesGetAll, new ApiEndPoint("/mandates/", RequestType.GET)},
            { MethodKey.MandatesGetTransactions, new ApiEndPoint("/mandates/{0}/transactions", RequestType.GET)},
            { MethodKey.MandatesGetForUser, new ApiEndPoint("/users/{0}/mandates/", RequestType.GET)},
            { MethodKey.MandatesGetForBankAccount, new ApiEndPoint("/users/{0}/bankaccounts/{1}/mandates/", RequestType.GET)},

            { MethodKey.ReportRequest, new ApiEndPoint("/reports/{0}", RequestType.POST)},
            { MethodKey.ReportGetAll, new ApiEndPoint("/reports", RequestType.GET)},
            { MethodKey.ReportGet, new ApiEndPoint("/reports/{0}", RequestType.GET)},

            { MethodKey.SingleSignOnAll, new ApiEndPoint("/clients/ssos", RequestType.GET)},
            { MethodKey.SingleSignOnCreate, new ApiEndPoint("/clients/ssos", RequestType.POST)},
            { MethodKey.SingleSignOnGet, new ApiEndPoint("/clients/ssos/{0}", RequestType.GET)},
            { MethodKey.SingleSignOnSave, new ApiEndPoint("/clients/ssos/{0}", RequestType.PUT)},
            { MethodKey.SingleSignOnExtendInvitation, new ApiEndPoint("/clients/ssos/{0}/extendinvitation", RequestType.PUT)},

            { MethodKey.PermissionGroupAll, new ApiEndPoint("/clients/permissiongroups", RequestType.GET)},
            { MethodKey.PermissionGroupAllSsos, new ApiEndPoint("/clients/permissiongroups/{0}/SSOs", RequestType.GET)},
            { MethodKey.PermissionGroupCreate, new ApiEndPoint("/clients/permissiongroups", RequestType.POST)},
            { MethodKey.PermissionGroupGet, new ApiEndPoint("/clients/permissiongroups/{0}", RequestType.GET)},
            { MethodKey.PermissionGroupSave, new ApiEndPoint("/clients/permissiongroups/{0}", RequestType.PUT)},

            { MethodKey.SingleSignOnsMe, new ApiEndPoint("/ssos/me", RequestType.GET, false)},
            { MethodKey.SingleSignOnsMePermissionGroup , new ApiEndPoint("/ssos/me/permissiongroup", RequestType.GET, false)},
            
            { MethodKey.UboDeclarationCreate, new ApiEndPoint("/users/{0}/kyc/ubodeclarations", RequestType.POST)},
            { MethodKey.UboDeclarationUpdate, new ApiEndPoint("/users/{0}/kyc/ubodeclarations/{1}", RequestType.PUT)},
            { MethodKey.UboDeclarationsGet, new ApiEndPoint("/users/{0}/kyc/ubodeclarations",RequestType.GET)},
            { MethodKey.UboDeclarationGet, new ApiEndPoint("/users/{0}/kyc/ubodeclarations/{1}",RequestType.GET)},
            { MethodKey.UboDeclarationGetById, new ApiEndPoint("/kyc/ubodeclarations/{0}",RequestType.GET)},
            { MethodKey.UboGet,new ApiEndPoint("/users/{0}/kyc/ubodeclarations/{1}/ubos/{2}",RequestType.GET)},
            { MethodKey.UboCreate,new ApiEndPoint("/users/{0}/kyc/ubodeclarations/{1}/ubos",RequestType.POST)},
            { MethodKey.UboUpdate,new ApiEndPoint("/users/{0}/kyc/ubodeclarations/{1}/ubos/{2}",RequestType.PUT) },
            
            { MethodKey.BankAccountsGetTransactions, new ApiEndPoint("/bankaccounts/{0}/transactions", RequestType.GET)},
            
            { MethodKey.CountryAuthorizationGet,new ApiEndPoint("/countries/{0}/authorizations",RequestType.GET)},
            { MethodKey.CountryAuthorizationGetAll,new ApiEndPoint("/countries/authorizations",RequestType.GET)},
            
            { MethodKey.DepositsCreate,new ApiEndPoint("/deposit-preauthorizations/card/direct",RequestType.POST)},
            { MethodKey.DepositsGet,new ApiEndPoint("/deposit-preauthorizations/{0}",RequestType.GET)},
            { MethodKey.DepositsCancel,new ApiEndPoint("/deposit-preauthorizations/{0}",RequestType.PUT)},
            
            { MethodKey.GetConversionRate,new ApiEndPoint("/conversions/rate/{0}/{1}",RequestType.GET)},
            { MethodKey.CreateInstantConversion,new ApiEndPoint("/conversions/instant-conversion",RequestType.POST)},
            { MethodKey.CreateQuotedConversion, new ApiEndPoint("/conversions/quoted-conversion", RequestType.POST) },
            { MethodKey.GetConversion,new ApiEndPoint("/conversions/{0}",RequestType.GET)},
            { MethodKey.CreateConversionQuote,new ApiEndPoint("/conversions/quote",RequestType.POST)},
            { MethodKey.GetConversionQuote, new ApiEndPoint("/conversions/quote/{0}", RequestType.GET) },
            
            { MethodKey.VirtualAccountCreate, new ApiEndPoint("/wallets/{0}/virtual-accounts", RequestType.POST) },
            { MethodKey.VirtualAccountDeactivate, new ApiEndPoint("/wallets/{0}/virtual-accounts/{1}", RequestType.PUT) },
            { MethodKey.VirtualAccountGet, new ApiEndPoint("/wallets/{0}/virtual-accounts/{1}", RequestType.GET) },
            { MethodKey.VirtualAccountGetAll, new ApiEndPoint("/wallets/{0}/virtual-accounts", RequestType.GET) },
            { MethodKey.VirtualAccountGetAvailabilities, new ApiEndPoint("/virtual-accounts/availability", RequestType.GET) },
            
            { MethodKey.IdentityVerificationCreate, new ApiEndPoint("/users/{0}/identity-verifications", RequestType.POST) },
            { MethodKey.IdentityVerificationGet, new ApiEndPoint("/identity-verifications/{0}", RequestType.GET) },
            { MethodKey.IdentityVerificationGetChecks, new ApiEndPoint("/identity-verifications/{0}/checks", RequestType.GET) }
        };

        /// <summary>Creates new API instance.</summary>
        /// <param name="root">Root/parent instance that holds the OAuthToken and Configuration instance.</param>
        protected ApiBase(MangoPayApi root)
        {
            Root = root;
        }

        /// <summary>Gets an instance of <see cref="ApiEndPoint"/> for given method key</summary>
        /// <param name="key">The method key to get the end point details for</param>
        /// <returns></returns>
        protected ApiEndPoint GetApiEndPoint(MethodKey key)
        {
            if (!_methods.ContainsKey(key))
            {
                throw new Exception("Unknown method key: " + key);
            }

            return (ApiEndPoint)_methods[key].Clone();
        }

        /// <summary>Creates the DTO instance.</summary>
        /// <typeparam name="U">Return type.</typeparam>
        /// <typeparam name="T">Type on behalf of which the request is being called.</typeparam>
        /// <param name="methodKey">Relevant method key.</param>
        /// <param name="entity">DTO instance that is going to be sent.</param>
        /// <param name="idempotentKey">Idempotent key for this request.</param>
        /// <param name="entitiesId">Entity identifier.</param>
        /// <returns>The DTO instance returned from API.</returns>
        protected async Task<U> CreateObjectAsync<U, T>(MethodKey methodKey, T entity, string idempotentKey = null, params string[] entitiesId)
            where U : EntityBase, new()
            where T : EntityPostBase
        {
            var endPoint = GetApiEndPoint(methodKey);
            endPoint.SetParameters(entitiesId);

            var restTool = new RestTool(this.Root, true);
            return await restTool.RequestAsync<U, T>(endPoint, null, entity, idempotentKey: idempotentKey);
        }

        /// <summary>Gets the DTO instance from API.</summary>
        /// <typeparam name="T">Type on behalf of which the request is being called.</typeparam>
        /// <param name="methodKey">Relevant method key.</param>
        /// <param name="idempotentKey">Idempotent key for this request.</param>
        /// <param name="entitiesId">Entities identifier.</param>
        /// <returns>The DTO instance returned from API.</returns>
        protected async Task<T> GetObjectAsync<T>(MethodKey methodKey,
            Dictionary<string, string> additionalUrlParams,
            params string[] entitiesId)
            where T: EntityBase, new()
        {
            var endPoint = GetApiEndPoint(methodKey);
            endPoint.SetParameters(entitiesId);
            endPoint.IncludeClientId = true;

            return await GetObjectAsync<T>(endPoint, additionalUrlParams);
        }
        
        private async Task<T> GetObjectAsync<T>(ApiEndPoint endPoint,
            Dictionary<string, string> additionalUrlParams)
            where T: EntityBase, new()
        {
            var rest = new RestTool(this.Root, true);
            return await rest.RequestAsync<T, T>(endPoint, null, null, 
                additionalUrlParams: additionalUrlParams);
        }

        protected async Task<T> GetObjectAsync<T>(MethodKey methodKey,
            params string[] entitiesId)
            where T : EntityBase, new()
        {
            return await GetObjectAsync<T>(methodKey, additionalUrlParams: null, entitiesId);
        }
        
        protected async Task<T> GetObjectAsyncNoClientId<T>(MethodKey methodKey,
            params string[] entitiesId)
            where T : EntityBase, new()
        {
            var endPoint = GetApiEndPoint(methodKey);
            endPoint.SetParameters(entitiesId);
            endPoint.IncludeClientId = false;

            return await GetObjectAsync<T>(endPoint, null);
        }

        /// <summary>Gets the collection of Dto instances from API.</summary>
        /// <typeparam name="T">Type on behalf of which the request is being called.</typeparam>
        /// <param name="methodKey">Relevant method key.</param>
        /// <param name="pagination">Pagination object.</param>
        /// <param name="idempotentKey">Idempotent key for this request.</param>
        /// <param name="entitiesId">Entities identifier.</param>
        /// <param name="sort">Sort.</param>
        /// <param name="additionalUrlParams">Collection of key-value pairs of request parameters.</param>
        /// <returns>Collection of Dto instances returned from API.</returns>
        protected async Task<ListPaginated<T>> GetListAsync<T>(MethodKey methodKey, Pagination pagination = null, Sort sort = null, 
            Dictionary<string, string> additionalUrlParams = null, string idempotentKey = null, params string[] entitiesId)
            where T : EntityBase, new()
        {
            var endPoint = GetApiEndPoint(methodKey);
            endPoint.SetParameters(entitiesId);
            endPoint.IncludeClientId = true;

            return await GetListAsync<T>(endPoint, pagination, sort, additionalUrlParams, idempotentKey);
        }

        protected async Task<ListPaginated<T>> GetListAsyncNoClientId<T>(MethodKey methodKey, Pagination pagination = null, Sort sort = null, 
            Dictionary<string, string> additionalUrlParams = null, string idempotentKey = null, params string[] entitiesId)
            where T : EntityBase, new()
        {
            var endPoint = GetApiEndPoint(methodKey);
            endPoint.SetParameters(entitiesId);
            endPoint.IncludeClientId = false;

            return await GetListAsync<T>(endPoint, pagination, sort, additionalUrlParams, idempotentKey);
        }
        
        private async Task<ListPaginated<T>> GetListAsync<T>(ApiEndPoint endPoint, Pagination pagination = null,
            Sort sort = null,
            Dictionary<string, string> additionalUrlParams = null, string idempotentKey = null)
            where T : EntityBase, new()
        {
            if (sort != null && sort.IsSet)
            {
                if (additionalUrlParams == null)
                    additionalUrlParams = new Dictionary<string, string>();

                additionalUrlParams.Add(Constants.SORT_URL_PARAMETER_NAME, sort.GetFields());
            }

            var restTool = new RestTool(this.Root, true);

            return await restTool.RequestListAsync<T>(endPoint, null, additionalUrlParams, pagination, idempotentKey);
        }

        /// <summary>Saves the Dto instance.</summary>
        /// <typeparam name="U">Return type.</typeparam>
        /// <typeparam name="T">Type on behalf of which the request is being called.</typeparam>
        /// <param name="methodKey">Relevant method key.</param>
        /// <param name="entity">Dto instance that is going to be sent.</param>
        /// <param name="idempotentKey">Idempotent key for this request.</param>
        /// <param name="entitiesId">Entities identifier.</param>
        /// <returns>The Dto instance returned from API.</returns>
        protected async Task<U> UpdateObjectAsync<U, T>(MethodKey methodKey, T entity, string idempotentKey = null, params string[] entitiesId)
            where U : EntityBase, new()
            where T : EntityPutBase
        {
            var endPoint = GetApiEndPoint(methodKey);
            endPoint.SetParameters(entitiesId);

            var restTool = new RestTool(this.Root, true);
            return await restTool.RequestAsync<U, T>(endPoint, null, entity, idempotentKey: idempotentKey);
        }

        protected Type GetObjectForIdempotencyUrl()
        {
            return typeof(UserNaturalDTO);
        }
    }
}
