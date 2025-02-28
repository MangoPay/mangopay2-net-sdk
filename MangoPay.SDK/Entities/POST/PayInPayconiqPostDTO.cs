namespace MangoPay.SDK.Entities.POST
{
    /// <summary>
    /// Payconiq Post Class
    /// </summary>
    public class PayInPayconiqPostDTO : EntityPostBase
    {
        /// <summary>
        /// A user’s ID
        /// Required
        /// </summary>
        public string AuthorId { get; set; }

        /// <summary>
        /// The user ID who is credited (defaults to the owner of the wallet)
        /// Optional
        /// </summary>
        public string CreditedUserId { get; set; }

        /// <summary>
        /// The ID of the wallet where money will be credited
        /// Required
        /// </summary>
        public string CreditedWalletId { get; set; }

        /// <summary>
        /// Information about the funds that are being debited
        /// Required
        /// </summary>
        public Money DebitedFunds { get; set; }

        /// <summary>
        /// Information about the fees that were taken by the client for this transaction(and where hence transferred to the Client’s platform wallet)
        /// Required
        /// </summary>
        public Money Fees { get; set; }

        /// <summary>
        /// This is the URL where users are automatically redirected after the payment is validated
        /// Required
        /// </summary>
        public string ReturnURL { get; set; }

        /// <summary>
        /// The country of your user (should be BE, NL or LU)
        /// Required
        /// </summary>
        public string Country { get; set; }
        
        /// <summary> A custom description to appear </summary>
        public string StatementDescriptor { get; set; }
    }
}
