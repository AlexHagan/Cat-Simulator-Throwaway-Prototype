using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

// Constants


public class Cat : MonoBehaviour
{
	// Cat Stat-related Thresholds
	const double sleep_threshold = 20; // Cat sleeps when energy reaches this level
	const double awake_threshold = 100; // Cat awakens when energy reaches this level
	
	public class CatStats
	{
		// Stats
		public double energy;
		public double hunger;
		
		// Constructor
		public CatStats(double _energy = 100, double _hunger = 100)
		{
			energy = _energy;
			hunger = _hunger;
		}
	}
	
	public class CatPersonality
	{
		// Stat Decay rates
		public double energy_decay_rate;
		public double hunger_decay_rate;
		
		// Stat Gain rates
		public double energy_gain_rate;
		public double hunger_gain_rate;
		
		// Constructor
		public CatPersonality(double _edr = 5, double _hdr = 1, double _egr = 10, double _hgr = 1) {
			
			energy_decay_rate = _edr;
			hunger_decay_rate = _hdr;
			
			energy_gain_rate = _egr;
			hunger_gain_rate = _hgr;
			
		}
	}
	
	// Possible states for the cat to be in at any given time
	enum CatStates {Idle, 
					Wandering, 
					Eating, 
					Sleeping};
	
	CatStates current_state;
	CatStats cat_stats = new CatStats();
	CatPersonality cat_personality = new CatPersonality();
	
	NavMeshAgent agent;
	public Slider hunger_slider;
	public Slider sleep_slider;	

	float change_state_delay = 5F; // A time delay (in seconds) before the cat will randomly choose a new state
	float delta_time = 0F;
	float time_of_last_state_change = 0F; // Time at which the cat's state last changed
	float time_of_last_update = 0F; // Time at which Update() was last called
	float floor_size_modifier = 2.5F; // The floor is 5 by 5 units wide, so the random position can be anywhere between (-2.5, -2.5, 0) and (2.5, 2.5, 0)
	
	bool is_drag;
	double drag_start_time;
	
    // Start is called before the first frame update
    void Start()
    {
		hunger_slider = GameObject.Find("HungerSlider").GetComponent <Slider> ();
		sleep_slider = GameObject.Find("SleepSlider").GetComponent <Slider> ();

        current_state = CatStates.Wandering;		
		agent = GetComponent<NavMeshAgent>();
		time_of_last_state_change = Time.time;
		
		
		
		//Debug.Log("Cat's Starting Position: " + GetComponent<Transform>().position);
	
    }

    // Update is called once per frame
    void Update()
    {
		delta_time = Time.time - time_of_last_update;
		time_of_last_update = Time.time;
		
		hunger_slider.value = (float) cat_stats.hunger;
		sleep_slider.value = 100F - (float) cat_stats.energy;
		Debug.Log("cat_stats.energy = " + cat_stats.energy);
		
		//Debug.Log("delta_time = " + delta_time);
		
		// IDLE STATE
        if (current_state == CatStates.Idle) {
			// Cat does nothing; waits for player input or new state change
			Debug.Log("Cat State: Idle");

			if (Time.time - time_of_last_state_change > change_state_delay) {
				current_state = CatStates.Wandering;
				time_of_last_state_change = Time.time;
			}
			
		}
		
		// WANDERING STATE
		if (current_state == CatStates.Wandering) {
			Vector3 random_position = new Vector3(Random.value * floor_size_modifier, Random.value * floor_size_modifier, Random.value * floor_size_modifier); // Random.value eturns a random number between 0.0 [inclusive] and 1.0 [inclusive].
			Debug.Log("Cat State: Wandering");
			agent.destination = random_position;
			
			current_state = CatStates.Idle;
			time_of_last_state_change = Time.time;
		}
		
		// EATING STATE
		if (current_state == CatStates.Eating) {
			
			// ...
			
		}
		
		// SLEEPING STATE
		if (current_state == CatStates.Sleeping) {
			cat_stats.energy += cat_personality.energy_gain_rate * delta_time;

			if (cat_stats.energy >= awake_threshold) {
				current_state = CatStates.Idle;
				time_of_last_state_change = Time.time;
				
				Debug.Log("Cat State: Idle (Awake)");
			}
		}
		// IF NOT SLEEPING
		else {
			cat_stats.energy -= cat_personality.energy_decay_rate * delta_time;
			
			if (cat_stats.energy <= sleep_threshold) {
				current_state = CatStates.Sleeping;
				time_of_last_state_change = Time.time;
				Debug.Log("Cat State: Sleeping");
			}
		}		
    }
	
	void OnMouseDown(){
		// If mouse just went down, start counting drag time
		if (!is_drag) {
			is_drag = true;
			drag_start_time = Time.time;
		}
	}
	
	void OnMouseUp(){
		// When mouse released, act based on accumulated drag
		is_drag = false;
		double drag_time = Time.time - drag_start_time;
		Debug.Log("Dragged for " + drag_time);
		
		// A short drag is registered as a click, causing cat to approach user
		if (drag_time < 0.5) {
			Vector3 new_position = new Vector3(0, 0, 0);
			agent.destination = new_position;
			Debug.Log("Cat clicked on");	
		}
	}
}
