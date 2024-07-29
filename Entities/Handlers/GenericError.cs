using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Handlers
{
 
    public class GenericError
    {
        public string Message { get; set; }
        public string Code { get; set; }
        public bool Status { get; set; }
        public Guid RequestId { get; set; }
        public DateTime SentDate { get; set; }

 
        public GenericError(string message, string code, Guid reqId, DateTime reqDate, bool status = false)
        {
            this.Status = status;
            this.Message = message;
            this.Code = code;
            this.RequestId = reqId;
            this.SentDate = reqDate;
        }
    }
}
