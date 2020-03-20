using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using waypoints;


public class PRM_Graph_Manager : MonoBehaviour
{
    public Transform stt;
    public Transform got;
    Vector3 st;
    Vector3 go;

    public bool pathfinding = false;
    public bool re_init = false;

    public float mass = 150f;
    public float g = 9.81f / 6f;


    public Terrain moon;
    public GameObject cone;
    public GameObject line;

    public int N = 5000;
    public float R = 1.0f, max_slope = 10.0f;

    public bool isComplete;

    public List<GameObject> obstacles;

    private float terrainWidth; // terrain size (x)
    private float terrainLength; // terrain size (z)
    private float terrainPosX; // terrain position x
    private float terrainPosZ; // terrain position z

    List<Vector3> PRM = new List<Vector3>();
    List<Edge> E = new List<Edge>();
    Vector3 sample;


    taskPlanner planner;

    List<Vector3> path;
    public node nbst;
    List<node> O;
    List<node> C;
    Edge e;
    int limit = 5000;
    int ii = 0;


    // Start is called before the first frame update
    void Start()
    {
        //initializing the terrain, getting the start and goal points
        float posy = Terrain.activeTerrain.SampleHeight(new Vector3(stt.position.x, 0, stt.position.z));
        st = new Vector3(stt.position.x, posy, stt.position.z);
        float posyd = Terrain.activeTerrain.SampleHeight(new Vector3(got.position.x, 0, got.position.z));
        go = new Vector3(got.position.x, posyd, got.position.z);
        //finding terrain limits
        terrainWidth = (int)moon.terrainData.size.x;
        terrainLength = (int)moon.terrainData.size.z;
        terrainPosX = (int)moon.transform.position.x;
        terrainPosZ = (int)moon.transform.position.z;


        planner = GetComponent<taskPlanner>();
        //Creating a probability Road Map (PRM)


        // adding start and goal to the PRM
        Waypoint start = new Waypoint(0, st, 0, 0);
        //PRM = new List<Vector3>();
        PRM.Add(st);
        PRM.Add(go);

        // adding n random points to the PRM, basically dividing the map into very small grids

        PRM.AddRange(sample_n(N, PRM));
        Astar_init();
    }

    // Update is called once per frame
    void Update()
    {
        //intialize if re_init is clicked in Unity Editor
        if (re_init)
        {
            Astar_init();//(PRM, E, st, go);
            re_init = false;
        }

        //start pathfinding if 'pathfinding' checkboc is clicked
        if (pathfinding) Astar_find();// (PRM, E, st, go);
    }



    //this function re-initialized the pathfinding algorithm to start finding a new path
    public void Astar_init()//(List<Vector3> _PRM, List<Edge> _E, Vector3 _start, Vector3 _goal)
    {
        path = new List<Vector3>();
        O = new List<node>();
        C = new List<node>();
        E = new List<Edge>();
        limit = 100000;
        ii = 0;
        isComplete = false;

        float posy = Terrain.activeTerrain.SampleHeight(new Vector3(stt.position.x, 0, stt.position.z));
        st = new Vector3(stt.position.x, posy, stt.position.z);
        float posyg = Terrain.activeTerrain.SampleHeight(new Vector3(got.position.x, 0, got.position.z));
        go = new Vector3(got.position.x, posyg, got.position.z);
        PRM.Add(st);
        PRM.Add(go);
        O.Add(new node(st, 0, 0, 0, 0));
        var nbst = O[0];

    }


    //this is the main pathfinding function called every frame when in pathfinding mode

