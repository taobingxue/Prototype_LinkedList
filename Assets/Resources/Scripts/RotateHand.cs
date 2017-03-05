using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateHand : MonoBehaviour {

    public float range;
    public GameObject wheel_center;
    public float circle_to_num_of_flags;

    public GameObject wheel;

    public TextMesh text;

    float flag_offset;
    float angle;
    Vector3 center_pos;
    float last_angle;
    bool grabbing;
    bool rotating;
	// Use this for initialization
	void Start () {
        flag_offset = 0;
        angle = 0;
        Debug.Assert(wheel_center != null);
        last_angle = 0;
        grabbing = false;
        rotating = false;
	}
	
	// Update is called once per frame
	void Update () {
        center_pos = wheel_center.transform.position;
        update_grab();
        update_rotation();     
        set_rotation();

       // text.text = "grab: " + grabbing + "\nrotating: " + rotating + "\nangle = " + angle + "\ndis = " + Vector3.Distance(transform.position, center_pos);
	}

    bool check_grab() {
        return (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger) || OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger));
    }

    bool check_rotating() {
        //Debug.Log(Vector3.Distance(transform.position, center_pos));
        return grabbing && (Vector3.Distance(transform.position, center_pos) <= range);
    }

    float get_angle() {
        Vector3 vec_now = transform.position;
        Vector3 vec_dis = vec_now - center_pos;
        vec_dis.z = 0;
        float new_angle = Vector3.Angle(Vector3.up, vec_dis.normalized);
        return new_angle * (vec_dis.x > 0 ? 1 : -1);
    }

    void update_grab() {
        bool new_grabbing = check_grab();
        if (new_grabbing == false && grabbing) {
            angle = 0;
        }

        grabbing = new_grabbing;
    }

    float dis_angle(float angle0, float angle1) {
        float res = 0;
        if (angle1 > angle0) {
            res = angle1 - angle0;
        } else {
            res = angle1 + 360 - angle0;
        }
        return res > 0 && res < 180 ? res : 0;
    }

    void update_rotation() {
        bool new_rotating = check_rotating();
        if (new_rotating) {
            float new_angle = get_angle();
            if (!rotating) {
                last_angle = new_angle;
            }
            else {
                angle = (angle + dis_angle(last_angle, new_angle)) % 360;
                last_angle = new_angle;
            }
        }

        rotating = new_rotating;
    }

    void set_rotation() {
        wheel.transform.localRotation = Quaternion.Euler(0, 0, angle);
    }
}
