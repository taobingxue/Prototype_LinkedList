using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LinkedRope : MonoBehaviour {
    const float DELTA = 0.01f;

    public static LinkedRope instance = null;
	public GameObject rope_prefab;
    public Transform head;
    public Transform tail;
	public float rotate_angle;
    public GameObject level_controller;

    public FakeRope start_rope;
    public FakeRope end_rope;
    public GameObject result_ui;
    public GameObject rotate_wheel;

    bool init_finished;
    Vector3 origin_pos;
    Vector3 origin_pos_start;
    Vector3 origin_pos_end;
    Vector3 fly_velocity= new Vector3(-0.1f,0.1f,0f);
    Transform line_start;
    string current_string;
    Transform line_end;
    float tmp = 0f;
    bool grabbing_end = false;

    int connection_count = 0;
    
	
	void Awake () {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

	}

    void Start() {
        origin_pos = transform.position + 0.25f * Vector3.left;
        
    }

	void Update(){
        if (init_finished) {
            float angle = GameObject.Find("hand_left").GetComponent<RotateHand>().get_offset_angle();
            if (angle > 0f)
                line_start.position = origin_pos_start + DELTA * angle * Vector3.right;
            //time of releasing the wheel
            else  {
                //search for the path from end to start
                if (!grabbing_end)               
                    line_end.position = origin_pos + (connection_count + 1) * FakeRope.DEFAULT_LEN * Vector3.left;
                 else
                      //hook start
                      line_start.position = origin_pos_start;                                  
            }
          

             //control head and tail UI;
            if (line_start.position.x < -3.5f) {
                head.gameObject.SetActive(true);
                head.position = line_start.position + 0.15f * Vector3.up;   
            }
            else 
                head.gameObject.SetActive(false);
            
            if (line_end.position.x < -3.4f) {
                tail.gameObject.SetActive(true);
                tail.position = line_end.position + 0.15f * Vector3.up;              
            } 
            else
                tail.gameObject.SetActive(false);
            
        }      
    }

    IEnumerator fix_end() {
        //singleton
        int i = 100;
        while (i > 0) {
            line_end.position = origin_pos + (connection_count + 1) * FakeRope.DEFAULT_LEN * Vector3.left;
            i--;
            yield return new WaitForEndOfFrame();
        }
      

    }

    int still_connect() {
        FakeRope fp = start_rope;
        int cnt = 0;
        current_string = "";
        while (fp != end_rope) {
            current_string += fp.ch.ToString();           
            fp = fp.next_rope();
            cnt++;
            if (fp == null) return -1;
        }
        current_string += fp.ch.ToString();
        result_ui.GetComponent<Text>().text = "Current string :" + current_string;
        Debug.Log(level_controller.GetComponent<LevelController>().current_target());
        if (current_string == level_controller.GetComponent<LevelController>().current_target()) {
            level_controller.GetComponent<LevelController>().level_up();
            transform.FindChild("cheers").gameObject.SetActive(true);
            Invoke("close_cheers", 3f);
        }
        return cnt;
    }
    void close_cheers() {
        transform.FindChild("cheers").gameObject.SetActive(false);
    }

	/*Initialize ropes according to input string*/
	public void init_flags(string s) {
		IEnumerator co = init_linked_rope(s);
		StartCoroutine(co);
	}

	IEnumerator init_linked_rope (string s) {
		int cnt = s.Length;
        Transform last_rope = transform;
        for (int i = 0; i < cnt ; i++) {
            GameObject my_rope = Instantiate(rope_prefab);
            my_rope.transform.SetParent(transform);
            my_rope.transform.localPosition =(cnt - i)*FakeRope.DEFAULT_LEN * new Vector3(4f, 0f, 0f);
            yield return new WaitForSeconds(0.1f);
            if (i == 0) {
                line_end = my_rope.GetComponent<FakeRope>().start;
                origin_pos_end = line_end.position;
                end_rope = my_rope.GetComponent<FakeRope>();
            }
			if (i > 0 ){
                my_rope.GetComponent<FakeRope>().attach(last_rope);
			}
            my_rope.GetComponent<FakeRope>().set_character(s[i]);
            last_rope = my_rope.transform;
            yield return new WaitForEndOfFrame();
        }
        start_rope = last_rope.GetComponent<FakeRope>();
        line_start = start_rope.end;
        start_rope.end.position = origin_pos;
        origin_pos_start = line_start.position;
        connection_count = still_connect();
        init_finished = true;
	}



	public bool attach_ropes(Transform hand, Transform src, Transform dest = null) {
        grabbing_end = false;
        if (hand.FindChild("start") == null && src.FindChild("start") == null) {
            Debug.LogError("The operating node is missing.");
        }
        hand.FindChild("start").SetParent(src);
        if (src.GetComponent<FakeRope>() == null){
			Debug.Log("Nothing to be operated.");
			return false;
		}
   

        if (dest == null) {
            connection_count = still_connect();
            if (connection_count == -1) {
                StartCoroutine("on_disconnected");
            }
            return true;
		}
        FakeRope src_fr = src.GetComponent<FakeRope>();
        if (dest == start_rope.transform && src_fr == end_rope) {
            //invalid operation
            connection_count = still_connect();
            if (connection_count == -1) {
                StartCoroutine("on_disconnected");
            }
            return false;
        }
        //if there is no error, could get rid of this
        if (dest.GetComponent<FakeRope>() == src_fr.first_child()) {
            //invalid operation
            connection_count = still_connect();
            if (connection_count == -1) {
                StartCoroutine("on_disconnected");
            }
            return false;
        }
        src_fr.attach(dest);
                   
        if (dest == start_rope.transform) {
            FakeRope tmp = src_fr;
            while (tmp.first_child() != null) {
                tmp = tmp.first_child();
            }
            start_rope = tmp;
            line_start = tmp.end;
            line_start.position = origin_pos_start;
            Debug.LogWarning("Attaching to the startnode.");
            //change here: start node should be the child of scr_fr until there is none
        }

        if (src_fr == end_rope) {
            if (dest.GetComponent<FakeRope>() != null) {
                FakeRope tmp = dest.GetComponent<FakeRope>();
                //It is here that crushed!
                if (!circle_formed()) {
                    while (tmp.next_rope() != null) {
                        tmp = tmp.next_rope();
                    }
                    end_rope = tmp.GetComponent<FakeRope>();
                    line_end = end_rope.start;
                }
            }
        }
        //detect connections
        //detect circles from start, if so, reject connection
        if (circle_formed())
            src_fr.dettach();
        //fixing crush
        if (Mathf.Abs(GameObject.Find("hand_left").GetComponent<RotateHand>().get_offset_angle()) < 1e-5) {
            line_start.position = origin_pos_start;
        }
        connection_count = still_connect();
        if (connection_count == -1) {
            StartCoroutine("on_disconnected");
        }
        return true;
	}

    bool circle_formed() {
        FakeRope fp = start_rope;
        List<FakeRope> previous = new List<FakeRope>();
        Stack<FakeRope> node_to_search = new Stack<FakeRope>();
        node_to_search.Push(fp);
        while (node_to_search.Count > 0) {
            fp = node_to_search.Pop();
            if (!previous.Contains(fp)) {
                previous.Add(fp);
                if (fp.next_rope() == null) return false;
                node_to_search.Push(fp.next_rope());
            } else {
                // there is a circle in the list
                return true;
            }
        }
        return false;
    }
	/*++++++++++++++!!!!!!!!!!!!!!!!!!!!++++++++++++++++++++++++
				Please call this when grabbing!
	++++++++++++++++!!!!!!!!!!!!!!!!!!!!++++++++++++++++++*/
	public bool grab(Transform hand, Transform target) {
        //when grab something, move the !START_NODE of a rope with hand
        FakeRope fk_rp = target.GetComponent<FakeRope>();
        if (!target.GetComponent<FakeRope>().not_connect_others()){
            Debug.LogWarning("Dettaching my rope!");
            //I am connecting to someone else
            fk_rp.dettach();
        }

        if (target == end_rope.transform) {
            grabbing_end = true;
        }
        
        Transform start_node = target.FindChild("start");
        start_node.SetParent(hand);
        start_node.localPosition = Vector3.zero;
        return true;	
	}

    public bool clear_flags() {
        GameObject[] flags = GameObject.FindGameObjectsWithTag("flag");
        foreach (GameObject f in flags) {
            if(!f.GetComponent<FakeRope>().is_template)
                Destroy(f);
        }
        init_finished = false;
        return true;
    }

    IEnumerator on_disconnected() {
        List<FakeRope> to_be_delete = new List<FakeRope>();
        Transform velocity_target = line_end;
        //search for all the children of ENd node
        Stack<FakeRope> search_stack = new Stack<FakeRope>();
        search_stack.Push(end_rope);
        FakeRope currentnode;
        rope_reconstruction();
        connection_count = still_connect();
        while (search_stack.Count > 0) {
            currentnode = search_stack.Pop();        
            if (to_be_delete.Contains(currentnode))
                continue;
            to_be_delete.Add(currentnode);
            if (currentnode.first_child() != null) {
                search_stack.Push(currentnode.first_child());
            }
            if (currentnode.children().Count > 0){
                foreach (FakeRope fk in currentnode.children())
                    search_stack.Push(fk);
            }            
        }
        int i = 0;
        while (i < 300) {
            velocity_target.position +=  fly_velocity;
            yield return new WaitForEndOfFrame();
            i++;
        }
        
       foreach (FakeRope fk in to_be_delete) {
            Destroy(fk.gameObject,0.1f);
        }
        Debug.LogWarning("delete list:" + to_be_delete.Count);
        
    }

    public void rope_reconstruction() {
        FakeRope fp = start_rope;
        while (fp.next_rope() != null) {
            fp = fp.next_rope();
        }
        end_rope = fp;
        line_end = fp.start;
    }
}
