using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

readonly struct Booking
{
    public readonly GameTime startTime;
    public readonly GameTime endTime;
    public readonly string EventName;
    public Booking(GameTime start, GameTime end, string eventName = "untitled")
    {
        startTime = start;
        endTime = end;
        EventName = eventName;
    }
    public override string ToString() => $"{EventName}: {startTime} - {endTime}";
    public readonly bool HasConflict(Booking other)
    {
        return !((startTime < endTime && endTime <= other.startTime && other.startTime < other.endTime)
                || (endTime <= other.startTime && other.startTime < other.endTime && other.endTime <= startTime)
                || (other.startTime < other.endTime && other.endTime <= startTime && startTime < endTime)
                || (other.endTime <= startTime && startTime < endTime && endTime <= other.startTime));
    }
}
public class Schedule
{
    private SortedList<GameTime, Booking> scheduleList = new SortedList<GameTime, Booking>();
    public override string ToString() => string.Join(",\n", scheduleList.Values);

    public void Reserve(GameTime start, Duration duration, string name)
    {

        Booking b = new Booking(start, start + duration, name);
        scheduleList.Add(start, b);
    }

    public void Reserve(GameTime start, GameTime end, string name)
    {
        Booking b = new Booking(start, end, name);
        scheduleList.Add(start, b);
    }

    public void Free(GameTime time)
    {
        scheduleList.Remove(time);
    }

    public GameTime GetFirstOpening(Duration d) => GetFirstOpening(d, GameTime.Midnight, GameTime.Midnight);
    public GameTime GetFirstOpening(Duration d, GameTime start) => GetFirstOpening(d, start, start);

    public GameTime GetFirstOpening(Duration d, GameTime gtStart, GameTime gtEnd)
    {
        if (d.Hours == 0 && d.Minutes == 0)
        {
            return GameTime.Sentinel;
        }
        bool needsLoop = gtEnd <= gtStart;
        if (gtStart != gtEnd)
        {
            gtEnd.Sub(d);
        }
        Duration length = gtEnd - gtStart;
        
        if (new Duration() < length && length < d)
        {
            Debug.Log($"Window ({gtStart} - {gtEnd}) too tight for given duration {d}");
            return GameTime.Sentinel;
        }
        if (scheduleList.Count == 0)
        {
            return gtStart;
        }

        GameTime openTime = getFirstOpenTime(gtStart);
        GameTime desiredEndTime = openTime + d;
        int index = getNextIndex(openTime);
        if (index == -1) {
            return gtStart;
        }

        while (openTime < gtEnd || needsLoop)
        {
            Booking possibleBooking = new Booking(openTime, desiredEndTime);
            Booking b = scheduleList.Values[index];

            if (!possibleBooking.HasConflict(b))
            {
                return openTime;
            }

            if (b.endTime < openTime) {
                needsLoop = false;
            }
            openTime = b.endTime;
            desiredEndTime = b.endTime + d;
            index = ++index % scheduleList.Count;
        }

        return GameTime.Sentinel;
    }

    public bool IsScheduleFree(GameTime startTime, GameTime endTime)
    {
        Booking timeSlot = new Booking(startTime, endTime);
        foreach (Booking b in scheduleList.Values)
        {
            if (timeSlot.HasConflict(b))
            {
                return false;
            }
        }

        return true;
    }

    private GameTime getFirstOpenTime(GameTime gt)
    {
        foreach (Booking b in scheduleList.Values)
        {
            if (b.startTime > gt)
            {
                break;
            }
            if (b.endTime > gt)
            {
                return b.endTime;
            }
        }

        return gt;
    }

    private int getNextIndex(GameTime gt)
    {
        for (int i = 0; i < scheduleList.Keys.Count; i++)
        {
            if (scheduleList.Keys[i] >= gt)
            {
                return i;
            }
        }

        return scheduleList.Count > 0 ? 0 : -1;
    }
}
