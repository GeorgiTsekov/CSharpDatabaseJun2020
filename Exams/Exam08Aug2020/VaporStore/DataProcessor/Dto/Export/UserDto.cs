using System.Xml.Serialization;

namespace VaporStore.DataProcessor.Dto.Export
{
    [XmlType("User")]
    public class UserDto
    {
        [XmlAttribute("username")]
        public string username { get; set; }

        [XmlArray("Purchases")]
        public PurchaseDto[] Purchases { get; set; }

        [XmlElement("TotalSpent")]
        public decimal TotalSpent { get; set; }
    }
}
