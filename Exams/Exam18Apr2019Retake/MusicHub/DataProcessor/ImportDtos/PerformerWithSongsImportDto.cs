using System;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace MusicHub.DataProcessor.ImportDtos
{
    [XmlType("Performer")]
    public class PerformerWithSongsImportDto
    {
        [XmlElement("FirstName")]
        [Required]
        [MinLength(3), MaxLength(20)]
        public string FirstName { get; set; }

        [XmlElement("LastName")]
        [Required]
        [MinLength(3), MaxLength(20)]
        public string LastName { get; set; }

        [XmlElement("Age")]
        [Required]
        [Range(18, 70)]
        public int Age { get; set; }

        [XmlElement("NetWorth")]
        [Required]
        [Range(0, double.MaxValue)]
        public decimal NetWorth { get; set; }

        [XmlArray("PerformersSongs")]
        public SongPerformerImportDto[] PerformerSongs { get; set; }
    }
}
