using AutoWrapper.Wrappers;
using Shared.RequestFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Handlers
{
    public class GenericResponse
    {
        public int Code { get; set; }
        public string Message { get; set; } = default!;
        public bool Status { get; set; }
        public string SentDate { get; set; }
        public IEnumerable<object> Payload { get; set; } = default!;
        public MetaData Pagination { get; set; } = default!;
         public GenericResponse(string sentDate, IEnumerable<object> payload, MetaData pagination, string message="",int statusCode = 200,bool status = true)
        {
            this.Code = statusCode;
            this.Message = message ;
            this.Payload = payload;
            this.SentDate = sentDate;
            this.Pagination = pagination;
            this.Status = status;
        }
    }
}

