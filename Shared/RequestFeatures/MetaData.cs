using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.RequestFeatures
{
  
    public class MetaData
    {
      
        public int CurrentPage { get; set; } =1;
        public int TotalPages { get; set; } = 1;
        public int PageSize { get; set; } = 1;
        public int TotalCount { get; set; } = 1;    
        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
        public int NextPage => HasNext ? CurrentPage + 1 : CurrentPage;
        public int PrevPage => HasPrevious ? CurrentPage - 1 : CurrentPage;
        public MetaData() { }
    }
}
