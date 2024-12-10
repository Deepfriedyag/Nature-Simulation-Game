using UnityEngine;
using UnityEngine.AI;

// the GrasshopperAI class extends the AnimalAI base class to define specific behaviors for a grasshopper
public class GrasshopperAI : AnimalAI // inherit from the AnimalAI base class
{
    protected override void Start() // calls the base class's Start() method to ensure standard initialization
    {
        base.Start(); // initialize base class variables and tasks
    }
    
    protected override void Update()
    {
        base.Update(); // ensures the base Update logic (e.g., hunger decay, stamina management) is preserved while still allowing for custom behavior in this method
    }

    protected override void SearchForFood() // defines grasshopper's custom behaviour for finding food as the behaviour is different for each animal species
    {
        GameObject[] food_items = GameObject.FindGameObjectsWithTag("Grass"); // grasshoppers eat grass
        GameObject nearest_food = null;
        float closest_distance = Mathf.Infinity;
        Vector3 closest_navmesh_point = Vector3.zero; // stores the nearest navigable point
        
        foreach (var food in food_items) // iterate through all grass objects to find the nearest one
        {
            if (NavMesh.SamplePosition(food.transform.position, out NavMeshHit nav_hit, 2f, NavMesh.AllAreas)) // check if the grass object is on a valid NavMesh
            {
                float distance = Vector3.Distance(transform.position, nav_hit.position);
                
                if (distance < closest_distance) // update the closest grass if the distance is smaller
                {
                    closest_distance = distance;
                    nearest_food = food;
                    closest_navmesh_point = nav_hit.position;
                }
            }
        }

        if (nearest_food != null) // if a valid grass object is found, set it as the target and move toward it
        {
            target = nearest_food.transform;
            agent.SetDestination(closest_navmesh_point); // navigate to the grass

            if (Vector3.Distance(transform.position, closest_navmesh_point) <= eat_range + agent.stoppingDistance) // check if the grass is within eating range and schedule an "eat" task if it is
            {
                ScheduleTask(State.Eat, 20f);
            }
        }
        else
        {
            if (!IsTaskScheduled(State.Wander) && current_state != State.Wander) // if no grass is found, schedule a wander task to look for food elsewhere
            {
                ScheduleTask(State.Wander, 30f);
            }
        }
    }

    protected override void Eat() // defines grasshoppers' behaviour for eating food
    {
        try
        {

            if (target == null) // ensure the target is not null before attempting to eat
            {
                throw new System.Exception("Target is null. Unable to eat grass.");
            }

            
            Destroy(target.gameObject); // destroy/delete the grass object after eating

            current_hunger = Mathf.Min(current_hunger + 20f, max_hunger);

            target = null;
        }
        catch (System.Exception)
        {
            ScheduleTask(State.Hunt, 10f); // reschedule a hunt task to find new food
        }
    }

    protected override void Flee() // defines grasshoppers' behaviour for fleeing from predators
    {
        if (target != null && current_stamina > 0)
        {
            Vector3 flee_direction = (transform.position - target.position).normalized * 10f; // calculate a direction opposite to the predator's position

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