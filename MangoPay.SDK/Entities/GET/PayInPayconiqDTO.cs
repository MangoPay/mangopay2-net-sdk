namespace MangoPay.SDK.Entities.GET
{
    public class PayInPayconiqDTO : PayInDTO
    {
        public string DeepLinkURL { get; set; }

        public string ReturnURL { get; set; }

        public string RedirectURL { get; set; }

        public string ExpirationDate { get; set; }
        
        public string StatementDescriptor { get; set; }
        
        public string Country { get; set; }
        
        public string QRCodeURL { get; set; }
    }
}
