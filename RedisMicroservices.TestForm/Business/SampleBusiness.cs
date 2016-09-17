using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedisMicroservices.Domain;
using RedisMicroservices.Domain.StorageMock;

namespace RedisMicroservices.TestForm.Business
{
    public class SampleBusiness
    {
        public static List<Sample> GetAll()
        {
            //use entity framework or somthing to select data from db
            return SampleMockStorage.All();
        }
    }
}