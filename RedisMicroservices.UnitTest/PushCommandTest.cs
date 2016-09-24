using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RedisMicroservices.Core.Distributed;
using RedisMicroservices.Core.Redis;
using RedisMicroservices.Core.Utils;
using RedisMicroservices.Domain.DataModel;
using RedisMicroservices.Domain.Entity;
using RedisMicroservices.Repository;
using RedisMicroservices.Services;

namespace RedisMicroservices.UnitTest
{
    [TestClass]
    public class RedisTest
    {
        [TestMethod]
        public void Get()
        {
            var total = 0;
            while (true)
            {
                  var xxx = RedisServices.RedisDatabase.ListLeftPop("QueueRedisMicroservices.Domain.DataModel.SampleData");
                if (!xxx.HasValue) break;

                total++;
            Console.WriteLine(xxx);
            }
          
            Console.WriteLine(total);
        }
    }

    [TestClass]
    public class PushCommandTest
    {

        public PushCommandTest()
        {
            RepositoryEngine.Boot();
            ServicesEngine.Boot();
        }

        [TestMethod]
        public void PushRandom()
        {
            var dc=new DistributedServices();

            var sampleData = new SampleData()
            {
                Id=Guid.NewGuid(),
                Name= DateTime.Now.ToString(),
                Version= DateTime.Now.ToString()
            };
            dc.PublishDataModel(new DistributedCommandDataModel<SampleData>(
                sampleData, EntityAction.Insert));

            Console.WriteLine(sampleData.Id);

            Thread.Sleep(2000);
        }

        [TestMethod]
        public void Aaa()
        {
            while (true)
            {
                PushRandom();
                Thread.Sleep(1000);
            }
        }

        [TestMethod]
        public void TestRedisQueue()
        {
            new Thread(() =>
            {
                while (true)
                {
                    var redisValue = "enqueue: " + Guid.NewGuid() + DateTime.Now.ToString();
                    RedisServices.RedisDatabase.ListRightPush("testqueue", redisValue);
                    Console.WriteLine(redisValue);
                    Log.Write(redisValue);
                    Thread.Sleep(500);
                }
            }).Start();
            new Thread(()=> {}).Start();

            new Thread(() =>
            {
                while (true)
                {
                    var redisValue = "dequeue: " + RedisServices.RedisDatabase.ListLeftPop("testqueue");
                    Console.WriteLine(redisValue);
                    Thread.Sleep(3000);
                    Log.Write(redisValue);
                }
            }).Start();
            new Thread(() => { }).Start();
            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }
}
