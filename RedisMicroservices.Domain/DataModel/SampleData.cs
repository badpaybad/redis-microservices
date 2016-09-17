using System;

namespace RedisMicroservices.Domain.DataModel
{
    public class SampleData: IDataModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string Version { get; set; }

        public string LanguageCode { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
