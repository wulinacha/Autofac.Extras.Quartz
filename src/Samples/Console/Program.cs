﻿#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014-2018 Alphacloud.Net

#endregion

// ReSharper disable MethodSupportsCancellation

namespace ConsoleScheduler
{
    using System;
    using System.Threading;
    using Autofac;
    using Quartz;
    using Shared.Log.Logging;
    using SimpleService.Configuration;
    using SimpleService.Jobs;

    static class Program
    {
        static ILog _log;

        static void Main(string[] args)
        {
            Bootstrap.InitializeLogger();
            _log = LogProvider.GetLogger(typeof(Program));
            var container = Bootstrap.ConfigureContainer(new ContainerBuilder()).Build();

            Console.WriteLine("This sample demonstrates how to integrate Quartz and Autofac.");
            _log.Info("Starting...");
            try
            {
                container = Bootstrap.ConfigureContainer(new ContainerBuilder()).Build();

                var job = JobBuilder.Create<HeartbeatJob>().WithIdentity("Heartbeat", "Maintenance").Build();
                var trigger = TriggerBuilder.Create()
                    .WithIdentity("Heartbeat", "Maintenance")
                    .StartNow()
                    .WithSchedule(SimpleScheduleBuilder.RepeatSecondlyForever(2)).Build();
                var cts = new CancellationTokenSource();

                var scheduler = container.Resolve<IScheduler>();
                scheduler.ScheduleJob(job, trigger, cts.Token);

                scheduler.Start().Wait();

                Console.WriteLine("======================");
                Console.WriteLine("Press Enter to exit...");
                Console.WriteLine("======================");
                Console.ReadLine();

                cts.Cancel();
                scheduler.Shutdown().Wait();
            }
            catch (Exception ex)
            {
                _log.FatalException("Unhandled exception caught", ex);
            }

            container.Dispose();
        }
    }
}
