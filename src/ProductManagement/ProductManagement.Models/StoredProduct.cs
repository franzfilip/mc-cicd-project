using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductManagement.Models
{
    public class StoredProduct
    {
        public long ProductId { get; set; }
        public Product Product { get; set; }
        public long WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }
        public string ProductUUID { get; set; }
    }
}
