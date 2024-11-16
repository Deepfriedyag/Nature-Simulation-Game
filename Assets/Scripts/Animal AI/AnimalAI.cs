using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public abstract class AnimalAI : MonoBehaviour
{
    [SerializeField] private float hungerThreshold = 50f; // Hunger % threshold to start hunting
    [SerializeField, HideInInspector] private float baseSearchFoodRange = 15f;

    [SerializeField] private float maxHealth = 100f; // Maximum health
    [SerializeField] private float healthDecayRate = 5f; // Health decay rate per second when hunger is zero

    protected virtual float SearchFoodRange => baseSearchFoodRange;
    protected virtual float MaxHunger => 100f;
    protected virtual float HungerDecayRate => 1f;

    protected float currentHunger;
    protected float currentHealth;
    protected NavMeshAgent agent;
    protected Transform target;

    protected bool isDead = false;
    protected bool isIdle = true;

    protected PriorityQueue taskQueue;

    // States
    protected enum State
    {
        Idle,
        Wander,
        Hunt,
        Flee,
        Eat
    }

    protected State currentState;

    private float idleTimer = 0f; // Timer to track how long the animal has been idle
    private float wanderDelay = 3f; // Delay before scheduling a Wander task while idle

    protected class Task
    {
        public State state;
        public float priority;

        public Task(State state, float priority)
        {
            this.state = state;
            this.priority = priority;
        }
    }

    protected class PriorityQueue
    {
        private List<Task> elements = new List<Task>();

        public int Count => elements.Count;

        public void Enqueue(Task item)
        {
            elements.Add(item);
            elements.Sort((a, b) => b.priority.CompareTo(a.priority)); // Higher priority first
        }

        public Task Dequeue()
        {
            if (elements.Count == 0) return null;

            Task item = elements[0];
            elements.RemoveAt(0);
            return item;
        }

        public List<Task> GetAllTasks()
        {
            return new List<Task>(elements); // Return a copy of the list
        }
    }

    protected virtual void Start()
    {
        currentHunger = MaxHunger;
        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();
        taskQueue = new PriorityQueue();

        // Start in Idle state
        currentState = State.Idle;
        ScheduleTask(State.Idle, 1f);  // Initial idle task
    }

    protected virtual void Update()
    {
        if (isDead) return;

        AgeAndHungerDecay();
        ProcessTaskQueue();

        // Handle idle state logic
        if (currentState == State.Idle)
        {
            idleTimer += Time.deltaTime;

            // Schedule a Wander task if idle for too long
            if (idleTimer >= wanderDelay)
            {
                ScheduleTask(State.Wander, 1f);
                idleTimer = 0f;
            }
        }

        // Schedule a Hunt task if hunger falls below the threshold
        if (currentHunger / MaxHunger < hungerThreshold / 100f && !IsTaskScheduled(State.Hunt))
        {
            ScheduleTask(State.Hunt, 10f); // Hunting is a high-priority task
        }
    }

    protected void AgeAndHungerDecay()
    {
        currentHunger -= HungerDecayRate * Time.deltaTime;

        if (currentHunger <= 0)
        {
            currentHunger = 0;
            currentHealth -= healthDecayRate * Time.deltaTime;

            if (currentHealth <= 0 && !isDead)
            {
                Die("health depletion");
            }
        }
    }

    protected abstract void SearchForFood();

    protected void Die(string cause)
    {
        isDead = true;
        agent.isStopped = true;
        Debug.Log($"{gameObject.name} has died of {cause}.");
        Destroy(gameObject); // Destroy the GameObject when dead
    }

    protected void ScheduleTask(State state, float priority)
    {
        taskQueue.Enqueue(new Task(state, priority));
    }

    protected bool IsTaskScheduled(State state)
    {
        foreach (var task in taskQueue.GetAllTasks())
        {
            if (task.state == state) return true;
        }
        return false;
    }

    private void ProcessTaskQueue()
    {
        if (taskQueue.Count == 0 && currentState != State.Idle)
        {
            // Return to Idle state if no tasks are left
            currentState = State.Idle;
            isIdle = true;
            return;
        }

        if (taskQueue.Count > 0)
        {
            Task nextTask = taskQueue.Dequeue();
            currentState = nextTask.state;
            idleTimer = 0f; // Reset the idle timer when a task is executed

            switch (currentState)
            {
                case State.Idle:
                    Idle();
                    break;
                case State.Wander:
                    Wander();
                    break;
                case State.Hunt:
                    SearchForFood();
                    break;
                case State.Flee:
                    Flee();
                    break;
                case State.Eat:
                    Eat();
                    break;
            }
        }
    }

    protected virtual void Idle()
    {
        isIdle = true;
    }

    protected virtual void Wander()
    {
        isIdle = false;
        Vector3 randomDirection = Random.insideUnitSphere * 5f;
        randomDirection += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, 5f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }

        // Once wandering is complete, return to idle
        ScheduleTask(State.Idle, 1f);
    }

    protected virtual void Flee()
    {
        if (target == null) return;

        Vector3 fleeDirection = (transform.position - target.position).normalized * 10f;
        agent.SetDestination(transform.position + fleeDirection);

        target = null;
        isIdle = true;
    }

    protected virtual void Eat()
    {
        currentHunger = Mathf.Min(currentHunger + 20f, MaxHunger);
        ScheduleTask(State.Idle, 1f); // After eating, return to idle
    }

    // Enhanced Debug Overlay
    private void OnGUI()
    {
        if (Time.timeScale == 0) return; // Do not render debug info when paused

        Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position);
        if (screenPosition.z > 0)  // Only show if in front of the camera
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 14;

            // Display current state, hunger, health, idle timer, and task queue
            string debugText = $"State: {currentState}\n" +
                               $"Hunger: {currentHunger:F1}/{MaxHunger}\n" +
                               $"Health: {currentHealth:F1}/{maxHealth}\n" +
                               $"Idle Timer: {idleTimer:F1}/{wanderDelay} seconds\n" +
                               $"Task Queue: {taskQueue.Count} tasks\n";

            // Display each task's state and priority in the task queue
            List<Task> tasks = taskQueue.GetAllTasks();
            foreach (var task in tasks)
            {
                debugText += $"  - {task.state} (Priority: {task.priority})\n";
            }

            GUI.Label(new Rect(screenPosition.x, Screen.height - screenPosition.y, 200, 100), debugText, style);
        }
    }
}
