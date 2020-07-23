using System.Xml.Serialization;

namespace ProductShop.Dtos.Export
{
    public class ExportUsersCountAndUsersDto
    {
        [XmlElement("count")]
        public int Count { get; set; }

        [XmlArray("users")]
        public ExportUserWithAgeFLNameAndProductsDto[] Users { get; set; }
    }
}
