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
	const double hunger_threshold = 30; // Cat eats when hunger reaches this level
	
	// Cat Stat-related Constants
	const double full_energy = 100;
	const double full_hunger = 100;
	
	public class CatStats
	{
		// Stats
		public double energy;
		public double hunger;
		
		// Constructor
		public CatStats(double _energy = full_energy, double _hunger = 100)
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
		
		// Time happy for each second petted
		public double joyfullness;
		
		// Constructor
		public CatPersonality(double _edr = 2, double _hdr = 5, double _egr = 10, double _hgr = 30, double jf = 1.5) {
			
			energy_decay_rate = _edr;
			hunger_decay_rate = _hdr;
			
			energy_gain_rate = _egr;
			hunger_gain_rate = _hgr;
			
			joyfullness = jf;
		}
	}
	
	// Possible states for the cat to be in at any given time
	enum CatStates {Idle, 
					Wandering, 
					Eating, 
					Sleeping,
					Happy};
	
	CatStates current_state;
	CatStats cat_stats = new CatStats();
	CatPersonality cat_personality = new CatPersonality();
	
	NavMeshAgent agent;
	public Slider hunger_slider;
	public Slider sleep_slider;
	SpriteRenderer heart_icon;
	SpriteRenderer hungry_icon;
	SpriteRenderer sleep_icon;

	float change_state_delay = 3F; // A time delay (in seconds) before the cat will randomly choose a new state
	float delta_time = 0F;
	float time_of_last_state_change = 0F; // Time at which the cat's state last changed
	float time_of_last_update = 0F; // Time at which Update() was last called
	float floor_size_modifier = 2.5F; // The floor is 5 by 5 units wide, so the random position can be anywhere between (-2.5, -2.5, 0) and (2.5, 2.5, 0)
	float happy_time = 0F;
	
	bool is_drag;
	double drag_start_time;
	
    // Start is called before the first frame update
    void Start()
    {
		hunger_slider = GameObject.Find("HungerSlider").GetComponent <Slider> ();
		sleep_slider = GameObject.Find("SleepSlider").GetComponent <Slider> ();

		heart_icon = GameObject.Find("catsim_heart_icon").GetComponent <SpriteRenderer> ();
		hungry_icon = GameObject.Find("catsim_hungry_icon").GetComponent <SpriteRenderer> ();
		sleep_icon = GameObject.Find("catsim_sleep_icon").GetComponent <SpriteRenderer> ();

		heart_icon.GetComponent<Renderer>().enabled = false;
		hungry_icon.GetComponent<Renderer>().enabled = false;
		sleep_icon.GetComponent<Renderer>().enabled = false;
		
		//hungry_icon.renderer.enabled = false;
		//sleep_icon.renderer.enabled = false;

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
		
		hunger_slider.value = 100F - (float) cat_stats.hunger;
		sleep_slider.value = 100F - (float) cat_stats.energy;
		//Debug.Log("cat_stats.energy = " + cat_stats.energy);
		
		//Debug.Log("delta_time = " + delta_time);
		
		// IDLE STATE
        if (current_state == CatStates.Idle) {
			// Cat does nothing; waits for player input or new state change

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
			Debug.Log("Cat State: Idle");
		}
		
		// Happy state (after petting)
		if (current_state == CatStates.Happy) {
			if (Time.time - time_of_last_state_change > happy_time) {
				current_state = CatStates.Idle;
				heart_icon.GetComponent<Renderer>().enabled = false;
			}
		}
		
		// EATING STATE
		if (current_state == CatStates.Eating) {
			cat_stats.hunger += cat_personality.hunger_gain_rate * delta_time;
			
			// If cat is not hungry, it will stop eating
			if (cat_stats.hunger >= full_hunger) {
				hungry_icon.GetComponent<Renderer>().enabled = false;
				current_state = CatStates.Idle;
				time_of_last_state_change = Time.time;
				
				Debug.Log("Cat State: Idle");
			}
			
		}
		// IF NOT EATING
		else {
			cat_stats.hunger -= cat_personality.hunger_decay_rate * delta_time;
			
			// If cat is hungry, it will eat
			if (current_state != CatStates.Sleeping && cat_stats.hunger <= hunger_threshold) {
				hungry_icon.GetComponent<Renderer>().enabled = true;
				current_state = CatStates.Eating;
				time_of_last_state_change = Time.time;
				
				Debug.Log("Cat State: Eating");
			}
			
		}
		
		// SLEEPING STATE
		if (current_state == CatStates.Sleeping) {
			cat_stats.energy += cat_personality.energy_gain_rate * delta_time;
			//Debug.Log("cat_personality.energy_gain_rate * delta_time = " + (cat_personality.energy_gain_rate * delta_time));
			//Debug.Log("cat_stats.energy = " + cat_stats.energy);
			//Debug.Log("cat_personality.energy_gain_rate = " + (cat_personality.energy_gain_rate));
			
			// If cat is rested, it will wake up
			if (cat_stats.energy >= full_energy) {
				sleep_icon.GetComponent<Renderer>().enabled = false;
				current_state = CatStates.Idle;
				time_of_last_state_change = Time.time;
				
				Debug.Log("Cat State: Idle (Awake)");
			}
		}
		// IF NOT SLEEPING
		else {
			cat_stats.energy -= cat_personality.energy_decay_rate * delta_time;
			
			// If cat is tired, it will go to sleep
			if (cat_stats.energy <= sleep_threshold) {
				sleep_icon.GetComponent<Renderer>().enabled = true;
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
		if (drag_time < 0.1) {
			Vector3 new_position = new Vector3(0, 0, 0);
			agent.destination = new_position;
			Debug.Log("Cat clicked on");
		
		// For longer drags, count as petting
		} else if (current_state != CatStates.Sleeping && current_state != CatStates.Eating) {
			if (current_state != CatStates.Happy) {
				current_state = CatStates.Happy;
				Debug.Log("Cat State: Happy");
				happy_time = 0F;
				time_of_last_state_change = Time.time;
				heart_icon.GetComponent<Renderer>().enabled = true;
			}
			happy_time += (float) (drag_time * cat_personality.joyfullness);
			
		}
	}
}
