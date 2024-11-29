using UnityEngine;
using UnityEngine.AI;

public class FoxAI : AnimalAI
{
    protected override void Start()
    {
        base.Start();
        agent.speed = 3.5f; // Foxes are faster than grasshoppers
        maxStamina = 80f; // Foxes have less stamina than grasshoppers
        staminaRegenRate = 0.8f; // Foxes regenerate stamina slower than grasshoppers
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
        if (currentState != State.Hunt) return; // Ensure fox is in Hunt state

        Collider[] preyItems = Physics.OverlapSphere(transform.position, SearchFoodRange, LayerMask.GetMask("Grasshopper"));
        GameObject nearestPrey = null;
        float closestDistance = Mathf.Infinity;
        Vector3 closestNavMeshPoint = Vector3.zero;

        foreach (var prey in preyItems)
        {
            float distance = Vector3.Distance(transform.position, prey.transform.position);

            if (NavMesh.SamplePosition(prey.transform.position, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
            {
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    nearestPrey = prey.gameObject;
                    closestNavMeshPoint = navHit.position;
                }
            }
        }

        if (nearestPrey != null)
        {
            target = nearestPrey.transform;
            agent.SetDestination(closestNavMeshPoint);
            Debug.Log($"{gameObject.name} is hunting for grasshopper: {nearestPrey.name}.");

            // Use Physics.CheckSphere to determine if the prey is within eatRange
            bool isInEatingRange = Physics.CheckSphere(transform.position, eatRange, LayerMask.GetMask("Grasshopper"));

            if (isInEatingRange)
            {
                Debug.Log($"{gameObject.name} is in eating range of grasshopper: {nearestPrey.name}. Scheduling Eat task.");
                ScheduleTask(State.Eat, 20f);
            }
        }
        else if (!IsTaskScheduled(State.Wander))
        {
            // If no prey is found, transition to Wander
            ScheduleTask(State.Wander, 30f);
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

            Debug.Log($"{gameObject.name} is eating grasshopper: {target.name}.");

            // Destroy the grasshopper object
            Destroy(target.gameObject);

            // Restore hunger
            currentHunger = Mathf.Min(currentHunger + 30f, MaxHunger); // Foxes gain more hunger from eating

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

        // After fleeing, return to idle when stamina regenerates
        ScheduleTask(State.Idle, 1f);
    }

}
