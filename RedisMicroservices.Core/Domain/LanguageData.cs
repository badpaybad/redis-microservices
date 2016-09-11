using System;

namespace RedisMicroservices.Core.Domain
{
    public class LanguageData : IDataModel
    {
        public Guid Id { get; set; }

        public string Code { get; set; }
    }
}