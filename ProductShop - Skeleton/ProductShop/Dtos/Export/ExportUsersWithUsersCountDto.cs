using System.Xml.Serialization;

namespace ProductShop.Dtos.Export
{
    public class ExportUsersWithUsersCountDto
    {
        [XmlElement("count")]
        public int Count { get; set; }

        [XmlArray("users")]
        public ExportUserWithAgeFLNameAndProductsDto[] ExportUserWithAgeFLNameAndProductsDto { get; set; }
    }
}
