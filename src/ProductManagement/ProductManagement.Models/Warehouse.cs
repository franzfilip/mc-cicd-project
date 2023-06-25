using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductManagement.Models
{
    public class Warehouse
    {
        public long Id { get; set; }
        public long Zip { get; set; }

        public ICollection<StoredProduct> StoredProducts { get; set; } = new List<StoredProduct>();
    }
}
