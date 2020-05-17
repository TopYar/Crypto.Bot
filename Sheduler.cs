using System;
using System.Collections.Generic;
using System.Threading;

namespace Crypto.Bot
{
    public class Sheduler
    {
        private static Sheduler _instance;

        private Sheduler() { }

        public static Sheduler Instance => _instance ?? (_instance = new Sheduler());

        public void ScheduleTask(int startHour, int endHour, double intervalInHour, ref Timer t, Action task)
        {
            DateTime now = DateTime.Now;
            DateTime firstRun = new DateTime(now.Year, now.Month, now.Day, startHour, 0, 0, 0);
            // Вычисляем время первого запуска таймера
            while (now > firstRun)
            {
                firstRun = firstRun.AddHours(intervalInHour);
            }
            // Вычисляем оставшееся время до первого запуска
            TimeSpan timeToGo = firstRun - now;

            // Если оставшееся время отрицательно, то запустить таймер надо немедленно
            if (timeToGo <= TimeSpan.Zero)
            {
                timeToGo = TimeSpan.Zero;
            }

            t = new Timer(x =>
            {
                DateTime time1 = DateTime.Now;
                DateTime time2 = DateTime.Now;
                time2 = time2.AddSeconds(-5);
                // Если время начала и конец совпадают, то считаем что интервал составляет целые сутки
                if (startHour == endHour)
                {
                    startHour = 0;
                    endHour = 24;
                }
                // Если текущее время входит в интервал, заданный пользователем, то выполняем задачу
                if (time1.Hour >= startHour && time2.Hour < endHour)
                    task.Invoke();
            }, null, timeToGo, TimeSpan.FromHours(intervalInHour));
        }
    }
    public static class MyScheduler
    {
        public static void IntervalInSeconds(int hour, int sec, double interval, ref Timer t, Action task)
        {
            interval = interval / 3600;
            Sheduler.Instance.ScheduleTask(hour, sec, interval, ref t, task);
        }
        public static void IntervalInMinutes(int hour, int min, double interval, ref Timer t, Action task)
        {
            interval = interval / 60;
            Sheduler.Instance.ScheduleTask(hour, min, interval, ref t, task);
        }
        public static void IntervalInHours(int hour, int min, double interval, ref Timer t, Action task)
        {
            Sheduler.Instance.ScheduleTask(hour, min, interval, ref t, task);
        }
        public static void IntervalInDays(int hour, int min, double interval, ref Timer t, Action task)
        {
            interval = interval * 24;
            Sheduler.Instance.ScheduleTask(hour, min, interval, ref t, task);
        }

    }
}