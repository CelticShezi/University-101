using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Lecture
{
    public GameTime startTime;
    public GameTime endTime;
    public Classroom classroom;
    public string className;
    public Teacher teacher;
    public int MaxStudents { get; private set; }
    
    public Duration Length
    {
        get
        {
            return endTime - startTime;
        }
    }

    public List<Student> students;
    private bool isActive = false;

    public Lecture(Classroom cr, GameTime start, GameTime end, string name, Teacher teacher)
    {
        classroom = cr;
        MaxStudents = classroom.Capacity;
        students = new List<Student>(MaxStudents);
        className = name;
        startTime = start;
        endTime = end;
        this.teacher = teacher;

        GameManager.HourChange += HandleHourChange;

        classroom.ReserveClassroom(start, Length, className);
        this.teacher.AddClassToSchedule(start, Length, className);
    }

    public int FillLecture(Course c, List<Student> possibleStudents)
    {
        for (int i = 0; i < possibleStudents.Count; i++)
        {
            bool eligible = true;
            if (c.GetPrerequisites().Length > 0)
            {
                foreach (string prereq in c.GetPrerequisites())
                {
                    if (!possibleStudents[i].classesTaken.Contains(prereq))
                    {
                        eligible = false;
                        break;
                    }
                }
            }
            if (!eligible)
            {
                continue;
            }
            else if (!EnrollStudent(possibleStudents[i])) break;
        }

        return students.Count;
    }

    public bool EnrollStudent(Student s)
    {
        if (students.Count == MaxStudents)
        {
            return false;
        }

        students.Add(s);
        s.EnrollInClass(className, startTime, endTime);
        return true;
    }

    public void SendToClass()
    {
        while (classroom.Reserved);
        Debug.Log($"Sending {students.Count} of {MaxStudents} students to {className}");
        classroom.Reserved = true;
        foreach (Student s in students)
        {
            s.GoToClass(this);
        }
        teacher.TeachInClassroom(classroom);
        isActive = true;
    }

    public void EndClass()
    {
        foreach (Student s in students)
        {
            s.LeaveClass();
        }
        teacher.EndClass();
        classroom.Reserved = false;
        isActive = false;
    }

    public void Delete()
    {
        GameManager.HourChange -= HandleHourChange;
        foreach (Student s in students)
        {
            s.ReleaseFromClass(className, startTime);
        }
        classroom.ReleaseClassroom(startTime);
        teacher.RemoveClassFromSchedule(startTime);
    }

    private void HandleHourChange(GameTime now)
    {
        if ((now >= endTime || now < startTime) && isActive)
        {
            EndClass();
        } else if (now >= startTime && now < endTime && !isActive)
        {
            SendToClass();
        }
    }

    public override string ToString()
    {
        return $"{startTime}-{endTime}: {className} Class of {students.Count}";
    }
}
