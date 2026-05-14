using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/*
 * Build Classroom/Office/Dorms...
 * Hire Teacher(s)
 * Create Degree and courses
 * Create Electives (Reasearch option?)
 * Assign/Set Class Times
 */

public enum ScheduleActivity
{
    Class = 0,
    Sleep = 1,
};

public class GameManager : MonoBehaviour
{
    public static event System.Action<GameTime> HourChange;
    public static int TimeDelay { get; } = 1;
    //public static float LearningRate { get; } = 20.0f / 24.0f; // Weeklong learning rate
    public static float LearningRate { get; } = 100.0f / 24.0f;
    public TextMeshProUGUI clockText;
    public Student studentPrefab;
    //public int GameSpeed { get; private set; } = 1;

    public static GameManager Instance { get; private set; }

    private Dictionary<GameTime, ScheduleActivity> routine = new Dictionary<GameTime, ScheduleActivity>(24);
    private GameTime currentTime;
    private int currentDay = 0;
    private List<Lecture> activeClasses = new List<Lecture>();
    private List<Degree> degrees = new List<Degree>();
    private List<Student> students = new List<Student>();
    private List<Teacher> teachers = new List<Teacher>();
    private List<Lecture> lectures = new List<Lecture>();
    private List<Classroom> classrooms = new List<Classroom>();
    private Queue<GameObject> availableBeds = new Queue<GameObject>();
    private readonly string[] weekdays = {"Mon", "Tues", "Wed", "Thurs", "Fri"};

    public Degree GetRandomDegree()
    {
        if (degrees.Count == 0) return null;
        return degrees[Random.Range(0, degrees.Count)];
    }

    public List<string> GetDegrees()
    {
        return degrees.Select(d => d.DegreeName).ToList();
    }

    public (GameTime time, int day) GetCurrentTime()
    {
        return (currentTime, currentDay);
    }

    public List<Student> GetEligibleStudentsForClass(Course c)
    {
        return students.FindAll(s => s.IsEligibleForCourse(c));
    }

    public List<Teacher> GetEligibleTeachersForClass(string className)
    {
        return teachers.FindAll(t => t.teachableClasses.Contains(className));
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        currentTime = new GameTime(4);
        for (int i = 0; i < 24; i++)
        {
            ScheduleActivity activity = 8 <= i && i <= 22 ? ScheduleActivity.Class : ScheduleActivity.Sleep;
            routine.Add(new GameTime(i), activity);
        }
    }

