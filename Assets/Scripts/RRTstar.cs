using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using waypoints;


public class RRTstar : MonoBehaviour
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

    List<Vector3> path;
    public node nbst, x_new = new node();
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
        T.Add(new node(st, 0, 0, 0, 0));
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
        T.Add(new node(st, 0, 0, 0, 0));
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
        if (pathfinding)
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

                float posx = Random.Range(-1f, 1f) * (c_best / 2) * Vector3.Distance(zst, zgo);
                float posz = Random.Range(-1f, 1f) * Mathf.Sqrt(Mathf.Pow(c_best, 2) + Mathf.Pow(c_min, 2));


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
                var pills = new List<Vector3>() { sample, sample, sample };
                var choice = Random.Range(0, pills.Count);
                Debug.Log(pills[choice]);
                sample = pills[choice];

                //var ob3 = Instantiate(cone, sample, Quaternion.identity); // ----------------

                //Debug.Log(sample);
                //sample = new Vector3(posx, posy, posz);
                if (ii >= 500)
                {

                    Debug.Log("Pathfind successfull");
                    var xr = pathfind;//x_new.P;
                    //ln.SetPosition(0, nd.P);
                    //ln.SetPosition(1, nbst.P);


                    //if it is a goal, retrace the shortest path back to start
                    var pp = 0;
                    while (xr.P != st && pp < 100)
                    {
                        Debug.Log("Path While");
                        path.Add(xr.P);
                        foreach (var e in E)
                        {
                            if (e.E2.P == xr.P)
                            {
                                Debug.Log("Path EEing");
                                xr = e.E1;
                                var ob33 = Instantiate(cone, xr.P, Quaternion.identity);
                                var ln4 = ob33.AddComponent<LineRenderer>();
                                ln4.SetPosition(0, xr.P + new Vector3(0, 0.2f, 0));
                                ln4.SetPosition(1, e.E2.P + new Vector3(0, 0.2f, 0));



                            }
                        }
                        pp++;
                        if (xr.P == path[path.Count - 1])
                        {
                            Debug.Log("Failed retrace");
                            //xr.P = path[path.Count - 2];
                            break;
                        }
                    }
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
            var opills = new List<float>() { 0.25f, 0.5f, 0.75f, 10f, 1f, 5f, 50f};
            var ochoice = Random.Range(0, opills.Count);

            var MAX_LEN = 10f;
            if (min_dist > MAX_LEN) sample = Vector3.Lerp(x_nearest, sample, MAX_LEN / min_dist);
            float posy = Terrain.activeTerrain.SampleHeight(sample);
            var vec = new Vector3(sample.x, posy, sample.z);
            sample = vec;
            var diff = sample - x_nearest;
            var slope = Mathf.Rad2Deg * Mathf.Atan2(diff.y, Mathf.Pow(Mathf.Pow(diff.x, 2) + Mathf.Pow(diff.z, 2), 0.5f));
            var x_new = new node();
            if (Mathf.Abs(slope) < max_slope || true)
            {
                float p_len = Vector3.Distance(x_nearest, sample);

                bool valid_path = true;
                for (float dx = 0.1f; dx < 1; dx += 0.1f)
                {
                    var on_line = Vector3.Lerp(x_nearest, sample, dx);
                    var y_terr = moon.SampleHeight(on_line);
                    if (Mathf.Abs(y_terr - on_line.y) > 1f)
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

                    }
                    if (valid_path)
                    {
                        x_new.P = on_line;
                        var y_at_new = moon.GetComponent<Terrain>().SampleHeight(x_new.P);
                        var p = new Vector3(x_new.P.x, y_at_new, x_new.P.z);
                        x_new.P = p;
                    }
                    //else return;
                }
            }



            float path_len = Vector3.Distance(x_nearest, x_new.P);

            met_cost(path_len, slope, out var edge_cost, out var tim);// +1f+path_len;

            float h = Vector3.Distance(go, x_new.P);
            var nd = new node(x_new.P, nbst.G + edge_cost + h, nbst.G + edge_cost, nbst.d + path_len, nbst.t + tim);
            x_new = nd;

            if (Vector3.Distance(x_new.P, x_nearest) > 50f || Vector3.Distance(x_new.P, x_nearest) < 5f) return;
            //Physics.Raycast(x_nearest, x_new.P, out var hitobs, Mathf.Infinity);
            //if (hitobs.Collider.gameObject.tramsform == moon.transform) return;

            bool valid_path_p = true;
            for (float dx = 0f; dx < 1; dx += 0.01f)
            {
                var on_line = Vector3.Lerp(x_nearest, sample, dx);
                var y_terr = moon.SampleHeight(on_line);
                if (Mathf.Abs(y_terr - on_line.y) > 0.05f)
                {
                    valid_path_p = false;
                    break;
                }
                foreach (GameObject obs in obstacles)
                {
                    if (obs.GetComponent<Collider>().bounds.Contains(on_line))
                    {
                        valid_path_p = false;
                        break;
                    }

                }
                if (!valid_path_p) break;
            }
            if (valid_path_p)
            {

                V.Add(x_new.P);
                T.Add(x_new);
            }
            else return;
            //else return;


            var X_near = new List<node>();
            foreach (var node in T) if (Vector3.Distance(x_new.P, node.P) < R) X_near.Add(node);

            node x_min = T.Find((t) => t.P == x_nearest);

            foreach (var xx in X_near)
            {

                var diff2 = xx.P - x_new.P;
                var slope2 = Mathf.Rad2Deg * Mathf.Atan2(diff2.y, Mathf.Pow(Mathf.Pow(diff2.x, 2) + Mathf.Pow(diff2.z, 2), 0.5f));
                met_cost(path_len, slope, out var edge_cost2, out var tim2);// +1f+path_len;
                float h2 = 0 * Vector3.Distance(go, x_min.P);
                var c_min = x_min.F + edge_cost + h;
                var c_new = xx.F + edge_cost2 + h2;
                if (Mathf.Abs(slope2) < max_slope && c_new < c_min)
                {
                    float p_len = Vector3.Distance(xx.P, x_new.P);

                    bool valid_path = true;
                    var step = 0.01f;
                    for (float dx = 0f; dx < 1; dx += step / p_len)
                    {
                        var on_line = Vector3.Lerp(x_nearest, sample, dx);
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

                        }
                        if (!valid_path) break;
                    }
                    if (valid_path)
                    {
                        x_min = xx;
                        c_min = x_new.F;
                    }
                    //       else return;
                }

            }

            E.Add(new Edge(x_min, x_new));
            var ob = Instantiate(cone, x_new.P, Quaternion.identity);
            var ln = ob.AddComponent<LineRenderer>();
            ln.SetPosition(0, x_new.P);
            ln.SetPosition(1, x_min.P);


            foreach (var xx in X_near)
            {
                //Debug.Log("testing optimality");
                var c_near = xx.F;

                var diff2 = x_new.P - xx.P;
                var slope2 = Mathf.Rad2Deg * Mathf.Atan2(diff2.y, Mathf.Pow(Mathf.Pow(diff2.x, 2) + Mathf.Pow(diff2.z, 2), 0.5f));
                met_cost(path_len, slope, out var edge_cost2, out var tim2);// +1f+path_len;

                float h2 = 0 * Vector3.Distance(go, x_new.P);
                var c_new = x_new.F + edge_cost2 + h2;

                //Debug.Log("testing optimality  "+c_near + "  -------------------------  " + c_new);
                if (Mathf.Abs(slope2) < max_slope && c_new < c_near)
                {
                    float p_len = Vector3.Distance(xx.P, x_new.P);

                    bool valid_path = true;
                    var step = 0.01f;
                    for (float dx = 0f; dx < 1; dx += step / p_len)
                    {
                        var on_line = Vector3.Lerp(xx.P, x_new.P, dx);
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

                        }
                        if (!valid_path) break;
                    }
                    if (valid_path && x_min.P != x_new.P)
                    {
                        var e = new Edge();
                        foreach (var _e in E) if (_e.E2.P == xx.P) e = _e;
                        Debug.Log("DELETEEEEEE ");
                        E.Remove(e);
                        E.Add(new Edge(x_new, xx));
                        var ob2 = Instantiate(cone, x_new.P, Quaternion.identity);
                        var ln2 = ob2.AddComponent<LineRenderer>();
                        ln2.SetPosition(0, x_new.P);
                        ln2.SetPosition(1, xx.P);
                    }


                }
            }
            if (Vector3.Distance(x_new.P, go) < 0.5f)
            {
                Debug.Log("Pathfound");
                //pathfinding = false;
                X_soln.Add(x_new);
            }
        }
        Debug.Log(V.Count + " ____________ " + E.Count + " ____________________ " + X_soln.Count);
    }



    //public List<Vector3> Astar_find()//(List<Vector3> _PRM, List<Edge> _E, Vector3 _start, Vector3 _goal)
    //{


    //    if (O.Count > 0 && ii < limit)
    //    {
    //        ++ii;
    //        var min_F = 9999999f;

    //        //select point with minimum cost (nbst) in th O list
    //        foreach (var o in O)
    //        {
    //            if (o.F < min_F)
    //            {
    //                nbst = o;
    //                min_F = nbst.F;
    //            }
    //        }

    //        //remove it from O and add to C
    //        O.Remove(nbst);
    //        C.Add(nbst);
    //        Vector3 x;


    //        //find all neighbors of O


    //        //check if any neighbor is goal
    //        if (nbst.P == go)
    //        {
    //            Debug.Log("Pathfind successfull");
    //            x = nbst.P;

    //            //ln.SetPosition(0, nd.P);
    //            //ln.SetPosition(1, nbst.P);


    //            //if it is a goal, retrace the shortest path back to start
    //            var pp = 0;
    //            while (x != st && pp < 1000)
    //            {
    //                path.Add(x);
    //                foreach (var e in E)
    //                {
    //                    if (e.E2 == x)
    //                    {
    //                        x = e.E1;
    //                        var ob = Instantiate(cone, x, Quaternion.identity);
    //                        var ln = ob.AddComponent<LineRenderer>();
    //                        ln.SetPosition(0, x + new Vector3(0, 0.2f, 0));
    //                        ln.SetPosition(1, e.E2 + new Vector3(0, 0.2f, 0));
    //                        pp++;
    //                    }
    //                }
    //                if (x == path[path.Count - 1])
    //                {
    //                    Debug.Log("Failed retrace");
    //                    x = path[path.Count - 2];
    //                    continue;
    //                }
    //            }
    //            pathfinding = false;
    //            isComplete = true;

    //            return path;
    //        }
    //        //for each neighboring point: add to O if not already in C
    //        foreach (var v in PRM)
    //        {
    //            if (Vector3.Distance(nbst.P, v) < R && nbst.P != v)
    //            {
    //                var diff = v - nbst.P;
    //                var slope = Mathf.Rad2Deg * Mathf.Atan2(diff.y, Mathf.Pow(Mathf.Pow(diff.x, 2) + Mathf.Pow(diff.z, 2), 0.5f));
    //                //Debug.Log("SLOOOOOOOOOOOOOOOOOOOOOOPPPPPPPPPPPPEEEEEEEEEEEEEE   : " + slope);

    //                //check if it is a valid path (slope limits)
    //                //compute the metabolic cost of the path


    //                if (Mathf.Abs(slope) < max_slope)
    //                {
    //                    x = v;

    //                    float path_len = Vector3.Distance(nbst.P, x);

    //                    met_cost(path_len, slope, out var edge_cost, out var tim);// +1f+path_len;

    //                    float h = 2f * Vector3.Distance(x, go);
    //                    var nd = new node(x, nbst.G + edge_cost + h, nbst.G + edge_cost, nbst.d + path_len, nbst.t + tim);
    //                    if (C.Find(node => node.P == x).P != x && O.Find(node => node.P == x).P != x)
    //                    {
    //                        bool valid_path = true;
    //                        for (float dx = 0.1f; dx < 1; dx += 0.1f)
    //                        {
    //                            var on_line = Vector3.Lerp(nbst.P, nd.P, dx);
    //                            var y_terr = moon.SampleHeight(on_line);
    //                            if (Mathf.Abs(y_terr - on_line.y) > 0.05)
    //                            {
    //                                valid_path = false;
    //                                break;
    //                            }
    //                        }



    //                        if (valid_path)
    //                        {


    //                            O.Add(nd);
    //                            e = new Edge(nbst.P, nd.P);
    //                            E.Add(e);
    //                            //var ob = Instantiate(cone, nd.P, Quaternion.identity);
    //                            //var ln = ob.AddComponent<LineRenderer>();
    //                            //ln.SetPosition(0, nd.P);
    //                            //ln.SetPosition(1, nbst.P);
    //                        }
    //                    }

    //                    else if (nd.G < nbst.G)
    //                    {
    //                        nbst = nd;
    //                    }
    //                }
    //            }
    //        }

    //        Debug.Log("Length of lit O: " + O.Count + "nbst" + nbst.P);
    //    }
    //    else pathfinding = false;

    //    return path;
    //}


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