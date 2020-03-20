using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Set_Camera : MonoBehaviour
{
    public Transform cam;
    public Transform Target;
    public Terrain moon;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var y = moon.SampleHeight(new Vector3(Target.position.x, 0, Target.position.z));
        cam.position = new Vector3(Target.position.x, y + 3f, Target.position.z);
    }
}
