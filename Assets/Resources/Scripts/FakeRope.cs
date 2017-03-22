using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FakeRope : MonoBehaviour {
    const int SUBDIV = 30;
    const float THREAD = 0.01f;
    public static float DEFAULT_LEN = 0.6f;

    //start is a fixed position where the rope fixed to
    public Transform start;
    public Transform end;
    public bool is_template;
    public GameObject[] selected_ring;
	public float vanishing_x;
    
	GameObject[] ring;
    GameObject rotate_wheel;
    protected Vector3[] my_rope_nodes;
    //start connect to connecting_to 's end 
    public FakeRope connecting_to;
    //end connect with connecting to's start
    public FakeRope come_from;
    protected LineRenderer rend;
    public List<FakeRope> candidate;

    private Vector3[] last_positions = new Vector3[2];
    private float rope_dis;
    int roller = 0; 

    #region mono behavior
    void Awake () {
        rend = GetComponent<LineRenderer>();
        instantiate_rope();
        last_positions[0] = start.position;
        last_positions[1] = end.position;
    }

    void Update()
    {
        //updateforce();       
        if (end.position != last_positions[1])
            bi_dir_movement(false);
        if (start.position != last_positions[0])
            bi_dir_movement();

        last_positions[0] = start.position;
        last_positions[1] = end.position;
        update_arrow();
    }
    #endregion


    #region rope physics
    /*--------------------------------------------------------
     * If rope has no start end nodes, it will create a tmp one
    ---------------------------------------------------------*/
    void instantiate_rope() {
        ring = new GameObject[2];
        ring[0] = null;
        ring[1] = null;
        connecting_to = null;
        come_from = null;
        start = transform.FindChild("start");
        end = transform.FindChild("end");
        Debug.LogWarning(gameObject.name +"start:" + start.name);
        start.localPosition = new Vector3(-DEFAULT_LEN /2, 0f,0f);
        
        end.localPosition = new Vector3(DEFAULT_LEN / 2, 0f, 0f);
        Debug.LogWarning("end:" + end.name);
        rend.numPositions = SUBDIV;
        my_rope_nodes = new Vector3[SUBDIV];
        candidate = new List<FakeRope>();
        int i;
        //simple blinear interpolation
        for (i = 0; i < SUBDIV; i++)
        {
            Vector3 tmp = ((SUBDIV - 1 - i) * start.transform.position + i * end.transform.position) / (SUBDIV - 1);
            if (Vector3.Distance(tmp, my_rope_nodes[i]) > THREAD)
                my_rope_nodes[i] = tmp;        
        }
        rend.SetPositions(my_rope_nodes);
        rope_dis = Vector3.Distance(my_rope_nodes[0], my_rope_nodes[SUBDIV - 1]) / (SUBDIV - 1);
		Color rc = Random.ColorHSV(0f,1f,0.8f,1f); 
		rend.material.SetColor("_Color",rc);
        update_arrow(true);
    }


    void update_arrow(bool init = false) {
        LineRenderer ld =transform.FindChild("arrow").GetComponent<LineRenderer>();
        if (init) {
            ld.numPositions = 5;     
            ld.startWidth = 0.05f;
            ld.endWidth = 0.01f;
            ld.endColor = new Color(1f, 0f, 0f, 0.5f);
            ld.startColor = new Color(0f, 0f, 1f, 0.5f);
        }
        Vector3[] pos = new Vector3[5];
        int idx = 0;
        for (idx = 0; idx < 5; idx++) {
            pos[idx] = my_rope_nodes[4 - idx]+ Vector3.one * 0.00001f;
        }
        ld.SetPositions(pos);
    }

    /*---------Rope UPdate Function----------------------------------------------------
     * Drag the rope from a place to another place will update of rope
     *    dis = node[x+1] - node[x]
     *    node[x+1] = (node[x+2] - node[x]).norm() * dis + node[x];   
     * No spring first  
     ----------------------------------------------------------------------------------*/
    void update_my_rope(bool order = true) {
        int i = 1;
        Vector3 dir;
        if (order)
        {
            while (i < SUBDIV - 1)
            {
                dir = (my_rope_nodes[i + 1] - my_rope_nodes[i - 1]).normalized;
                my_rope_nodes[i] = my_rope_nodes[i - 1] + dir * rope_dis;
                i++;
            }
            if (come_from == null)
                dir = (my_rope_nodes[i] - my_rope_nodes[i - 1]).normalized;
            else
                dir = (come_from.my_rope_nodes[1] - my_rope_nodes[i - 1]).normalized;
            my_rope_nodes[i] = my_rope_nodes[i - 1] + rope_dis * dir;
        }
        else {
            i = SUBDIV - 2;
            while (i > 0)
            {
                dir = (my_rope_nodes[i  - 1] - my_rope_nodes[i + 1]).normalized;
                my_rope_nodes[i] = my_rope_nodes[i + 1] + dir * rope_dis;
                i--;
            }
            if(connecting_to == null)
                dir = (my_rope_nodes[i] - my_rope_nodes[i + 1]).normalized;
            else
                dir = (connecting_to.my_rope_nodes[SUBDIV - 2] - my_rope_nodes[i + 1]).normalized;
            my_rope_nodes[i] = my_rope_nodes[i + 1] + rope_dis * dir;
        }
        rend.SetPositions(my_rope_nodes);
    }

    void bi_dir_movement(bool start_move = true) {
        if (start_move){
            my_rope_nodes[0] = start.transform.position;
            update_my_rope();
            end.transform.position = my_rope_nodes[SUBDIV - 1];
        }
        else {
            my_rope_nodes[SUBDIV - 1] = end.transform.position;
            update_my_rope(false);
            start.transform.position = my_rope_nodes[0];
        }
        update_flag_pos();
    }

    void update_flag_pos() {
        Transform flag = transform.FindChild("LetterFlag");
        Transform arrow = transform.FindChild("arrow");
        flag.position =  my_rope_nodes[(SUBDIV - 1)-3];
        float _z = flag.position.z;
        if (flag.position.x > vanishing_x && (_z - 4.8f) * (_z - 4f) < 0) {
            flag.gameObject.SetActive(false);
            arrow.gameObject.SetActive(false);
        } else {
            flag.gameObject.SetActive(true);
            arrow.gameObject.SetActive(true);
        }
    }


    bool stretch(){
        float _dis = Vector3.Distance(my_rope_nodes[0], my_rope_nodes[SUBDIV - 1]);
        if (Mathf.Abs(_dis - rope_dis * (SUBDIV - 1)) < 1e-4){
            return true;
        }
        return false;
    }

    void move_to(Vector3 target_pos) {
        int i = 0;
        Vector3 mov_vec = target_pos - my_rope_nodes[0]; 
        for (i = 0; i < SUBDIV; i++)
            my_rope_nodes[i] += mov_vec;

    }
    /*--------------------force behave-----------------------
     * if there is a rigidbody attached to either side
     * pass the force along the rope
     * One Side Force:
     *      if the rope is stretched, it will maintain the same length, 
     *      so it will set the rigid body's force's projection on the rope to 0
     *      Vector3 new_force = force + dot(force, dir) * dir   
    ---------------------------------------------------------*/
    void updateforce() {
        Rigidbody rb_start = start.GetComponent<Rigidbody>();
        Rigidbody rb_end = end.GetComponent<Rigidbody>();
        if (rb_start != null && rb_end != null) {
            Vector3 dir = ( my_rope_nodes[1] - my_rope_nodes[0]).normalized;
            ConstantForce f1 = start.GetComponent<ConstantForce>();
            ConstantForce f2 = end.GetComponent<ConstantForce>();
            //add force if stretched
            
            if (stretch()) {
                rb_start.AddForce(Vector3.Dot(f2.force, dir) * dir);    
                rb_end.AddForce(Vector3.Dot(f1.force, dir) * dir);
            }
            else Debug.Log("Loose!");
        }
    }
    #endregion

    #region rope operations
    /* attach one side of the rope to some existing node*/
    public bool attach(Transform target, bool start_node = true){
        if (target == null) return false;
        if (start_node) {
            FakeRope _rp = target.GetComponent<FakeRope>();
            Debug.Log("Attaching my start node onto target's end");
            Transform old_start = start;
            start = _rp.end;
            Destroy(old_start.gameObject);
            //only update when _rp does not have constraint
            if (_rp.come_from == null)
                _rp.come_from = this;
            else 
                if(!_rp.candidate.Contains(this) && _rp.come_from != this)
                    _rp.candidate.Add(this);
            connecting_to = _rp;
        }      
        return true;
    }

    public bool dettach() {
        if (transform.FindChild("start") == null) {
            GameObject tmp_start_node = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tmp_start_node.transform.position = start.position + 0.2f * Vector3.one;
            tmp_start_node.name = "start";
            tmp_start_node.transform.SetParent(transform);
            tmp_start_node.transform.localScale = 0.02f * Vector3.one;
            start = tmp_start_node.transform;
        }
        //if who I am connecting to comes from me
        if (connecting_to != null) {
            if (connecting_to.come_from == this) {
                connecting_to.come_from = connecting_to.candidate.Count == 0 ? null : connecting_to.candidate[0];
                Debug.Log("fixing candidates" + connecting_to.candidate.Count);
            } 
            else {
                connecting_to.candidate.Remove(this);
            }
        }
        connecting_to = null; 
        return true;
    }

	public bool not_connect_others() {
		return connecting_to == null;
	}

    public List<FakeRope> other_children() {
        return candidate;
    }

    public FakeRope first_child() {
        return come_from;
    }

    public FakeRope next_rope() {
        return connecting_to;
    }

    public void turn_clipping(bool on_off) {
        float val = on_off ? -3.2f : -500f;
        GetComponent<MeshRenderer>().material.SetFloat("_ClipPoint", val);
        MeshRenderer[] children_rends = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer rend in children_rends) {
            rend.material.SetFloat("_ClipPoint", val);
        }
        vanishing_x = val;
    }

    #endregion

    #region listnode
    public char ch;
    public void set_character(char _ch)
    {
        transform.FindChild("LetterFlag").FindChild("Canvas").FindChild("flagletter").GetComponent<Text>().text = _ch + "";
        ch = _ch;
    }

    public void select(int ty = 0)
    {
        if (ring[ty] != null)
        {
            Debug.Log("Invalid call.");
        }
        ring[ty] = Instantiate(selected_ring[ty]) as GameObject;
        ring[ty].transform.parent = transform.FindChild("LetterFlag");
        ring[ty].transform.localPosition = Vector3.up * 0.4f;
        ring[ty].transform.localScale = Vector3.one;
        ring[ty].name = "ringring";
    }

    public void unselect(int ty = 0)
    {
        if (ring[ty] == null)
        {
            Debug.Log("Invalid call.");
            return;
        }

        Destroy(ring[ty]);
        ring[ty] = null;
    }
    #endregion

}
