using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using PRM_Astar;
using waypoints;

public class taskPlanner : MonoBehaviour
{

    PRM_Graph_Manager local_planner;
    [Space]
    [Header("AGENTS")]
    [TextArea(4, 10)]
    public string Agents_info = "Agents are EVA/IVA astronauts or teleoperated/semi-autonomous robots /n Please enter number of agents, NAMES, unuque ID and provide the gameObjects for each";
    public List<agent> agents;
    [Space]
    [Header("LANDMARKS")]
    [TextArea(4, 10)]
    public string landmarks_info = "Landmarks are locations of interest to be visited";
    public List<landmark> landmarks;
    [Space]
    [Header("CUSTOM TASKS LIST")]
    [TextArea(4, 10)]
    public string tasks_info = "Each task has pre-conditions: agents locations and task states proir to the action \n Each task has a pst-condition task states \n use -1 for independent state";
    public List<customTask> Tasklist = new List<customTask>();
    [Space]
    [Header("START and END State")]
    //[TextArea(4, 10)]
    //public string start_info = "START State";
    public node start_node;
    //[TextArea(4, 10)]
    public node end_node;
    bool pathfound;
    int linear_t;
    //[SerializeField]
    //[Help("This is some help text!")]
    [Space]
    [HideInInspector]
    public node nbst;
    List<node> O;
    List<node> C;
    List<Edge> E;
    List<walk_costs> path_list;
    [HideInInspector]
    public node st_cpoy;
    List<string> procedure;
    void Start()
    {
        O = new List<node>();
        C = new List<node>();
        E = new List<Edge>();
        st_cpoy = new node(start_node);
        O.Add(start_node);
        nbst = new node(O[0]);
        procedure = new List<string>();
        pathfound = false;
        linear_t = 0;
        local_planner = GetComponent<PRM_Graph_Manager>();
        preplan = false;
        l1 = 0;
        l2 = 0;
        path_list = new List<walk_costs>();
    }
    bool preplan;
    int l1, l2;
    // Update is called once per frame
    void Update()
    {
        if (!preplan && !local_planner.pathfinding)
        {
            if (local_planner.isComplete)
            {
                var g = local_planner.nbst.G;
                var t = local_planner.nbst.t;
                var ac = new action(g, t, true);
                var wc = new walk_costs(l1, l2-1, ac);
                path_list.Add(wc);
            }


            if (l1 < landmarks.Count)
            {
                if (l2 < landmarks.Count)
                {
                    if (l1 != l2)
                    {
                        WALK(l1, l2, out var carbage);
                    }
                    l2++;
                }
                else
                {
                    l2 = 0;
                    l1++;
                }
            }
            else preplan = true;
        }


        if (preplan && !pathfound)
        {
            if (O.Count > 0)
            {
                var min_F = 9999999f;
                foreach (var o in O)
                {
                    if (o.met+o.t < min_F)
                    {
                        nbst = o;
                        min_F = nbst.met+nbst.t;
                    }
                }
                O.Remove(nbst);
                C.Add(new node(nbst));

                if (nbst.isGoal(end_node))
                {
                    pathfound = true;
                    var x = new node(nbst);
                    while (!x.locEqual(start_node) || !x.taskEqual(start_node))
                    {
                        procedure.Insert(0,x.pre_action);
                        foreach (var e in E)
                        {
                            if (e.E2.locEqual(x) && e.E2.taskEqual(x))
                            {
                                x = e.E1;
                                break;
                            }
                        }
                    }
                    Debug.Log("_________________---PROCEDURE PLAN GENERATED---_________________");
                    foreach (var task in procedure)
                    {
                        Debug.Log(task);
                    }


                    return;
                }
                foreach (var task in Tasklist)
                {
                    if (task.sat_pre_loc(nbst.locations_id) && task.sat_pre_tasks(nbst.tasks))
                    {
                        Debug.Log("performing task: " + task.name);
                        var x = new node(new List<int>(nbst.locations_id), task.set_post_tasks(new List<int>(nbst.tasks)), task.costs.met_cost + nbst.met, task.costs.time_elapsed + nbst.t, task.name);
                        var _e = new Edge(new node(nbst), new node(x));
                        var falg = false;
                        foreach (var c in C)
                        {
                            if (c.taskEqual(x) && c.locEqual(x))
                            {
                                falg = true;
                            }
                        }

                        if (!falg && !nbst.taskEqual(x))
                        {
                            O.Add(x);
                            E.Add(_e);
                        }
                    }
                }
                foreach (var loc in landmarks)
                {
                    if (nbst.locations_id[0] != loc.id)
                    {
                        WALK(nbst.locations_id[0],loc.id, out var walk_costs);
                        var on = nbst;
                        var new_loc = new List<int>(nbst.locations_id);
                        
                        new_loc[0] = loc.id;
                        var x = new node(new_loc, nbst.tasks, walk_costs.met_cost + nbst.met, walk_costs.time_elapsed + nbst.t,"walk FROM "+landmarks[nbst.locations_id[0]].name +" TO "+ landmarks[loc.id].name);

                        var flag = false;
                        foreach (var c in C)
                        {
                            if (x.taskEqual(c) && x.locEqual(c))
                            {
                                flag = true;
                            }
                        }

                        if (!nbst.locEqual(x) && !flag)
                        {
                            var _e = new Edge(new node(nbst), new node(x));
                            O.Add(x);
                            E.Add(_e);
                        }

                    }
                }
                Debug.Log(O.Count);
                Debug.Log(nbst.locations_id[0]);
            }

            linear_t++;
        }
    }
    [System.Serializable]
    public struct agent
    {
        public string name;
        public int id;
        public bool isBusy;
        public GameObject go;
        //[HideInInspector]
        public landmark location;
        public agent(string _name, int _id, GameObject _go, landmark loc)
        {
            id = _id;
            name = _name;
            isBusy = false;
            go = _go;
            location = loc;
        }
    }
    [System.Serializable]
    public struct node
    {
        public List<int> locations_id;
        public List<int> tasks;
        public float met;
        public float t;
        public string pre_action;
        public node(List<int> _lm, List<int> _tasks, float mt, float _t, string _st = "")
        {
            locations_id = new List<int>(_lm);
            tasks = new List<int>(_tasks);
            met = mt;
            t = _t;
            pre_action = _st;
        }
        public node(node _n)
        {
            locations_id = new List<int>(_n.locations_id);
            tasks = new List<int>(_n.tasks);
            met = _n.met;
            t = _n.t;
            pre_action = _n.pre_action;
        }



