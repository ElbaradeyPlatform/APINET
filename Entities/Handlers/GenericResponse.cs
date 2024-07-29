using Entities.DataModels;
using Entities.LinkModels;
using Shared.RequestFeatures;
using System.Dynamic;

namespace Entities.Handlers
{
 
    public class GenericResponse
    {
        public int Code { get; set; }
        public string Message { get; set; } = default!;
        public bool Status { get; set; }
        public string SentDate { get; set; }
      //  public IEnumerable<Entity> Entities { get; set; } = default!;
        public LinkCollectionWrapper<Entity> LinkedEntities { get; set; } = default!;
        //public IEnumerable<ShapedEntity> ShapedEntities { get; set; } = default!;
  //      public IEnumerable<object> Payload { get; set; } = default!;
        public  object Payload { get; set; } = default!;

        public MetaData Pagination { get; set; } = default!;
        //public GenericResponse(string sentDate, IEnumerable<ShapedEntity> shapedEntities, MetaData pagination, string message = "", int statusCode = 200, bool status = true)
        //{
        //    this.Code = statusCode;
        //    this.Message = message;
        //    this.ShapedEntities = shapedEntities;
        //    this.SentDate = sentDate;
        //    this.Pagination = pagination;
        //    this.Status = status;
        //}
        //public GenericResponse(string sentDate, IEnumerable<Entity> entities, MetaData pagination, string message="",int statusCode = 200,bool status = true)
        //{
        //    this.Code = statusCode;
        //    this.Message = message ;
        //    this.Entities = entities;
        //    this.SentDate = sentDate;
        //    this.Pagination = pagination;
        //    this.Status = status;
        //}
        public GenericResponse(string sentDate, object payload, MetaData pagination, string message = "", int statusCode = 200, bool status = true)
        {
            this.Code = statusCode;
            this.Message = message;
            this.Payload = payload;
            this.SentDate = sentDate;
            this.Pagination = pagination;
            this.Status = status;
        }
        public GenericResponse(string sentDate, IEnumerable<object> payload, MetaData pagination, string message = "", int statusCode = 200, bool status = true)
        {
            this.Code = statusCode;
            this.Message = message;
            this.Payload = payload;
            this.SentDate = sentDate;
            this.Pagination = pagination;
            this.Status = status;
        }
    }
}

