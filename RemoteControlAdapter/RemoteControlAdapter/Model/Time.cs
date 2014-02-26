using System;
using Livet;

namespace RemoteControlAdapter.Model
{
    public class Time : NotificationObject
    {
        public Time() { }

        public Time(int hour, int minute, int second)
        {
            this.Hour = hour;
            this.Minute = minute;
            this.Second = second;
        }

        private int hour;
        public int Hour
        {
            get
            {
                return this.hour;
            }
            set
            {
                if (value < 0 || 24 <= value) throw new ArgumentException();

                if (this.hour != value)
                {
                    this.hour = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private int minute;
        public int Minute
        {
            get
            {
                return this.minute;
            }
            set
            {
                if (value < 0 || 60 <= value) throw new ArgumentException();

                if (this.minute != value)
                {
                    this.minute = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        private int second;
        public int Second
        {
            get
            {
                return this.second;
            }
            set
            {
                if (value < 0 || 60 <= value) throw new ArgumentException();

                if (this.second != value)
                {
                    this.second = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public static Time Zero
        {
            get
            {
                return new Time(0, 0, 0);
            }
        }

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
