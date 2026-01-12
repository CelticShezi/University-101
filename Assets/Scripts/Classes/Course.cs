using System.Collections.Generic;
using UnityEngine;

public class Course
{
    public string Name { get; private set; }
    public Duration ClassLength { get; private set; }

    private List<string> prerequisites = new List<string>();
    private List<string> dependents = new List<string>();

    public Course(string name, int classLength = 2)
    {
        Name = name;
        ClassLength = new Duration(hour: classLength);
    }

    public void AddPrerequisite(Course prerequisite)
    {
        prerequisites.Add(prerequisite.Name);
        prerequisite.dependents.Add(Name);
    }
    public void RemovePrerequisite(Course prerequisite)
    {
        prerequisites.Remove(prerequisite.Name);
        prerequisite.dependents.Remove(Name);
    }

    public string[] GetPrerequisites() { return prerequisites.ToArray(); }
    public string[] GetDependents() { return dependents.ToArray(); }

    public override string ToString()
    {
        string s = $"Class: {Name}";
        if (prerequisites.Count > 0)
        {
            s += $"; Prerequisites: {string.Join(", ", prerequisites)}";
        }
        if (dependents.Count > 0)
        {
            s += $"; Dependents: {string.Join(", ", dependents)}";
        }
        return s;
    }
}
