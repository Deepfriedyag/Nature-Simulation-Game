using UnityEngine;
using UnityEngine.AI;

public class GrasshopperAI : AnimalAI
{
    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void SearchForFood()
    {
        GameObject[] foodItems = GameObject.FindGameObjectsWithTag("Grass");
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

            Debug.Log($"{gameObject.name} is pathfinding to grass: {nearestFood.name}. Destination: {closestNavMeshPoint}");

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
                throw new System.Exception("Target is null. Unable to eat grass.");
            }

            Debug.Log($"{gameObject.name} is eating grass: {target.name}.");

            // Destroy the grass object
            Destroy(target.gameObject);

            // Restore hunger
            currentHunger = Mathf.Min(currentHunger + 20f, MaxHunger);

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
            Vector3 fleeDirection = (transform.position - target.position).normalized * 10f;
            NavMeshHit hit;

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
