using Newtonsoft.Json;
using System;

namespace MangoPay.SDK.Core
{
    /// <summary>Response exception class.</summary>
    public class ResponseException : ApplicationException
    {
        /// <summary>Instantiates new ResponseException object.</summary>
        public ResponseException() { }

		/// <summary>Instantiates new ResponseException object.</summary>
		/// <param name="message">JSON data that came as a response from API.</param>
		/// <param name="responseStatusCode">Response status code from API.</param>
		public ResponseException(string message, int responseStatusCode) : base(message) 
        {
            try
            {
                this.ResponseErrorRaw = message;
				this.ResponseStatusCode = responseStatusCode;
                if (message != null) {
                    this.ResponseError = JsonConvert.DeserializeObject<ResponseError>(message);
                }
			}
            catch (JsonException)
            {
                // Intentionally suppress optional deserialize exception. //
            }
        }

        /// <summary>Raw text data that came as response from API.</summary>
        public string ResponseErrorRaw;

        /// <summary>Deserialized response error data.</summary>
        public ResponseError ResponseError = new ResponseError();

		/// <summary>Response status code from API.</summary>
		public int ResponseStatusCode;
	}
}
