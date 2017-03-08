﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FakeRope : MonoBehaviour {
    const int SUBDIV = 30;
    const float THREAD = 0.01f;
    public static float DEFAULT_LEN = 1f;

    //start is a fixed position where the rope fixed to
    public Transform start;
    public Transform end;
    public bool is_template;
    public GameObject[] selected_ring;
    GameObject[] ring;

    public Transform test_target;

    protected Vector3[] my_rope_nodes;
    //start connect to connecting_to 's end 
    protected FakeRope connecting_to;
    //end connect with connecting to's start
    protected FakeRope come_from;

    private LineRenderer rend;
    private Vector3[] last_positions = new Vector3[2];
    private float rope_dis;
    int roller = 0; 

    #region mono behavior
    void Start () {
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
        start.localPosition = new Vector3(-DEFAULT_LEN /2, 0f,0f);
        end.localPosition = new Vector3(DEFAULT_LEN / 2, 0f, 0f);
        rend.numPositions = SUBDIV;
        my_rope_nodes = new Vector3[SUBDIV]; 
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
    }

    /*---------Rope UPdate Function----------------------------------------------------
     * Drag the rope from a place to another place will update of rope
     *    dis = node[x+1] - node[x]
     *    node[x+1] = (node[x+2] - node[x]).norm() * dis + node[x];   
     * No spring first  
     ----------------------------------------------------------------------------------*/
    void update_my_rope( bool order = true) {
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
    readonly Vector3 offset_flag = new Vector3(0f, -0.2f, -0.18f);
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
        flag.position =  offset_flag + my_rope_nodes[(SUBDIV - 1) / 2]; 
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
        if (start_node) {
            FakeRope _rp = target.GetComponent<FakeRope>();
            Debug.Log("Attaching my start node onto target's end");
            Transform old_start = start;
            start = _rp.end;
            Destroy(old_start.gameObject);
            _rp.come_from = this;
            connecting_to = _rp;
        }      
        return true;
    }

    public bool dettach() {
        GameObject tmp_start_node = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tmp_start_node.transform.position = start.position + 0.2f * Vector3.one;
        tmp_start_node.name = "start";
        tmp_start_node.transform.SetParent(transform);
        start = tmp_start_node.transform;
        return true;
    }

    #endregion

    #region listnode
    public char ch;
    public void set_character(char _ch)
    {
        transform.FindChild("Canvas").FindChild("flagletter").gameObject.GetComponent<Text>().text = _ch + "";
        ch = _ch;
    }

    public void select(int ty = 0)
    {
        if (ring[ty] != null)
        {
            Debug.Log("Invalid call.");
        }
        ring[ty] = Instantiate(selected_ring[ty]) as GameObject;
        ring[ty].transform.parent = transform.FindChild("flagletter");
        ring[ty].transform.localPosition = Vector3.zero;
        ring[ty].transform.localScale = Vector3.one;
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