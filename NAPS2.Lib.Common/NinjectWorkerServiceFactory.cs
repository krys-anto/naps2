﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using NAPS2.Recovery;
using NAPS2.Images.Storage;
using NAPS2.Worker;
using Ninject;

namespace NAPS2.DI
{
    public class NinjectWorkerServiceFactory : IWorkerServiceFactory
    {
        private readonly IKernel kernel;

        public NinjectWorkerServiceFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public WorkerContext Create()
        {
            var worker = kernel.Get<WorkerContext>();
            try
            {
                // TODO: Simplify
                worker.Service.Init(((RecoveryStorageManager)FileStorageManager.Current).RecoveryFolderPath);
            }
            catch (EndpointNotFoundException)
            {
                // Retry once
                worker = kernel.Get<WorkerContext>();
                worker.Service.Init(((RecoveryStorageManager)FileStorageManager.Current).RecoveryFolderPath);
            }
            return worker;
        }
    }
}
