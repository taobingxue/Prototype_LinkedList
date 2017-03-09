﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkedRope : MonoBehaviour {
    public static LinkedRope instance = null;
	public GameObject rope_prefab;
	public float rotate_angle;
	public Transform test_src;
	public Transform test_dest;
	
	void Awake () {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
	}

    void Start() {
		init_flags("test");	
    }

	void Update(){
		if (Input.GetKeyDown(KeyCode.A)) {
			attach_ropes(test_src, test_dest);
		}
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
            my_rope.transform.localPosition = i*FakeRope.DEFAULT_LEN * new Vector3(1f, 0f, 0f);
            yield return new WaitForSeconds(0.1f);
			if (i > 0){
				my_rope.GetComponent<FakeRope>().attach(last_rope);
				my_rope.GetComponent<FakeRope>().set_character(s[i-1]);
			}
            last_rope = my_rope.transform;
            yield return new WaitForSeconds(0.1f);
        }
	}

	public bool attach_ropes(Transform src, Transform dest = null) {
		if (src.GetComponent<FakeRope>() == null){
			Debug.Log("Nothing to be operated.");
			return false;
		}
		FakeRope src_fr = src.GetComponent<FakeRope>();
		if (dest == null) {
			Debug.Log("Dettach.");
			src_fr.dettach();
			return true;
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
	public bool grab(Transform target, Transform hand) {
		//when grab something, move the !START_NODE of a rope with hand
		
		return true;
			
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawWireSphere( Vector3.zero, 1f);
	}

}
