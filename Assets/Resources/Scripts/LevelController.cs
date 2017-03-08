using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Level {
    public string init;
    public string target;

    /*  
     * possibilities
     *      minimum operations
     *      hints
     *      etc.
     */

    public static string get_char(string s) {

    }

}

public class LevelController : MonoBehaviour {

    public Level[] levels;
    int idx = 0;
     
    public int max_size;
    public GameObject[] lists;

    public GameObject edge_prefab;
    public GameObject node_prefab;

    public GameObject[] templates;

    private void Awake() {
        lists = new GameObject[max_size];
        for (int i = 0; i < max_size; ++i) {
            lists[i] = null;
        }

        create_level();
    }
    
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void level_up() {

    }

    void create_level() {

    }

    void clean_level() {

    }
}
