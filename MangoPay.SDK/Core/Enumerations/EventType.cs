﻿
using System;

namespace MangoPay.SDK.Core.Enumerations
{
    /// <summary>Event types enumeration.</summary>
    public enum EventType
    {
        All,

        PAYIN_NORMAL_CREATED,
        PAYIN_NORMAL_SUCCEEDED,	
        PAYIN_NORMAL_FAILED,
        PAYOUT_NORMAL_CREATED,	
        PAYOUT_NORMAL_SUCCEEDED,
        PAYOUT_NORMAL_FAILED,
        TRANSFER_NORMAL_CREATED,
        TRANSFER_NORMAL_SUCCEEDED,
        TRANSFER_NORMAL_FAILED,
        PAYIN_REFUND_CREATED,
        PAYIN_REFUND_SUCCEEDED,
        PAYIN_REFUND_FAILED,
        PAYOUT_REFUND_CREATED,	
        PAYOUT_REFUND_SUCCEEDED,
        PAYOUT_REFUND_FAILED,
        TRANSFER_REFUND_CREATED,
        TRANSFER_REFUND_SUCCEEDED,
        TRANSFER_REFUND_FAILED,
        KYC_CREATED,
        KYC_VALIDATION_ASKED,
        KYC_SUCCEEDED,
        KYC_FAILED,
        PAYIN_REPUDIATION_CREATED, 
        PAYIN_REPUDIATION_SUCCEEDED, 
        PAYIN_REPUDIATION_FAILED,
        DISPUTE_DOCUMENT_CREATED, 
        DISPUTE_DOCUMENT_VALIDATION_ASKED, 
        DISPUTE_DOCUMENT_SUCCEEDED,
        DISPUTE_DOCUMENT_FAILED,
        DISPUTE_CREATED,
        DISPUTE_SUBMITTED,
        DISPUTE_ACTION_REQUIRED,
        DISPUTE_FURTHER_ACTION_REQUIRED, 
        DISPUTE_CLOSED,
        DISPUTE_SENT_TO_BANK,
        TRANSFER_SETTLEMENT_CREATED,
        TRANSFER_SETTLEMENT_SUCCEEDED,
        TRANSFER_SETTLEMENT_FAILED,
        MANDATE_CREATED,	
        MANDATE_FAILED,
        MANDATE_ACTIVATED,
        MANDATE_SUBMITTED,
        PREAUTHORIZATION_PAYMENT_WAITING,
        PREAUTHORIZATION_PAYMENT_EXPIRED,
        PREAUTHORIZATION_PAYMENT_CANCELED,
        PREAUTHORIZATION_PAYMENT_VALIDATED,
        UBO_DECLARATION_CREATED,
        UBO_DECLARATION_VALIDATION_ASKED,
        UBO_DECLARATION_REFUSED,
        UBO_DECLARATION_VALIDATED,
        UBO_DECLARATION_INCOMPLETE,
        MANDATE_EXPIRED,
        USER_KYC_REGULAR,
        KYC_OUTDATED,
        USER_KYC_LIGHT,
        USER_INFLOWS_BLOCKED,
        USER_INFLOWS_UNBLOCKED,
        USER_OUTFLOWS_BLOCKED,
        USER_OUTFLOWS_UNBLOCKED,
        PREAUTHORIZATION_CREATED,
        PREAUTHORIZATION_SUCCEEDED,
        PREAUTHORIZATION_FAILED,

        INSTANT_PAYOUT_SUCCEEDED,
        INSTANT_PAYOUT_FALLBACKED,
        INSTANT_PAYOUT_FAILED,

        // RecurringRegistration
        RECURRING_REGISTRATION_CREATED,
        RECURRING_REGISTRATION_AUTH_NEEDED,
        RECURRING_REGISTRATION_IN_PROGRESS,
        RECURRING_REGISTRATION_ENDED,
        
        COUNTRY_AUTHORIZATION_UPDATED,
        
        DEPOSIT_PREAUTHORIZATION_CREATED,
        DEPOSIT_PREAUTHORIZATION_FAILED,
        DEPOSIT_PREAUTHORIZATION_PAYMENT_WAITING,
        DEPOSIT_PREAUTHORIZATION_PAYMENT_EXPIRED,
        DEPOSIT_PREAUTHORIZATION_PAYMENT_CANCEL_REQUESTED,
        DEPOSIT_PREAUTHORIZATION_PAYMENT_CANCELED,
        DEPOSIT_PREAUTHORIZATION_PAYMENT_VALIDATED,
        DEPOSIT_PREAUTHORIZATION_PAYMENT_NO_SHOW_REQUESTED,
        DEPOSIT_PREAUTHORIZATION_PAYMENT_NO_SHOW,
        DEPOSIT_PREAUTHORIZATION_PAYMENT_TO_BE_COMPLETED,
        DEPOSIT_PREAUTHORIZATION_PAYMENT_FAILED,
        
        CARD_VALIDATION_CREATED,
        CARD_VALIDATION_FAILED,
        CARD_VALIDATION_SUCCEEDED,
        
        LEGAL_COMPANY_NUMBER_VALIDATION_SUCCEEDED,
        LEGAL_COMPANY_NUMBER_VALIDATION_FAILED
    }
}
