﻿using MangoPay.SDK.Core;
using MangoPay.SDK.Core.Enumerations;
using MangoPay.SDK.Entities;
using MangoPay.SDK.Entities.GET;
using MangoPay.SDK.Entities.POST;
using MangoPay.SDK.Entities.PUT;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MangoPay.SDK.Tests
{
    [TestFixture]
    public class ApiCardPreAuthorizationsTest : BaseTest
    {
        [Test]
        public async Task Test_CardPreAuthorization_Create()
        {
            try
            {
                CardPreAuthorizationDTO cardPreAuthorization = await this.GetJohnsCardPreAuthorization();

                Assert.IsTrue(cardPreAuthorization.Id.Length > 0);
                Assert.AreEqual(cardPreAuthorization.Status, PreAuthorizationStatus.SUCCEEDED);
                Assert.AreEqual(cardPreAuthorization.PaymentStatus, PaymentStatus.WAITING);
                Assert.AreEqual(cardPreAuthorization.ExecutionType, PreAuthorizationExecutionType.DIRECT);
                Assert.AreEqual(cardPreAuthorization.PaymentType, PreAuthorizationPaymentType.CARD);
                Assert.IsNull(cardPreAuthorization.PayInId);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task Test_CardPreAuthorization_Create_WithBilling()
        {
            try
            {
                var john = await GetJohn();
                CardPreAuthorizationPostDTO cardPreAuthorization = await GetPreAuthorization(john.Id);
                Billing billing = new Billing();
                Address address = new Address
                {
                    City = "Test city",
                    AddressLine1 = "Test address line 1",
                    AddressLine2 = "Test address line 2",
                    Country = CountryIso.RO,
                    PostalCode = "65400"
                };
                billing.Address = address;
                cardPreAuthorization.Billing = billing;

                CardPreAuthorizationDTO cardPreAuthorizationWithBilling = await this.Api.CardPreAuthorizations.CreateAsync(cardPreAuthorization);

                Assert.IsNotNull(cardPreAuthorizationWithBilling);
                Assert.IsNotNull(cardPreAuthorizationWithBilling.Billing);
                Assert.IsNotNull(cardPreAuthorizationWithBilling.SecurityInfo);
                Assert.IsNotNull(cardPreAuthorizationWithBilling.SecurityInfo.AVSResult);
                Assert.AreEqual(cardPreAuthorizationWithBilling.SecurityInfo.AVSResult, AVSResult.NO_CHECK);

            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task Test_CardPreAuthorization_Get()
        {
            try
            {
                CardPreAuthorizationDTO cardPreAuthorization = await this.GetJohnsCardPreAuthorization();

                CardPreAuthorizationDTO getCardPreAuthorization = await this.Api.CardPreAuthorizations.GetAsync(cardPreAuthorization.Id);

                Assert.AreEqual(cardPreAuthorization.Id, getCardPreAuthorization.Id);
                Assert.AreEqual(getCardPreAuthorization.ResultCode, "000000");
                Assert.IsNotNull(getCardPreAuthorization.MultiCapture);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task Test_CardPreAuthorizationTransactions_Get()
        {
            try
            {
                var cardPreAuthorization = await this.GetJohnsCardPreAuthorization();
                var wallet = await this.GetJohnsWalletWithMoney();
                var user = await this.GetJohn();
                var payIn = new PayInPreauthorizedDirectPostDTO(user.Id,
                    new Money {Amount = 10000, Currency = CurrencyIso.EUR},
                    new Money {Amount = 0, Currency = CurrencyIso.EUR}, wallet.Id, cardPreAuthorization.Id)
                {
                    SecureModeReturnURL = "http://test.com"
                };

                await Api.PayIns.CreatePreauthorizedDirectAsync(payIn);

                var preAuthTransactions = await this.Api.CardPreAuthorizations.GetTransactionsAsync(cardPreAuthorization.Id, new Pagination(1, 10));

                Assert.NotNull(preAuthTransactions);
                Assert.NotNull(preAuthTransactions.ElementAt(0));
                Assert.AreEqual(preAuthTransactions.ElementAt(0).Status, TransactionStatus.SUCCEEDED);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task Test_CardPreAuthorization_Update()
        {
            try
            {
                CardPreAuthorizationDTO cardPreAuthorization = await this.GetJohnsCardPreAuthorization();
                CardPreAuthorizationPutDTO cardPreAuthorizationPut = new CardPreAuthorizationPutDTO
                {
                    Tag = cardPreAuthorization.Tag,
                    PaymentStatus = PaymentStatus.CANCELED
                };

                CardPreAuthorizationDTO resultCardPreAuthorization = await this.Api.CardPreAuthorizations.UpdateAsync(cardPreAuthorizationPut, cardPreAuthorization.Id);

                Assert.AreEqual(resultCardPreAuthorization.Status, PreAuthorizationStatus.SUCCEEDED);
                Assert.AreEqual(resultCardPreAuthorization.PaymentStatus, PaymentStatus.CANCELED);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task Test_CardPreAuthorizations_GetPreAuthorizationsForUser()
        {
            try
            {
                var cardPreAuthorization = await GetJohnsCardPreAuthorization();

                var pagination = new Pagination(1, 1);

                var filter = new FilterPreAuthorizations
                {
                    ResultCode = cardPreAuthorization.ResultCode,
                    PaymentStatus = cardPreAuthorization.PaymentStatus,
                    Status = cardPreAuthorization.Status
                };

                var sort = new Sort();
                sort.AddField("CreationDate", SortDirection.desc);

                var preAuthorizations = await Api.CardPreAuthorizations.GetPreAuthorizationsForUserAsync(cardPreAuthorization.AuthorId, pagination, filter, sort);

                Assert.IsTrue(preAuthorizations.Count > 0);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task Test_CardPreAuthorizations_GetPreAuthorizationsForCard()
        {
            try
            {
                var cardPreAuthorization = await GetJohnsCardPreAuthorization();

                var pagination = new Pagination(1, 1);

                var filter = new FilterPreAuthorizations
                {
                    ResultCode = cardPreAuthorization.ResultCode,
                    PaymentStatus = cardPreAuthorization.PaymentStatus,
                    Status = cardPreAuthorization.Status
                };

                var sort = new Sort();
                sort.AddField("CreationDate", SortDirection.desc);

                var preAuthorizations = await Api.CardPreAuthorizations.GetPreAuthorizationsForCardAsync(cardPreAuthorization.CardId, pagination, filter, sort);

                Assert.IsTrue(preAuthorizations.Count > 0);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}
