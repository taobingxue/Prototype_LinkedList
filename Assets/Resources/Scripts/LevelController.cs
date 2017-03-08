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
        string ans = "";
        for (int i = 0; i < 26; ++i) {
            if (s.IndexOf((char) (i + 65)) >= 0) {
                ans += (char) (i + 65);
            }
        }
        return ans;
    }

}

public class LevelController : MonoBehaviour {

    public Level[] levels;
    int idx = 0;
     
    public int max_size;
    public GameObject[] lists;

    public GameObject edge_prefab;
    public GameObject node_prefab;

    public ListNode[] templates;

    private void Awake() {
        lists = new GameObject[max_size];
        for (int i = 0; i < max_size; ++i) {
            lists[i] = null;
        }

        level_up();
    }
    
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Space)) {
            level_up();
        }
	}

    public void level_up() {
        clean_level();
        if (idx == levels.Length) {
            Debug.Log("The end!");
            return ;
        }
        create_level();
        idx += 1;
    }

    void create_level() {
        set_template_char();
        create_flags();
    }
    
    void set_template_char() {
        string s = Level.get_char(levels[idx].target);
        int l = s.Length;
        if (s.Length > templates.Length) {
            Debug.Log("Too many characters for level " + idx);
            return ;
        }
        for (int i = 0; i < 6; ++i) {
            templates[i].set_character(i < l ? s[i]:' ');
        }
    }

    void create_flags() {
        lists[0] = Instantiate(edge_prefab) as GameObject;
        lists[1] = Instantiate(edge_prefab) as GameObject;
        lists[0].GetComponent<ListNode>().set_character(' ');
		lists[1].GetComponent<ListNode>().set_character(' ');
		
		int l = levels[idx].init.Length;
		for (int i = 0; i < l; ++i) {
			lists[i + 2] = Instantiate(node_prefab) as GameObject;
		}
		for (int i = l - 1; i >= 0; --i) {
			ListNode tmp = lists[i + 2].GetComponent<ListNode>();
			tmp.set_character(levels[idx].init[i]);
			if (i < l - 1) {
				tmp.next_node = lists[i + 3];
			} else {
				tmp.next_node = lists[1];
			}
		}
        lists[0].GetComponent<ListNode>().next_node = lists[2];

        for (int i = 0; i < l + 2; ++i) {
            lists[i].transform.parent = transform;
        }
    }
    
    void clean_level() {
        for (int i = 0; i < max_size; ++i) {
            if (lists[i] == null) return ;
            Destroy(lists[i]);
            lists[i] = null;
        }
    }
}
