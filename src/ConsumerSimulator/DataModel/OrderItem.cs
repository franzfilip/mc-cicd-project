using Newtonsoft.Json;

namespace ConsumerSimulator
{
    public class OrderItem
    {
        [JsonProperty("product_id")]
        public int ProductId { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }
    }
}
