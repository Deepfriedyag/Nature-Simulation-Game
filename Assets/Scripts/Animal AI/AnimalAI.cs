using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

// abstract class for implementing different animal AIs, provides a base framework for various animal behaviors.
public abstract class AnimalAI : MonoBehaviour
{
    [Header("Health")] // this attribute groups the following variables under a header in the Unity inspector. useful for staying organised
    [SerializeField] protected float max_health = 100f;
    [SerializeField] protected float starving_health_decay_rate = 5f;

    [Header("Hunger")]
    [SerializeField] protected float max_hunger = 100f;
    [SerializeField] protected float hunger_decay_rate = 1f;
    [SerializeField] protected float hunger_threshold = 50f; // hunger threshold to start hunting
    [SerializeField] protected float search_food_range = 20f;
    [SerializeField] protected float eat_range = 2f;

    [Header("Stamina")]
    [SerializeField] protected float max_stamina = 100f;
    [SerializeField] protected float stamina_regen_rate = 1f;
    [SerializeField] protected float stamina_drain_rate = 5f; // stamina drain rate per second while in certain states
    [SerializeField] protected float low_stamina_speed_multiplier = 0.5f;

    [Header("Mating")]
    [SerializeField] protected float mate_range = 0.5f; // range at which the animal can mate with another
    [SerializeField] protected float mate_approach_range = 10f; // range to detect potential mates
    [SerializeField] protected float mate_cooldown_duration = 120f;
    [SerializeField] protected float mating_hunger_cost = 20f;
    [SerializeField] protected float mating_stamina_cost = 30f;
    [SerializeField] protected GameObject resultant_animal_prefab; // what animal to spawn as the result of mating

    [Header("Aging")]
    [SerializeField] protected float max_age = 100f; // the animal dies upon reaching this age
    [SerializeField] protected float aging_rate = 0.1f;

    [Header("Predator Detection")]
    [SerializeField] protected float predator_detection_range = 10f;
    [SerializeField] protected List<string> predator_tags = new List<string>(); // list of animals that are considered predators

    [Header("Wandering Behaviour")]
    [SerializeField] protected float wander_range = 5f;
    [SerializeField] protected float wander_delay = 3f; // timer to schedule a wander task after being idle for too long

    protected float time_multiplier => Time.timeScale; // time multiplier to control the speed of the simulation of animals to be on par with the rest of the game
    protected float current_stamina;
    protected float current_hunger;
    protected float current_health;
    protected float mate_cooldown_timer;
    protected bool is_destination_reached => !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    protected bool is_dead = false;

    protected AnimalAI potential_mate;
    protected NavMeshAgent agent;
    protected Transform target;
    protected PriorityQueue task_queue;
    protected State current_state;

    private float current_age = 0f;
    private float idle_timer = 0f;

    public enum State
    {
        Idle,
        Wander,
        Hunt,
        Flee,
        Eat,
        Sleep,
        Mate
    }

    protected virtual void Start() // reserved Unity method. called once when the script is first loaded
    {
        // initialise some variables
        current_hunger = max_hunger;
        current_stamina = max_stamina;
        current_health = max_health;
        mate_cooldown_timer = mate_cooldown_duration;
        agent = GetComponent<NavMeshAgent>();
        task_queue = new PriorityQueue();

        // start with an idle task
        current_state = State.Idle;
        ScheduleTask(State.Idle, 1f);
    }

    protected virtual void Update() // reserved Unity method. called once per frame. contains the main logic for the animal AI
    {
        if (is_dead) return; // do not update if the animal is dead

        // update the animal's stats and behaviour
        AgeAndHungerDecay();
        ManageStamina();
        DetectPredators();
        ProcessTaskQueue();

        AddAppropriateTasks();
    }

