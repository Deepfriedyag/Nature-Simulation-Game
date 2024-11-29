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

    protected override void Idle()
    {
        currentStamina = Mathf.Min(currentStamina + staminaRegenRate * Time.deltaTime, maxStamina);
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
            }
        }
        else
        {
            // If stamina is depleted, move slowly
            agent.speed *= lowStaminaSpeedMultiplier;
        }
    }

}
