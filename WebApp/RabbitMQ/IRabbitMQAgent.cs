using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.RabbitMQ
{
    public interface IRabbitMqAgent
    {
        public void Start();
        public void Stop();
    }
}
