using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.CompilerServices;

enum StudentState
{
    Sleep = 0,
    Free = 1,
    Class = 2,
    Graduated = 3,
}

public class Student : Person
{
    public List<string> classesNeeded = new List<string>();
    public Dictionary<string, float> classesEnrolled = new Dictionary<string, float>();
    public List<string> classesTaken = new List<string>();
    public static readonly Vector3 SpawnPosition = new Vector3(0, .5f, -9.5f);
    private float learningRate = 1;
    private float gpa = 0;
    private int classCount = 0;

    private CurrentClass cc;
    private StudentState Status = StudentState.Free;
    private Degree degree;
    private GameObject bed;

    // Update is called once per frame
    protected void Update()
    {
        //base.Update();
        switch (Status) {
            case StudentState.Graduated:
                if (HasArrived)
                {
                    Destroy(gameObject);
                }
                break;
            case StudentState.Class:
                if (HasArrived && cc != null && cc.lecture.teacher.IsTeaching())
                {
                    float learningSpeed = Time.deltaTime * GameManager.LearningRate * learningRate * cc.lecture.teacher.Skill * Time.timeScale;
                    string className = cc.lecture.className;
                    classesEnrolled[className] += learningSpeed;
                    if (classesEnrolled[className] > 100) classesEnrolled[className] = 100;
                }
                break;
            default:
                break;
        }
    }

    private void OnDestroy()
    {
        GameManager.Instance.RemoveStudent(this, bed);
    }

    public string PrintSchedule()
    {
        return PersonalSchedule.ToString();
    }

    public void EnrollForDegree(Degree d)
    {
        degree = d;
        foreach (Course c in degree.GetCourses())
        {
            classesNeeded.Add(c.Name);
        }
    }

    public void Graduate()
    {
        SetDestination(SpawnPosition);
        Status = StudentState.Graduated;
    }

    public void DropOut()
    {

    }

    public bool IsEligibleForCourse(Course c)
    {
        if (!classesNeeded.Contains(c.Name) || classesEnrolled.ContainsKey(c.Name)) return false;

        foreach (string p in c.GetPrerequisites())
        {
            if (!classesTaken.Contains(p)) return false;
        }
        return true;
    }

    public bool EnrollInClass(string className, GameTime classStart, GameTime classEnd)
    {
        if (AddToSchedule(classStart, classEnd, className))
        {
            classesEnrolled.Add(className, 0);
            return true;
        }
        return false;
    }

    public void ReleaseFromClass(string className, GameTime lectureTime)
    {
        Debug.Log($"{Name} has completed {className} with a grade of {classesEnrolled[className]}");
        float grade = classesEnrolled[className];
        bool hasPassed = grade >= 70;
        float points = 0;
        if (grade >= 90) points = 4;
        else if (grade >= 80) points = 3;
        else if (grade >= 70) points = 2;
        else if (grade >= 60) points = 1;
        gpa = (gpa * classCount + points) / (classCount + 1);
        classCount++;

        classesEnrolled.Remove(className);
        PersonalSchedule.Free(lectureTime);
        if (hasPassed)
        {
            classesTaken.Add(className);
            classesNeeded.Remove(className);
        }

        Debug.Log($"{Name} Classes needed: {string.Join(", ", classesNeeded)}\nClasses Taken: {string.Join(", ", classesTaken)}");
    }

    public void AssignBed(GameObject bed)
    {
        home = bed.transform.position;
        this.bed = bed;
    }

    public void GoToClass(Lecture l)
    {
        Classroom cr = l.classroom;
        GameObject desk = cr.ReserveDesk();
        if (desk == null)
        {
            Debug.Log("Unable to reserve desk");
            return;
        }
        cc = new CurrentClass(l, desk);
        Status = StudentState.Class;
        SetDestination(desk.transform.position);
    }

    public void GoHome()
    {
        SetDestination(home);
    }

    public void LeaveClass()
    {
        cc.LeaveClass();
        Status = StudentState.Free;
        GoHome();
    }

    public override string ToString()
    {
        return classesNeeded.ToString();
    }

    private class CurrentClass
    {
        public GameObject desk;
        public Lecture lecture;

        public CurrentClass(Lecture l, GameObject desk)
        {
            lecture = l;
            this.desk = desk;
        }

        public void LeaveClass()
        {
            string deskID = desk.GetComponent<IDable>().ID;
            lecture.classroom.ReleaseDesk(deskID);
        }
    }
}
