using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace waypoints
{
    public class Waypoint
    {
        public int id;
        public Vector3 xyz;
        public float toa, tod;
        public Waypoint(int _id, Vector3 _xyz, float _toa, float _tod)
        {
            id = _id;
            xyz = _xyz;
            toa = _toa;
            tod = _tod;
        }
    }
}