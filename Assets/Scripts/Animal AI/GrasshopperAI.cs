using UnityEngine;
using UnityEngine.AI;

public class GrasshopperAI : AnimalAI
{
    [SerializeField] private float searchFoodRange = 10f; // Grasshopper-specific range for finding food
    [SerializeField] private float wanderRange = 5f;      // Range for wandering

    protected override float SearchFoodRange => searchFoodRange;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, SearchFoodRange);
    }

    protected override void Wander()
    {
        isIdle = false;
        Vector3 randomDirection = Random.insideUnitSphere * wanderRange; // Use wanderRange for wandering
        randomDirection += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRange, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }

        // Once wandering is complete, return to idle
        ScheduleTask(State.Idle, 1f);
    }

    protected override void SearchForFood()
    {
        Collider[] foodItems = Physics.OverlapSphere(transform.position, SearchFoodRange, LayerMask.GetMask("Grass"));
        GameObject nearestFood = null;
        float closestDistance = Mathf.Infinity;

        foreach (var food in foodItems)
        {
            float distance = Vector3.Distance(transform.position, food.transform.position);

            // Check if the food object is reachable (on the NavMesh)
            if (distance < closestDistance && NavMesh.SamplePosition(food.transform.position, out _, 1f, NavMesh.AllAreas))
            {
                closestDistance = distance;
                nearestFood = food.gameObject;
            }
        }

        if (nearestFood != null)
        {
            agent.SetDestination(nearestFood.transform.position);
            Debug.Log($"{gameObject.name} is moving towards food at distance {closestDistance}.");
        }
        else
        {
            Debug.Log($"{gameObject.name} found no reachable food within range.");
        }
    }
}
