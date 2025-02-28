using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MangoPay.SDK.Core;
using MangoPay.SDK.Core.Enumerations;
using MangoPay.SDK.Core.Serializers;
using MangoPay.SDK.Entities;
using MangoPay.SDK.Entities.GET;
using MangoPay.SDK.Entities.POST;
using MangoPay.SDK.Entities.PUT;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MangoPay.SDK.Tests
{
    [TestFixture]
    public class ApiPayInsTest : BaseTest
    {
        [Test]
        public async Task Test_PayIns_Create_CardWeb()
        {
            try
            {
                PayInDTO payIn = null;
                payIn = await this.GetJohnsPayInCardWeb();

                Assert.IsTrue(payIn.Id.Length > 0);
                Assert.IsTrue(payIn.PaymentType == PayInPaymentType.CARD);
                Assert.IsTrue(payIn.ExecutionType == PayInExecutionType.WEB);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task Test_PayIns_Get_CardWeb()
        {
            try
            {
                PayInDTO payIn = await this.GetJohnsPayInCardWeb();

                var getPayIn = await this.Api.PayIns.GetAsync(payIn.Id);

                Assert.IsTrue(payIn.Id == getPayIn.Id);
                Assert.IsTrue(payIn.PaymentType == PayInPaymentType.CARD);
                Assert.IsTrue(payIn.ExecutionType == PayInExecutionType.WEB);

                AssertEqualInputProps(payIn, getPayIn);

                Assert.IsTrue(getPayIn.Status == TransactionStatus.CREATED);
                Assert.IsNull(getPayIn.ExecutionDate);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task Test_PayIns_Create_PayPal()
        {
            try
            {
                var wallet = await this.GetJohnsWallet();
                var user = await this.GetJohn();

                var payInPost = new PayInPayPalPostDTO(user.Id, new Money { Amount = 1000, Currency = CurrencyIso.EUR }, new Money { Amount = 0, Currency = CurrencyIso.EUR }, wallet.Id, "http://test/test");

                var payIn = await this.Api.PayIns.CreatePayPalAsync(payInPost);

                Assert.IsTrue(payIn.Id.Length > 0);
                Assert.IsTrue(payIn.PaymentType == PayInPaymentType.PAYPAL);
                Assert.IsTrue(payIn.ExecutionType == PayInExecutionType.WEB);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
        
        [Test]
        public async Task Test_PayIns_Create_PayPalWeb()
        {
            try
            {
                var user = await this.GetJohn();
                var wallet = await this.GetJohnsWallet();

                var payInPost = new PayInPayPalWebPostDTO(
                    user.Id,
                    new Money { Amount = 500, Currency = CurrencyIso.EUR },
                    new Money { Amount = 0, Currency = CurrencyIso.EUR },
                    wallet.Id,
                    "http://example.com",
                    new List<LineItem>
                    {
                        new LineItem
                        {
                            Name = "running shoes",
                            Quantity = 1,
                            UnitAmount = 500,
                            TaxAmount = 0,
                            Description = "seller1 ID"
                        }
                    },
                    new Shipping
                    {
                        Address = new Address
                        {
                            AddressLine1 = "Address line 1",
                            AddressLine2 = "Address line 2",
                            City = "City",
                            Country = CountryIso.FR,
                            PostalCode = "11222",
                            Region = "Region"
                        },
                        FirstName = user.FirstName,
                        LastName = user.LastName
                    },
                    "test",
                    // CultureCode.FR
                    shippingPreference: ShippingPreference.SET_PROVIDED_ADDRESS,
                    reference: "Reference"
                );
                
                var payIn = await this.Api.PayIns.CreatePayPalWebAsync(payInPost);
                var fetched = await this.Api.PayIns.GetPayPalWebAsync(payIn.Id);

                Assert.IsTrue(payIn.Id.Length > 0);
                Assert.AreEqual(PayInPaymentType.PAYPAL, payIn.PaymentType);
                Assert.AreEqual(PayInExecutionType.WEB, payIn.ExecutionType);
                Assert.AreEqual(TransactionStatus.CREATED, payIn.Status);
                Assert.IsNotNull(payIn.RedirectURL);
                
                Assert.AreEqual(payIn.Id, fetched.Id);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task Test_PayIns_Create_Payconiq()
        {
            try
            {
                var payInPost = await payconiqPostDto();
                var payIn = await this.Api.PayIns.CreatePayconiqAsync(payInPost);

                Assert.IsTrue(payIn.Id.Length > 0);
                Assert.IsTrue(payIn.PaymentType == PayInPaymentType.PAYCONIQ);
                Assert.IsTrue(payIn.ExecutionType == PayInExecutionType.WEB);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
        
        [Test]
        public async Task Test_PayIns_Create_Payconiq_V2()
        {
            try
            {
                var payInPost = await payconiqPostDto();
                var payIn = await this.Api.PayIns.CreatePayconiqV2Async(payInPost);

                Assert.IsTrue(payIn.Id.Length > 0);
                Assert.IsTrue(payIn.PaymentType == PayInPaymentType.PAYCONIQ);
                Assert.IsTrue(payIn.ExecutionType == PayInExecutionType.WEB);
                Assert.IsNotNull(payIn.DeepLinkURL);
                Assert.IsNotNull(payIn.QRCodeURL);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        private async Task<PayInPayconiqPostDTO> payconiqPostDto()
        {
            var wallet = await this.GetJohnsWallet();
            var user = await this.GetJohn();

            var payInPost = new PayInPayconiqPostDTO
            {
                Tag = "custom meta",
                AuthorId = user.Id,
                DebitedFunds = new Money
                {
                    Amount = 22,
                    Currency = CurrencyIso.EUR
                },
                Fees = new Money
                {
                    Amount = 10,
                    Currency = CurrencyIso.EUR
                },
                ReturnURL = "http://www.my-site.com/returnURL",
                CreditedWalletId = wallet.Id,
                Country = "BE",
                CreditedUserId = user.Id
            };

            return payInPost;
        }

        [Test]
        public async Task Test_PayIns_Create_PayPal_WithShippingAddress()
        {
            try
            {
                var wallet = await this.GetJohnsWallet();
                var user = await this.GetJohn();
                var AddressForShippingAddress = new Address
                {
                    AddressLine1 = "Address line 1",
                    AddressLine2 = "Address line 2",
                    City = "City",
                    Country = CountryIso.PL,
                    PostalCode = "11222",
                    Region = "Region"
                };
                var payInPost = new PayInPayPalPostDTO(user.Id, new Money { Amount = 1000, Currency = CurrencyIso.EUR }, new Money { Amount = 0, Currency = CurrencyIso.EUR }, wallet.Id, "http://test/test")
                {
                    ShippingAddress = new ShippingAddress("recipient name", AddressForShippingAddress)
                };

                var payIn = await this.Api.PayIns.CreatePayPalAsync(payInPost);

                Assert.IsNotNull(payIn.ShippingAddress);
                Assert.AreEqual("recipient name", payIn.ShippingAddress.RecipientName);
                Assert.IsNotNull(payIn.ShippingAddress.Address);
                Assert.AreEqual("Address line 1", payIn.ShippingAddress.Address.AddressLine1);
                Assert.AreEqual("Address line 2", payIn.ShippingAddress.Address.AddressLine2);
                Assert.AreEqual("City", payIn.ShippingAddress.Address.City);
                Assert.AreEqual(CountryIso.PL, payIn.ShippingAddress.Address.Country);
                Assert.AreEqual("11222", payIn.ShippingAddress.Address.PostalCode);
                Assert.AreEqual("Region", payIn.ShippingAddress.Address.Region);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task Test_PayIns_Create_CardDirect()
        {
            try
            {
                var johnWallet = await this.GetJohnsWalletWithMoney();
                var beforeWallet = await this.Api.Wallets.GetAsync(johnWallet.Id);

                var payIn = await this.GetNewPayInCardDirect();
                var wallet = await this.Api.Wallets.GetAsync(johnWallet.Id);
                var user = await this.GetJohn();

                Assert.IsTrue(payIn.Id.Length > 0);
                Assert.AreEqual(wallet.Id, payIn.CreditedWalletId);
                Assert.AreEqual(PayInPaymentType.CARD, payIn.PaymentType);
                Assert.AreEqual(PayInExecutionType.DIRECT, payIn.ExecutionType);
                Assert.IsTrue(payIn.DebitedFunds is Money);
                Assert.IsTrue(payIn.CreditedFunds is Money);
                Assert.IsTrue(payIn.Fees is Money);
                Assert.AreEqual(user.Id, payIn.AuthorId);
                Assert.IsTrue(wallet.Balance.Amount == beforeWallet.Balance.Amount + payIn.CreditedFunds.Amount);
                Assert.AreEqual(TransactionStatus.SUCCEEDED, payIn.Status);
                Assert.AreEqual(TransactionType.PAYIN, payIn.Type);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
        
        [Test]
        public async Task Test_PayIns_CardDirect_GetPaymentMethodMetadata()
        {
            try
            {
                var payIn = await this.GetNewPayInCardDirect();

                var metadata = new PaymentMethodMetadataPostDTO
                {
                    Type = "BIN",
                    Bin = payIn.CardInfo.BIN
                };
                
                var resultMetadata = await this.Api.PayIns.GetPaymentMethodMetadataAsync(metadata);
                
                Assert.IsNotNull(resultMetadata);
                Assert.IsNotNull(resultMetadata.IssuerCountryCode);
                Assert.IsNotNull(resultMetadata.IssuingBank);
                Assert.IsNotNull(resultMetadata.BinData);
                Assert.IsNotNull(resultMetadata.BinData[0].CardType);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
        
        [Test]
        public async Task Test_PayIns_Create_CardDirect_CheckCardInfo()
        {
            try
            {
                var payIn = await this.GetNewPayInCardDirect();

                Assert.IsNotNull(payIn.CardInfo);
                Assert.IsNotNull(payIn.CardInfo.IssuingBank);
                Assert.IsNotNull(payIn.CardInfo.Brand);
                Assert.IsNotNull(payIn.CardInfo.Type);
                Assert.IsNotNull(payIn.CardInfo.IssuerCountryCode);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
        
        [Test]
        public async Task Test_PayIns_Create_MbwayWeb()
        {
            try
            {
                var user = await GetJohn();
                var payIn = await GetNewPayInMbwayWeb();
                var fetched = await Api.PayIns.GetMbwayAsync(payIn.Id);

                Assert.IsTrue(payIn.Id.Length > 0);
                Assert.AreEqual(PayInPaymentType.MBWAY, payIn.PaymentType);
                Assert.AreEqual(PayInExecutionType.WEB, payIn.ExecutionType);
                Assert.IsTrue(payIn.DebitedFunds is Money);
                Assert.IsTrue(payIn.CreditedFunds is Money);
                Assert.IsTrue(payIn.Fees is Money);
                Assert.AreEqual(user.Id, payIn.AuthorId);
                Assert.AreEqual(TransactionStatus.CREATED, payIn.Status);
                Assert.AreEqual(TransactionType.PAYIN, payIn.Type);
                Assert.AreEqual(TransactionNature.REGULAR, payIn.Nature);
                
                Assert.AreEqual(payIn.Id, fetched.Id);

            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task Test_PayIns_Create_MultibancoWeb()
        {
            try
            {
                var user = await GetJohn();
                var payIn = await GetNewPayInMultibancoWeb();
                var fetched = await Api.PayIns.GetMultibancoAsync(payIn.Id);

                Assert.IsTrue(payIn.Id.Length > 0);
                Assert.AreEqual(PayInPaymentType.MULTIBANCO, payIn.PaymentType);
                Assert.AreEqual(PayInExecutionType.WEB, payIn.ExecutionType);
                Assert.IsTrue(payIn.DebitedFunds is Money);
                Assert.IsTrue(payIn.CreditedFunds is Money);
                Assert.IsTrue(payIn.Fees is Money);
                Assert.AreEqual(user.Id, payIn.AuthorId);
                Assert.AreEqual(TransactionStatus.CREATED, payIn.Status);
                Assert.AreEqual(TransactionType.PAYIN, payIn.Type);
                Assert.AreEqual(TransactionNature.REGULAR, payIn.Nature);
                
                Assert.AreEqual(payIn.Id, fetched.Id);
                Assert.IsNotNull(fetched.Id);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
        
        [Test]
        public async Task Test_PayIns_Create_SatispayWeb()
        {
            try
            {
                var user = await GetJohn();
                var payIn = await GetNewPayInSatispayWeb();
                var fetched = await Api.PayIns.GetSatispayAsync(payIn.Id);

                Assert.IsTrue(payIn.Id.Length > 0);
                Assert.AreEqual(PayInPaymentType.SATISPAY, payIn.PaymentType);
                Assert.AreEqual(PayInExecutionType.WEB, payIn.ExecutionType);
                Assert.IsTrue(payIn.DebitedFunds is Money);
                Assert.IsTrue(payIn.CreditedFunds is Money);
                Assert.IsTrue(payIn.Fees is Money);
                Assert.AreEqual(user.Id, payIn.AuthorId);
                Assert.AreEqual(TransactionStatus.CREATED, payIn.Status);
                Assert.AreEqual(TransactionType.PAYIN, payIn.Type);
                Assert.AreEqual(TransactionNature.REGULAR, payIn.Nature);
                
                Assert.AreEqual(payIn.Id, fetched.Id);
                Assert.IsNotNull(fetched.Id);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
        
        [Test]
        public async Task Test_PayIns_Create_BlikWeb()
        {
            try
            {
                var user = await GetJohn();
                var payIn = await GetNewPayInBlikWeb();
                var fetched = await Api.PayIns.GetBlikAsync(payIn.Id);

                Assert.IsTrue(payIn.Id.Length > 0);
                Assert.AreEqual(PayInPaymentType.BLIK, payIn.PaymentType);
                Assert.AreEqual(PayInExecutionType.WEB, payIn.ExecutionType);
                Assert.IsTrue(payIn.DebitedFunds is Money);
                Assert.IsTrue(payIn.CreditedFunds is Money);
                Assert.IsTrue(payIn.Fees is Money);
                Assert.AreEqual(user.Id, payIn.AuthorId);
                Assert.AreEqual(TransactionStatus.CREATED, payIn.Status);
                Assert.AreEqual(TransactionType.PAYIN, payIn.Type);
                Assert.AreEqual(TransactionNature.REGULAR, payIn.Nature);
                
                Assert.AreEqual(payIn.Id, fetched.Id);
                Assert.IsNotNull(fetched.Id);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
        
        [Test]
        public async Task Test_PayIns_Create_KlarnaWeb()
        {
            try
            {
                var user = await GetJohn();
                var payIn = await GetNewPayInKlarnaWeb();
                var fetched = await Api.PayIns.GetKlarnaAsync(payIn.Id);

                Assert.IsTrue(payIn.Id.Length > 0);
                Assert.AreEqual(PayInPaymentType.KLARNA, payIn.PaymentType);
                Assert.AreEqual(PayInExecutionType.WEB, payIn.ExecutionType);
                Assert.IsTrue(payIn.DebitedFunds is Money);
                Assert.IsTrue(payIn.CreditedFunds is Money);
                Assert.IsTrue(payIn.Fees is Money);
                Assert.AreEqual(user.Id, payIn.AuthorId);
                Assert.AreEqual(TransactionStatus.CREATED, payIn.Status);
                Assert.AreEqual(TransactionType.PAYIN, payIn.Type);
                Assert.AreEqual(TransactionNature.REGULAR, payIn.Nature);
                
                Assert.AreEqual(payIn.Id, fetched.Id);
                Assert.IsNotNull(fetched.Id);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
        
        [Test]
        public async Task Test_PayIns_Create_IdealWeb()
        {
            try
            {
                var user = await GetJohn();
                var payIn = await GetNewPayInIdealWeb();
                var fetched = await Api.PayIns.GetIdealAsync(payIn.Id);

                Assert.IsTrue(payIn.Id.Length > 0);
                Assert.AreEqual(PayInPaymentType.IDEAL, payIn.PaymentType);
                Assert.AreEqual(PayInExecutionType.WEB, payIn.ExecutionType);
                Assert.IsTrue(payIn.DebitedFunds is Money);
                Assert.IsTrue(payIn.CreditedFunds is Money);
                Assert.IsTrue(payIn.Fees is Money);
                Assert.AreEqual(user.Id, payIn.AuthorId);
                Assert.AreEqual(TransactionStatus.CREATED, payIn.Status);
                Assert.AreEqual(TransactionType.PAYIN, payIn.Type);
                Assert.AreEqual(TransactionNature.REGULAR, payIn.Nature);
                
                Assert.AreEqual(payIn.Id, fetched.Id);
                Assert.IsNotNull(fetched.Id);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
        
        [Test]
        public async Task Test_PayIns_Create_GiropayWeb()
        {
            try
            {
                var user = await GetJohn();
                var payIn = await GetNewPayInGiropayWeb();
                var fetched = await Api.PayIns.GetGiropayAsync(payIn.Id);

                Assert.IsTrue(payIn.Id.Length > 0);
                Assert.AreEqual(PayInPaymentType.GIROPAY, payIn.PaymentType);
                Assert.AreEqual(PayInExecutionType.WEB, payIn.ExecutionType);
                Assert.IsTrue(payIn.DebitedFunds is Money);
                Assert.IsTrue(payIn.CreditedFunds is Money);
                Assert.IsTrue(payIn.Fees is Money);
                Assert.AreEqual(user.Id, payIn.AuthorId);
                Assert.AreEqual(TransactionStatus.CREATED, payIn.Status);
                Assert.AreEqual(TransactionType.PAYIN, payIn.Type);
                Assert.AreEqual(TransactionNature.REGULAR, payIn.Nature);
                
                Assert.AreEqual(payIn.Id, fetched.Id);
                Assert.IsNotNull(fetched.Id);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
        
        [Test]
        public async Task Test_PayIns_Create_SwishWeb()
        {
            try
            {
                var user = await GetJohn();
                var payIn = await GetNewPayInSwishWeb();
                var fetched = await Api.PayIns.GetSwishAsync(payIn.Id);

                Assert.IsTrue(payIn.Id.Length > 0);
                Assert.AreEqual(PayInPaymentType.SWISH, payIn.PaymentType);
                Assert.AreEqual(PayInExecutionType.WEB, payIn.ExecutionType);
                Assert.IsTrue(payIn.DebitedFunds is Money);
                Assert.IsTrue(payIn.CreditedFunds is Money);
                Assert.IsTrue(payIn.Fees is Money);
                Assert.AreEqual(user.Id, payIn.AuthorId);
                Assert.AreEqual(TransactionStatus.CREATED, payIn.Status);
                Assert.AreEqual(TransactionType.PAYIN, payIn.Type);
                Assert.AreEqual(TransactionNature.REGULAR, payIn.Nature);
                
                Assert.AreEqual(payIn.Id, fetched.Id);
                Assert.IsNotNull(fetched.Id);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
        
        [Test]
        public async Task Test_PayIns_Create_BancontactWeb()
        {
            try
            {
                var user = await GetJohn();
                var payIn = await GetNewPayInBancontactWeb();
                var fetched = await Api.PayIns.GetBancontactAsync(payIn.Id);

                Assert.IsTrue(payIn.Id.Length > 0);
                Assert.AreEqual(PayInPaymentType.BCMC, payIn.PaymentType);
                Assert.AreEqual(PayInExecutionType.WEB, payIn.ExecutionType);
                Assert.IsTrue(payIn.DebitedFunds is Money);
                Assert.IsTrue(payIn.CreditedFunds is Money);
                Assert.IsTrue(payIn.Fees is Money);
                Assert.AreEqual(user.Id, payIn.AuthorId);
                Assert.AreEqual(TransactionStatus.CREATED, payIn.Status);
                Assert.AreEqual(TransactionType.PAYIN, payIn.Type);
                Assert.AreEqual(TransactionNature.REGULAR, payIn.Nature);
                
                Assert.AreEqual(payIn.Id, fetched.Id);
                Assert.IsNotNull(fetched.Id);
                Assert.AreEqual(CultureCode.NL, fetched.Culture);
                Assert.AreEqual(false, fetched.Recurring);
                Assert.AreEqual(PaymentFlow.APP, payIn.PaymentFlow);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
        
        [Test]
        public async Task Test_PayIns_Create_Legacy_IdealWeb()
        {
            var john = await GetJohn();

            var wallet = await CreateJohnsWallet();
            PayInCardWebDTO payIn = await CreateLegacyIdealPayInCardWeb(wallet.Id);
            
            Assert.IsNotNull(payIn.BankName);
            Assert.IsTrue(payIn.Id.Length > 0);
            Assert.AreEqual(PayInPaymentType.CARD, payIn.PaymentType);
            Assert.AreEqual(PayInExecutionType.WEB, payIn.ExecutionType);
            Assert.IsTrue(payIn.DebitedFunds is Money);
            Assert.IsTrue(payIn.CreditedFunds is Money);
            Assert.IsTrue(payIn.Fees is Money);
            Assert.AreEqual(TransactionStatus.CREATED, payIn.Status);
            Assert.AreEqual(TransactionType.PAYIN, payIn.Type);
            Assert.AreEqual(TransactionNature.REGULAR, payIn.Nature);
        }

        [Test]
        public async Task Test_Payins_CardDirect_Create_WithBilling()
        {
            try
            {
                var johnWallet = await this.GetJohnsWalletWithMoney();
                var wallet = await this.Api.Wallets.GetAsync(johnWallet.Id);
                var user = await this.GetJohn();

                var payIn = await this.GetNewPayInCardDirectWithBilling();

                Assert.IsTrue(payIn.Id.Length > 0);
                Assert.AreEqual(wallet.Id, payIn.CreditedWalletId);
                Assert.AreEqual(PayInPaymentType.CARD, payIn.PaymentType);
                Assert.AreEqual(PayInExecutionType.DIRECT, payIn.ExecutionType);
                Assert.IsTrue(payIn.DebitedFunds is Money);
                Assert.IsTrue(payIn.CreditedFunds is Money);
                Assert.IsTrue(payIn.Fees is Money);
                Assert.AreEqual(user.Id, payIn.AuthorId);
                Assert.AreEqual(TransactionStatus.SUCCEEDED, payIn.Status);
                Assert.AreEqual(TransactionType.PAYIN, payIn.Type);
                Assert.IsNotNull(payIn.Billing);
                Assert.IsNotNull(payIn.SecurityInfo);
                Assert.IsNotNull(payIn.SecurityInfo.AVSResult);
                Assert.AreEqual(payIn.SecurityInfo.AVSResult, AVSResult.NO_CHECK);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task Test_PayIns_Get_CardDirect()
        {
            try
            {
                var payIn = await this.GetNewPayInCardDirect();

                var getPayIn = await this.Api.PayIns.GetCardDirectAsync(payIn.Id);

                Assert.IsTrue(payIn.Id == getPayIn.Id);
                Assert.IsTrue(payIn.PaymentType == PayInPaymentType.CARD);
                Assert.IsTrue(payIn.ExecutionType == PayInExecutionType.DIRECT);
                AssertEqualInputProps(payIn, getPayIn);
                Assert.IsNotNull(getPayIn.CardId);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task Test_PayIns_CreateRefund_CardDirect()
        {
            try
            {
                var payIn = await this.GetNewPayInCardDirect();
                var wallet = await this.GetJohnsWalletWithMoney();
                var walletBefore = await this.Api.Wallets.GetAsync(wallet.Id);

                var refund = await this.GetNewRefundForPayIn(payIn);
                var walletAfter = await this.Api.Wallets.GetAsync(wallet.Id);

                Assert.IsTrue(refund.Id.Length > 0);
                Assert.IsTrue(refund.DebitedFunds.Amount == payIn.DebitedFunds.Amount);
                Assert.IsTrue(walletBefore.Balance.Amount == (walletAfter.Balance.Amount + payIn.DebitedFunds.Amount));
                Assert.AreEqual(TransactionType.PAYOUT, refund.Type);
                Assert.AreEqual(TransactionNature.REFUND, refund.Nature);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
        
        [Test]
        public async Task Test_PayIns_CreatePartialRefund_CardDirect()
        {
            try
            {
                var payIn = await this.GetNewPayInCardDirect();
                var wallet = await this.GetJohnsWalletWithMoney();
                var walletBefore = await this.Api.Wallets.GetAsync(wallet.Id);

                var refund = await this.GetPartialRefundForPayIn(payIn);
                var walletAfter = await this.Api.Wallets.GetAsync(wallet.Id);

                Assert.IsTrue(refund.Id.Length > 0);
                Assert.IsTrue(walletAfter.Balance.Amount == (walletBefore.Balance.Amount - refund.DebitedFunds.Amount));
                Assert.AreEqual(TransactionType.PAYOUT, refund.Type);
                Assert.AreEqual(TransactionNature.REFUND, refund.Nature);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task Test_PayIns_PreAuthorizedDirect()
        {
            try
            {
                var cardPreAuthorization = await this.GetJohnsCardPreAuthorization();
                var wallet = await this.GetJohnsWalletWithMoney();
                var user = await this.GetJohn();

                // create pay-in PRE-AUTHORIZED DIRECT
                var payIn = new PayInPreauthorizedDirectPostDTO(user.Id, new Money { Amount = 100, Currency = CurrencyIso.EUR }, new Money { Amount = 0, Currency = CurrencyIso.EUR }, wallet.Id, cardPreAuthorization.Id)
                {
                    SecureModeReturnURL = "http://test.com"
                };

                var createPayIn = await this.Api.PayIns.CreatePreauthorizedDirectAsync(payIn);

                Assert.IsTrue("" != createPayIn.Id);
                Assert.AreEqual(wallet.Id, createPayIn.CreditedWalletId);
                Assert.AreEqual(PayInPaymentType.PREAUTHORIZED, createPayIn.PaymentType);
                Assert.AreEqual(PayInExecutionType.DIRECT, createPayIn.ExecutionType);
                Assert.IsTrue(createPayIn.DebitedFunds is Money);
                Assert.IsTrue(createPayIn.CreditedFunds is Money);
                Assert.IsTrue(createPayIn.Fees is Money);
                Assert.AreEqual(user.Id, createPayIn.AuthorId);
                Assert.AreEqual(TransactionStatus.SUCCEEDED, createPayIn.Status);
                Assert.AreEqual(TransactionType.PAYIN, createPayIn.Type);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task Test_PayIns_BankWireDirect_Create()
        {
            try
            {
                var wallet = await this.GetJohnsWallet();
                var user = await this.GetJohn();

                // create pay-in BANKWIRE DIRECT
                var payIn = new PayInBankWireDirectPostDTO(user.Id, wallet.Id, new Money { Amount = 100, Currency = CurrencyIso.EUR }, new Money { Amount = 0, Currency = CurrencyIso.EUR })
                {
                    CreditedWalletId = wallet.Id,
                    AuthorId = user.Id
                };

                PayInDTO createPayIn = await this.Api.PayIns.CreateBankWireDirectAsync(payIn);

                Assert.IsTrue(createPayIn.Id.Length > 0);
                Assert.AreEqual(wallet.Id, createPayIn.CreditedWalletId);
                Assert.AreEqual(PayInPaymentType.BANK_WIRE, createPayIn.PaymentType);
                Assert.AreEqual(PayInExecutionType.DIRECT, createPayIn.ExecutionType);
                Assert.AreEqual(user.Id, createPayIn.AuthorId);
                Assert.AreEqual(TransactionStatus.CREATED, createPayIn.Status);
                Assert.AreEqual(TransactionType.PAYIN, createPayIn.Type);
                Assert.IsNotNull(((PayInBankWireDirectDTO)createPayIn).WireReference);
                Assert.AreEqual(((PayInBankWireDirectDTO)createPayIn).BankAccount.Type, BankAccountType.IBAN);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        /*
		 * Uncomment the attribute below to test payins with a mandate
		 * This test needs your manual confirmation on the web page (see note in test's body)
		 */
        //[Test]
        public async Task Test_PayIns_MandateDirect_Create_Get()
        {
            try
            {
                var wallet = await this.GetJohnsWallet();
                var user = await this.GetJohn();

                var johnsAccount = await GetJohnsAccount();
                var bankAccountId = johnsAccount.Id;
                var returnUrl = "http://test.test";
                var mandatePost = new MandatePostDTO(bankAccountId, CultureCode.EN, returnUrl);
                var mandate = await this.Api.Mandates.CreateAsync(mandatePost);

                /*	
				 *	! IMPORTANT NOTE !
				 *	
				 *	In order to make this test pass, at this place you have to set a breakpoint,
				 *	navigate to URL the mandate.RedirectURL property points to and click "CONFIRM" button.
				 * 
				 */

                var payIn = new PayInMandateDirectPostDTO(user.Id, new Money { Amount = 100, Currency = CurrencyIso.EUR }, new Money { Amount = 0, Currency = CurrencyIso.EUR }, wallet.Id, "http://test.test", mandate.Id);

                PayInDTO createPayIn = await this.Api.PayIns.CreateMandateDirectDebitAsync(payIn);

                Assert.IsNotNull(createPayIn);
                Assert.AreNotEqual(TransactionStatus.FAILED, createPayIn.Status, "In order to make this test pass, after creating mandate and before creating the payin you have to navigate to URL the mandate.RedirectURL property points to and click CONFIRM button.");

                Assert.IsTrue(createPayIn.Id.Length > 0);
                Assert.AreEqual(wallet.Id, createPayIn.CreditedWalletId);
                Assert.AreEqual(PayInPaymentType.DIRECT_DEBIT, createPayIn.PaymentType);
                Assert.AreEqual(PayInExecutionType.DIRECT, createPayIn.ExecutionType);
                Assert.AreEqual(user.Id, createPayIn.AuthorId);
                Assert.AreEqual(TransactionStatus.CREATED, createPayIn.Status);
                Assert.AreEqual(TransactionType.PAYIN, createPayIn.Type);
                Assert.IsNotNull(((PayInMandateDirectDTO)createPayIn).MandateId);
                Assert.AreEqual(((PayInMandateDirectDTO)createPayIn).MandateId, mandate.Id);

                var getPayIn = await this.Api.PayIns.GetMandateDirectDebitAsync(createPayIn.Id);

                Assert.IsNotNull(getPayIn);
                Assert.IsTrue(getPayIn.Id == createPayIn.Id);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task Test_PayIns_BankWireDirect_Get()
        {
            try
            {
                var wallet = await this.GetJohnsWallet();
                var user = await this.GetJohn();

                // create pay-in BANKWIRE DIRECT
                var payIn = new PayInBankWireDirectPostDTO(user.Id, wallet.Id, new Money { Amount = 100, Currency = CurrencyIso.EUR }, new Money { Amount = 0, Currency = CurrencyIso.EUR })
                {
                    CreditedWalletId = wallet.Id,
                    AuthorId = user.Id
                };

                var createdPayIn = await this.Api.PayIns.CreateBankWireDirectAsync(payIn);

                var getPayIn = await this.Api.PayIns.GetBankWireDirectAsync(createdPayIn.Id);

                Assert.AreEqual(getPayIn.Id, createdPayIn.Id);
                Assert.AreEqual(PayInPaymentType.BANK_WIRE, getPayIn.PaymentType);
                Assert.AreEqual(PayInExecutionType.DIRECT, getPayIn.ExecutionType);
                Assert.AreEqual(user.Id, getPayIn.AuthorId);
                Assert.AreEqual(TransactionType.PAYIN, getPayIn.Type);
                Assert.IsNotNull(getPayIn.WireReference);
                Assert.AreEqual(getPayIn.BankAccount.Type, BankAccountType.IBAN);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task Test_PayIns_DirectDebit_Create_Get()
        {
            var wallet = await this.GetJohnsWallet();
            var user = await this.GetJohn();
            // create pay-in DIRECT DEBIT
            var payIn = new PayInDirectDebitPostDTO(user.Id, new Money { Amount = 100, Currency = CurrencyIso.EUR }, new Money { Amount = 10, Currency = CurrencyIso.EUR }, wallet.Id, "http://www.mysite.com/returnURL/", CultureCode.FR, DirectDebitType.GIROPAY)
            {
                TemplateURLOptions = new TemplateURLOptions { PAYLINE = "https://www.maysite.com/payline_template/" },
                Tag = "DirectDebit test tag"
            };

            var createPayIn = await this.Api.PayIns.CreateDirectDebitAsync(payIn);

            Assert.IsNotNull(createPayIn);
            Assert.IsTrue(createPayIn.Id.Length > 0);
            Assert.AreEqual(wallet.Id, createPayIn.CreditedWalletId);
            Assert.IsTrue(createPayIn.PaymentType == PayInPaymentType.DIRECT_DEBIT);
            Assert.IsTrue(createPayIn.DirectDebitType == DirectDebitType.GIROPAY);
            Assert.IsTrue(createPayIn.Culture == CultureCode.FR);
            Assert.AreEqual(user.Id, createPayIn.AuthorId);
            Assert.IsTrue(createPayIn.Status == TransactionStatus.CREATED);
            Assert.IsTrue(createPayIn.Type == TransactionType.PAYIN);
            Assert.IsNotNull(createPayIn.DebitedFunds);
            Assert.IsTrue(createPayIn.DebitedFunds is Money);
            Assert.AreEqual(100, createPayIn.DebitedFunds.Amount);
            Assert.IsTrue(createPayIn.DebitedFunds.Currency == CurrencyIso.EUR);

            Assert.IsNotNull(createPayIn.CreditedFunds);
            Assert.IsTrue(createPayIn.CreditedFunds is Money);
            Assert.AreEqual(90, createPayIn.CreditedFunds.Amount);
            Assert.IsTrue(createPayIn.CreditedFunds.Currency == CurrencyIso.EUR);

            Assert.IsNotNull(createPayIn.Fees);
            Assert.IsTrue(createPayIn.Fees is Money);
            Assert.AreEqual(10, createPayIn.Fees.Amount);
            Assert.IsTrue(createPayIn.Fees.Currency == CurrencyIso.EUR);

            Assert.IsNotNull(createPayIn.ReturnURL);
            Assert.IsNotNull(createPayIn.RedirectURL);


            var getPayIn = await this.Api.PayIns.GetDirectDebitAsync(createPayIn.Id);

            Assert.IsNotNull(getPayIn);
            Assert.IsTrue(getPayIn.Id == createPayIn.Id);
            Assert.IsTrue(getPayIn.Tag == createPayIn.Tag);
        }

        [Ignore("card is not 3dsecure")]
        [Test]
        public async Task TestApplePayIn()
        {
            var wallet = await GetJohnsWallet();
            var user = await GetMatrix();
            var paymentData = new PaymentData
            {
                Network = "VISA",
                TransactionId = "061EB32181A2D9CA42AD16031B476EEBAA62A9A095AD660E2759FBA52B51A61",
                TokenData = "{\"version\":\"EC_v1\"," +
                            "\"data\":\"w4HMBVqNC9ghPP4zncTA\\/0oQAsduERfsx78oxgniynNjZLANTL6+0koEtkQnW\\/K38Zew8qV1GLp+fLHo+qCBpiKCIwlz3eoFBTbZU+8pYcjaeIYBX9SOxcwxXsNGrGLk+kBUqnpiSIPaAG1E+WPT8R1kjOCnGvtdombvricwRTQkGjtovPfzZo8LzD3ZQJnHMsWJ8QYDLyr\\/ZN9gtLAtsBAMvwManwiaG3pOIWpyeOQOb01YcEVO16EZBjaY4x4C\\/oyFLWDuKGvhbJwZqWh1d1o9JT29QVmvy3Oq2JEjq3c3NutYut4rwDEP4owqI40Nb7mP2ebmdNgnYyWfPmkRfDCRHIWtbMC35IPg5313B1dgXZ2BmyZRXD5p+mr67vAk7iFfjEpu3GieFqwZrTl3\\/pI5V8Sxe3SIYKgT5Hr7ow==\"," +
                            "\"signature\":\"MIAGCSqGSIb3DQEHAqCAMIACAQExDzANBglghkgBZQMEAgEFADCABgkqhkiG9w0BBwEAAKCAMIID5jCCA4ugAwIBAgIIaGD2mdnMpw8wCgYIKoZIzj0EAwIwejEuMCwGA1UEAwwlQXBwbGUgQXBwbGljYXRpb24gSW50ZWdyYXRpb24gQ0EgLSBHMzEmMCQGA1UECwwdQXBwbGUgQ2VydGlmaWNhdGlvbiBBdXRob3JpdHkxEzARBgNVBAoMCkFwcGxlIEluYy4xCzAJBgNVBAYTAlVTMB4XDTE2MDYwMzE4MTY0MFoXDTIxMDYwMjE4MTY0MFowYjEoMCYGA1UEAwwfZWNjLXNtcC1icm9rZXItc2lnbl9VQzQtU0FOREJPWDEUMBIGA1UECwwLaU9TIFN5c3RlbXMxEzARBgNVBAoMCkFwcGxlIEluYy4xCzAJBgNVBAYTAlVTMFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEgjD9q8Oc914gLFDZm0US5jfiqQHdbLPgsc1LUmeY+M9OvegaJajCHkwz3c6OKpbC9q+hkwNFxOh6RCbOlRsSlaOCAhEwggINMEUGCCsGAQUFBwEBBDkwNzA1BggrBgEFBQcwAYYpaHR0cDovL29jc3AuYXBwbGUuY29tL29jc3AwNC1hcHBsZWFpY2EzMDIwHQYDVR0OBBYEFAIkMAua7u1GMZekplopnkJxghxFMAwGA1UdEwEB\\/wQCMAAwHwYDVR0jBBgwFoAUI\\/JJxE+T5O8n5sT2KGw\\/orv9LkswggEdBgNVHSAEggEUMIIBEDCCAQwGCSqGSIb3Y2QFATCB\\/jCBwwYIKwYBBQUHAgIwgbYMgbNSZWxpYW5jZSBvbiB0aGlzIGNlcnRpZmljYXRlIGJ5IGFueSBwYXJ0eSBhc3N1bWVzIGFjY2VwdGFuY2Ugb2YgdGhlIHRoZW4gYXBwbGljYWJsZSBzdGFuZGFyZCB0ZXJtcyBhbmQgY29uZGl0aW9ucyBvZiB1c2UsIGNlcnRpZmljYXRlIHBvbGljeSBhbmQgY2VydGlmaWNhdGlvbiBwcmFjdGljZSBzdGF0ZW1lbnRzLjA2BggrBgEFBQcCARYqaHR0cDovL3d3dy5hcHBsZS5jb20vY2VydGlmaWNhdGVhdXRob3JpdHkvMDQGA1UdHwQtMCswKaAnoCWGI2h0dHA6Ly9jcmwuYXBwbGUuY29tL2FwcGxlYWljYTMuY3JsMA4GA1UdDwEB\\/wQEAwIHgDAPBgkqhkiG92NkBh0EAgUAMAoGCCqGSM49BAMCA0kAMEYCIQDaHGOui+X2T44R6GVpN7m2nEcr6T6sMjOhZ5NuSo1egwIhAL1a+\\/hp88DKJ0sv3eT3FxWcs71xmbLKD\\/QJ3mWagrJNMIIC7jCCAnWgAwIBAgIISW0vvzqY2pcwCgYIKoZIzj0EAwIwZzEbMBkGA1UEAwwSQXBwbGUgUm9vdCBDQSAtIEczMSYwJAYDVQQLDB1BcHBsZSBDZXJ0aWZpY2F0aW9uIEF1dGhvcml0eTETMBEGA1UECgwKQXBwbGUgSW5jLjELMAkGA1UEBhMCVVMwHhcNMTQwNTA2MjM0NjMwWhcNMjkwNTA2MjM0NjMwWjB6MS4wLAYDVQQDDCVBcHBsZSBBcHBsaWNhdGlvbiBJbnRlZ3JhdGlvbiBDQSAtIEczMSYwJAYDVQQLDB1BcHBsZSBDZXJ0aWZpY2F0aW9uIEF1dGhvcml0eTETMBEGA1UECgwKQXBwbGUgSW5jLjELMAkGA1UEBhMCVVMwWTATBgcqhkjOPQIBBggqhkjOPQMBBwNCAATwFxGEGddkhdUaXiWBB3bogKLv3nuuTeCN\\/EuT4TNW1WZbNa4i0Jd2DSJOe7oI\\/XYXzojLdrtmcL7I6CmE\\/1RFo4H3MIH0MEYGCCsGAQUFBwEBBDowODA2BggrBgEFBQcwAYYqaHR0cDovL29jc3AuYXBwbGUuY29tL29jc3AwNC1hcHBsZXJvb3RjYWczMB0GA1UdDgQWBBQj8knET5Pk7yfmxPYobD+iu\\/0uSzAPBgNVHRMBAf8EBTADAQH\\/MB8GA1UdIwQYMBaAFLuw3qFYM4iapIqZ3r6966\\/ayySrMDcGA1UdHwQwMC4wLKAqoCiGJmh0dHA6Ly9jcmwuYXBwbGUuY29tL2FwcGxlcm9vdGNhZzMuY3JsMA4GA1UdDwEB\\/wQEAwIBBjAQBgoqhkiG92NkBgIOBAIFADAKBggqhkjOPQQDAgNnADBkAjA6z3KDURaZsYb7NcNWymK\\/9Bft2Q91TaKOvvGcgV5Ct4n4mPebWZ+Y1UENj53pwv4CMDIt1UQhsKMFd2xd8zg7kGf9F3wsIW2WT8ZyaYISb1T4en0bmcubCYkhYQaZDwmSHQAAMYIBizCCAYcCAQEwgYYwejEuMCwGA1UEAwwlQXBwbGUgQXBwbGljYXRpb24gSW50ZWdyYXRpb24gQ0EgLSBHMzEmMCQGA1UECwwdQXBwbGUgQ2VydGlmaWNhdGlvbiBBdXRob3JpdHkxEzARBgNVBAoMCkFwcGxlIEluYy4xCzAJBgNVBAYTAlVTAghoYPaZ2cynDzANBglghkgBZQMEAgEFAKCBlTAYBgkqhkiG9w0BCQMxCwYJKoZIhvcNAQcBMBwGCSqGSIb3DQEJBTEPFw0xOTA1MjMxMTA1MDdaMCoGCSqGSIb3DQEJNDEdMBswDQYJYIZIAWUDBAIBBQChCgYIKoZIzj0EAwIwLwYJKoZIhvcNAQkEMSIEIIvfGVQYBeOilcB7GNI8m8+FBVZ28QfA6BIXaggBja2PMAoGCCqGSM49BAMCBEYwRAIgU01yYfjlx9bvGeC5CU2RS5KBEG+15HH9tz\\/sg3qmQ14CID4F4ZJwAz+tXAUcAIzoMpYSnM8YBlnGJSTSp+LhspenAAAAAAAA\"," +
                            "\"header\":" +
                            "{\"ephemeralPublicKey\":\"MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAE0rs3wRpirXjPbFDQfPRdfEzRIZDWm0qn7Y0HB0PNzV1DDKfpYrnhRb4GEhBF\\/oEXBOe452PxbCnN1qAlqcSUWw==\"," +
                            "\"publicKeyHash\":\"saPRAqS7TZ4bAYwzBj8ezDDC55ZolyH1FL+Xc8fd93o=\"," +
                            "\"transactionId\":\"b061eb32181a2d9ca42ad16031b476eebaa62a9a095ad660e2759fba52b51a61\"}}"
            };
            var applePayIn = new ApplePayDirectPayInPostDTO
            {
                CreditedWalletId = wallet.Id,
                AuthorId = user.Id,
                CreditedUserId = user.Id,
                DebitedFunds = new Money
                {
                    Amount = 199,
                    Currency = CurrencyIso.EUR
                },
                Fees = new Money
                {
                    Amount = 1,
                    Currency = CurrencyIso.EUR
                },
                Tag = "Create an ApplePay card direct Payin",
                PaymentType = PayInPaymentType.APPLEPAY,
                ExecutionType = PayInExecutionType.DIRECT,
                PaymentData = paymentData
            };

            var getPayIn = await Api.PayIns.CreateApplePayAsync(applePayIn, DateTime.Now.Ticks.ToString());

            Assert.IsNotNull(getPayIn);
            Assert.AreEqual(getPayIn.AuthorId, applePayIn.AuthorId);
            Assert.AreEqual(getPayIn.PaymentType, PayInPaymentType.APPLEPAY);
            Assert.AreEqual(getPayIn.Status, TransactionStatus.SUCCEEDED);
        }

        [Ignore("Cannot test Google Pay")]
        public async Task TestGooglePayIn()
        {
            var wallet = await GetJohnsWallet();
            var user = await GetNewJohn();
            var paymentData = new PaymentData
            {
                Network = "VISA",
                TransactionId = "061EB32181A2D9CA42AD16031B476EEBAA62A9A095AD660E2759FBA52B51A61",
                TokenData = "tokenData"
            };
            var googlePayIn = new GooglePayDirectPayInPostDTO
            {
                CreditedWalletId = wallet.Id,
                AuthorId = user.Id,
                CreditedUserId = user.Id,
                DebitedFunds = new Money
                {
                    Amount = 200,
                    Currency = CurrencyIso.EUR
                },
                Fees = new Money
                {
                    Amount = 0,
                    Currency = CurrencyIso.EUR
                },
                Tag = "Create an GooglePay card direct Payin",
                PaymentData = paymentData,
                StatementDescriptor = "Bob",
                Billing = new Billing
                {
                    Address = user.Address,
                    FirstName = "John",
                    LastName = "Doe"
                }
            };

            var getPayIn = await Api.PayIns.CreateGooglePayAsync(googlePayIn);

            Assert.IsNotNull(getPayIn);
            Assert.AreEqual(getPayIn.AuthorId, googlePayIn.AuthorId);
            Assert.AreEqual(getPayIn.PaymentType, PayInPaymentType.GOOGLEPAY);
            Assert.AreEqual(getPayIn.Status, TransactionStatus.SUCCEEDED);
        }

        [Ignore("Cannot test Google Pay because the necessary data cannot be generated in the test")]
        public async Task Test_PayIns_Create_GooglePayDirect()
        {
            try
            {
                var user = await GetJohn();
                var payIn = await GetNewPayInGooglePayDirect();

                Assert.IsTrue(payIn.Id.Length > 0);
                Assert.AreEqual(PayInPaymentType.GOOGLE_PAY, payIn.PaymentType);
                Assert.AreEqual(PayInExecutionType.DIRECT, payIn.ExecutionType);
                Assert.IsTrue(payIn.DebitedFunds is Money);
                Assert.IsTrue(payIn.CreditedFunds is Money);
                Assert.IsTrue(payIn.Fees is Money);
                Assert.AreEqual(user.Id, payIn.AuthorId);
                Assert.AreEqual(TransactionType.PAYIN, payIn.Type);
                Assert.AreEqual(TransactionNature.REGULAR, payIn.Nature);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task Test_PayIns_Get_PayPal()
        {
            try
            {
                PayInDTO payIn = null;
                var wallet = await this.GetJohnsWallet();
                var user = await this.GetJohn();

                var payInPost = new PayInPayPalPostDTO(user.Id, new Money { Amount = 1000, Currency = CurrencyIso.EUR }, new Money { Amount = 0, Currency = CurrencyIso.EUR }, wallet.Id, "http://test/test");

                payIn = await this.Api.PayIns.CreatePayPalAsync(payInPost);

                Assert.IsTrue(payIn.Id.Length > 0);
                Assert.IsTrue(payIn.PaymentType == PayInPaymentType.PAYPAL);
                Assert.IsTrue(payIn.ExecutionType == PayInExecutionType.WEB);

                var getPayIn = await this.Api.PayIns.GetPayPalAsync(payIn.Id);

                Assert.IsNotNull(getPayIn);
                Assert.IsTrue(getPayIn.Id == payIn.Id);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task Test_PayIns_Get_PayPal_WithShippingAddress()
        {
            try
            {
                PayInDTO payIn = null;
                var wallet = await this.GetJohnsWallet();
                var user = await this.GetJohn();
                var addressForShippingAddress = new Address
                {
                    AddressLine1 = "Address line 1",
                    AddressLine2 = "Address line 2",
                    City = "City",
                    Country = CountryIso.PL,
                    PostalCode = "11222",
                    Region = "Region"
                };

                var payInPost = new PayInPayPalPostDTO(user.Id, new Money { Amount = 1000, Currency = CurrencyIso.EUR }, new Money { Amount = 0, Currency = CurrencyIso.EUR }, wallet.Id, "http://test/test")
                {
                    ShippingAddress = new ShippingAddress("recipient name", addressForShippingAddress)
                };

                payIn = await this.Api.PayIns.CreatePayPalAsync(payInPost);

                var getPayIn = await this.Api.PayIns.GetPayPalAsync(payIn.Id);

                Assert.IsNotNull(getPayIn.ShippingAddress);
                Assert.AreEqual("recipient name", getPayIn.ShippingAddress.RecipientName);
                Assert.IsNotNull(getPayIn.ShippingAddress.Address);
                Assert.AreEqual("Address line 1", getPayIn.ShippingAddress.Address.AddressLine1);
                Assert.AreEqual("Address line 2", getPayIn.ShippingAddress.Address.AddressLine2);
                Assert.AreEqual("City", getPayIn.ShippingAddress.Address.City);
                Assert.AreEqual(CountryIso.PL, getPayIn.ShippingAddress.Address.Country);
                Assert.AreEqual("11222", getPayIn.ShippingAddress.Address.PostalCode);
                Assert.AreEqual("Region", getPayIn.ShippingAddress.Address.Region);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        [Ignore("Expire payin id")]
        public async Task Test_PayIns_Get_PayPal_WithPayPalBuyerAccountEmail()
        {
            try
            {
                var payInId = "54088959";
                var payPalBuyerEmail = "paypal-buyer-user@mangopay.com";
                var payIn = await Api.PayIns.GetPayPalAsync(payInId);

                Assert.NotNull(payIn);
                Assert.NotNull(payIn.Id);
                Assert.NotNull(payIn.PaypalBuyerAccountEmail);
                Assert.AreEqual(payInId, payIn.Id);
                Assert.AreEqual(payPalBuyerEmail, payIn.PaypalBuyerAccountEmail);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task Test_Payins_Get_Payconiq()
        {
            try
            {
                var wallet = await this.GetJohnsWallet();
                var user = await this.GetJohn();

                PayInPayconiqPostDTO payInPost = new PayInPayconiqPostDTO()
                {
                    AuthorId = user.Id,
                    Country = "BE",
                    CreditedWalletId = wallet.Id,
                    DebitedFunds = new Money
                    {
                        Amount = 2222,
                        Currency = CurrencyIso.EUR
                    },
                    Fees = new Money
                    {
                        Amount = 22,
                        Currency = CurrencyIso.EUR
                    },
                    ReturnURL = "http://test/test"
                };

                PayInPayconiqDTO payInCreated = await this.Api.PayIns.CreatePayconiqAsync(payInPost);

                PayInPayconiqDTO payInGet = await this.Api.PayIns.GetPayconiqAsync(payInCreated.Id);

                Assert.IsNotNull(payInGet);
                Assert.AreEqual(payInCreated.Id, payInGet.Id);
                Assert.AreEqual(payInCreated.DebitedFunds.Amount, payInGet.DebitedFunds.Amount);
                Assert.AreEqual(payInGet.PaymentType, PayInPaymentType.PAYCONIQ);
                Assert.AreEqual(payInGet.ExecutionType, PayInExecutionType.WEB);
                Assert.IsNotNull(payInGet.DeepLinkURL);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task Test_PayIns_GetBankWireExternalInstructionIBAN()
        {
            try
            {
                var payInId = "payin_m_01JK6199ED4VGBP98ABRJVDS9D";

                var payIn = await this.Api.PayIns.GetAsync(payInId);

                Assert.IsNotNull(payIn);

                Assert.True(payIn.Type == TransactionType.PAYIN);
                Assert.True(payIn.PaymentType == PayInPaymentType.BANK_WIRE);
                Assert.True(payIn.ExecutionType == PayInExecutionType.EXTERNAL_INSTRUCTION);

                Assert.True(payIn.Status == TransactionStatus.SUCCEEDED);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        [Ignore("Expire payin id")]
        public async Task Test_PayIns_GetBankWireExternalInstructionAccountNumber()
        {
            try
            {
                var payInId = "74981216";

                var payIn = await this.Api.PayIns.GetAsync(payInId);

                Assert.IsNotNull(payIn);

                Assert.True(payIn.Type == TransactionType.PAYIN);
                Assert.True(payIn.PaymentType == PayInPaymentType.BANK_WIRE);
                Assert.True(payIn.ExecutionType == PayInExecutionType.EXTERNAL_INSTRUCTION);

                Assert.True(payIn.Status == TransactionStatus.SUCCEEDED);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task Test_PayIns_Create_CardDirectWithBrowserInfo()
        {
            try
            {
                var johnWallet = await this.GetJohnsWalletWithMoney();
                var beforeWallet = await this.Api.Wallets.GetAsync(johnWallet.Id);

                var payIn = await this.GetNewPayInCardDirect();
                payIn.BrowserInfo = this.getBrowserInfo();

                var wallet = await this.Api.Wallets.GetAsync(johnWallet.Id);
                var user = await this.GetJohn();

                Assert.IsTrue(payIn.Id.Length > 0);
                Assert.AreEqual(wallet.Id, payIn.CreditedWalletId);
                Assert.AreEqual(PayInPaymentType.CARD, payIn.PaymentType);
                Assert.AreEqual(PayInExecutionType.DIRECT, payIn.ExecutionType);
                Assert.IsTrue(payIn.DebitedFunds is Money);
                Assert.IsTrue(payIn.CreditedFunds is Money);
                Assert.IsTrue(payIn.Fees is Money);
                Assert.AreEqual(user.Id, payIn.AuthorId);
                Assert.IsTrue(wallet.Balance.Amount == beforeWallet.Balance.Amount + payIn.CreditedFunds.Amount);
                Assert.AreEqual(TransactionStatus.SUCCEEDED, payIn.Status);
                Assert.AreEqual(TransactionType.PAYIN, payIn.Type);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task Test_PayIns_Create_CardDirectWithIpAddress()
        {
            try
            {
                var johnWallet = await this.GetJohnsWalletWithMoney();
                var beforeWallet = await this.Api.Wallets.GetAsync(johnWallet.Id);

                var payIn = await this.GetNewPayInCardDirect();
                payIn.IpAddress = "2001:0620:0000:0000:0211:24FF:FE80:C12C";

                var wallet = await this.Api.Wallets.GetAsync(johnWallet.Id);
                var user = await this.GetJohn();

                Assert.IsTrue(payIn.Id.Length > 0);
                Assert.AreEqual(wallet.Id, payIn.CreditedWalletId);
                Assert.AreEqual(PayInPaymentType.CARD, payIn.PaymentType);
                Assert.AreEqual(PayInExecutionType.DIRECT, payIn.ExecutionType);
                Assert.IsTrue(payIn.DebitedFunds is Money);
                Assert.IsTrue(payIn.CreditedFunds is Money);
                Assert.IsTrue(payIn.Fees is Money);
                Assert.AreEqual(user.Id, payIn.AuthorId);
                Assert.IsTrue(wallet.Balance.Amount == beforeWallet.Balance.Amount + payIn.CreditedFunds.Amount);
                Assert.AreEqual(TransactionStatus.SUCCEEDED, payIn.Status);
                Assert.AreEqual(TransactionType.PAYIN, payIn.Type);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task Test_Payins_CardDirect_Create_WithBillingAndShipping()
        {
            try
            {
                var johnWallet = await this.GetJohnsWalletWithMoney();
                var wallet = await this.Api.Wallets.GetAsync(johnWallet.Id);
                var user = await this.GetJohn();

                var payIn = await this.GetNewPayInCardDirectWithBillingAndShipping();
                Assert.AreEqual(TransactionStatus.SUCCEEDED, payIn.Status);
                Assert.AreEqual(TransactionType.PAYIN, payIn.Type);
                Assert.IsNotNull(payIn.Billing);
                Assert.IsNotNull(payIn.SecurityInfo);
                Assert.IsNotNull(payIn.SecurityInfo.AVSResult);
                Assert.AreEqual(payIn.SecurityInfo.AVSResult, AVSResult.NO_CHECK);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task Test_PayIns_Create_Recurring()
        {
            try
            {
                var data = await GetNewJohnsWalletWithMoneyAndCardId(1000);
                var cardId = data.Item1;
                var wallet = data.Item2;
                var userId = wallet.Owners.FirstOrDefault();

                var payInPost = new RecurringPayInRegistrationPostDTO
                {
                    AuthorId = userId,
                    CardId = cardId,
                    CreditedUserId = userId,
                    CreditedWalletId = wallet.Id,
                    FirstTransactionDebitedFunds = new Money
                    {
                        Amount = 12,
                        Currency = CurrencyIso.EUR
                    },
                    FirstTransactionFees = new Money
                    {
                        Amount = 1,
                        Currency = CurrencyIso.EUR
                    },
                    Billing = new Billing
                    {
                        FirstName = "Joe",
                        LastName = "Blogs",
                        Address = new Address
                        {
                            AddressLine1 = "1 MangoPay Street",
                            AddressLine2 = "The Loop",
                            City = "Paris",
                            Region = "Ile de France",
                            PostalCode = "75001",
                            Country = CountryIso.FR
                        }
                    },
                    Shipping = new Shipping
                    {
                        FirstName = "Joe",
                        LastName = "Blogs",
                        Address = new Address
                        {
                            AddressLine1 = "1 MangoPay Street",
                            AddressLine2 = "The Loop",
                            City = "Paris",
                            Region = "Ile de France",
                            PostalCode = "75001",
                            Country = CountryIso.FR
                        }
                    },
                    EndDate = DateTime.Now.AddDays(365),
                    Migration = true,
                    NextTransactionDebitedFunds = new Money
                    {
                        Amount = 12,
                        Currency = CurrencyIso.EUR
                    },
                    NextTransactionFees = new Money
                    {
                        Amount = 1,
                        Currency = CurrencyIso.EUR
                    },
                    FreeCycles = 0
                };

                var result = await this.Api.PayIns.CreateRecurringPayInRegistration(payInPost);

                Assert.NotNull(result);
                Assert.IsTrue(userId == result.CreditedUserId);
                Assert.IsTrue(cardId == result.CardId);
                Assert.IsTrue(wallet.Id == result.CreditedWalletId);
                Assert.NotNull(result.Shipping);
                Assert.NotNull(result.Billing);
                Assert.NotNull(result.FreeCycles);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task Test_PayIns_Get_Recurring()
        {
            try
            {
                var data = await GetNewJohnsWalletWithMoneyAndCardId(1000);
                var cardId = data.Item1;
                var wallet = data.Item2;
                var userId = wallet.Owners.FirstOrDefault();

                var payInPost = new RecurringPayInRegistrationPostDTO
                {
                    AuthorId = userId,
                    CardId = cardId,
                    CreditedUserId = userId,
                    CreditedWalletId = wallet.Id,
                    FirstTransactionDebitedFunds = new Money
                    {
                        Amount = 12,
                        Currency = CurrencyIso.EUR
                    },
                    FirstTransactionFees = new Money
                    {
                        Amount = 1,
                        Currency = CurrencyIso.EUR
                    },
                    Billing = new Billing
                    {
                        FirstName = "Joe",
                        LastName = "Blogs",
                        Address = new Address
                        {
                            AddressLine1 = "1 MangoPay Street",
                            AddressLine2 = "The Loop",
                            City = "Paris",
                            Region = "Ile de France",
                            PostalCode = "75001",
                            Country = CountryIso.FR
                        }
                    },
                    Shipping = new Shipping
                    {
                        FirstName = "Joe",
                        LastName = "Blogs",
                        Address = new Address
                        {
                            AddressLine1 = "1 MangoPay Street",
                            AddressLine2 = "The Loop",
                            City = "Paris",
                            Region = "Ile de France",
                            PostalCode = "75001",
                            Country = CountryIso.FR
                        }
                    },
                    EndDate = DateTime.Now.AddDays(365),
                    Migration = true,
                    NextTransactionDebitedFunds = new Money
                    {
                        Amount = 12,
                        Currency = CurrencyIso.EUR
                    },
                    NextTransactionFees = new Money
                    {
                        Amount = 1,
                        Currency = CurrencyIso.EUR
                    },
                    FreeCycles = 0
                };

                var result = await this.Api.PayIns.CreateRecurringPayInRegistration(payInPost);

                var get = await this.Api.PayIns.GetRecurringPayInRegistration(result.Id);

                Assert.NotNull(get);
                Assert.IsTrue(get.CreditedUserId == result.CreditedUserId);
                Assert.IsTrue(get.CardId == result.CardId);
                Assert.IsTrue(get.CreditedWalletId == result.CreditedWalletId);
                Assert.NotNull(get.Shipping);
                Assert.NotNull(get.Billing);
                Assert.NotNull(get.CurrentState);
                Assert.NotNull(get.FreeCycles);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task Test_PayIns_Update_Recurring()
        {
            try
            {
                var data = await GetNewJohnsWalletWithMoneyAndCardId(1000);
                var cardId = data.Item1;
                var wallet = data.Item2;
                var userId = wallet.Owners.FirstOrDefault();

                var payInPost = new RecurringPayInRegistrationPostDTO
                {
                    AuthorId = userId,
                    CardId = cardId,
                    CreditedUserId = userId,
                    CreditedWalletId = wallet.Id,
                    FirstTransactionDebitedFunds = new Money
                    {
                        Amount = 12,
                        Currency = CurrencyIso.EUR
                    },
                    FirstTransactionFees = new Money
                    {
                        Amount = 1,
                        Currency = CurrencyIso.EUR
                    },
                    Billing = new Billing
                    {
                        FirstName = "Joe",
                        LastName = "Blogs",
                        Address = new Address
                        {
                            AddressLine1 = "1 MangoPay Street",
                            AddressLine2 = "The Loop",
                            City = "Paris",
                            Region = "Ile de France",
                            PostalCode = "75001",
                            Country = CountryIso.FR
                        }
                    },
                    Shipping = new Shipping
                    {
                        FirstName = "Joe",
                        LastName = "Blogs",
                        Address = new Address
                        {
                            AddressLine1 = "1 MangoPay Street",
                            AddressLine2 = "The Loop",
                            City = "Paris",
                            Region = "Ile de France",
                            PostalCode = "75001",
                            Country = CountryIso.FR
                        }
                    },
                    EndDate = DateTime.Now.AddDays(365),
                    Migration = true,
                    NextTransactionDebitedFunds = new Money
                    {
                        Amount = 12,
                        Currency = CurrencyIso.EUR
                    },
                    NextTransactionFees = new Money
                    {
                        Amount = 1,
                        Currency = CurrencyIso.EUR
                    }
                };

                var result = await this.Api.PayIns.CreateRecurringPayInRegistration(payInPost);

                var get = await this.Api.PayIns.GetRecurringPayInRegistration(result.Id);

                var putObject = new RecurringPayInPutDTO
                {
                    Billing = new Billing
                    {
                        Address = new Address
                        {
                            AddressLine1 = "NEW",
                            AddressLine2 = "NEW",
                            City = "NEW",
                            Region = "NEW",
                            PostalCode = "75001",
                            Country = CountryIso.FR
                        },
                        FirstName = "New Name",
                        LastName = "New Last Name"
                    },
                    Shipping = new Shipping
                    {
                        Address = new Address
                        {
                            AddressLine1 = "NEWW",
                            AddressLine2 = "NEWW",
                            City = "NEW",
                            Region = "NEW",
                            PostalCode = "75001",
                            Country = CountryIso.FR
                        },
                        FirstName = "New Name",
                        LastName = "New Last Name"
                    },
                    Status = RecurringPaymentStatus.ENDED
                };

                var put = await this.Api.PayIns.UpdateRecurringPayInRegistration(get.Id, putObject);

                Assert.NotNull(put);
                Assert.IsTrue(put.CreditedUserId == result.CreditedUserId);
                Assert.IsTrue(put.CardId == result.CardId);
                Assert.IsTrue(put.CreditedWalletId == result.CreditedWalletId);
                Assert.NotNull(put.Shipping);
                Assert.NotNull(put.Billing);
                Assert.NotNull(put.CurrentState);
                Assert.AreNotEqual(put.Shipping, get.Shipping);
                Assert.AreNotEqual(put.Billing, get.Billing);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task Test_PayIns_Create_Recurring_CIT()
        {
            try
            {
                var data = await GetNewJohnsWalletWithMoneyAndCardId(1000);
                var cardId = data.Item1;
                var wallet = data.Item2;
                var userId = wallet.Owners.FirstOrDefault();

                var payInPost = new RecurringPayInRegistrationPostDTO
                {
                    AuthorId = userId,
                    CardId = cardId,
                    CreditedUserId = userId,
                    CreditedWalletId = wallet.Id,
                    FirstTransactionDebitedFunds = new Money
                    {
                        Amount = 12,
                        Currency = CurrencyIso.EUR
                    },
                    FirstTransactionFees = new Money
                    {
                        Amount = 1,
                        Currency = CurrencyIso.EUR
                    },
                    Billing = new Billing
                    {
                        FirstName = "Joe",
                        LastName = "Blogs",
                        Address = new Address
                        {
                            AddressLine1 = "1 MangoPay Street",
                            AddressLine2 = "The Loop",
                            City = "Paris",
                            Region = "Ile de France",
                            PostalCode = "75001",
                            Country = CountryIso.FR
                        }
                    },
                    Shipping = new Shipping
                    {
                        FirstName = "Joe",
                        LastName = "Blogs",
                        Address = new Address
                        {
                            AddressLine1 = "1 MangoPay Street",
                            AddressLine2 = "The Loop",
                            City = "Paris",
                            Region = "Ile de France",
                            PostalCode = "75001",
                            Country = CountryIso.FR
                        }
                    },
                    EndDate = DateTime.Now.AddDays(365),
                    Migration = true,
                    NextTransactionDebitedFunds = new Money
                    {
                        Amount = 12,
                        Currency = CurrencyIso.EUR
                    },
                    NextTransactionFees = new Money
                    {
                        Amount = 1,
                        Currency = CurrencyIso.EUR
                    }
                };

                Thread.Sleep(2000);
                var createdPayInRegistration = await this.Api.PayIns.CreateRecurringPayInRegistration(payInPost);

                Assert.NotNull(createdPayInRegistration);
                Assert.IsTrue(userId == createdPayInRegistration.CreditedUserId);
                Assert.IsTrue(cardId == createdPayInRegistration.CardId);
                Assert.IsTrue(wallet.Id == createdPayInRegistration.CreditedWalletId);
                Assert.NotNull(createdPayInRegistration.Shipping);
                Assert.NotNull(createdPayInRegistration.Billing);

                var cit = new RecurringPayInCITPostDTO
                {
                    RecurringPayinRegistrationId = createdPayInRegistration.Id,
                    BrowserInfo = new BrowserInfo
                    {
                        AcceptHeader = "text/html, application/xhtml+xml, application/xml;q=0.9, /;q=0.8",
                        JavaEnabled = true,
                        Language = "FR-FR",
                        ColorDepth = 4,
                        ScreenHeight = 1800,
                        ScreenWidth = 400,
                        JavascriptEnabled = true,
                        TimeZoneOffset = "+60",
                        UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 13_6_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/15E148"
                    },
                    IpAddress = "2001:0620:0000:0000:0211:24FF:FE80:C12C",
                    SecureModeReturnURL = "http://www.my-site.com/returnurl",
                    StatementDescriptor = "lorem",
                    Tag = "custom meta",
                    DebitedFunds = new Money
                    {
                        Amount = 12,
                        Currency = CurrencyIso.EUR
                    },
                    Fees = new Money
                    {
                        Amount = 1,
                        Currency = CurrencyIso.EUR
                    }
                };

                Thread.Sleep(2000);
                var createdCit = await this.Api.PayIns.CreateRecurringPayInRegistrationCIT(cit);

                Assert.NotNull(createdCit);
                Assert.IsTrue(userId == createdCit.CreditedUserId);
                Assert.IsTrue(cardId == createdCit.CardId);
                Assert.IsTrue(wallet.Id == createdCit.CreditedWalletId);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task Test_PayIns_Create_Recurring_MIT()
        {
            try
            {
                var data = await GetNewJohnsWalletWithMoneyAndCardId(1000);
                var cardId = data.Item1;
                var wallet = data.Item2;
                var userId = wallet.Owners.FirstOrDefault();

                var payInPost = new RecurringPayInRegistrationPostDTO
                {
                    AuthorId = userId,
                    CardId = cardId,
                    CreditedUserId = userId,
                    CreditedWalletId = wallet.Id,
                    FirstTransactionDebitedFunds = new Money
                    {
                        Amount = 12,
                        Currency = CurrencyIso.EUR
                    },
                    FirstTransactionFees = new Money
                    {
                        Amount = 1,
                        Currency = CurrencyIso.EUR
                    },
                    Billing = new Billing
                    {
                        FirstName = "Joe",
                        LastName = "Blogs",
                        Address = new Address
                        {
                            AddressLine1 = "1 MangoPay Street",
                            AddressLine2 = "The Loop",
                            City = "Paris",
                            Region = "Ile de France",
                            PostalCode = "75001",
                            Country = CountryIso.FR
                        }
                    },
                    Shipping = new Shipping
                    {
                        FirstName = "Joe",
                        LastName = "Blogs",
                        Address = new Address
                        {
                            AddressLine1 = "1 MangoPay Street",
                            AddressLine2 = "The Loop",
                            City = "Paris",
                            Region = "Ile de France",
                            PostalCode = "75001",
                            Country = CountryIso.FR
                        }
                    },
                    EndDate = DateTime.Now.AddDays(365),
                    Migration = true,
                    NextTransactionDebitedFunds = new Money
                    {
                        Amount = 12,
                        Currency = CurrencyIso.EUR
                    },
                    NextTransactionFees = new Money
                    {
                        Amount = 1,
                        Currency = CurrencyIso.EUR
                    },
                    Frequency = "Daily",
                    FixedNextAmount = true,
                    FractionedPayment = false
                };

                var createdPayInRegistration = await this.Api.PayIns.CreateRecurringPayInRegistration(payInPost);

                Assert.NotNull(createdPayInRegistration);
                Assert.IsTrue(userId == createdPayInRegistration.CreditedUserId);
                Assert.IsTrue(cardId == createdPayInRegistration.CardId);
                Assert.IsTrue(wallet.Id == createdPayInRegistration.CreditedWalletId);
                Assert.NotNull(createdPayInRegistration.Shipping);
                Assert.NotNull(createdPayInRegistration.Billing);

                var cit = new RecurringPayInCITPostDTO
                {
                    RecurringPayinRegistrationId = createdPayInRegistration.Id,
                    BrowserInfo = new BrowserInfo
                    {
                        AcceptHeader = "text/html, application/xhtml+xml, application/xml;q=0.9, /;q=0.8",
                        JavaEnabled = true,
                        Language = "FR-FR",
                        ColorDepth = 4,
                        ScreenHeight = 1800,
                        ScreenWidth = 400,
                        JavascriptEnabled = true,
                        TimeZoneOffset = "+60",
                        UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 13_6_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/15E148"
                    },
                    IpAddress = "2001:0620:0000:0000:0211:24FF:FE80:C12C",
                    SecureModeReturnURL = "http://www.my-site.com/returnURL",
                    StatementDescriptor = "lorem",
                    Tag = "custom meta",
                    DebitedFunds = new Money
                    {
                        Amount = 12,
                        Currency = CurrencyIso.EUR
                    },
                    Fees = new Money
                    {
                        Amount = 1,
                        Currency = CurrencyIso.EUR
                    }
                };

                var createdCit = await this.Api.PayIns.CreateRecurringPayInRegistrationCIT(cit);

                Assert.NotNull(createdCit);
                Assert.IsTrue(userId == createdCit.CreditedUserId);
                Assert.IsTrue(cardId == createdCit.CardId);
                Assert.IsTrue(wallet.Id == createdCit.CreditedWalletId);

                var mit = new RecurringPayInMITPostDTO
                {
                    RecurringPayinRegistrationId = createdPayInRegistration.Id,
                    StatementDescriptor = "lorem",
                    Tag = "custom meta",
                    DebitedFunds = new Money
                    {
                        Amount = 10,
                        Currency = CurrencyIso.EUR
                    },
                    Fees = new Money
                    {
                        Amount = 1,
                        Currency = CurrencyIso.EUR
                    }
                };

                var createdMit = await this.Api.PayIns.CreateRecurringPayInRegistrationMIT(mit);

                Assert.NotNull(createdMit);
                Assert.IsTrue(userId == createdMit.CreditedUserId);
                Assert.IsTrue(cardId == createdMit.CardId);
                Assert.IsTrue(wallet.Id == createdMit.CreditedWalletId);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
        
        /*[Test]
        public async Task Test_CreateCardPreAuthorizedDepositPayIn()
        {
            try
            {
                DepositDTO deposit = await this.CreateNewDeposit();
                var wallet = await this.GetJohnsWallet();
                
                var debitedFunds = new Money();
                debitedFunds.Amount = 1000;
                debitedFunds.Currency = CurrencyIso.EUR;
                
                var fees = new Money();
                fees.Amount = 0;
                fees.Currency = CurrencyIso.EUR;
                
                var dto = new CardPreAuthorizedDepositPayInPostDTO(wallet.Id, debitedFunds, fees, deposit.Id);
                var payIn = await this.Api.PayIns.CreateCardPreAuthorizedDepositPayIn(dto);
				
                Assert.IsNotNull(payIn);
                Assert.AreEqual(TransactionStatus.SUCCEEDED, payIn.Status);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }*/
        
        
         [Test]
        public async Task Test_PayIns_Create_Recurring_MIT_CheckCardInfo()
        {
            try
            {
                var data = await GetNewJohnsWalletWithMoneyAndCardId(1000);
                var cardId = data.Item1;
                var wallet = data.Item2;
                var userId = wallet.Owners.FirstOrDefault();

                var payInPost = new RecurringPayInRegistrationPostDTO
                {
                    AuthorId = userId,
                    CardId = cardId,
                    CreditedUserId = userId,
                    CreditedWalletId = wallet.Id,
                    FirstTransactionDebitedFunds = new Money
                    {
                        Amount = 12,
                        Currency = CurrencyIso.EUR
                    },
                    FirstTransactionFees = new Money
                    {
                        Amount = 1,
                        Currency = CurrencyIso.EUR
                    },
                    Billing = new Billing
                    {
                        FirstName = "Joe",
                        LastName = "Blogs",
                        Address = new Address
                        {
                            AddressLine1 = "1 MangoPay Street",
                            AddressLine2 = "The Loop",
                            City = "Paris",
                            Region = "Ile de France",
                            PostalCode = "75001",
                            Country = CountryIso.FR
                        }
                    },
                    Shipping = new Shipping
                    {
                        FirstName = "Joe",
                        LastName = "Blogs",
                        Address = new Address
                        {
                            AddressLine1 = "1 MangoPay Street",
                            AddressLine2 = "The Loop",
                            City = "Paris",
                            Region = "Ile de France",
                            PostalCode = "75001",
                            Country = CountryIso.FR
                        }
                    },
                    EndDate = DateTime.Now.AddDays(365),
                    Migration = true,
                    NextTransactionDebitedFunds = new Money
                    {
                        Amount = 12,
                        Currency = CurrencyIso.EUR
                    },
                    NextTransactionFees = new Money
                    {
                        Amount = 1,
                        Currency = CurrencyIso.EUR
                    },
                    Frequency = "Daily",
                    FixedNextAmount = true,
                    FractionedPayment = false
                };

                var createdPayInRegistration = await this.Api.PayIns.CreateRecurringPayInRegistration(payInPost);

                Assert.NotNull(createdPayInRegistration);
                Assert.IsTrue(userId == createdPayInRegistration.CreditedUserId);
                Assert.IsTrue(cardId == createdPayInRegistration.CardId);
                Assert.IsTrue(wallet.Id == createdPayInRegistration.CreditedWalletId);
                Assert.NotNull(createdPayInRegistration.Shipping);
                Assert.NotNull(createdPayInRegistration.Billing);

                var cit = new RecurringPayInCITPostDTO
                {
                    RecurringPayinRegistrationId = createdPayInRegistration.Id,
                    BrowserInfo = new BrowserInfo
                    {
                        AcceptHeader = "text/html, application/xhtml+xml, application/xml;q=0.9, /;q=0.8",
                        JavaEnabled = true,
                        Language = "FR-FR",
                        ColorDepth = 4,
                        ScreenHeight = 1800,
                        ScreenWidth = 400,
                        JavascriptEnabled = true,
                        TimeZoneOffset = "+60",
                        UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 13_6_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/15E148"
                    },
                    IpAddress = "2001:0620:0000:0000:0211:24FF:FE80:C12C",
                    SecureModeReturnURL = "http://www.my-site.com/returnURL",
                    StatementDescriptor = "lorem",
                    Tag = "custom meta",
                    DebitedFunds = new Money
                    {
                        Amount = 12,
                        Currency = CurrencyIso.EUR
                    },
                    Fees = new Money
                    {
                        Amount = 1,
                        Currency = CurrencyIso.EUR
                    }
                };

                var createdCit = await this.Api.PayIns.CreateRecurringPayInRegistrationCIT(cit);

                Assert.IsNotNull(createdCit.CardInfo);
                Assert.IsNotNull(createdCit.CardInfo.IssuingBank);
                Assert.IsNotNull(createdCit.CardInfo.Brand);
                Assert.IsNotNull(createdCit.CardInfo.Type);
                Assert.IsNotNull(createdCit.CardInfo.IssuerCountryCode);

                var mit = new RecurringPayInMITPostDTO
                {
                    RecurringPayinRegistrationId = createdPayInRegistration.Id,
                    StatementDescriptor = "lorem",
                    Tag = "custom meta",
                    DebitedFunds = new Money
                    {
                        Amount = 10,
                        Currency = CurrencyIso.EUR
                    },
                    Fees = new Money
                    {
                        Amount = 1,
                        Currency = CurrencyIso.EUR
                    }
                };

                var createdMit = await this.Api.PayIns.CreateRecurringPayInRegistrationMIT(mit);

                Assert.IsNotNull(createdMit.CardInfo);
                Assert.IsNotNull(createdMit.CardInfo.IssuingBank);
                Assert.IsNotNull(createdMit.CardInfo.Brand);
                Assert.IsNotNull(createdMit.CardInfo.Type);
                Assert.IsNotNull(createdMit.CardInfo.IssuerCountryCode);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public async Task Test_Serialize_CardInfo()
        {
            var settings = new JsonSerializerSettings
            {
                Converters = { new CardInfoTypeConverter() }
            };

            var cardInfo = new CardInfo();
            cardInfo.Type = null;
            
            var payIn = new PayInCardDirectDTO();
            payIn.CardInfo = cardInfo;
            
            string json = JsonConvert.SerializeObject(payIn, settings);
            Assert.IsTrue(json.Contains("\"Type\":null"));

            cardInfo.Type = CardInfoType.CREDIT;
            json = JsonConvert.SerializeObject(payIn, settings);
            Assert.IsTrue(json.Contains("\"Type\":\"CREDIT\""));
            
            cardInfo.Type = CardInfoType.CHARGE_CARD;
            json = JsonConvert.SerializeObject(payIn, settings);
            Assert.IsTrue(json.Contains("\"Type\":\"CHARGE CARD\""));
        }
        
        [Test]
        public async Task Test_Dserialize_CardInfo()
        {
            var settings = new JsonSerializerSettings
            {
                Converters = { new CardInfoTypeConverter() }
            };

            string json =
                "{\"CardInfo\":{\"BIN\":null,\"IssuingBank\":null,\"IssuerCountryCode\":0,\"Type\":null,\"Brand\":null,\"SubType\":null}}";
            var deserialized = JsonConvert.DeserializeObject<PayInCardDirectDTO>(json, settings);
            Assert.AreEqual(null, deserialized.CardInfo.Type);
            
            json =
                "{\"CardInfo\":{\"BIN\":null,\"IssuingBank\":null,\"IssuerCountryCode\":0,\"Type\":\"DEBIT\",\"Brand\":null,\"SubType\":null}}";
            deserialized = JsonConvert.DeserializeObject<PayInCardDirectDTO>(json, settings);
            Assert.AreEqual(CardInfoType.DEBIT, deserialized.CardInfo.Type);
            
            json =
                "{\"CardInfo\":{\"BIN\":null,\"IssuingBank\":null,\"IssuerCountryCode\":0,\"Type\":\"CHARGE_CARD\",\"Brand\":null,\"SubType\":null}}";
            deserialized = JsonConvert.DeserializeObject<PayInCardDirectDTO>(json, settings);
            Assert.AreEqual(CardInfoType.CHARGE_CARD, deserialized.CardInfo.Type);
        }
    }
}