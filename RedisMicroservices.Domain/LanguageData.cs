using System;

namespace RedisMicroservices.Domain
{
    public class LanguageData : IDataModel
    {
        public Guid Id { get; set; }

        public string Code { get; set; }
    }
}