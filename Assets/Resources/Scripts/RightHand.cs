using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightHand : MonoBehaviour {

    public float grab_dis;
    bool grabbing;
    GameObject grab_obj;
    GameObject pointing;
    GameObject inhand;

    LineRenderer line_renderer;
    public float line_length;
    
    void Awake() {
        pointing = null;
        grab_obj = null;
        inhand = null;
    }
	// Use this for initialization
	void Start () {
        line_renderer = GetComponent<LineRenderer>();
        line_renderer.enabled = false;
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
        if (inhand == null) {
            return null;
        }

        RaycastHit hitInfo;
        Physics.Raycast(transform.position, get_point_dir(), out hitInfo, line_length);
        if (hitInfo.collider != null) {
            if (hitInfo.collider.transform.parent.tag == "flag" && hitInfo.collider.transform.parent.gameObject.GetComponent<FakeRope>().is_template == false) {
                return hitInfo.collider.gameObject;
            }
        }
        
        return null;
    }
    
    void update_pointing() {
        draw_line();
        
        GameObject tmp = find_pointing();
        if (tmp != null) {
            tmp = tmp.transform.parent.gameObject;
        }
        if (tmp != pointing) {
            if (pointing != null) {
                pointing.GetComponent<FakeRope>().unselect(1);
            }
            if (tmp != null && tmp.GetComponent<FakeRope>().is_template == false) {
                tmp.GetComponent<FakeRope>().select(1);
            }
            pointing = tmp;
        }
    }
    
    GameObject find_grab_obj() {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("flag");
        int l = objs.Length;
        
        float minm = 2100000000;
        int idx = -1;
        for (int i = 0; i < l; ++i) {
            float dis = Vector3.Distance(transform.position, objs[i].transform.FindChild("LetterFlag").position);
            if (minm > dis) {
                minm = dis;
                idx = i;
            }
        }

        return minm < grab_dis ? objs[idx] : null;
    }
    
    void update_selected() {
        GameObject tmp = find_grab_obj();
        if (tmp != grab_obj) {
            if (grab_obj != null) {
                grab_obj.GetComponent<FakeRope>().unselect(0);
            }
            if (tmp != null) {
                tmp.GetComponent<FakeRope>().select(0);
            }
            grab_obj = tmp;
        }
    }
    
    bool check_grabbing() {
        return (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger));
    }
    
    void update_grabbing() {
        bool tmp = check_grabbing();
        if (tmp && (!grabbing)) {
            if (grab_obj == null) {
                Debug.Log("You grab nothing");
                grabbing = tmp;
                return ;
            }
            line_renderer.enabled = true;
            if (grab_obj.GetComponent<FakeRope>().is_template) {
                GameObject obj = Instantiate(grab_obj) as GameObject;
                obj.GetComponent<FakeRope>().is_template = false;
                //Luna
                grab_obj.SetActive(false);
                IEnumerator c = recover_template(grab_obj.transform);
                StartCoroutine(c);
                while (obj.transform.FindChild("LetterFlag").FindChild("ringring") != null) {
                    DestroyImmediate(obj.transform.FindChild("LetterFlag").FindChild("ringring").gameObject);
                }
                LinkedRope.instance.grab(transform, obj.transform);
                inhand = obj;
            } else {
               if(LinkedRope.instance.grab(transform, grab_obj.transform))
                    inhand = grab_obj;
            }
        } else if ((!tmp) && grabbing && inhand != null) {
            line_renderer.enabled = false;
            LinkedRope.instance.attach_ropes(transform, inhand.transform, pointing == null ? null : pointing.transform);
            inhand = null;
        }
        grabbing = tmp;
    }

    IEnumerator recover_template(Transform _template) {
        yield return new WaitForSeconds(5f);
        _template.gameObject.SetActive(true);
       
    }
}
