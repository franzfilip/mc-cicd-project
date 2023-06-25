using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductManagement.Models
{
    public class OrderResultItem
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public Product Product { get; set; }
        public string ProductUUID { get; set; }
        public long WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; }
        public long OrderResultId { get; set; }
        public OrderResult OrderResult { get; set; }
    }
}
