using AutoWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Handlers
{
 
    public class MapResponse
    {
        [AutoWrapperPropertyMap(Prop.Result)]
        public object Data { get; set; }
        [AutoWrapperPropertyMap(Prop.StatusCode)]
        public int Code { get; set; }
        [AutoWrapperPropertyMap(Prop.Message)]
        public string Message { get; set; }

        [AutoWrapperPropertyMap(Prop.IsError)]
        public bool Status { get; set; }
        [AutoWrapperPropertyMap(Prop.ResponseException)]
        public GenericError  Error { get; set; }

    }
}