    public List<Vector3> Astar_find()//(List<Vector3> _PRM, List<Edge> _E, Vector3 _start, Vector3 _goal)
    {


        if (O.Count > 0 && ii < limit)
        {
            ++ii;
            var min_F = 9999999f;

            //select point with minimum cost (nbst) in th O list
            foreach (var o in O)
            {
                if (o.F < min_F)
                {
                    nbst = o;
                    min_F = nbst.F;
                }
            }

            //remove it from O and add to C
            O.Remove(nbst);
            C.Add(nbst);
            Vector3 x;


            //find all neighbors of O


            //check if any neighbor is goal
            if (nbst.P == go)
            {
                Debug.Log("Pathfind successfull");
                x = nbst.P;

                //ln.SetPosition(0, nd.P);
                //ln.SetPosition(1, nbst.P);


                //if it is a goal, retrace the shortest path back to start
                var pp = 0;
                while (x != st && pp < 1000)
                {
                    path.Add(x);
                    foreach (var e in E)
                    {
                        if (e.E2 == x)
                        {
                            x = e.E1;
                            var ob = Instantiate(cone, x, Quaternion.identity);
                            var ln = ob.AddComponent<LineRenderer>();
                            ln.SetPosition(0, x + new Vector3(0, 0.2f, 0));
                            ln.SetPosition(1, e.E2 + new Vector3(0, 0.2f, 0));
                            pp++;
                        }
                    }
                    if (x == path[path.Count - 1])
                    {
                        Debug.Log("Failed retrace");
                        x = path[path.Count - 2];
                        continue;
                    }
                }
                pathfinding = false;
                isComplete = true;
                
                return path;
            }
            //for each neighboring point: add to O if not already in C
            foreach (var v in PRM)
            {
                if (Vector3.Distance(nbst.P, v) < R && nbst.P != v)
                {
                    var diff = v - nbst.P;
                    var slope = Mathf.Rad2Deg * Mathf.Atan2(diff.y,Mathf.Pow(Mathf.Pow(diff.x,2)+ Mathf.Pow(diff.z,2),0.5f));
                    //Debug.Log("SLOOOOOOOOOOOOOOOOOOOOOOPPPPPPPPPPPPEEEEEEEEEEEEEE   : " + slope);
                    
                    //check if it is a valid path (slope limits)
                    //compute the metabolic cost of the path


                    if (Mathf.Abs(slope) < max_slope)
                    {
                        x = v;

                        float path_len = Vector3.Distance(nbst.P, x);

                        met_cost(path_len, slope, out var edge_cost, out var tim);// +1f+path_len;

                        float h = 2f*Vector3.Distance(x, go);
                        var nd = new node(x, nbst.G + edge_cost + h, nbst.G + edge_cost, nbst.d + path_len, nbst.t + tim);
                        if (C.Find(node => node.P == x).P != x && O.Find(node => node.P == x).P != x)
                        {
                            bool valid_path = true;
                            for (float dx = 0.1f; dx < 1; dx += 0.1f)
                            {
                                var on_line = Vector3.Lerp(nbst.P, nd.P, dx);
                                var y_terr = moon.SampleHeight(on_line);
                                if (Mathf.Abs(y_terr - on_line.y) > 0.05)
                                {
                                    valid_path = false;
                                    break;
                                }
                            }



                            if (valid_path)
                            {


                                O.Add(nd);
                                e = new Edge(nbst.P, nd.P);
                                E.Add(e);
                                //var ob = Instantiate(cone, nd.P, Quaternion.identity);
                                //var ln = ob.AddComponent<LineRenderer>();
                                //ln.SetPosition(0, nd.P);
                                //ln.SetPosition(1, nbst.P);
                            }
                        }

                        
                    }
                }
            }

            Debug.Log("Length of lit O: " + O.Count + "nbst" + nbst.P);
        }
        else pathfinding = false;

        return path;
    }


