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
     
    public FakeRope[] templates;
    
    private void Awake() {
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
        LinkedRope.instances.init_flags(levels[idx].init);
    }
    
    void clean_level() {
        LinkedRope.instance.clean_flags();
    }
}
