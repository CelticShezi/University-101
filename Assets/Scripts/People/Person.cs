using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public abstract class Person : MonoBehaviour, IPointerClickHandler
{
    public string Name { get; private set; }
    public bool HasArrived { get; private set; }
    protected Schedule PersonalSchedule { get; } = new Schedule();
    protected List<string> firstNames = new List<string>() {"Bob", "Alice", "George", "Geraldine"};
    protected List<string> lastNames = new List<string>() { "Salamaton", "Crocotic", "Blastoceros", "Lobsteroid"};
    protected NavMeshAgent agent;
    protected Vector3 home;
    protected InputAction click;
    private float baseSpeed;
    private float baseAcceleration;
    private float baseAngularSpeed;
    private bool calculatingPath = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Awake()
    {
        
        agent = GetComponent<NavMeshAgent>();
        baseSpeed = agent.speed;
        baseAcceleration = agent.acceleration;
        baseAngularSpeed = agent.angularSpeed;
        home = agent.transform.position;
        Name = $"{firstNames[Random.Range(0, firstNames.Count)]} {lastNames[Random.Range(0, lastNames.Count)]}";
        click = InputSystem.actions.FindAction("Click");
    }

    // Update is called once per frame
    //protected virtual void Update()
    //{
    //    if (click.WasPressedThisFrame()) {
    //        Debug.Log($"You have clicked on {Name}");
    //    }
    //}

    public void OnPointerClick(PointerEventData ped)
    {
        Debug.Log($"You have clicked on {Name}");
    }

    public bool IsAvailable(GameTime startTime, GameTime endTime)
    {
        return PersonalSchedule.IsScheduleFree(startTime, endTime);
    }

    protected bool HasReachedDestination()
    {
        //Debug.Log($"{Name}: {calculatingPath}, {agent.hasPath}");
        if (calculatingPath && agent.hasPath) calculatingPath = false;
        return !calculatingPath && agent.remainingDistance < agent.stoppingDistance;
    }

    protected bool AddToSchedule(GameTime startTime, GameTime endTime, string className)
    {
        if (PersonalSchedule.IsScheduleFree(startTime, endTime))
        {
            PersonalSchedule.Reserve(startTime, endTime, className);
            return true;
        }
        else
        {
            return false;
        }
    }

    protected void SetDestination(Vector3 destination)
    {
        agent.ResetPath();
        if ((destination - transform.position).magnitude < agent.stoppingDistance)
        {
            Debug.Log($"{Name} has already arrived.");
            HasArrived = true;
            return;
        }
        calculatingPath = true;
        if (!agent.SetDestination(destination))
        {
            Debug.Log($"Unable to set destination {destination}");
        }
        HasArrived = false;
        StartCoroutine(MarkAsArrived());
    }

    private IEnumerator MarkAsArrived()
    {
        while (!HasReachedDestination())
        {
            yield return new WaitForSeconds(.25f);
        }
        HasArrived = true;
    }
}
