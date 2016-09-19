using System;
using RedisMicroservices.Core.Repository;
using RedisMicroservices.Domain;
using RedisMicroservices.Domain.DataModel;
using RedisMicroservices.Domain.Entity;

namespace RedisMicroservices.Core.Distributed
{
    public interface IDistributedServices
    {
        void Publish<T>(DistributedCommand<T> cmd, Action<Exception> errorCallback = null) where T : class;
        void Subscribe<T>(Action<string, DistributedCommand<T>> callBack, Action<Exception> errorCallback = null) where T : class;

        void PublishEntity<T>(DistributedCommandEntity<T> cmd, Action<Exception> errorCallback = null) where T : class, IEntity;
        void SubscribeEntity<T>(Action<string, DistributedCommandEntity<T>> callBack, Action<Exception> errorCallback = null) where T : class, IEntity;

        void PublishDataModel<T>(DistributedCommandDataModel<T> cmd, Action<Exception> errorCallback = null) where T : class, IDataModel;
        void SubscribeDataModel<T>(Action<string, DistributedCommandDataModel<T>> callBack, Action<Exception> errorCallback = null) where T : class, IDataModel;
    }
}