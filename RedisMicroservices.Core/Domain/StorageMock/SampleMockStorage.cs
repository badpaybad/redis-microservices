using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RedisMicroservices.Core.Domain.StorageMock
{
    public static class SampleMockStorage
    {
        static List<Sample> Data = new List<Sample>();

        public static void Add(Sample entity)
        {
            try
            {
                Data.Add(entity);

                SaveChange();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static List<Sample> All()
        {
            try
            {
                string readToEnd;
                var combine = Path.Combine("d:/", "sample.json");
                if (!File.Exists(combine))
                {
                    Data = new List<Sample>();
                    return Data;
                }
                using (var sr = new StreamReader(combine))
                {
                    readToEnd = sr.ReadToEnd();
                }
                Data = JsonConvert.DeserializeObject<List<Sample>>(readToEnd);

                return Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new List<Sample>();
            }
        }

        static void SaveChange()
        {
            var serializeObject = JsonConvert.SerializeObject(Data);
            var combine = Path.Combine("d:/", "sample.json");
            using (var sw = new StreamWriter(combine,false))
            {
                sw.Write(serializeObject);
            }
        }
    }
}