using UnityEngine;
using UnityEngine.AI;

public class FoxAI : AnimalAI
{
    protected override void Start()
    {
        base.Start();
        agent.speed = 3.5f; // Foxes are faster than grasshoppers
        maxStamina = 50f; // Foxes have less stamina than grasshoppers
        staminaRegenRate = 0.1f; // Foxes regenerate stamina slower than grasshoppers
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void SearchForFood()
    {
        GameObject[] foodItems = GameObject.FindGameObjectsWithTag("Grasshopper");
        GameObject nearestFood = null;
        float closestDistance = Mathf.Infinity;
        Vector3 closestNavMeshPoint = Vector3.zero;

        foreach (var food in foodItems)
        {
            if (NavMesh.SamplePosition(food.transform.position, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
            {
                float distance = Vector3.Distance(transform.position, navHit.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    nearestFood = food;
                    closestNavMeshPoint = navHit.position;
                }
            }
        }

        if (nearestFood != null)
        {
            target = nearestFood.transform;
            agent.SetDestination(closestNavMeshPoint);

            Debug.Log($"{gameObject.name} is pathfinding {nearestFood.name}. Destination: {closestNavMeshPoint}");

            // Check if already in range
            if (Vector3.Distance(transform.position, closestNavMeshPoint) <= eatRange + agent.stoppingDistance)
            {
                Debug.Log($"{gameObject.name} is in eating range of grass: {nearestFood.name}. Scheduling Eat task.");
                ScheduleTask(State.Eat, 20f);
            }
        }
        else
        {
            if (!IsTaskScheduled(State.Wander) && currentState != State.Wander)
            {
                Debug.Log($"{gameObject.name} found no grass. Switching to Wander.");
                ScheduleTask(State.Wander, 30f);
            }
        }
    }

    protected override void Eat()
    {
        try
        {
            if (target == null)
            {
                throw new System.Exception("Target is null. Unable to eat prey.");
            }

            Debug.Log($"{gameObject.name} is eating {target.name}.");

            // Destroy the grasshopper object
            Destroy(target.gameObject);

            // Restore hunger
            currentHunger = Mathf.Min(currentHunger + 30f, MaxHunger);

            // Reset the target and schedule Idle
            target = null;
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
            Vector3 fleeDirection = (transform.position - target.position).normalized * 12f; // Foxes flee faster
            NavMeshHit hit;

            // Use the already-declared 'hit' variable
            if (NavMesh.SamplePosition(transform.position + fleeDirection, out hit, 10f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
        else
        {
            // If stamina is depleted, move slowly
            agent.speed *= lowStaminaSpeedMultiplier;
        }
    }

}