    private void AddAppropriateTasks() // adds appropriate tasks to the priority queue based on the animal's current state and stats
    {
        // schedule a sleep task if stamina falls below 10%
        if (current_stamina <= max_stamina * 0.1f && !IsTaskScheduled(State.Sleep) && current_state != State.Sleep)
        {
            ScheduleTask(State.Sleep, 15f);
        }

        // schedule a hunt task if hunger falls below the threshold
        if (current_hunger / max_hunger < hunger_threshold / 100f && !IsTaskScheduled(State.Hunt) && current_state != State.Hunt)
        {
            ScheduleTask(State.Hunt, 10f);
        }

        // search for mates if the animal meets the requirements. SearchForMate() method then schedules a mate task if a potential mate is found
        if (mate_cooldown_timer > 0)
        {
            mate_cooldown_timer -= Time.deltaTime * time_multiplier;
            if (mate_cooldown_timer < 0) mate_cooldown_timer = 0; // clamp to zero
        }
        else if (current_hunger > mating_hunger_cost && current_stamina > mating_stamina_cost &&
                 !IsTaskScheduled(State.Mate) && current_state != State.Mate)
        {
            SearchForMate();
        }

        // schedule idle if no tasks are left
        if (task_queue.count == 0)
        {
            ScheduleTask(State.Idle, 1f);
        }

    }

    protected virtual void SearchForMate() // searches for potential mates within the vicinity, schedules a mate task if a suitable mate is found
    {
        if (mate_cooldown_timer > 0) return;

        Collider[] nearby_animals = Physics.OverlapSphere(transform.position, mate_approach_range); // find all animals within the approach range
        foreach (var animal_collider in nearby_animals)
        {
            AnimalAI other_animal = animal_collider.GetComponent<AnimalAI>();

            // check if the found animal is a suitable mate
            if (other_animal != null && other_animal != this && other_animal.GetType() == this.GetType() && other_animal.mate_cooldown_timer <= 0 &&
                other_animal.current_hunger > mating_hunger_cost && other_animal.current_stamina > mating_stamina_cost)
            {
                // synchronize "mate" state between the two animals
                if (current_state != State.Mate)
                {
                    ScheduleTask(State.Mate, 9f);
                }

                potential_mate = other_animal;
                potential_mate.potential_mate = this;
                return;  // we found a mate, so no need to schedule wander anymore
            }
        }

        // if no mates found, proceed with wandering to search mates
        if (!IsTaskScheduled(State.Wander) && current_state != State.Wander)
        {
            ScheduleTask(State.Wander, 1f);
        }
    }

    protected virtual void Mate() // spawns offspring if the animal is in range of the potential mate
    {
        if (mate_cooldown_timer > 0)
        {
            return;
        }

        if (potential_mate != null)
        {
            // ensure that both animals are still in the mating state
            if (potential_mate.current_state != State.Mate)
            {
                potential_mate = null;
                return;
            }

            float distance = Vector3.Distance(transform.position, potential_mate.transform.position);

            // update destination while out of range
            if (distance > mate_range)
            {
                agent.SetDestination(potential_mate.transform.position);
                return;
            }

            Vector3 midpoint = (transform.position + potential_mate.transform.position) / 2f;

            // spawn offspring at the midpoint of the two parent animals
            if (NavMesh.SamplePosition(midpoint, out NavMeshHit nav_hit, 1f, NavMesh.AllAreas))
            {
                Vector3 spawnPosition = nav_hit.position;

                if (resultant_animal_prefab != null)
                {
                    Instantiate(resultant_animal_prefab, spawnPosition, Quaternion.identity);
                    IngameConsole.Instance.LogMessage($"A new {gameObject.name} was born");
                }
                else
                {
                    Debug.LogWarning("Animal prefab is not assigned!");
                }
            }
            else
            {
                Debug.LogWarning($"{gameObject.name}: Failed to spawn offspring. No valid NavMesh position at the midpoint.");
            }

            // deduct costs, reset cooldowns and clear potential mate
            current_hunger -= mating_hunger_cost;
            current_stamina -= mating_stamina_cost;
            mate_cooldown_timer = mate_cooldown_duration;

            potential_mate.mate_cooldown_timer = potential_mate.mate_cooldown_duration;
            potential_mate = null;
        }
    }

    private void AgeAndHungerDecay() // handles the aging and hunger decay logic
    {
        // increment age
        current_age += aging_rate * Time.deltaTime * time_multiplier;

        if (current_age >= max_age)
        {
            Die("Old age");
            return; // stop further processing after death
        }

        // hunger decay logic
        current_hunger -= hunger_decay_rate * Time.deltaTime * time_multiplier;

        if (current_hunger <= 0)
        {
            current_hunger = 0;
            current_health -= starving_health_decay_rate * Time.deltaTime * time_multiplier;

            if (current_health <= 0)
            {
                Die("Starvation");
                return;
            }
        }
    }
    
