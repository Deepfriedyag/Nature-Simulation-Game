using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public abstract class AnimalAI : MonoBehaviour
{
    // I can edit all of the values with the [serializeField] attribute in the unity inspector
    [Header("Health")]
    [SerializeField] protected float maxHealth = 100f; // Maximum health
    [SerializeField] protected float healthDecayRate = 5f; // Health decay rate per second when hunger is zero

    [Header("Hunger")]
    [SerializeField] protected float MaxHunger = 100f;
    [SerializeField] protected float HungerDecayRate = 1f;
    [SerializeField] protected float hungerThreshold = 50f; // Hunger % threshold to start hunting
    [SerializeField] protected float SearchFoodRange = 20f;
    [SerializeField] protected float eatRange = 2f;        // Distance within which grass can be eaten

    [Header("Stamina")]
    [SerializeField] protected float maxStamina = 100f; // Maximum stamina
    [SerializeField] protected float staminaRegenRate = 1f; // Stamina regeneration rate per second
    [SerializeField] protected float staminaDrainRate = 5f; // Stamina drain rate per second while moving
    [SerializeField] protected float lowStaminaSpeedMultiplier = 0.5f; // Speed multiplier when stamina is low

    [Header("Mating")]
    [SerializeField] protected float mateRange = 5f; // Range to detect potential mates
    [SerializeField] protected float mateApproachRange = 10f; // Larger range to detect potential mates
    [SerializeField] protected float mateCooldownDuration = 120f; // Cooldown in seconds between mating
    [SerializeField] protected GameObject resultant_animal_prefab; // Prefab to spawn new animals
    [SerializeField] protected float hungerCostForMating = 20f; // Hunger cost for mating
    [SerializeField] protected float staminaCostForMating = 30f; // Stamina cost for mating

    [Header("Aging")]
    [SerializeField] protected float maxAge = 100f; // Maximum age before death
    [SerializeField] protected float agingRate = 0.1f; // Rate of aging, modified by Time.timeScale

    [Header("Predator Detection")]
    [SerializeField] protected float predatorDetectionRange = 10f; // Detection range for predators
    [SerializeField] protected List<string> predatorTags = new List<string>(); // List of predator tags

    [Header("Wandering Behaviour")]
    [SerializeField] protected float wanderRange = 5f; // Range for wandering
    [SerializeField] protected float wanderDelay = 3f; // Delay before scheduling a Wander task while idle

    protected float time_multiplier => Time.timeScale;
    protected AnimalAI potentialMate;
    protected float currentStamina;
    protected float currentHunger;
    protected float currentHealth;

    private float currentAge = 0f;
    private float idleTimer = 0f;
    private float mateCooldownTimer;

    protected bool destinationReached => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    protected bool isDead = false;

    protected NavMeshAgent agent;
    protected Transform target;
    protected PriorityQueue taskQueue;
    protected State currentState;

    protected enum State
    {
        Idle,
        Wander,
        Hunt,
        Flee,
        Eat,
        Sleep,
        Mate
    }

    protected virtual void Start()
    {
        currentHunger = MaxHunger;
        currentStamina = maxStamina;
        currentHealth = maxHealth;
        mateCooldownTimer = mateCooldownDuration;
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

        AddAppropriateTasks();
    }

    private void AddAppropriateTasks()
    {
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
        if (currentHunger / MaxHunger < hungerThreshold / 100f && !IsTaskScheduled(State.Hunt) && currentState != State.Hunt)
        {
            ScheduleTask(State.Hunt, 10f);
        }

        if (mateCooldownTimer > 0)
        {
            mateCooldownTimer -= Time.deltaTime * time_multiplier;
            if (mateCooldownTimer < 0) mateCooldownTimer = 0; // Clamp to zero
        }
        else if (currentHunger > hungerCostForMating && currentStamina > staminaCostForMating &&
                 !IsTaskScheduled(State.Mate) && currentState != State.Mate)
        {
                SearchForMate();
        }

        if (currentStamina <= maxStamina * 0.1f && !IsTaskScheduled(State.Sleep) && currentState != State.Sleep)
        {
            ScheduleTask(State.Sleep, 5f);
        }
    }

    protected virtual void SearchForMate()
    {
        if (mateCooldownTimer > 0)
        {
            Debug.Log($"{gameObject.name}: Mating cooldown active. Time remaining: {mateCooldownTimer:F1}s");
            return;
        }

        Collider[] nearbyAnimals = Physics.OverlapSphere(transform.position, mateApproachRange);
        foreach (var animalCollider in nearbyAnimals)
        {
            AnimalAI otherAnimal = animalCollider.GetComponent<AnimalAI>();
            if (otherAnimal != null && otherAnimal != this && otherAnimal.GetType() == this.GetType() &&
                otherAnimal.currentState != State.Mate && otherAnimal.mateCooldownTimer <= 0 &&
                otherAnimal.currentHunger > hungerCostForMating && otherAnimal.currentStamina > staminaCostForMating)
            {
                potentialMate = otherAnimal; // Set the potential mate
                float distance = Vector3.Distance(transform.position, otherAnimal.transform.position);

                if (distance <= mateRange)
                {
                    Debug.Log($"{gameObject.name} is ready to mate with {otherAnimal.name}");
                    ScheduleTask(State.Mate, 15f);
                    otherAnimal.ScheduleTask(State.Mate, 15f); // Synchronize the mate's task
                    return;
                }
                else
                {
                    Debug.Log($"{gameObject.name} is approaching {otherAnimal.name} for mating.");
                    agent.SetDestination(otherAnimal.transform.position); // Continuously move toward the mate
                }
            }
        }

        if (!IsTaskScheduled(State.Wander))
        {
            ScheduleTask(State.Wander, 1f); // Wander if no mates are found
        }
    }


    protected virtual void Mate()
    {
        if (mateCooldownTimer > 0)
        {
            Debug.Log($"{gameObject.name}: Attempted to mate during cooldown.");
            return;
        }

        // Ensure the mate is still in range
        if (potentialMate == null || Vector3.Distance(transform.position, potentialMate.transform.position) > mateRange)
        {
            Debug.LogWarning($"{gameObject.name}: Mate is out of range. Aborting mating.");
            ScheduleTask(State.Idle, 1f);
            return;
        }

        Debug.Log($"{gameObject.name} is mating with {potentialMate.name}.");

        // Spawn a new animal
        bool isSpawner = Random.value > 0.5f; // Randomly decide which parent spawns the offspring
        if (isSpawner)
        {
            Vector3 spawnPosition = transform.position + Random.insideUnitSphere * 2f;
            spawnPosition.y = transform.position.y;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(spawnPosition, out hit, 2f, NavMesh.AllAreas))
            {
                spawnPosition = hit.position;

                if (resultant_animal_prefab != null)
                {
                    Instantiate(resultant_animal_prefab, spawnPosition, Quaternion.identity);
                    Debug.Log($"A new {gameObject.name} has been spawned!");
                }
                else
                {
                    Debug.LogWarning("Animal prefab is not assigned!");
                }
            }
            else
            {
                Debug.LogWarning($"{gameObject.name}: Failed to spawn offspring on a valid NavMesh position.");
            }
        }

        // Deduct costs and reset cooldown
        currentHunger -= hungerCostForMating;
        currentStamina -= staminaCostForMating;
        mateCooldownTimer = mateCooldownDuration;
        potentialMate = null;
    }




    private void AgeAndHungerDecay()
    {
        // Increment age
        currentAge += agingRate * Time.deltaTime * time_multiplier;

        if (currentAge >= maxAge)
        {
            Die("Old age");
            return; // Stop further processing after death
        }

        // Hunger decay logic
        currentHunger -= HungerDecayRate * Time.deltaTime * time_multiplier;

        if (currentHunger <= 0)
        {
            currentHunger = 0;
            currentHealth -= healthDecayRate * Time.deltaTime * time_multiplier;

            if (currentHealth <= 0)
            {
                Die("Starvation");
                return;
            }
        }
    }

    protected virtual void Sleep()
    {
        currentStamina = Mathf.Min(currentStamina + staminaRegenRate * Time.deltaTime, maxStamina);
        currentHealth = Mathf.Min(currentHealth + healthDecayRate * Time.deltaTime, maxHealth);

        if (currentStamina == maxStamina)
        {
            ScheduleTask(State.Idle, 1f);
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
    }

    private void DetectPredators()
    {
        Collider[] detectedObjects = Physics.OverlapSphere(transform.position, predatorDetectionRange);
        foreach (var detected in detectedObjects)
        {
            if (predatorTags.Contains(detected.tag))
            {
                target = detected.transform;

                if (IsTaskScheduled(State.Flee)) return; // Do not schedule another Flee task if already fleeing
                else ScheduleTask(State.Flee, 50f); // Flee tasks have the highest priority
            }
        }
    }

    protected abstract void SearchForFood(); // different for each animal, overwrite in child classes
    protected abstract void Idle();

    private void Die(string cause)
    {
        isDead = true;
        agent.isStopped = true;
        IngameConsole.Instance.LogMessage($"{gameObject.name} has died from {cause}.");
        Destroy(gameObject); // Destroy the GameObject when dead
    }

    protected virtual void Wander()
    {
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
        if (target == null && !IsTaskScheduled(State.Idle))
        {
            ScheduleTask(State.Idle, 1f);
        }
        else
        {
            Vector3 fleeDirection = (transform.position - target.position).normalized * 10f;
            agent.SetDestination(transform.position + fleeDirection);
            target = null;
        }
    }

    protected virtual void Eat()
    {
        currentHunger = Mathf.Min(currentHunger + 20f, MaxHunger);

        if (IsTaskScheduled(State.Idle))
        {
            Debug.Log("Idle task already scheduled. returned from eat");
            return;
            
        }
        else 
        {
            ScheduleTask(State.Idle, 1f);
        }
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

    protected void ProcessTaskQueue()
    {
        if (taskQueue.Count == 0 && currentState != State.Idle)
        {
            currentState = State.Idle;
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
                case State.Sleep:
                    Sleep();
                    break;
                case State.Mate:
                    Mate();
                    break;
            }
        }
    }

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

    private void OnGUI() // reserved Unity method. renders the specified info on the screen as text
    {
        if (Time.timeScale == 0) return; // Do not render debug info when paused

        Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position);
        if (screenPosition.z > 0)  // Only show if in front of the camera
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 12;

            // Build the debug text with proper formatting and spacing
            string debugText = $"State: {currentState}\n" +
                               $"Age: {currentAge:F1}/{maxAge}\n" +
                               $"Hunger: {currentHunger:F1}/{MaxHunger}\n" +
                               $"Health: {currentHealth:F1}/{maxHealth}\n" +
                               $"Stamina: {currentStamina:F1}/{maxStamina}\n" +
                               $"Idle Timer: {idleTimer:F1}/{wanderDelay} seconds\n" +
                               $"Mating Cooldown: {mateCooldownTimer:F1}/{mateCooldownDuration:F1}s\n" +
                               $"Task Queue: {taskQueue.Count} tasks\n";

            // Display each task's state and priority in the task queue
            List<Task> tasks = taskQueue.GetAllTasks();
            foreach (var task in tasks)
            {
                debugText += $"  - {task.state} (Priority: {task.priority})\n";
            }

            // Display the text
            GUI.Label(new Rect((screenPosition.x - 25), (Screen.height - screenPosition.y - 125), 250, 300), debugText, style);
        }
    }

    private void OnDrawGizmosSelected() // reserved Unity method. visualises the different ranges. for debugging
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, SearchFoodRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, eatRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, predatorDetectionRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, mateRange);
    }

}