        public bool taskEqual(node _n)
        {
            for (int i = 0; i < tasks.Count; i++) if (tasks[i] != _n.tasks[i]) return false;
            return true;
        }
        public bool locEqual(node _n)
        {
            for (int i = 0; i < locations_id.Count; i++) if (locations_id[i] != _n.locations_id[i]) return false;
            return true;
        }

        public bool isGoal(node _end)
        {
            for (int i = 0; i < locations_id.Count; i++)
            {
                if (_end.locations_id[i] != -1 && _end.locations_id[i] != locations_id[i])
                {
                    return false;
                }
            }
            for (int i = 0; i < tasks.Count; i++)
            {
                if (_end.tasks[i] != -1 && _end.tasks[i] != tasks[i])
                {
                    return false;
                }
            }
            
            return true;
        }
    }
    [System.Serializable]
    public struct landmark
    {
        public string name;
        public int id;
        public Vector3 location;
        public bool isWalkBack;
        public GameObject go;
        public landmark(string _name, int _id, GameObject _go)
        {
            id = _id;
            name = _name;
            isWalkBack = false;
            go = _go;
            float posy = Terrain.activeTerrain.SampleHeight(new Vector3(go.transform.position.x, 0, go.transform.position.z));
            var loc = new Vector3(go.transform.position.x, posy, go.transform.position.z);
            location = loc;
        }
        public void update()
        {
            float posy = Terrain.activeTerrain.SampleHeight(new Vector3(go.transform.position.x, 0, go.transform.position.z));
            var loc = new Vector3(go.transform.position.x, posy, go.transform.position.z);
            location = loc;
        }
    }
    public struct walk_costs
    {
        public int l1;
        public int l2;
        public action cost;
        public walk_costs(int _l1, int _l2, action ac)
        {
            l1 = _l1;
            l2 = _l2;
            cost = ac;
        }
    }

    public void WALK(int start_id, int end_id, out action cost)
    {
        cost = new action(0, 0);
        
        //PRM_Astar.PRM_Graph_Manager.
        if (!preplan)
        {
            local_planner.stt = landmarks[start_id].go.transform;
            local_planner.got = landmarks[end_id].go.transform;
            local_planner.re_init = true;
            local_planner.pathfinding = true;
        }
        else
        {
            foreach (var path_cost in path_list)
            {
                if (path_cost.l1 == start_id && path_cost.l2 == end_id)
                {
                    cost = path_cost.cost;
                }
                
            }
        }
    }
    [System.Serializable]
    public struct customTask
    {
        public string name;
        public List<int> precondition_locations;
        public List<int> precondition_tasks;
        public List<int> postcondition_tasks;
        public action costs;

        public bool sat_pre_loc(List<int> _loc)
        {
            for (int i=0;i<precondition_locations.Count;i++) if (precondition_locations[i] != -1 && _loc[i] != precondition_locations[i]) return false;
            return true;
        }
        public bool sat_pre_tasks(List<int> _tasks)
        {
            for (int i = 0; i < precondition_tasks.Count; i++) if (precondition_tasks[i] != -1 && _tasks[i] != precondition_tasks[i]) return false;
            return true;
        }
        public List<int> set_post_tasks(List<int> _tasks)
        {
            for (int i = 0; i < postcondition_tasks.Count; i++) if (postcondition_tasks[i] != -1) _tasks[i] = postcondition_tasks[i];
            return _tasks;
        }
    }

    [System.Serializable]
    public struct action
    {
        public float met_cost;
        public float time_elapsed;
        public bool isSafe;
        public action(float m, float t, bool s = true)
        {
            met_cost = m;
            time_elapsed = t;
            isSafe = s;
        }
    }
    public struct Edge
    {
        public node E1;
        public node E2;
        //public Vector3 E1;
        public Edge(node _E1, node _E2)
        {
            E1 = _E1;
            E2 = _E2;
        }
    }

}




