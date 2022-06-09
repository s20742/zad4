using System.Net;

namespace Transactions.Models
{
    public class MethodResult
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Message { get; set; }
    }
}
