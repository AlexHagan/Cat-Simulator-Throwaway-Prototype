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
		public CatStats(double _energy = full_energy, double _hunger = full_hunger)
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
					Playing,
					Happy,
					User_Interaction};
	
	CatStates current_state;
	CatStats cat_stats = new CatStats();
	CatPersonality cat_personality = new CatPersonality();
	NavMeshAgent agent;
	Transform cat_transform;
	GameObject cat_toy;
	GameObject food_bowl;
	GameObject kibble;
	
	public Slider hunger_slider;
	public Slider sleep_slider;
	SpriteRenderer heart_icon;
	SpriteRenderer hungry_icon;
	SpriteRenderer sleep_icon;	
	Image hunger_slider_fill;
	Image sleep_slider_fill;
	Color high_stat_bar_color;
	Color low_stat_bar_color;

	float change_state_delay = 3F; // A time delay (in seconds) before the cat will randomly choose a new state
	float delta_time = 0F;
	float time_of_last_state_change = 0F; // Time at which the cat's state last changed
	float time_of_last_update = 0F; // Time at which Update() was last called
	float floor_size_modifier = 2.5F; // The floor is 5 by 5 units wide, so the random position can be anywhere between (-2.5, -2.5, 0) and (2.5, 2.5, 0)
	float happy_time = 0F;
	float chase_toy_time_delay = 4F; // Wait x seconds before updating cat's destination
	float time_toy_last_chased = 0F; // Time the cat's position was last updated
	
	bool is_drag;
	double drag_start_time;
	
	bool can_eat; // Is the cat physically close enough to its food bowl to eat?
	
	public Texture2D pettingCursor; // Set in unity editor
	
	Vector3 in_front_of_user_position;
	
	Vector3 default_camera_focus_position;
	Vector3 default_camera_rotation;
	Vector3 default_camera_position;
	
    // Start is called before the first frame update
    void Start()
    {
		hunger_slider = GameObject.Find("HungerSlider").GetComponent <Slider> ();
		sleep_slider = GameObject.Find("SleepSlider").GetComponent <Slider> ();
		hunger_slider_fill = GameObject.Find("Hunger Slider Fill").GetComponent <Image> ();
		sleep_slider_fill = GameObject.Find("Sleep Slider Fill").GetComponent <Image> ();
		high_stat_bar_color = new Color32(10, 200, 55, 255);
		low_stat_bar_color = new Color32(226, 214, 29, 255);

		heart_icon = GameObject.Find("catsim_heart_icon").GetComponent <SpriteRenderer> ();
		hungry_icon = GameObject.Find("catsim_hungry_icon").GetComponent <SpriteRenderer> ();
		sleep_icon = GameObject.Find("catsim_sleep_icon").GetComponent <SpriteRenderer> ();

		heart_icon.GetComponent<Renderer>().enabled = false;
		hungry_icon.GetComponent<Renderer>().enabled = false;
		sleep_icon.GetComponent<Renderer>().enabled = false;
		
		can_eat = false;

        current_state = CatStates.Wandering;		
		agent = GetComponent<NavMeshAgent>();
		cat_transform = GetComponent<Transform>();
		cat_toy = GameObject.Find("Cat Toy");
		food_bowl = GameObject.Find("Food Bowl");
		kibble = GameObject.Find("Cat Food");
		
		time_of_last_state_change = Time.time;
		
		in_front_of_user_position = new Vector3(0F, 0.5F, -3F);
		default_camera_focus_position = new Vector3(0F, 0F, 0F);
		default_camera_rotation = new Vector3(7F, 0, 0);
		default_camera_position = new Vector3(0, 3.5F, -9F);
		
	
    }

    // Update is called once per frame
    void Update()
    {
		delta_time = Time.time - time_of_last_update;
		time_of_last_update = Time.time;

		// Update Sliders
		hunger_slider.value = (float) cat_stats.hunger;
		sleep_slider.value = (float) cat_stats.energy;
		
		// IDLE STATE
        if (current_state == CatStates.Idle) {
			// Cat does nothing; waits for player input or new state change
			
			
			if (Time.time - time_of_last_state_change > change_state_delay) {
				
				// Choose a new state 
				if (Random.value >= 0.75) {
					current_state = CatStates.Wandering;
				}
				else {
					current_state = CatStates.Playing;
					Debug.Log("Cat State: Playing");
				}
				
				time_of_last_state_change = Time.time;
				
			}
			
		}
		
		// WANDERING STATE
		if (current_state == CatStates.Wandering) {
			Debug.Log("Cat State: Wandering");
			
			Vector3 random_position = new Vector3(Random.value * floor_size_modifier, Random.value * floor_size_modifier, Random.value * floor_size_modifier); // Random.value eturns a random number between 0.0 [inclusive] and 1.0 [inclusive].
			Debug.Log("Cat State: Wandering");
			agent.destination = random_position;
			
			current_state = CatStates.Idle;
			time_of_last_state_change = Time.time;
			Debug.Log("Cat State: Idle");
		}
		
		// PLAYING STATE
		if (current_state == CatStates.Playing) {
			
			agent.speed = 7F;
			
			if (Time.time - time_toy_last_chased >= chase_toy_time_delay) {
				time_toy_last_chased = Time.time;
				
				Vector3 go_here = cat_toy.GetComponent<Transform>().position;
				agent.destination = go_here;
				
			}
			
			// Focus on toy for 10 seconds before wandering off
			if (Time.time - time_of_last_state_change >= 10F) {
				current_state = CatStates.Wandering;
				agent.speed = 3.5F;
				Debug.Log("Cat State: Wandering");
				time_of_last_state_change = Time.time;
			}
			
		}
		
		if (current_state == CatStates.User_Interaction) {
			Camera.main.transform.LookAt(cat_transform); // main camera will follow cat
			
			// Focus on user for 10 seconds before wandering off
			if (Time.time - time_of_last_state_change >= 10F) {
				current_state = CatStates.Wandering;
				Debug.Log("Cat Loses Focus");
				time_of_last_state_change = Time.time;
				//Camera.main.GetComponent<Transform>().LookAt(default_camera_focus_position);
				Camera.main.GetComponent<Transform>().localEulerAngles = default_camera_rotation;
			}
		}
		
		// Happy state (after petting)
		if (current_state == CatStates.Happy) {
			Camera.main.transform.LookAt(cat_transform); // main camera will follow cat
			
			// Focus on user for x seconds before wandering off
			if (Time.time - time_of_last_state_change >= 7F) {
				current_state = CatStates.Wandering;
				Debug.Log("Cat Loses Focus");
				time_of_last_state_change = Time.time;
				//Camera.main.GetComponent<Transform>().LookAt(default_camera_focus_position);
				Camera.main.GetComponent<Transform>().localEulerAngles = default_camera_rotation;
			}
				
			if (Time.time - time_of_last_state_change > happy_time) {
				heart_icon.GetComponent<Renderer>().enabled = false;
			}
			
		}
		
		// EATING STATE
		if (current_state == CatStates.Eating) {
			
			// If there is no food in the bowl...
			if (kibble.activeSelf == false) {
				current_state = CatStates.Wandering;
				time_of_last_state_change = Time.time;
			}
			
			if ((can_eat == true) && (kibble.activeSelf == true)) {
				cat_stats.hunger += cat_personality.hunger_gain_rate * delta_time;
				
				// If hunger stat is getting high, change stat bar color
				if (cat_stats.hunger >= (full_hunger * 0.5)) {
					hunger_slider_fill.color = high_stat_bar_color;
				}
				
			}
			
			// If cat is not hungry, it will stop eating
			if (cat_stats.hunger >= full_hunger) {
				hungry_icon.GetComponent<Renderer>().enabled = false;
				current_state = CatStates.Wandering;
				time_of_last_state_change = Time.time;
				
				// Deactivate cat food
				kibble.SetActive(false);
				
				Debug.Log("Cat State: Wandering");
			}
			
		}
		// IF NOT EATING (and not currently focusing on user)
		else if (current_state != CatStates.Happy) {
			cat_stats.hunger -= cat_personality.hunger_decay_rate * delta_time;
			
			// If hunger stat is getting low, change stat bar color
			if (cat_stats.hunger <= (full_hunger * 0.5)) {
				hunger_slider_fill.color = low_stat_bar_color;
			}
			
			// If cat is hungry, it will try to eat
			if ( (current_state != CatStates.Sleeping) && (cat_stats.hunger <= hunger_threshold) ) {
				
				// If there is food in the bowl, the cat will eat
				if (kibble.activeSelf == true) {
					hungry_icon.GetComponent<Renderer>().enabled = true;
					current_state = CatStates.Eating;
					time_of_last_state_change = Time.time;
					
					agent.destination = food_bowl.GetComponent<Transform>().position;
					
					Debug.Log("Cat State: Eating");
				}
				
			}
			
		}
		
		// SLEEPING STATE
		if (current_state == CatStates.Sleeping) {
			cat_stats.energy += cat_personality.energy_gain_rate * delta_time;
			
			// If sleep stat is getting full, change stat bar color
			if (cat_stats.energy >= (full_energy * 0.5) ) {
				sleep_slider_fill.color = high_stat_bar_color;
			}
			
			// If cat is rested, it will wake up
			if (cat_stats.energy >= full_energy) {
				sleep_icon.GetComponent<Renderer>().enabled = false;
				current_state = CatStates.Idle;
				time_of_last_state_change = Time.time;
				
				Debug.Log("Cat State: Idle (Awake)");
			}
		}
		// IF NOT SLEEPING and not focusing on user
		else if (current_state != CatStates.Happy) {
			cat_stats.energy -= cat_personality.energy_decay_rate * delta_time;
			
			// If sleep stat is getting low, change stat bar color
			if (cat_stats.energy <= (full_energy * 0.5)) {
				sleep_slider_fill.color = low_stat_bar_color;
			}
			
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
		
		// If cat is in front of user...
		if (Vector3.Distance(cat_transform.position, in_front_of_user_position) <= 3F ) { // Yay, magic numbers! *throws confetti*
			Cursor.SetCursor(pettingCursor, Vector2.zero, CursorMode.ForceSoftware);
		}
	}
	
	void OnMouseUp() {
		// When mouse released, act based on accumulated drag
		is_drag = false;
		double drag_time = Time.time - drag_start_time;
		Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
		
		Debug.Log("Dragged for " + drag_time);
		
		// A short drag is registered as a click, causing cat to approach user
		if (drag_time < 0.1) {
			Camera.main.transform.LookAt(cat_transform); // main camera will follow cat
			agent.destination = in_front_of_user_position; // cat approaches user
			current_state = CatStates.User_Interaction;
			time_of_last_state_change = Time.time;
			
			Debug.Log("Cat clicked on");
		} 
		// For longer drags, count as petting
		else if (current_state != CatStates.Sleeping && current_state != CatStates.Eating) {
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
	
	void OnCollisionEnter(Collision collision) {
		
		//Debug.Log("Collision Entered.");
		
		if (collision.gameObject.name == "Food Bowl") {
			can_eat = true;
			agent.destination = GetComponent<Transform>().position; // haxx to prevent the cat from spinning circles around the food bowl...........
			Debug.Log("can_eat = true;");
		}
		
	}
	
	void OnCollisionExit(Collision collision) {
		
		//Debug.Log("Collision Exited.");
		
		if (collision.gameObject.name == "Food Bowl") {
			can_eat = false;
			Debug.Log("can_eat = false;");
		}
		
	}
}