    private void Start()
    {
        Invoke("SetUp", 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeGameSpeed(int newSpeed)
    {
        //OnSpeedChange?.Invoke(newSpeed);
        Time.timeScale = newSpeed;
        //GameSpeed = newSpeed;
    }

    public void RemoveStudent(Student s, GameObject bed)
    {
        availableBeds.Enqueue(bed);
        students.Remove(s);
    }

    private void SetUp()
    {
        GameObject[] beds = GameObject.FindGameObjectsWithTag("Bed");
        foreach (GameObject bed in beds)
        {
            availableBeds.Enqueue(bed);
        }

        GameObject[] teacherObjects = GameObject.FindGameObjectsWithTag("Teacher");
        foreach (GameObject teacherObject in teacherObjects)
        {
            teachers.Add(teacherObject.GetComponent<Teacher>());
        }

        GameObject[] cr = GameObject.FindGameObjectsWithTag("Classroom");
        foreach (GameObject c in cr)
        {
            classrooms.Add(c.GetComponent<Classroom>());
        }
        degrees.Add(CreateDegree());

        StartCoroutine(UpdateClock());
        Debug.Log("Done");
    }

    private Degree CreateDegree()
    {
        Debug.Log("Creating Degree");
        List<string> degreeCourses = new List<string> {
            "Apples 101",
            "Apples 102",
            "Bananas 101",
            "Cherries 101",
            "Durians 201",
        };
        Degree d = new Degree("Fruit", degreeCourses);
        /* 
         * A101   B101   C101
         *  |              /
         *  A102          /
         *    \          /
         *     \        /
         *      \      /
         *        D201
         */
        d.SetDependency("Apples 101", "Apples 102");
        d.SetDependency("Apples 102", "Durians 201");
        d.SetDependency("Cherries 101", "Durians 201");

        return d;
    }

    private void GetClasses()
    {
        Debug.Log("Getting classes");
        (GameTime, GameTime)[] classPeriods = GetTimesForRoutine(ScheduleActivity.Class);
        foreach (Course c in degrees.SelectMany(d => d.GetCourses()))
        {
        RepeatCourse:
            Debug.Log($"Scheduling class {c.Name}");
            List<Teacher> possibleTeachers = GetEligibleTeachersForClass(c.Name);
            if (possibleTeachers.Count == 0) continue; 
            List<Student> possibleStudents = GetEligibleStudentsForClass(c);
            if (possibleStudents.Count == 0) continue;
            foreach ((GameTime startOfPeriod, GameTime endOfPeriod) in classPeriods)
            {
                Duration offset = new Duration(0, 0);
                GameTime classStart;
                Classroom eac;
                List<Teacher> openTeachers;
                List<Student> openStudents;
                
                do
                {
                     System.Func<Classroom, GameTime> cmpClassroom = (cr => cr.ClassroomSchedule.GetFirstOpening(c.ClassLength, startOfPeriod + offset, endOfPeriod));
                    // Get Earliest Available Classroom
                    eac = classrooms.Aggregate((min, next) => cmpClassroom(min) <= cmpClassroom(next) ? min : next);
                    classStart = cmpClassroom(eac);
                    openTeachers = possibleTeachers.FindAll(t => t.IsAvailable(classStart, classStart + c.ClassLength));
                    openStudents = possibleStudents.FindAll(s => s.IsAvailable(classStart, classStart + c.ClassLength));
                    offset = classStart - startOfPeriod + Duration.Hour;
                } while ((openTeachers.Count == 0 || openStudents.Count == 0) && classStart != GameTime.Sentinel);
                if (classStart == GameTime.Sentinel) continue;

                Teacher t = openTeachers[0];

                Lecture l = new Lecture(eac, classStart, classStart + c.ClassLength, c.Name, t);
                int studentsEnrolled = l.FillLecture(c, openStudents);
                if (studentsEnrolled == 0)
                {
                    l.Delete();
                }
                else
                {
                    lectures.Add(l);
                    if (studentsEnrolled == l.MaxStudents)
                    {
                        goto RepeatCourse;
                    }
                }
            }
        }
    }
    private void CheckForGraduatesAndDropouts()
    {
        foreach (Student s in students)
        {

        }
    }

    private (GameTime, GameTime)[] GetTimesForRoutine(ScheduleActivity sa)
    {
        Debug.Log($"Getting Times for Routine {sa}");
        List<(GameTime, GameTime)> times = new List<(GameTime, GameTime)>();
        GameTime startTime = GameTime.Sentinel;
        GameTime endTime = GameTime.Sentinel;
        foreach (GameTime gt in routine.Keys)
        {
            if (routine[gt] == sa)
            {
                if (startTime == GameTime.Sentinel)
                {
                    startTime = gt;
                }
                endTime = gt;
            } else if (startTime != GameTime.Sentinel)
            {
                times.Add((startTime, endTime));
                startTime = GameTime.Sentinel;
                endTime = GameTime.Sentinel;
            }
        }
        if (startTime != GameTime.Sentinel)
        {
            times.Add((startTime, endTime));
        }

        return times.ToArray();
    }

    private void SpawnStudents(int studentsToSpawn)
    {
        Debug.Log($"Spawning {studentsToSpawn} students");
        for (int i = 0; i < studentsToSpawn; i++)
        {
            Student s = Instantiate<Student>(studentPrefab, Student.SpawnPosition, studentPrefab.transform.rotation);
            s.EnrollForDegree(GetRandomDegree());
            if (availableBeds.Count == 0) break;
            GameObject bed = availableBeds.Dequeue();
            s.AssignBed(bed);
            s.GoHome();
            students.Add(s);
        }
        GetClasses();
    }

    private IEnumerator UpdateClock()
    {
        while (true)
        {
            yield return new WaitForSeconds((float) TimeDelay / Time.timeScale);
            Duration interval = new Duration(0, 5);
            currentTime += interval;
            
            if (currentTime.IsOnTheHour())
            {
                HourChange?.Invoke(currentTime);
            }
            // Reset classes and reschedule at the end of term (current term 1 day)
            if (currentTime == GameTime.Midnight)
            {
                currentDay = (currentDay + 1) % weekdays.Length;
                foreach (Lecture l in lectures)
                {
                    l.Delete();
                }
                lectures.Clear();
                GetClasses();
            }
            // Spawn new students to fill beds if any are empty
            if (currentDay == 0 && currentTime == new GameTime(hour: 6))
            {
                SpawnStudents(availableBeds.Count);
            }
            clockText.text = $"{weekdays[currentDay]} {currentTime}";
        }
    }
}
