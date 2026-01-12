using System.Collections.Generic;
using UnityEngine;

//string[] Electives = {
//    "Hooky 101",
//    "Banjo 101",
//    "Witch Doctor",
//};

public class Degree
{
    public string DegreeName { get; private set; }

    private Dictionary<string, Course> courses = new Dictionary<string, Course>();

    public Degree(string name) { 
        DegreeName = name;
    }

    public Degree(string name, List<string> courseNames) : this(name)
    {
        foreach (var course in courseNames) {
            courses[course] = new Course(course);
        }
    }

    public Course[] GetCourses()
    {
        List<Course> courseList = new List<Course>(courses.Count);
        foreach (Course c in courses.Values)
        {
            courseList.Add(c);
        }
        return courseList.ToArray();
    }

    public void AddCourse(string courseName) {
        courses[courseName] = new Course(courseName);
    }

    public void RemoveCourse(string courseName)
    {
        Course old = courses[courseName];
        string[] prerequisites = old.GetPrerequisites();
        string[] dependencies = old.GetDependents();

        foreach (string p in prerequisites)
        {
            RemoveDependency(p, courseName);
            foreach (string d in dependencies)
            {
                SetDependency(p, d);
            }
        }
        foreach (string d in dependencies)
        {
            RemoveDependency(courseName, d);
        }

        courses.Remove(courseName);
    }

    public void SetDependency(string first, string second)
    {
        courses[second].AddPrerequisite(courses[first]);
    }

    public void RemoveDependency(string first, string second)
    {
        courses[second].RemovePrerequisite(courses[first]);
    }
}
