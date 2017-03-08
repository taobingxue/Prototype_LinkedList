using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ListNode : MonoBehaviour {

    public bool is_template;
    public GameObject[] selected_ring;
    GameObject[] ring;

    public char ch = ' ';
    public GameObject next_node = null;
    public int index;

    private void Awake() {
        ring = new GameObject[2];
        ring[0] = null;
        ring[1] = null;
        index = -1;
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void set_character(char _ch) {
        transform.FindChild("Canvas").FindChild("flagletter").gameObject.GetComponent<Text>().text = _ch + "";
        ch = _ch;
    }

    public void select(int ty = 0) {
        if (ring[ty] != null) {
            Debug.Log("Hehe!");
        }
        ring[ty] = Instantiate(selected_ring[ty]) as GameObject;
        ring[ty].transform.parent = transform.FindChild("flagletter");
        ring[ty].transform.localPosition = Vector3.zero;
        ring[ty].transform.localScale = Vector3.one;
    }

    public void unselect(int ty = 0) {
        if (ring[ty] == null) {
            Debug.Log("Hehe!");
            return;
        }

        Destroy(ring[ty]);
        ring[ty] = null;
    }
}
