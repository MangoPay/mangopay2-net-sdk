﻿
namespace MangoPay.SDK.Core.Enumerations
{
    /// <summary>Method key enumeration.</summary>
    public enum MethodKey
    {
        AuthenticationBase,
        AuthenticationOAuth,
        ApplePayinsDirectCreate,
        CardGet,
        CardSave,
        CardRegistrationCreate,
        CardRegistrationGet,
        CardRegistrationSave,
        CardPreauthorizations,
        CardValidation,
        GetCardValidation,
        CardTransactions,
        CardByFingerprintGet,
        EventsAll,
        GooglePayinsDirectCreate,
        GooglePayinsDirectCreateV2,
        HooksAll,
        HooksCreate,
        HooksGet,
        HooksSave,
        PayinsPayPalCreate,
        PayinsPayPalWebCreate,
        PayinsBankwireDirectCreate,
        PayinsPayconiqWebCreate,
        PayinsPayconiqV2WebCreate,
        PayinsCardDirectCreate,
        PayinsCardWebCreate,
        PayinsCardWebGetCardData,
        PayinsCreateRefunds,
        PayinsGetRefunds,
        PayinsGet,
        PayinsPreauthorizedDirectCreate,
        PayinsDirectDebitCreate,
        PayinsMandateDirectDebitCreate,
        PayinsRecurringRegistration,
        PayinsGetRecurringRegistration,
        PayinsPutRecurringRegistration,
        PayinsRecurringCardDirect,
        PayinsMbwayWebCreate,
        PayinsMultibancoWebCreate,
        PayinsSatispayWebCreate,
        PayinsBlikWebCreate,
        PayinsKlarnaWebCreate,
        PayinsSwishWebCreate,
        PayinsIdealWebCreate,
        PayinsGiropayWebCreate,
        PayinsBancontactWebCreate,
        GetPaymentMethodMetadata,
        PayoutsBankwireCreate,
        PayoutsBankwireGet,
        PayoutsGet,
        PayoutsGetRefunds,
        PayoutsEligibility,
        PreauthorizationCreate,
        PreauthorizationGet,
        PreauthorizationSave,
        PreauthorizationTransactionsGet,
        RefundsGet,
        TransfersCreate,
        TransfersCreateRefunds,
        TransfersGet,
        TransfersGetRefunds,
        UsersAll,
        UsersAllBankAccount,
        UsersAllCards,
        UsersAllTransactions,
        UsersAllWallets,
        UsersPreauthorizations,
        UsersCreateBankAccountsCa,
        UsersCreateBankAccountsGb,
        UsersCreateBankAccountsIban,
        UsersCreateBankAccountsOther,
        UsersCreateBankAccountsUs,
        UsersCreateKycDocument,
        UsersCreateKycPage,
        UsersCreateLegals,
        UsersCreateNaturals,
        UsersGet,
        UsersGetBankAccount,
        UsersGetKycDocument,
        UsersGetKycDocuments,
        UsersGetLegals,
        UsersGetNaturals,
        UsersSaveKycDocument,
        UsersSaveLegals,
        UsersSaveNaturals,
        UsersSaveBankAccount,
        UsersEmoneyGet,
        UsersEmoneyYearGet,
        UsersEmoneyYearMonthGet,
        UsersEmoneyMonthGet,
        UsersRegulatory,
        WalletsAllTransactions,
        WalletsCreate,
        WalletsGet,
        WalletsSave,
        BankingAliasCreateIban,
        BankingAliasAll,
        BankingAliasGet,
        BankingAliasSave,

        ClientGetKycDocuments,
        GetKycDocument,
		KycDocumentConsult,

		ClientGetWalletsDefault,
        ClientGetWalletsFees,
        ClientGetWalletsCredit,
        ClientGetWalletsDefaultWithCurrency,
        ClientGetWalletsFeesWithCurrency,
        ClientGetWalletsCreditWithCurrency,
        ClientGetTransactions,
        ClientGetWalletTransactions,
        ClientCreateBankwireDirect,
        ClientGet,
        ClientSave,
        ClientUploadLogo,
        ClientBankAccount,
        ClientPayout,
        
        DisputesGet,
        DisputesSaveTag,
        DisputesSaveContestFunds,
        DisputeSaveClose,
        DisputesGetTransactions,        
        DisputesGetAll,
        DisputesGetForWallet,
        DisputesGetForUser,
        DisputesGetPendingSettlement,
        DisputesDocumentCreate,
        DisputesDocumentPageCreate,
        DisputesDocumentSubmit,
        DisputesDocumentGet,
        DisputesDocumentGetForDispute,
        DisputesDocumentGetForClient,
        DisputesRepudiationGet,
        DisputesRepudiationGetRefunds,
        DisputesRepudiationCreateSettlement,
		DisputesDocumentConsult,
		SettlementsGet,

        IdempotencyResponseGet,

        MandateCreate,
        MandateGet,
        MandateCancel,
        MandatesGetAll,
		MandatesGetTransactions,
        MandatesGetForUser,
        MandatesGetForBankAccount,

        ReportRequest,
        ReportGetAll,
        ReportGet,

        SingleSignOnAll,
        SingleSignOnCreate,
        SingleSignOnGet,
        SingleSignOnSave,
        SingleSignOnExtendInvitation,

        PermissionGroupAll,
        PermissionGroupAllSsos,
        PermissionGroupCreate,
        PermissionGroupGet,
        PermissionGroupSave,

		SingleSignOnsMe,
		SingleSignOnsMePermissionGroup,

		UboDeclarationCreate,
		UboDeclarationUpdate,
		UboDeclarationsGet,
		UboDeclarationGet,
        UboDeclarationGetById,
        UboGet,
		UboCreate,
		UboUpdate,

		BankAccountsGetTransactions,
		
		CountryAuthorizationGet,
		CountryAuthorizationGetAll,
		
		DepositsCreate,
		DepositsGet,
		DepositsCancel,
		PayInsCreateCardPreAuthorizedDeposit,
		
		GetConversionRate,
		CreateInstantConversion,
		CreateQuotedConversion,
		GetConversion,
		CreateConversionQuote,
		GetConversionQuote,
		PayInsAddTrackingInformation,
		
		VirtualAccountCreate,
		VirtualAccountGet,
		VirtualAccountGetAll, 
		VirtualAccountDeactivate,
		VirtualAccountGetAvailabilities,
		
		IdentityVerificationCreate,
		IdentityVerificationGet,
		IdentityVerificationGetChecks
    }
}
