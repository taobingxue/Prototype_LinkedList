using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightHand : MonoBehaviour {

    public LevelController level;

    bool grabbing;
    GameObject grab_obj;
    GameObject pointing;

    LineRenderer line_renderer;
	// Use this for initialization
	void Start () {
        line_renderer = GetComponent<LineRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        update_pointing();
        update_grabbing();
	}

    void update_pointing() {

    }

    void update_grabbing() {

    }
}
