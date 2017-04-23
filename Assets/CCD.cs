using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class CCD : MonoBehaviour {

	public Transform root;
	public Transform target;
	private Transform[] joints= new Transform[99];

	private int joint_size = 0;
	private float delta = float.MaxValue;
	private int iter_count = 0;
	private int max_iter_count = 4;

	private int end_effector_index;
	private int current_joint_index;

	private bool is_running;

	// UI
	public Slider joints_count_slider;
	public Slider iteration_count_slider;
	public Button create_button;

	// Prefabs
	public Transform joint_prefab;
	public Transform bone_prefab;

	// Materials
	public Material normal;
	public Material outline;


    	void Awake () {
        	is_running = false;
    	}

	void Start () {
		// Slider change
		joints_count_slider.onValueChanged.AddListener (delegate {
			joints_count_slider.transform.Find ("Text").GetComponent<Text> ().text = "Joints Count:     " + joints_count_slider.value.ToString ();
		});
		iteration_count_slider.onValueChanged.AddListener (delegate {
			iteration_count_slider.transform.Find ("Text").GetComponent<Text> ().text = "Iteration Count:     " + iteration_count_slider.value.ToString ();
		});
		create_button.onClick.AddListener (delegate {
			is_running = false;
			joint_size = (int)joints_count_slider.value;
			max_iter_count = (int)iteration_count_slider.value;
			end_effector_index = joint_size - 1;
			Destroy(root.gameObject);
			root = Instantiate(joint_prefab) as Transform;
			root.name = "0";
			root.tag = "joint";
			root.position = Vector3.zero;
			// Initialize joints
			Transform current_joint = root;
			int joint_index = 0;
			joints[joint_index] = root;

			for(int i=1; i<joint_size; i++) {
				current_joint = Instantiate(joint_prefab, joints[i-1]) as Transform;
				current_joint.position = new Vector3((Random.value - 0.5f) * 5f,(Random.value - 0.5f) * 5f,(Random.value - 0.5f) * 5f);
				current_joint.name = i.ToString();
				current_joint.tag = "joint";
				joints[i] = current_joint;
			}
			joints [end_effector_index].GetComponent<Renderer> ().material = outline;

			for(int i=0; i<joint_size-1; i++) {
				Transform current_bone;
				current_bone = Instantiate(bone_prefab, joints[i]) as Transform;
				current_bone.position = Vector3.zero;
				Vector3 newScale = current_bone.localScale;
				newScale.y = Vector3.Distance(joints[i].position, joints[i+1].position)/2f/joint_prefab.localScale.x;
				current_bone.localScale = newScale;
				current_bone.position = (joints[i+1].position + joints[i].position)/2f;
				current_bone.rotation = Quaternion.FromToRotation(Vector3.up, joints[i+1].position - joints[i].position); 
			}
		});



	}
		
	void Update () {
		if (is_running) {
			current_joint_index = end_effector_index  - 1;
			while (current_joint_index >= 0) {

				Vector3 end_effector_vec = joints [end_effector_index].position;
				Vector3 current_joint_vec = joints [current_joint_index].position;
				Vector3 target_vec = target.position;

				Vector3 v1 = end_effector_vec - current_joint_vec;
				Vector3 v2 = target_vec - current_joint_vec;

				// Vector3 rot_axis = Vector3.Cross (v1, v2);
				// float rot_angle = Vector3.Angle (v1, v2);
				Quaternion current_quaternion = joints [current_joint_index].rotation;
				joints[current_joint_index].rotation = Quaternion.FromToRotation (v1, v2) * current_quaternion;
				current_joint_index--;

				// Calculate the distance between the target and the end effector
				delta = (target.position - joints [joint_size - 1].position).sqrMagnitude;

			}
			iter_count++;
		}

		// Control the target position
		if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.S) 
			|| Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E)) {
			Vector3 movement = Vector3.zero;
			if (Input.GetKey (KeyCode.A)) {
				movement = Vector3.left;
			} else if (Input.GetKey (KeyCode.W)) {
				movement = Vector3.up;
			} else if (Input.GetKey (KeyCode.D)) {
				movement = Vector3.right;
			} else if (Input.GetKey (KeyCode.S)) {
				movement = Vector3.down;
			} else if (Input.GetKey (KeyCode.Q)) {
				movement = Vector3.forward;
			} else if (Input.GetKey (KeyCode.E)) {
				movement = Vector3.back;
			}
			target.transform.position += movement;
			iter_count = 0;
			delta = float.MaxValue;
			is_running = true;
		}

		if (Input.GetKey (KeyCode.U)) {
			Camera.main.transform.position += Vector3.forward;
		} else if (Input.GetKey (KeyCode.O)) {
			Camera.main.transform.position += Vector3.back;
		} else if (Input.GetKey (KeyCode.J)) {
			Camera.main.transform.position += Vector3.left;
		} else if (Input.GetKey (KeyCode.L)) {
			Camera.main.transform.position += Vector3.right;
		} else if (Input.GetKey (KeyCode.I)) {
			Camera.main.transform.position += Vector3.up;
		} else if (Input.GetKey (KeyCode.K)) {
			Camera.main.transform.position += Vector3.down;
		}

		Camera.main.transform.LookAt (root);

		// Check if the calculation should be performed
		if (delta < .1f || iter_count > max_iter_count) {
			is_running = false;
		}

		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast (ray, out hit)) {
			Transform object_hit = hit.transform;
			if(object_hit.tag == "joint") {
				if (Input.GetMouseButtonDown (0)) {
					int x = joint_size - 1;
					int.TryParse (object_hit.name, out x);
					if (x >= 0 && x < joint_size) {
						joints [end_effector_index].GetComponent<Renderer> ().material = normal;
						end_effector_index = x;
						joints [end_effector_index].GetComponent<Renderer> ().material = outline;
					}
				}
			}
		}
		
	}
}
