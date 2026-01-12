using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;

public readonly struct Duration
{
    public Duration(int hour = 0, int minute = 0)
    {
        Hours = hour;
        Minutes = minute;
    }
    public readonly int Hours { get; }
    public readonly int Minutes { get; }
    public readonly override string ToString() => $"{Hours} hour(s) {Minutes} minute(s)";
    public static bool operator <(Duration l, Duration r) => l.Hours == r.Hours ? l.Minutes < r.Minutes : l.Hours < r.Hours;
    public static bool operator >(Duration l, Duration r) => l.Hours == r.Hours ? l.Minutes > r.Minutes : l.Hours > r.Hours;
    public static Duration operator +(Duration l, Duration r)
    {
        int newHour = l.Hours + r.Hours;
        int newMinutes = l.Minutes + r.Minutes;

        while (newMinutes >= 60)
        {
            newHour++;
            newMinutes -= 60;
        }

        return new Duration(newHour, newMinutes);
    }

    public static Duration Hour { get => new Duration(hour: 1); }
    public static Duration Minute { get => new Duration(minute: 1); }
}

public struct GameTime : IEquatable<GameTime>, IComparable<GameTime>
{
    private int hour;
    private int minute;
    public readonly override string ToString()
    {
        return $"{hour:D2}:{minute:D2}";
    }

    public readonly bool IsOnTheHour()
    {
        return minute == 0;
    }

    public static GameTime Midnight { get => new GameTime(); }
    public static GameTime Sentinel { get => new GameTime(-1, -1); }

    public GameTime(int hour = 0, int minute = 0)
    {
        this.hour = hour;
        this.minute = minute;
    }

    public GameTime(string time)
    {
        string[] splitTime = time.Split(':');
        hour = int.Parse(splitTime[0]);
        minute = int.Parse(splitTime[1]);
    }

    public void Add(Duration duration)
    {
        minute += duration.Minutes;
        while (minute >= 60)
        {
            minute -= 60;
            hour++;
        }
        hour += duration.Hours;
        while (hour >= 24)
        {
            hour -= 24;
        }
    }
    public void Sub(Duration duration)
    {
        minute -= duration.Minutes;
        while (minute < 0)
        {
            minute += 60;
            hour--;
        }
        hour -= duration.Hours;
        while (hour < 0)
        {
            hour += 24;
        }
    }

    public static bool operator <(GameTime a, GameTime b) => a.getComparable() < b.getComparable();
    public static bool operator >(GameTime a, GameTime b) => a.getComparable() > b.getComparable();
    public static bool operator <=(GameTime a, GameTime b) => a.getComparable() <= b.getComparable();
    public static bool operator >=(GameTime a, GameTime b) => a.getComparable() >= b.getComparable();
    public static bool operator ==(GameTime a, GameTime b) => a.getComparable() == b.getComparable();
    public static bool operator !=(GameTime a, GameTime b) => a.getComparable() != b.getComparable();
    public static Duration operator -(GameTime a, GameTime b) {
        int hours = a.hour - b.hour;
        int minutes = a.minute - b.minute;
        while (minutes < 0)
        {
            minutes += 60;
            hours--;
        }
        while (hours < 0)
        {
            hours += 24;
        }
        return new Duration(hours, minutes);
    }
    public static GameTime operator+(GameTime gt, Duration d)
    {
        gt.Add(d);
        return gt;
    }

    public static GameTime operator -(GameTime gt, Duration d)
    {
        gt.Sub(d);
        return gt;
    }

    public readonly bool Equals(GameTime other)
    {
        if (other == null) return false;
        return getComparable() == other.getComparable();
    }

    public readonly int CompareTo(GameTime other)
    {
        return getComparable() - other.getComparable();
    }

    private readonly int getComparable() => hour * 60 + minute;

    public readonly override bool Equals(object obj) => Equals((GameTime) obj);
    public readonly override int GetHashCode() => getComparable().GetHashCode();
}
