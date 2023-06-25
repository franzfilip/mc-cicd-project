using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductManagement.Models
{
    public class OrderResult
    {
        public long Id { get; set; }
        public Guid CustomerId { get; set; }
        public decimal TotalAmount { get; set; }
        public ICollection<OrderResultItem> OrderResultItems { get; set; }
    }
}
