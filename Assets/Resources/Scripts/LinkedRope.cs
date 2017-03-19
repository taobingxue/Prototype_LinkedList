using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkedRope : MonoBehaviour {
    const float DELTA = 0.01f;

    public static LinkedRope instance = null;
	public GameObject rope_prefab;
    public GameObject head;
    public GameObject tail;
	public float rotate_angle;

    public FakeRope start_rope;
    public FakeRope end_rope;

    bool init_finished;
    Vector3 origin_pos;
    Vector3 origin_pos_start;
    Vector3 origin_pos_end;
    Vector3 fly_velocity= new Vector3(-0.2f,0.2f,0f);
    Transform line_start;
    public Transform line_end;

    public int connection_count = 0;
    
	
	void Awake () {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

	}

    void Start() {
        origin_pos = transform.position;
    }

	void Update(){
        if (init_finished) {
            float angle = GameObject.Find("hand_left").GetComponent<RotateHand>().get_offset_angle();
            if (angle > 0f)
                line_start.position = origin_pos_start + DELTA * angle * Vector3.right;
            else {
                //search for the path from end to start
                if (connection_count != -1)  
                    line_end.position = origin_pos + (connection_count + 1) * FakeRope.DEFAULT_LEN * Vector3.left;
                }
        }
	
	}

    int still_connect() {
        FakeRope fp = start_rope;
        int cnt = 0;
        while (fp != end_rope) {
            
            fp = fp.next_rope();
            cnt++;
            if (fp == null) return -1;
        }
        return cnt;
    }

	/*Initialize ropes according to input string*/
	public void init_flags(string s) {
		IEnumerator co = init_linked_rope(s);
		StartCoroutine(co);
	}

	IEnumerator init_linked_rope (string s) {
		int cnt = s.Length + 2;
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
                if(i < cnt -1)
				    my_rope.GetComponent<FakeRope>().set_character(s[i-1]);
			}
            last_rope = my_rope.transform;
            yield return new WaitForSeconds(0.1f);
        }
        start_rope = last_rope.GetComponent<FakeRope>();
        line_start = start_rope.end;
        start_rope.end.position = origin_pos;
        origin_pos_start = line_start.position;
        connection_count = still_connect();
        init_finished = true;
	}



	public bool attach_ropes(Transform hand, Transform src, Transform dest = null) {
        if (hand.FindChild("start") == null && src.FindChild("start") == null) {
            Debug.LogError("The operating node is missing.");
        }
        hand.FindChild("start").SetParent(src);
        if (src.GetComponent<FakeRope>() == null){
			Debug.Log("Nothing to be operated.");
			return false;
		}
		FakeRope src_fr = src.GetComponent<FakeRope>();
		if (dest == null) {
			Debug.Log("Dettach.");
			src_fr.dettach();
            connection_count = still_connect();
            if (connection_count == -1) {
                StartCoroutine("on_disconnected");
            }
            return true;
		}
		if (src_fr.not_connect_others())
			src_fr.attach(dest);
		else {
			src_fr.dettach();
			src_fr.attach(dest);
		}
        if (dest == start_rope.transform) {
            start_rope = src_fr;
            line_start = src_fr.end;
            line_start.position = origin_pos_start;
            Debug.LogWarning("Attaching to the startnode.");
        }

        if (src_fr == end_rope) {
            if (dest.GetComponent<FakeRope>() != null) {
                end_rope = dest.GetComponent<FakeRope>();
                line_end = end_rope.start;
            }
        }
        //detect connections
        connection_count = still_connect();
        if (connection_count == -1) {
            StartCoroutine("on_disconnected");
        }
        return true;
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
            if (currentnode.first_child() == null)
                continue;
            else {
                search_stack.Push(currentnode.first_child());
                if (currentnode.other_children().Count == 0) continue;
                else {
                    foreach (FakeRope fk in currentnode.other_children())
                        search_stack.Push(fk);
                }
            }
        }
        int i = 0;
        while (i < 90) {
            velocity_target.position +=  fly_velocity;
            yield return new WaitForEndOfFrame();
            i++;
        }

        foreach (FakeRope fk in to_be_delete) {
            Destroy(fk.gameObject,0.1f);
        }
    }

    public void rope_reconstruction() {
        FakeRope fp = start_rope;
        while (fp.next_rope() != null) {
            fp = fp.next_rope();
        }
        end_rope = fp;
        line_end = fp.start;
    }

    private void OnDrawGizmos()
	{
		Gizmos.DrawWireSphere( Vector3.zero, 1f);
	}

}
