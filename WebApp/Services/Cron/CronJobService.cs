﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using Microsoft.Extensions.Hosting;
using Timer = System.Timers.Timer;

namespace WebApp.Services.Cron
{
    public abstract class CronJobService : BackgroundService
    {
        private Timer _timer;
        private readonly CronExpression _expression;

        protected CronJobService(string expression)
        {
            _expression = CronExpression.Parse(expression);
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await ScheduleAsync(cancellationToken);
        }

        protected virtual async Task ScheduleAsync(CancellationToken cancellationToken)
        {
            var next = _expression.GetNextOccurrence(DateTimeOffset.Now, TimeZoneInfo.Utc);
            if (next.HasValue)
            {
                var delay = next.Value - DateTimeOffset.Now;
                if (delay.TotalMilliseconds < 0)
                {
                    await ScheduleAsync(cancellationToken);
                }

                _timer = new Timer(delay.TotalMilliseconds);
                _timer.Elapsed += async (sender, args) =>
                {
                    _timer.Dispose();
                    _timer = null;

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await ExecuteAsync(cancellationToken);
                    }

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await ScheduleAsync(cancellationToken);
                    }
                };
                _timer.Start();
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Stop();
            _timer = null;
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer?.Dispose();
                _timer = null;
            }
            base.Dispose();
        }
    }
}