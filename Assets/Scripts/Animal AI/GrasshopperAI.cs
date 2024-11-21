using UnityEngine;
using UnityEngine.AI;

public class GrasshopperAI : AnimalAI
{
    [SerializeField] private float searchFoodRange = 10f; // Grasshopper-specific range for detecting food
    [SerializeField] private float eatRange = 2f;        // Distance within which grass can be eaten
    [SerializeField] private float wanderRange = 5f;     // Range for wandering

    protected override float SearchFoodRange => searchFoodRange;

    private void OnDrawGizmosSelected()
    {
        // Visualize the SearchFoodRange in the Scene view
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, SearchFoodRange);

        // Visualize the eatRange in the Scene view
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, eatRange);

        // Visualize predator detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, predatorDetectionRange);
    }

    protected override void Wander()
    {
        isIdle = false; // The animal is no longer idle

        Vector3 randomDirection = Random.insideUnitSphere * wanderRange;
        randomDirection += transform.position;
        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRange, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            Debug.Log($"{gameObject.name} is wandering to a new location within range: {wanderRange}.");
        }
    }


    protected override void SearchForFood()
    {
        if (currentState != State.Hunt) return; // Ensure the grasshopper is locked in the Hunt state

        Collider[] foodItems = Physics.OverlapSphere(transform.position, SearchFoodRange, LayerMask.GetMask("Grass"));
        GameObject nearestFood = null;
        float closestDistance = Mathf.Infinity;
        Vector3 closestNavMeshPoint = Vector3.zero;

        foreach (var food in foodItems)
        {
            float distance = Vector3.Distance(transform.position, food.transform.position);

            if (NavMesh.SamplePosition(food.transform.position, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
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
            Debug.Log($"{gameObject.name} is hunting for grass: {nearestFood.name}.");

            // Use Physics.CheckSphere to determine if the food is within eatRange
            bool isInEatingRange = Physics.CheckSphere(transform.position, eatRange, LayerMask.GetMask("Grass"));

            if (isInEatingRange)
            {
                Debug.Log($"{gameObject.name} is in eating range of grass: {nearestFood.name}. Scheduling Eat task.");
                ScheduleTask(State.Eat, 20f);
            }
        }
        else if (!IsTaskScheduled(State.Wander))
        {
            // If no grass is found, transition to Wander
            Debug.Log($"{gameObject.name} found no grass. Switching to Wander.");
            ScheduleTask(State.Wander, 30f);
        }

    }

    protected override void Eat()
    {
        try
        {
            if (target == null)
            {
                throw new System.Exception("Target is null. Unable to eat grass.");
            }

            Debug.Log($"{gameObject.name} is eating grass: {target.name}.");

            // Destroy the grass object
            Destroy(target.gameObject);

            // Restore hunger
            currentHunger = Mathf.Min(currentHunger + 20f, MaxHunger);

            // Reset the target and schedule Idle
            target = null;
            ScheduleTask(State.Idle, 1f);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"{gameObject.name} encountered an error while eating: {ex.Message}. Rescheduling Hunt.");
            // Reschedule Hunt task if something goes wrong
            ScheduleTask(State.Hunt, 10f);
        }
    }

    protected override void Flee()
    {
        if (target != null && currentStamina > 0)
        {
            // Move away from the predator
            Vector3 fleeDirection = (transform.position - target.position).normalized * 10f;
            NavMeshHit hit;

            if (NavMesh.SamplePosition(transform.position + fleeDirection, out hit, 10f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                Debug.Log($"{gameObject.name} is fleeing from a predator: {target.name}.");
            }
        }
        else
        {
            // If stamina is depleted, move slowly
            agent.speed *= lowStaminaSpeedMultiplier;
        }

        // After fleeing, return to idle when stamina regenerates
        ScheduleTask(State.Idle, 1f);
    }

}
