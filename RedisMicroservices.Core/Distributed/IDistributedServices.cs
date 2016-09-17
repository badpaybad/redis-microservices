using System;
using RedisMicroservices.Core.Repository;
using RedisMicroservices.Domain;

namespace RedisMicroservices.Core.Distributed
{
    public interface IDistributedServices
    {
        void Publish<T>(DistributedCommand<T> cmd) where T : class;
        void Subscribe<T>(Action<string, DistributedCommand<T>> callBack) where T : class;

        void PublishEntity<T>(DistributedCommandEntity<T> cmd) where T : class, IEntity;
        void SubscribeEntity<T>(Action<string, DistributedCommandEntity<T>> callBack) where T : class, IEntity;

        void PublishDataModel<T>(DistributedCommandDataModel<T> cmd) where T : class, IDataModel;
        void SubscribeDataModel<T>(Action<string, DistributedCommandDataModel<T>> callBack) where T : class, IDataModel;
    }
}