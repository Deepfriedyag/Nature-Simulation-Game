using UnityEngine;
using UnityEngine.AI;

// the FoxAI class extends the AnimalAI framework, adding fox-specific behavior.
public class FoxAI : AnimalAI
{
    protected override void Start() // overrides the reserved Start() method to initialize fox-specific attributes while calling the base class Start() method to ensure standard initialization.
    {
        base.Start();

        agent.speed = 3.5f; // foxes are faster than grasshoppers as they are predators
        max_stamina = 50f; // foxes have lower stamina compared to grasshoppers
        stamina_regen_rate = 0.1f; // foxes regenerate stamina slower than grasshoppers
    }

    protected override void Update()
    {
        base.Update(); // ensure base Update logic (e.g., hunger decay, task processing) is preserved
    }

    protected override void SearchForFood()
    {
        GameObject[] food_items = GameObject.FindGameObjectsWithTag("Grasshopper"); // foxes eat grasshoppers
        GameObject nearest_food = null;
        float closest_distance = Mathf.Infinity;
        Vector3 closest_navmesh_point = Vector3.zero; // stores the nearest navigable point

        foreach (var food in food_items) // iterate through all grasshoppers to find the nearest one
        {
            if (NavMesh.SamplePosition(food.transform.position, out NavMeshHit navHit, 2f, NavMesh.AllAreas)) // check if the grasshopper is on a valid NavMesh point
            {
                float distance = Vector3.Distance(transform.position, navHit.position);

                if (distance < closest_distance) // update the closest grasshopper if the distance is smaller
                {
                    closest_distance = distance;
                    nearest_food = food;
                    closest_navmesh_point = navHit.position;
                }
            }
        }
         
        if (nearest_food != null) // if a valid grasshopper is found, set it as the target and navigate to it
        {
            target = nearest_food.transform;
            agent.SetDestination(closest_navmesh_point);

            // check if the fox is within eating range of the grasshopper
            if (Vector3.Distance(transform.position, closest_navmesh_point) <= eat_range + agent.stoppingDistance)
            {
                ScheduleTask(State.Eat, 20f);
            }
        }
        else
        {
            // if no grasshoppers are found, schedule a wander task as to look for food elsewhere
            if (!IsTaskScheduled(State.Wander) && current_state != State.Wander)
            {
                ScheduleTask(State.Wander, 30f);
            }
        }
    }

    protected override void Eat() // defines foxes' behaviour for eating prey
    {
        try
        {
            if (target == null) // ensure the target is valid before eating
            {
                throw new System.Exception("Target is null. Unable to eat prey.");
            }

            Destroy(target.gameObject); // destroy the prey object after eating
            current_hunger = Mathf.Min(current_hunger + 30f, max_hunger); // restore hunger
            target = null; // reset the target
        }
        catch (System.Exception exception)
        {
            // handle errors, such as missing target, and reschedule a hunt task.
            Debug.LogWarning("Error while eating: " + exception.Message);
            ScheduleTask(State.Hunt, 10f);
        }
    }

    protected override void Flee()
    {
        if (target != null && current_stamina > 0)
        {
            // calculate a direction opposite to the predator's position
            Vector3 flee_direction = (transform.position - target.position).normalized * 12f; // foxes flee faster

            // check for a valid NavMesh position in the flee direction
            if (NavMesh.SamplePosition(transform.position + flee_direction, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
        else
        {
            // if stamina is depleted, reduce speed and flee at a slower pace
            agent.speed *= low_stamina_speed_multiplier;
        }
    }
}
