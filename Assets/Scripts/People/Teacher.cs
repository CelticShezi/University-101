using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum TeacherStatus
{
    Free = 0,
    Teaching = 1,
}
public class Teacher : Person
{
    public List<string> teachableClasses = new List<string>();

    public float Skill { get; private set; } = 1;
    public TeacherStatus Status { get; private set; } = TeacherStatus.Free;



    // Update is called once per frame
    //void Update()
    //{
        
    //}

    public void TeachInClassroom(Classroom cr)
    {
        Debug.Log($"{Name} is going to teach");
        Status = TeacherStatus.Teaching;
        SetDestination(cr.transform.position);
    }

    public bool IsTeaching()
    {
        return Status == TeacherStatus.Teaching && HasArrived;
    }

    public void EndClass()
    {
        Status = TeacherStatus.Free;
        SetDestination(home);
    }

    public void AddClassToSchedule(GameTime gtStart, Duration classLength, string className)
    {
        PersonalSchedule.Reserve(gtStart, classLength, className);
    }

    public void RemoveClassFromSchedule(GameTime classTime)
    {
        PersonalSchedule.Free(classTime);
    }
}
