using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Informed_RRT_star : MonoBehaviour
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

    List<Vector3> V = new List<Vector3>();

    List<node> X_soln = new List<node>();
    List<Edge> E = new List<Edge>();
    Vector3 sample;
    public GameObject tl, br;

    taskPlanner planner;
    public float h_scale = 0f;
    List<Vector3> path;
    node nbst, x_new = new node();
    List<node> T;
    List<node> C;
    Edge e;
    int limit = 5000;
    int ii = 0;


    // Start is called before the first frame update
    void Start()
    {
        //initializing the terrain, getting the start and goal points
        T = new List<node>();
        float posy = Terrain.activeTerrain.SampleHeight(new Vector3(stt.position.x, 0, stt.position.z));
        st = new Vector3(stt.position.x, posy, stt.position.z);
        float posyg = Terrain.activeTerrain.SampleHeight(new Vector3(got.position.x, 0, got.position.z));
        go = new Vector3(got.position.x, posyg, got.position.z);
        T.Add(new node(st, h_scale* Vector3.Distance(st,go), 0, 0, 0));
        E = new List<Edge>();
        V = new List<Vector3>();
        X_soln = new List<node>();
        //finding terrain limits
        terrainWidth = (int)moon.terrainData.size.x;
        terrainLength = (int)moon.terrainData.size.z;
        terrainPosX = (int)moon.transform.position.x;
        terrainPosZ = (int)moon.transform.position.z;

        V.Add(st);
        planner = GetComponent<taskPlanner>();
        //Creating a probability Road Map (PRM)
        x_new = new node();
        ii = 0;
        // adding start and goal to the PRM
        //Waypoint start = new Waypoint(0, st, 0, 0);
        //PRM = new List<Vector3>();
        //PRM.Add(st);
        //PRM.Add(go);
        path = new List<Vector3>();
        // adding n random points to the PRM, basically dividing the map into very small grids

        //PRM.AddRange(sample_n(N, PRM));
        //Astar_init();
    }

    // Update is called once per frame
    void Update()
    {
        //intialize if re_init is clicked in Unity Editor
        if (re_init)
        {
            RRTstar_init();//(PRM, E, st, go);
            re_init = false;
        }

        //start pathfinding if 'pathfinding' checkboc is clicked
        if (pathfinding) RRTstar_find();// (PRM, E, st, go);
    }


    public void RRTstar_init()
    {
        path = new List<Vector3>();
        isComplete = false;

        float posy = Terrain.activeTerrain.SampleHeight(new Vector3(stt.position.x, 0, stt.position.z));
        st = new Vector3(stt.position.x, posy, stt.position.z);
        float posyg = Terrain.activeTerrain.SampleHeight(new Vector3(got.position.x, 0, got.position.z));
        go = new Vector3(got.position.x, posyg, got.position.z);
        T.Add(new node(st, h_scale * Vector3.Distance(st, go), 0, 0, 0));
        E = new List<Edge>();
        V = new List<Vector3>();
        V.Add(st);
        X_soln = new List<node>();
        path = new List<Vector3>();
        ii = 0;
    }

    float c_best, c_min;
    public void RRTstar_find()
    {
        var pathfind = new node();
        if (X_soln.Count != 0)
        {
            var min_F = 9999999f;

            foreach (var x in X_soln)
            {
                if (x.F < min_F)
                {
                    //c_best = x;
                    min_F = x.F;
                    c_best = min_F;
                    pathfind = x;
                }
            }
        }
        else
        {
            c_best = -1f;
        }

        if (c_best != -1f && ii < 1011)
        {
            ii++;
            var zst = new Vector3(st.x, 0, st.z);
            var zgo = new Vector3(go.x, 0, go.z);
            met_cost(Vector3.Distance(zst, zgo), 0, out c_min, out var timbage);// +1f+path_len;
                                                                                //c_min = Vector3.Distance(zst, zgo);
            var x_center = (zst + zgo) / 2;

            var theta = Mathf.Atan2(zgo.z - zst.z, zgo.x - zst.x);

            float posx = Random.Range(-1f, 1f) * Mathf.Sqrt(Mathf.Pow(c_best, 2) + Mathf.Pow(c_min, 2));
            float posz = Random.Range(-1f, 1f) * (c_best / 2) * Vector3.Distance(zst, zgo);


            var l1 = (posx * Mathf.Cos(theta) - posz * Mathf.Sin(theta));
            var l3 = (posx * Mathf.Sin(theta) + posz * Mathf.Cos(theta));
            //var l2 = C_mat.m11 * Mathf.Sqrt(Mathf.Pow(c_best, 2) + Mathf.Pow(c_min, 2));
            //var l3 = C_mat.m22 * Mathf.Sqrt(Mathf.Pow(c_best, 2) + Mathf.Pow(c_min, 2));
            var rad = new List<float>();
            //rad[0] = c_best / 2;
            //for (int i = 1;i<V.Count;i++)
            //{
            //    rad[i] = Mathf.Sqrt(Mathf.Pow(c_best, 2) + Mathf.Pow(c_min, 2));
            //}
            float dist = Vector3.Distance(zst, zgo);
            float vx = l1 + x_center.x;
            float vz = l3 + x_center.z;
            float vy = Terrain.activeTerrain.SampleHeight(new Vector3(vx, 0, vz));
            sample = new Vector3(vx, vy, vz);
            bool flag = false;
            foreach (GameObject obs in obstacles)
            {
                if (obs.GetComponent<Collider>().bounds.Contains(sample))
                {
                    flag = true;
                    return;
                }

            }
            if (flag) return;
            //var pills = new List<Vector3>() { sample, sample, sample };
            //var choice = Random.Range(0, pills.Count);
            //Debug.Log(pills[choice]);
            //sample = pills[choice];

            //var ob3 = Instantiate(cone, sample, Quaternion.identity); // ----------------
             
            //Debug.Log(sample);
            //sample = new Vector3(posx, posy, posz);
            if (ii >= 100)
            {

                Debug.Log("Pathfind successfull");
                var xr = X_soln[X_soln.Count-1].P;//x_new.P;
                                  //ln.SetPosition(0, nd.P);
                                  //ln.SetPosition(1, nbst.P);


                //if it is a goal, retrace the shortest path back to start
                var pp = 0;
                while (xr != st && pp < 1000)
                {
                    //Debug.Log("Path While");
                    path.Add(xr);
                    foreach (var _e in E)
                    {
                        if (_e.E2.P == xr)
                        {
                            //Debug.Log("Path Retracing");
                            xr = _e.E1.P;
                            var ob33 = Instantiate(cone, xr, Quaternion.identity);
                            var ln4 = ob33.AddComponent<LineRenderer>();
                            ln4.useWorldSpace = true;
                            ln4.SetPosition(0, xr + new Vector3(0, 0.2f, 0));
                            ln4.SetPosition(1, _e.E2.P + new Vector3(0, 0.2f, 0));
                            break;
                        }
                    }
                    pp++;
                    if (xr == path[path.Count - 1])
                    {
                        Debug.Log("Failed retrace");
                        xr = path[path.Count - 2];
                        break;
                    }
                }

                //Debug.Log("Pathfind successfull");

                pathfinding = false;
                isComplete = true;
                return;
                //return path;
            }


        }
        else
        {
            float posx = Random.Range(tl.transform.position.x, br.transform.position.x);
            float posz = Random.Range(tl.transform.position.z, br.transform.position.z);
            float poy = Terrain.activeTerrain.SampleHeight(new Vector3(posx, 0, posz));
            sample = new Vector3(posx, poy, posz);
            bool flag = false;
            foreach (GameObject obs in obstacles)
            {
                if (obs.GetComponent<Collider>().bounds.Contains(sample))
                {
                    flag = true;
                    return;
                }

            }
            if (flag) return;
            //var pills = new List<Vector3>() { sample, go };
            var pills = new List<Vector3>() { sample, sample, sample, sample, go };
            var choice = Random.Range(0, pills.Count);
            //Debug.Log(pills[choice]);
            sample = pills[choice];
        }

        //Instantiate(cone, sample, Quaternion.identity);

        var min_dist = Mathf.Infinity;
        var x_nearest = new Vector3();
        foreach (var v in V)
        {
            if (Vector3.Distance(v, sample) < min_dist)
            {
                x_nearest = v;
                min_dist = Vector3.Distance(v, sample);

            }
        }
        if (min_dist < 0.5f) return;
        //Debug.Log(min_dist);

        var opills = new List<float>() { 0.5f, 1f, 2f, 5f }; // { 0.5f, 1f,2f, 5f };
        var ochoice = Random.Range(0, opills.Count);

        var MAX_LEN = opills[ochoice];
        if (min_dist > MAX_LEN) sample = Vector3.Lerp(x_nearest, sample, MAX_LEN / min_dist);
        sample.y = Terrain.activeTerrain.SampleHeight(sample);
        var heading = sample - x_nearest;
        var distance = heading.magnitude;
        var direction = heading / distance; // This is now the normalized direction.

        var slope = Mathf.Rad2Deg * Mathf.Atan2(direction.y, Mathf.Pow(Mathf.Pow(direction.x, 2) + Mathf.Pow(direction.z, 2), 0.5f));
        //Debug.Log(slope);

        if (!collisionfree(slope, sample, x_nearest)) return;

        
        met_cost(distance, slope, out var edge_cost, out var tim);// +1f+path_len;
        float h = h_scale * Vector3.Distance(go, sample);
        var nd = new node(sample, nbst.G + edge_cost + h, nbst.G + edge_cost, nbst.d + distance, nbst.t + tim);
        x_new = nd;


        V.Add(x_new.P);
        T.Add(x_new);
        var X_near = new List<node>();
        foreach (var node in T) if (Vector3.Distance(x_new.P, node.P) < R && node.P != x_new.P) X_near.Add(node);
        //Debug.Log(X_near.Count);



        node x_min = T.Find((t) => t.P == x_nearest);
        c_min = x_min.F + edge_cost + h;

        foreach (var x_near in X_near)
        {
            heading = x_near.P - x_new.P;
            distance = heading.magnitude;
            direction = heading / distance; // This is now the normalized direction.

            slope = Mathf.Rad2Deg * Mathf.Atan2(direction.y, Mathf.Pow(Mathf.Pow(direction.x, 2) + Mathf.Pow(direction.z, 2), 0.5f));
            //Debug.Log(slope);

            met_cost(distance, slope, out edge_cost, out tim);// +1f+path_len;
            h = h_scale * Vector3.Distance(go, sample);

            var c_new = x_near.F + edge_cost + h;
            if (collisionfree(slope, x_near.P, x_new.P))
            {
                x_min = x_near;
                c_min = c_new;
            }


        }
        e = new Edge(x_min, x_new);
        E.Add(e);
        Debug.Log(e.E1.P + "     " + e.E2.P);
        //var ob = Instantiate(cone, e.E1.P, Quaternion.identity);
        //var ln = ob.AddComponent<LineRenderer>();
        //ln.useWorldSpace = true;
        //ln.SetPosition(0, x_min.P);
        //ln.SetPosition(1, x_new.P);
        
        foreach (var x_near in X_near)
        {
            var c_near = x_near.F;
            heading = x_new.P - x_near.P;
            distance = heading.magnitude;
            direction = heading / distance; // This is now the normalized direction.

            slope = Mathf.Rad2Deg * Mathf.Atan2(direction.y, Mathf.Pow(Mathf.Pow(direction.x, 2) + Mathf.Pow(direction.z, 2), 0.5f));
            //Debug.Log(slope);

            met_cost(distance, slope, out edge_cost, out tim);// +1f+path_len;
            h = h_scale * Vector3.Distance(go, sample);

            var c_new = x_new.F + edge_cost + h;

            if (c_new < c_near)
            {
                if (collisionfree(slope, x_new.P, x_near.P))
                {
                    foreach (var _e in E) if (_e.E2.P == x_near.P) e = _e;
                    Debug.Log("DELETEEEEEE ");
                    E.Remove(e);
                    e = new Edge(x_new, x_near);
                    E.Add(e);
                    //var ob2 = Instantiate(cone, e.E1.P, Quaternion.identity);
                    //var ln2 = ob2.AddComponent<LineRenderer>();
                    //ln2.SetPosition(0, e.E1.P);
                    //ln2.SetPosition(1, e.E2.P);

                }
            }

        }



        if (Vector3.Distance(x_new.P, go) < 0.5f)
        {
            Debug.Log("Pathfound");
            //pathfinding = false;
            X_soln.Add(x_new);
        }



        Debug.Log(T.Count + "   ____________________________   " + E.Count);
    }

    bool collisionfree(float _slope, Vector3 _sample, Vector3 _x_nearest)
    {
        if (Mathf.Abs(_slope) > max_slope) return false;
        bool valid_path = true;
        for (float dx = 0f; dx < 1; dx += 0.01f)
        {
            var on_line = Vector3.Lerp(_x_nearest, _sample, dx);
            var y_terr = moon.SampleHeight(on_line);
            if (Mathf.Abs(y_terr - on_line.y) > 0.05f)
            {
                valid_path = false;
                break;
            }
            foreach (GameObject obs in obstacles)
            {
                if (obs.GetComponent<Collider>().bounds.Contains(on_line))
                {
                    valid_path = false;
                    break;
                }
                if (!valid_path) return false;
            }
            if (!valid_path) break;
        }
        if (valid_path)
        {
            return true;
        }
        else return false;

    }

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
        met = W * tim * 0.001f + path_len * 0.1f;  //  kJ
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
    //support functions
    public struct Edge
    {
        public node E1;
        public node E2;
        public Edge(node _E1, node _E2)
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



}
/*
 *
 * planetary sampling task
 * lunar regolith simulation
 * rock, geology pattern recognition
 * sets of instructions associated with sampling task 
 * (light/dark)
 * 
 * 
 */
