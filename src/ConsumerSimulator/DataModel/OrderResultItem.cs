using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ConsumerSimulator
{
    public class OrderResultItem
    {
        [JsonProperty("product_id")]
        public int ProductId { get; set; }
        [JsonProperty("product_uuid")]
        public Guid ProductUUID { get; set; }
        [JsonProperty("warehouse_id")]
        public int WarehouseId { get; set; }
    }
}
