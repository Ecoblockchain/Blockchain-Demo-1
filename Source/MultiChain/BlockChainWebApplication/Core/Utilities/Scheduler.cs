
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace BlockChainWebApplication.Core.Utilities
{
    public class Scheduler
    {
        public static void Schedule()
        {
            NameValueCollection _properties = new NameValueCollection();
            _properties["quartz.scheduler.instanceName"] = "SeratioBGWorker";
            _properties["quartz.threadPool.threadCount"] = "5";

            ISchedulerFactory _schedulerFactory = new StdSchedulerFactory(_properties);

            IScheduler _scheduler = _schedulerFactory.GetScheduler();
            _scheduler.Start();

            IJobDetail _goldPriceUpdator = JobBuilder.Create<GoldPriceSyncer>()
                    .WithIdentity("_goldPriceUpdator", "group1")
                    .Build();

            IJobDetail _pvScoreUpdator = JobBuilder.Create<PVScoreUpdater>().Build();

            ITrigger _goldPriceUpdatorTrigger = TriggerBuilder.Create()
                .WithIdentity("_goldPriceUpdatorTrigger", "group1")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInHours(1)
                                        .RepeatForever())
                .Build();

            ITrigger _pvScoreUpdateTrigger = TriggerBuilder.Create()
                .WithIdentity("_pvScoreUpdateTrigger", "group1")
                .StartNow()
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(15)
                                        .RepeatForever())
                .Build();

            _scheduler.ScheduleJob(_goldPriceUpdator, _goldPriceUpdatorTrigger);
            _scheduler.ScheduleJob(_pvScoreUpdator, _pvScoreUpdateTrigger);
        }
    }
}