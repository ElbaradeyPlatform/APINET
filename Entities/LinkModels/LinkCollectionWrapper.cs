using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.LinkModels
{
    public class LinkCollectionWrapper<T> //: LinkResourceBase
    {
        public List<T> Entities { get; set; } = new List<T>();
       // public bool HasLinks { get; set; }
        public LinkCollectionWrapper()
        { }
        public LinkCollectionWrapper(List<T> value) => Entities = value;
        public List<Link> Links { get; set; } = new List<Link>();
    }
}