    protected virtual void Sleep() // regenerates stamina and health while in the sleeping state
    {
        current_stamina = Mathf.Min(current_stamina + stamina_regen_rate * Time.deltaTime * 10, max_stamina);
        current_health = Mathf.Min(current_health + starving_health_decay_rate * Time.deltaTime, max_health);

        // check if stamina is fully restored
        if (current_stamina >= max_stamina)
        {
            // transition back to idle once the task is complete
            ScheduleTask(State.Idle, 1f);
        }
    }

    private void ManageStamina() // manages the stamina of the animal based on its current state
    {
        if (agent.velocity.sqrMagnitude > 0.1f) // if the animal is moving
        {
            if (current_state == State.Hunt || current_state == State.Flee) // drain stamina while hunting or fleeing
            {
                current_stamina -= stamina_drain_rate * Time.deltaTime * time_multiplier;
            }

            if (current_stamina <= 0)
            {
                current_stamina = 0;
                agent.speed *= low_stamina_speed_multiplier; // reduce speed when stamina is depleted
            }
        }
    }

    private void DetectPredators() // detects predators within the vicinity and schedules a flee task if found
    {
        Collider[] detected_predators = Physics.OverlapSphere(transform.position, predator_detection_range);
        foreach (var detected in detected_predators)
        {
            if (predator_tags.Contains(detected.tag))
            {
                target = detected.transform;

                if (IsTaskScheduled(State.Flee)) return; // do not schedule another Flee task if already fleeing
                else ScheduleTask(State.Flee, 50f); // flee tasks have the highest priority
            }
        }
    }

    protected abstract void SearchForFood(); // the behaviour for seeking out food. different for each animal, overwrite in child classes

    protected virtual void Idle() // the default idle behaviour for the animal. regenerates stamina and schedules a wander task if idle for too long
    {
        // reset the timer only if it's a new transition to Idle
        if (current_state != State.Idle)
        {
            idle_timer = 0f;
        }

        idle_timer += Time.deltaTime * time_multiplier;

        // schedule a wander task if idle for too long
        if (idle_timer >= wander_delay)
        {
            idle_timer = 0f;
            ScheduleTask(State.Wander, 1f);
        }

        current_stamina = Mathf.Min(current_stamina + stamina_regen_rate * Time.deltaTime, max_stamina);
    }


    private void Die(string cause) // handles the death of the animal
    {
        is_dead = true;
        agent.isStopped = true;
        IngameConsole.Instance.LogMessage($"{gameObject.name} has died from {cause}.");
        Destroy(gameObject); // destroy the GameObject (delete itself) when dead
    }

    protected virtual void Wander() // the default wandering behaviour for the animal. moves to a random location within a specified range
    {
        Vector3 random_direction = UnityEngine.Random.insideUnitSphere * wander_range;
        random_direction += transform.position;

        if (NavMesh.SamplePosition(random_direction, out NavMeshHit hit, wander_range, NavMesh.AllAreas)) // check if the random position is on the NavMesh so the animal doesn't get stuck or stand still
        {
            agent.SetDestination(hit.position);
        }

    }

    protected virtual void Flee() // the default fleeing behaviour for the animal. runs away from the detected predator in the opposite direction
    {
        if (target == null && !IsTaskScheduled(State.Idle))
        {
            ScheduleTask(State.Idle, 1f);
        }
        else
        {
            Vector3 flee_direction = (transform.position - target.position).normalized * 10f;
            agent.SetDestination(transform.position + flee_direction);
            target = null;
        }
    }

    protected virtual void Eat() // the default eating behaviour for the animal. restores hunger and schedules an idle task after eating
    {
        current_hunger = Mathf.Min(current_hunger + 20f, max_hunger);

        if (IsTaskScheduled(State.Idle))
        {
            return;
        }
        else
        {
            ScheduleTask(State.Idle, 1f);
        }
    }

    protected void ScheduleTask(State state, float priority) // schedules a task with a specified state and priority
    {
        task_queue.Enqueue(new Task(state, priority));
    }

    protected bool IsTaskScheduled(State state) // checks if a task with the specified state is already scheduled
    {
        foreach (var task in task_queue.GetAllTasks())
        {
            if (task.state == state) return true;
        }
        return false;
    }

