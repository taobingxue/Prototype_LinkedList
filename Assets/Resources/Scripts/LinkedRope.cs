using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkedRope : MonoBehaviour {
    const float DELTA = 0.01f;

    public static LinkedRope instance = null;
	public GameObject rope_prefab;
	public float rotate_angle;

    bool init_finished;
    Vector3 origin_pos;
    Vector3 origin_pos_start;
    Vector3 origin_pos_end;
    Transform line_start;
    Transform line_end;
    public FakeRope start_rope;
    public FakeRope end_rope;
    int roller =0;
    
	
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
                int cnt = still_connect();
                Debug.Log("path:"+cnt);
                if (cnt == -1) {
                    line_end.position += new Vector3(-0.1f, 0.2f, 0f);
                }
                else
                    line_end.position = origin_pos + (cnt + 1) * FakeRope.DEFAULT_LEN * Vector3.left;
            }
        }
	
	}

    int still_connect() {
        FakeRope fp = end_rope;
        int cnt = 0;
        while (fp != start_rope) {
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
		int cnt = s.Length + 1;
        Transform last_rope = transform;
        for (int i = 0; i < cnt ; i++) {
            GameObject my_rope = Instantiate(rope_prefab);
            my_rope.transform.SetParent(transform);
            my_rope.transform.localPosition = i*FakeRope.DEFAULT_LEN * new Vector3(4f, 0f, 0f);
            yield return new WaitForSeconds(0.1f);
            if (i == 0) {
                my_rope.GetComponent<FakeRope>().start.position = origin_pos;
                my_rope.GetComponent<FakeRope>().end.position = origin_pos - new Vector3(FakeRope.DEFAULT_LEN, 0f, 0f);
                line_start = my_rope.GetComponent<FakeRope>().start;
                origin_pos_start = line_start.position;
                start_rope = my_rope.GetComponent<FakeRope>();
            }
			if (i > 0){
				my_rope.GetComponent<FakeRope>().attach(last_rope);
				my_rope.GetComponent<FakeRope>().set_character(s[i-1]);
			}
            last_rope = my_rope.transform;
            yield return new WaitForSeconds(0.1f);
        }
        line_end = last_rope.GetComponent<FakeRope>().end;
        end_rope = last_rope.GetComponent<FakeRope>();
        origin_pos_end = line_end.position;
        init_finished = true;
	}


	public bool attach_ropes(Transform hand, Transform src, Transform dest = null) {
        hand.FindChild("start").SetParent(src);
        if (src.GetComponent<FakeRope>() == null){
			Debug.Log("Nothing to be operated.");
			return false;
		}
		FakeRope src_fr = src.GetComponent<FakeRope>();
		/*if (dest == null) {
			Debug.Log("Dettach.");
			src_fr.dettach();
			return true;
		}*/
        if (dest.GetComponent<FakeRope>() == end_rope) {
            Debug.LogWarning("Cannot attach to line end.");
            return false;
        }
		if (src_fr.not_connect_others())
			src_fr.attach(dest);
		else {
			src_fr.dettach();
			src_fr.attach(dest);
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
        if (fk_rp == start_rope) {
            Debug.Log("Cannot dettach head!");
            return false; 
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

	private void OnDrawGizmos()
	{
		Gizmos.DrawWireSphere( Vector3.zero, 1f);
	}

}
