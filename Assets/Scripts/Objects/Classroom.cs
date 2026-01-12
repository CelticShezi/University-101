using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class Classroom : MonoBehaviour
{
    public enum DeskStatus { Open = 0, Reserved = 1 };
    public bool Reserved { get; set; }
    private Dictionary<string, DeskInfo> desks = new Dictionary<string, DeskInfo>();
    //private Dictionary<GameTime, bool> openings;
    public Schedule ClassroomSchedule { get; } = new Schedule();

    public override string ToString()
    {
        string s = $"Schedule: {ClassroomSchedule}\n";
        s += "Desks: {\n";
        foreach (string k in desks.Keys)
        {
            s += $"{k}: {desks[k]}\n";
        }
        return s + "}";
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.CompareTag("Desk"))
            {
                string id = child.GetComponent<IDable>().ID;
                desks[id] = new DeskInfo(child.gameObject);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Desk"))
        {
            desks[other.GetComponent<IDable>().ID] = new DeskInfo(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Desk"))
        {
            desks.Remove(other.GetComponent<IDable>().ID);
        }
    }

    public GameObject ReserveDesk()
    {
        foreach (string deskID in desks.Keys)
        {
            DeskInfo info = desks[deskID];
            if (info.status == DeskStatus.Open)
            {
                info.status = DeskStatus.Reserved;
                return info.desk;
            }
        }
        return null;
    }

    public void ReserveClassroom(GameTime start, Duration d, string eventName)
    {
        ClassroomSchedule.Reserve(start, d, eventName);
    }

    public void ReleaseClassroom(GameTime start)
    {
        ClassroomSchedule.Free(start);
    }

    public void ReleaseDesk(string deskID)
    {
        desks[deskID].status = DeskStatus.Open;
    }

    public int Capacity
    {
        get { return desks.Count; }
    }

    private class DeskInfo
    {
        public DeskStatus status;
        public GameObject desk;
        public string id;

        public DeskInfo(GameObject desk)
        {
            this.desk = desk;
            this.id = desk.GetComponent<IDable>().ID;
            this.status = DeskStatus.Open;
        }

        public override string ToString() {
            return id + ": " + status.ToString();
        }
    }
}