    protected void ProcessTaskQueue()
    {
        // do nothing if there are no tasks in the queue
        if (task_queue.count == 0) return;

        // get the next task in the queue
        Task next_task = task_queue.GetAllTasks()[0];

        // transition to Sleep if it has a higher priority than the current state
        if (current_state != State.Sleep && next_task.state == State.Sleep)
        {
            current_state = State.Sleep;
            task_queue.Dequeue(); // remove the Sleep task from the queue
            Sleep();
            return;
        }

        // Allow Hunt to be interrupted by Sleep
        if (current_state == State.Hunt && next_task.priority > 15f && next_task.state == State.Sleep)
        {
            current_state = next_task.state;
            task_queue.Dequeue();
            Sleep();
            return;
        }

        // Prevent state change until the current task is complete
        if ((current_state == State.Mate && potential_mate != null && Vector3.Distance(transform.position, potential_mate.transform.position) > mate_range) ||
            (current_state == State.Wander && !is_destination_reached) ||
            (current_state == State.Flee && !is_destination_reached) ||
            (current_state == State.Hunt && agent.hasPath && !is_destination_reached))
        {
            return;
        }

        // Dequeue and execute the next task
        next_task = task_queue.Dequeue();
        current_state = next_task.state;

        switch (current_state)
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

    protected class Task // a class to represent a task with a state and priority
    {
        public State state;
        public float priority;

        public Task(State state, float priority) // constructor to initialize the task
        {
            this.state = state;
            this.priority = priority;
        }
    }

    protected class PriorityQueue // class to represent the priority queue of tasks
    {
        private List<Task> elements = new List<Task>();

        public int count => elements.Count;

        public void Enqueue(Task item) // adds a task to the queue. sorts the tasks based on priority
        {
            elements.Add(item);
            elements.Sort((a, b) => b.priority.CompareTo(a.priority)); // higher priority first
        }

        public Task Dequeue() // removes and returns the task with the highest priority
        {
            if (elements.Count == 0) return null;

            Task item = elements[0];
            elements.RemoveAt(0);
            return item;
        }

        public List<Task> GetAllTasks() // returns all of the tasks in the queue
        {
            return new List<Task>(elements);
        }
    }

    // these allow for controlled access to class data by using get and set accessors. these values are used in my SaveGameHandler class to save and load the game state
    public string CurrentState
    {
        get { return current_state.ToString(); } // convert the enum to a string
    }

    public float CurrentHealth
    {
        get { return current_health; }
        set { current_health = Mathf.Clamp(value, 0, max_health); } // clamps the value between 0 and <max_health>
    }

    public float CurrentHunger
    {
        get { return current_hunger; }
        set { current_hunger = Mathf.Clamp(value, 0, max_hunger); }
    }

    public float CurrentAge
    {
        get { return current_age; }
        set { current_age = Mathf.Clamp(value, 0, max_age); }
    }

    public float MatingCooldownTimer
    {
        get { return mate_cooldown_timer; }
        set { mate_cooldown_timer = Mathf.Clamp(value, 0, mate_cooldown_duration); }
    }

    private void OnGUI() // reserved Unity method. renders the specified info on the screen as text
    {
        if (Time.timeScale == 0) return; // do not render debug info when paused

        Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position);
        if (screenPosition.z > 0)  // only show if in front of the camera
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 12;

            // build the debug text with proper formatting and spacing
            string debugText = $"Name: {gameObject.name}\n" +
                               $"State: {current_state}\n" +
                               $"Age: {current_age:F1}/{max_age}\n" +
                               $"Hunger: {current_hunger:F1}/{max_hunger}\n" +
                               $"Health: {current_health:F1}/{max_health}\n" +
                               $"Stamina: {current_stamina:F1}/{max_stamina}\n" +
                               $"Idle Timer: {idle_timer:F1}/{wander_delay} seconds\n" +
                               $"Mating Cooldown: {mate_cooldown_timer:F1}/{mate_cooldown_duration:F1}s\n" +
                               $"Task Queue: {task_queue.count} tasks\n";

            // display each task's state and priority in the task queue
            List<Task> tasks = task_queue.GetAllTasks();
            foreach (var task in tasks)
            {
                debugText += $"  - {task.state} (Priority: {task.priority})\n";
            }

            // display the text
            GUI.Label(new Rect((screenPosition.x - 30), (Screen.height - screenPosition.y - 150), 250, 300), debugText, style);
        }
    }

    private void OnDrawGizmosSelected() // reserved Unity method. visualises the different ranges. for debugging during development
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, search_food_range);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, eat_range);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, predator_detection_range);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, mate_range);
    }
}
