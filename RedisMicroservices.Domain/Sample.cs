using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedisMicroservices.Domain
{
    [Table("Sample")]
    public class Sample : IEntity
    {
        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string Version { get; set; }
    }
}