using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace BookShop.DataProcessor.ImportDto
{
    public class BookFromAuthorImportDto
    {
        [JsonProperty("Id")]
        [Required]
        public int? BookId { get; set; }
    }
}
