using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkedRope : MonoBehaviour {
    public static LinkedRope instance = null;
    public GameObject rope_prefab;
	
	void Awake () {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
	}

    void Start() {
        StartCoroutine("init_linked_rope");
    }
    void Update() {
        
    }
	
	
	IEnumerator init_linked_rope () {
        Transform last_rope = transform;
        for (int i = 0; i < 5; i++) {
            GameObject my_rope = Instantiate(rope_prefab);
            my_rope.transform.SetParent(transform);
            my_rope.transform.localPosition = i*FakeRope.DEFAULT_LEN * new Vector3(1f, 0f, 0f);
            yield return new WaitForSeconds(0.1f);
            if (i > 0) 
                my_rope.GetComponent<FakeRope>().attach(last_rope);
            last_rope = my_rope.transform;
            yield return new WaitForSeconds(0.1f);
        }
	}
}
