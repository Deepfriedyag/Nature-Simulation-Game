using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public abstract class AnimalAI : MonoBehaviour
{
    [Header("Health and Hunger")]
    [SerializeField] protected float maxHealth = 100f; // Maximum health
    [SerializeField] protected float healthDecayRate = 5f; // Health decay rate per second when hunger is zero
    [SerializeField] protected float hungerThreshold = 50f; // Hunger % threshold to start hunting
    [SerializeField, HideInInspector] protected float baseSearchFoodRange = 15f;

    [Header("Stamina")]
    [SerializeField] protected float maxStamina = 100f; // Maximum stamina
    [SerializeField] protected float staminaRegenRate = 1f; // Stamina regeneration rate per second
    [SerializeField] protected float staminaDrainRate = 5f; // Stamina drain rate per second while moving
    [SerializeField] protected float lowStaminaSpeedMultiplier = 0.5f; // Speed multiplier when stamina is low

    [Header("Predator Detection")]
    [SerializeField] protected float predatorDetectionRange = 10f; // Detection range for predators
    [SerializeField] protected List<string> predatorTags = new List<string>(); // List of predator tags

    [SerializeField] protected float wanderRange = 5f; // Range for wandering

    protected bool destinationReached => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    protected float currentStamina;

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

    protected float idleTimer = 0f; // Timer to track how long the animal has been idle
    protected float wanderDelay = 3f; // Delay before scheduling a Wander task while idle

    // Time scaling multiplier based on game time
    protected float time_multiplier => Time.timeScale;

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
        currentStamina = maxStamina;
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
        ManageStamina();
        DetectPredators();
        ProcessTaskQueue();

        // Handle idle state logic
        if (currentState == State.Idle)
        {
            idleTimer += Time.deltaTime * time_multiplier;

            // Schedule a Wander task if idle for too long
            if (idleTimer >= wanderDelay)
            {
                ScheduleTask(State.Wander, 1f);
                idleTimer = 0f;
            }
        }
        else if (taskQueue.GetAllTasks().Count == 0)
        {
            ScheduleTask(State.Idle, 1f); // Return to Idle state if no tasks are left
        }

        // Schedule a Hunt task if hunger falls below the threshold
        if (currentHunger / MaxHunger < hungerThreshold / 100f && !IsTaskScheduled(State.Hunt))
        {
            ScheduleTask(State.Hunt, 10f); // Hunting is a high-priority task
        }
    }

    protected void AgeAndHungerDecay()
    {
        currentHunger -= HungerDecayRate * Time.deltaTime * time_multiplier;

        // ADD AGING []

        if (currentHunger <= 0)
        {
            currentHunger = 0;
            currentHealth -= healthDecayRate * Time.deltaTime * time_multiplier;

            if (currentHealth <= 0)
            {
                Die("Starvation");
            }
        }
    }

    private void ManageStamina()
    {
        if (agent.velocity.sqrMagnitude > 0.1f) // If the animal is moving
        {
            if (currentState == State.Hunt || currentState == State.Flee)
            {
                currentStamina -= staminaDrainRate * Time.deltaTime * time_multiplier;
            }

            if (currentStamina <= 0)
            {
                currentStamina = 0;
                agent.speed *= lowStaminaSpeedMultiplier; // Reduce speed when stamina is depleted
            }
        }
        else
        {
            currentStamina = Mathf.Min(currentStamina + staminaRegenRate * Time.deltaTime * time_multiplier, maxStamina);
        }
    }

    private void DetectPredators()
    {
        Collider[] detectedObjects = Physics.OverlapSphere(transform.position, predatorDetectionRange);
        foreach (var detected in detectedObjects)
        {
            if (predatorTags.Contains(detected.tag))
            {
                Debug.Log($"{gameObject.name} detected a predator: {detected.name}!");
                ScheduleTask(State.Flee, 50f); // Flee tasks have the highest priority
                target = detected.transform;
                return;
            }
        }
    }

    protected abstract void SearchForFood();

    protected void Die(string cause)
    {
        isDead = true;
        agent.isStopped = true;
        IngameConsole.Instance.LogMessage($"{gameObject.name} has died from {cause}.");
        Destroy(gameObject); // Destroy the GameObject when dead
    }

    protected void ScheduleTask(State state, float priority)
    {
        Debug.Log($"{gameObject.name} scheduling task: {state} with priority {priority}");
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
            Debug.Log($"{gameObject.name} has no tasks. Transitioning to Idle.");
            currentState = State.Idle;
            isIdle = true;
            return;
        }

        if (taskQueue.Count > 0)
        {
            // Only switch states if the current task is complete or not movement-based
            if ((currentState == State.Wander && !destinationReached) ||
                (currentState == State.Flee && !destinationReached))
            {
                return; // Do not dequeue the next task until the current movement task is done
            }

            Task nextTask = taskQueue.Dequeue();
            Debug.Log($"{gameObject.name} is switching to {nextTask.state} state.");

            currentState = nextTask.state;
            idleTimer = 0f;

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

    protected void Wander()
    {
        isIdle = false; // The animal is no longer idle

        Vector3 randomDirection = Random.insideUnitSphere * wanderRange;
        randomDirection += transform.position;
        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRange, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
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

    // Debug Overlay
    private void OnGUI()
    {
        if (Time.timeScale == 0) return; // Do not render debug info when paused

        Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position);
        if (screenPosition.z > 0)  // Only show if in front of the camera
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 14;

            // Build the debug text with proper formatting and spacing
            string debugText = $"State: {currentState}\n" +
                               $"Hunger: {currentHunger:F1}/{MaxHunger}\n" +
                               $"Health: {currentHealth:F1}/{maxHealth}\n" +
                               $"Stamina: {currentStamina:F1}/{maxStamina}\n" +
                               $"Idle Timer: {idleTimer:F1}/{wanderDelay} seconds\n" +
                               $"Task Queue: {taskQueue.Count} tasks\n";

            // Display each task's state and priority in the task queue
            List<Task> tasks = taskQueue.GetAllTasks();
            foreach (var task in tasks)
            {
                debugText += $"  - {task.state} (Priority: {task.priority})\n";
            }

            // Display the text
            GUI.Label(new Rect(screenPosition.x, Screen.height - screenPosition.y, 200, 200), debugText, style);
        }
    }

}
