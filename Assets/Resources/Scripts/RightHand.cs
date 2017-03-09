using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightHand : MonoBehaviour {

    public LevelController level;

    public flag grab_dis;
    bool grabbing;
    GameObject grab_obj;
    GameObject pointing;
    GameObject inhand;

    public LineRenderer line_renderer;
    public float line_length;
    
    void Awake() {
        pointing = null
        grab_obj = null;
        inhand = null;
    }
	// Use this for initialization
	void Start () {
        line_renderer = GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        update_pointing();
        update_selected();
        update_grabbing();
	}

    Vector3 get_point_dir() {
        return transform.forward.normalized;
    }
    
    void draw_line() {
        Vector3[] vecs = new Vector3[2];
        vecs[0] = transform.position;
        vecs[1] = vecs[0] + get_point_dir() * line_length;
        line_renderer.SetPositions(vecs);
    }
    
    GameObject find_pointing() {
        RaycastHit hitInfo;
        Raycast(transform.position, get_point_dir(), out hitInfo, line_length);
        if (hitInfo != null && hitInfo.collider != null) {
            return hitInfo.collider.gameObject;
        }
        
        return null;
    }
    
    void update_pointing() {
        draw_line();
        
        GameObject tmp = find_pointing();
        if (tmp != pointing) {
            if (pointing != null) {
                pointing.GetComponent<FakeRope>().unselect(1);
            }
            if (tmp != null) {
                tmp.GetComponent<FakeRope>().select(1);
            }
            pointing = tmp;
        }
    }
    
    GameObject grab_obj() {
        GameObject[] objs = GameObject.FindGameObjectsWithTag('flag');
        int l = objs.Length;
        
        float minm = 2100000000;
        int idx = -1;
        for (int i = 0; i < l; ++i) {
            float dis = Vector3.Distance(transform.position, objs[i].transform.position);
            if (minm > dis) {
                dis = minm;
                idx = i;
            }
        }
        return minm < grab_dis ? objs[minm] : null;
    }
    
    void update_selected() {
        GameObject tmp = grab_obj();
        if (tmp != grabbing) {
            if (grabbing != null) {
                grabbing.GetComponent<FakeRope>().unselect(0);
            }
            if (tmp != null) {
                tmp.GetComponent<FakeRope>().select(0);
            }
            grabbing = tmp;
        }
    }
    
    bool check_grabbing() {
        return (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger) || OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger));
    }
    
    void update_grabbing() {
        bool tmp = check_grabbing();
        if (tmp && (!grabbing)) {
            if (grabbing == null) {
                Debug.Log("You grab nothing");
                return ;
            }
            if (grabbing.GetComponent<FakeRope>().is_template) {
                GameObject obj = Instantiate(grabbing) as GameObject;
                obj.GetComponent<FakeRope>().is_template = false;
                LinkedRope.instance.grab(transform, obj.transform);
                inhand = obj;
            } else {
                LinkedRope.instance.grab(transform, grab_obj.transform);
                inhand = grab_obj;
            }
        } else if ((!tmp) && grabbing) {
            LinkedRope.instance.attach_rope(transform, grab_obj.transform, pointing.transform);
        }
    }
}
