using MangoPay.SDK.Core;
using MangoPay.SDK.Core.Enumerations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace MangoPay.SDK.Entities.POST
{
    /// <summary>Refund for Transfer POST entity.</summary>
    public class RefundTransferPostDTO : EntityPostBase
    {
        public RefundTransferPostDTO(string authorId, Money fees, Money debitedFunds)
        {
            AuthorId = authorId;
            Fees = fees;
            DebitedFunds = debitedFunds;
        }

        /// <summary>Author identifier.</summary>
        public string AuthorId { get; set; }
        
        /// <summary>Debited funds.</summary>
        public Money DebitedFunds { get; set; }
        
        /// <summary>Fees.</summary>
        public Money Fees { get; set; }
    }
}
