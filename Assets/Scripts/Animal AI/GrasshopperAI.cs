using UnityEngine;
using UnityEngine.AI;

public class GrasshopperAI : AnimalAI
{
    [SerializeField] private float searchFoodRange = 10f; // Grasshopper-specific range for detecting food
    [SerializeField] private float eatRange = 2f;        // Distance within which grass can be eaten
    [SerializeField] private float wanderRange = 5f;     // Range for wandering

    [SerializeField] private float huntSpeed = 4f;       // Speed during hunting
    [SerializeField] private float wanderSpeed = 2f;     // Speed during wandering

    private bool hasQueuedWander = false;                // Prevent multiple wander tasks

    protected override float SearchFoodRange => searchFoodRange;

    private void OnDrawGizmosSelected()
    {
        // Visualize the SearchFoodRange in the Scene view
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, SearchFoodRange);

        // Visualize the eatRange in the Scene view
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, eatRange);
    }

    protected override void Wander()
    {
        isIdle = false;
        agent.speed = wanderSpeed; // Set speed for wandering
        Vector3 randomDirection = Random.insideUnitSphere * wanderRange;
        randomDirection += transform.position;
        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRange, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            Debug.Log($"{gameObject.name} is wandering to a new location.");
        }

        // Reset the wander queue flag after the task is scheduled
        hasQueuedWander = false;
    }

    protected override void SearchForFood()
    {
        agent.speed = huntSpeed; // Set speed for hunting
        Collider[] foodItems = Physics.OverlapSphere(transform.position, SearchFoodRange, LayerMask.GetMask("Grass"));
        GameObject nearestFood = null;
        float closestDistance = Mathf.Infinity;
        Vector3 closestNavMeshPoint = Vector3.zero;

        foreach (var food in foodItems)
        {
            float distance = Vector3.Distance(transform.position, food.transform.position);

            // Find the closest NavMesh point to the grass object
            if (NavMesh.SamplePosition(food.transform.position, out NavMeshHit navHit, 1f, NavMesh.AllAreas))
            {
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    nearestFood = food.gameObject;
                    closestNavMeshPoint = navHit.position;
                }
            }
        }

        if (nearestFood != null)
        {
            target = nearestFood.transform;
            agent.SetDestination(closestNavMeshPoint);
            Debug.Log($"{gameObject.name} is moving to grass: {nearestFood.name}, located at {closestDistance} units.");
            ScheduleTask(State.Eat, 20f); // Add Eat task to follow Hunt
        }
        else if (!hasQueuedWander && agent.remainingDistance <= 0.1f && !agent.pathPending)
        {
            // Queue a high-priority Wander task only after reaching the destination
            Debug.Log($"{gameObject.name} found no grass. Queuing a wandering task to search...");
            ScheduleTask(State.Wander, 30f); // Higher priority than SearchForFood
            hasQueuedWander = true;
        }
    }

    protected override void Eat()
    {
        if (target != null && Vector3.Distance(transform.position, target.position) <= eatRange)
        {
            // Destroy the grass object
            Destroy(target.gameObject);

            // Restore hunger
            currentHunger = Mathf.Min(currentHunger + 20f, MaxHunger);
            Debug.Log($"{gameObject.name} ate grass: {target.name} and restored hunger.");

            target = null;
        }
        else
        {
            Debug.Log($"{gameObject.name} is not within eating range of the grass.");
        }

        // After eating, queue Idle task if none exists
        if (!IsTaskScheduled(State.Idle))
        {
            ScheduleTask(State.Idle, 1f);
        }
    }

    protected override void Idle()
    {
        isIdle = true;

        // Prevent multiple idle tasks
        if (!IsTaskScheduled(State.Idle))
        {
            Debug.Log($"{gameObject.name} is idle. Scheduling Wander task.");
            ScheduleTask(State.Wander, 1f);
        }
    }
}
