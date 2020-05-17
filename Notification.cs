using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Crypto.Bot
{
    public class Notification
    {
        public bool isWorking = false;
        public int StartHour = 11;
        public int EndHour = 23;
        public int Interval = 15;
        [JsonIgnore]
        public Action task;
        [JsonIgnore]
        public Timer timer;
        public void Start()
        {
            isWorking = true;
            MyScheduler.IntervalInMinutes(StartHour, EndHour, Interval, ref timer, task);
        }
        public void Stop()
        {
            isWorking = false;
            timer?.Dispose();
        }
        public void SetTask(Action t)
        {
            task = t;
            if (isWorking)
            {
                Start();
            }
        }
        public Notification(int s, int e, int i, Action t)
        {
            task = t;
            StartHour = s;
            EndHour = e;
            Interval = i;
        }
        public void UpdateParams(int s, int e, int i)
        {
            StartHour = s;
            EndHour = e;
            Interval = i;
        }
    }
}
