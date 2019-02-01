using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CatScript : MonoBehaviour
{
	enum CatStates {Idle, Wander};
	CatStates current_state;
	NavMeshAgent agent;
	float change_state_delay = 5F; // A time delay (in seconds) before the cat will randomly choose a new state
	float time_of_last_state_change; // Time at which the cat's state last changed
	float floor_size_modifier = 2.5F; // The floor is 5 by 5 units wide, so the random position can be anywhere between (-2.5, -2.5, 0) and (2.5, 2.5, 0)
	
    // Start is called before the first frame update
    void Start()
    {
        current_state = CatStates.Wander;		
		agent = GetComponent<NavMeshAgent>();
		time_of_last_state_change = Time.time;
		
		//Debug.Log("Cat's Starting Position: " + GetComponent<Transform>().position);
	
    }

    // Update is called once per frame
    void Update()
    {
        if (current_state == CatStates.Idle) {
			// Cat does nothing; waits for player input or new state change
			//Debug.Log("Cat's Current Position: " + GetComponent<Transform>().position);
			
			if ((Time.time - time_of_last_state_change) > change_state_delay) {
				current_state = CatStates.Wander;
				time_of_last_state_change = Time.time;
			}
		}
		if (current_state == CatStates.Wander) {
			Vector3 random_position = new Vector3(Random.value * floor_size_modifier, Random.value * floor_size_modifier, Random.value * floor_size_modifier); // Random.value eturns a random number between 0.0 [inclusive] and 1.0 [inclusive].
			Debug.Log("Cat's Target Position: " + GetComponent<Transform>().position);
			agent.destination = random_position;
			
			current_state = CatStates.Idle;
		}
    }
}
