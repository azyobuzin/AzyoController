using System;

namespace RemoteControlAdapter.Model
{
    public struct Time
    {
        public Time(int hour, int minute, int second) : this()
        {
            this.Hour = hour;
            this.Minute = minute;
            this.Second = second;
        }

        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }

        public static readonly Time Zero = new Time(0, 0, 0);

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(Time a, Time b)
        {
            return a.Hour == b.Hour && a.Minute == b.Minute && a.Second == b.Second;
        }

        public static bool operator !=(Time a, Time b)
        {
            return a.Hour != b.Hour || a.Minute != b.Minute || a.Second != b.Second;
        }

        public static bool operator <(Time a, Time b)
        {
            if (a.Hour == b.Hour)
            {
                if (a.Minute == b.Minute)
                {
                    return a.Second < b.Second;
                }
                else
                {
                    return a.Minute < b.Minute;
                }
            }
            else
            {
                return a.Hour < b.Hour;
            }
        }

        public static bool operator >(Time a, Time b)
        {
            if (a.Hour == b.Hour)
            {
                if (a.Minute == b.Minute)
                {
                    return a.Second > b.Second;
                }
                else
                {
                    return a.Minute > b.Minute;
                }
            }
            else
            {
                return a.Hour > b.Hour;
            }
        }

        public static bool operator <=(Time a, Time b)
        {
            return a == b || a < b;
        }

        public static bool operator >=(Time a, Time b)
        {
            return a == b || a > b;
        }

        public static bool IsInTimeSpan(DateTime target, Time start, Time end)
        {
            var time = new Time(target.Hour, target.Minute, target.Second);
            if (start <= end)
                return start <= time && time <= end;
            else
                return start <= time || time <= end;
        }
    }
}