    //metabolic cost calculator for given path length and slope
    void met_cost(float path_len, float slope, out float met, out float tim)
    {
        var vel = 0f;
        if (slope >= -20 && slope < -10) vel = 0.095f * slope + 1.95f;
        else if (slope >= -10 && slope < 0) vel = 0.06f * slope + 1.6f;
        else if (slope >= 0 && slope < 6) vel = -0.02f * slope + 1.6f;
        else if (slope >= 6 && slope < 15) vel = -0.039f * slope + 0.0634f;
        else vel = 0.05f;
        vel = Mathf.Abs(vel);
        var W_level = (3.28f * mass + 71.1f) * (0.661f * Mathf.Cos(slope * Mathf.Deg2Rad) * vel + 0.115f);
        var W_slope = 0f;
        if (slope > 0) W_slope = 3.5f * mass * g * vel * Mathf.Sin(slope * Mathf.Deg2Rad);
        else if (slope < 0) W_slope = 2.4f * mass * g * vel * Mathf.Sin(slope * Mathf.Deg2Rad) * Mathf.Pow(0.3f, Mathf.Abs(slope) * 7.65f);
        var W = W_level + W_slope; // J/s
        tim = path_len / vel;
        met = W * tim * 0.001f + path_len* 0.1f;  //  kJ
    }

    //sampling n random/grid points on the terrain
    List<Vector3> sample_n(float n, List<Vector3> _PRM)
    {
        n = 500.0f;
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                //float posx = Random.Range(terrainPosX, terrainPosX + terrainWidth);
                //float posz = Random.Range(terrainPosZ, terrainPosZ + terrainLength);
                float posx = i * (terrainPosX + terrainWidth) / n;
                float posz = j * (terrainPosZ + terrainLength) / n;
                float posy = Terrain.activeTerrain.SampleHeight(new Vector3(posx, 0, posz));
                sample = new Vector3(posx, posy, posz);
                bool flag = false;
                foreach (GameObject ob in obstacles)
                {
                    if (ob.GetComponent<Collider>().bounds.Contains(sample))
                    {
                        flag = true;
                    }

                }
                if (!flag) _PRM.Add(sample);
            }
        }
        return _PRM;
    }


    //display path waypoints
    void disp_cone(List<Vector3> _PRM, GameObject _cone)
    {
        for (int i = 0; i < _PRM.Count; i++)
        {
            Instantiate(cone, _PRM[i], Quaternion.identity);
        }
    }

    void disp_con(List<Edge> _E, GameObject _cone)
    {
        for (int i = 0; i < _E.Count; i++)
        {
            Debug.DrawLine(_E[i].E1, _E[i].E2, color: Color.red);
            var ob = Instantiate(cone, _E[i].E1, Quaternion.identity);
            var ln = ob.AddComponent<LineRenderer>();
            ln.SetPosition(0, _E[i].E1);
            ln.SetPosition(1, _E[i].E2);
        }
    }
    //support functions
    public struct Edge
    {
        public Vector3 E1;
        public Vector3 E2;
        public Edge(Vector3 _E1, Vector3 _E2)
        {
            E1 = _E1;
            E2 = _E2;
        }
    }

    public struct node
    {
        public Vector3 P;
        public float F;
        public float G;
        public float d;
        public float t;
        public node(Vector3 _P, float _F, float _d, float _G, float _t)
        {
            P = _P;
            F = _F;
            G = _G;
            d = _d;
            t = _t;
        }
    }

    //connect all points on the map
    public List<Edge> conn_PRM(List<Vector3> _PRM)
    {
        var _E = new List<Edge>();
        for (var i = 0; i < _PRM.Count; i++)
        {
            for (var j = i; j < _PRM.Count; j++)
            {
                var v = _PRM[i];
                var v2 = _PRM[j];
                if (Vector3.Distance(v, v2) < R && v != v2)
                {
                    var diff = v2 - v;
                    var slope = Mathf.Atan2(diff.y, diff.x);
                    if (Mathf.Abs(slope) < max_slope)
                    {
                        _E.Add(new Edge(v, v2));
                        //Debug.Log(_E.Count);
                    }
                }
            }

        }
        Debug.Log(_E.Count);
        return _E;
    }
}