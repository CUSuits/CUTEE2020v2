using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EVA1Tracker : MonoBehaviour
{
    public Transform EVA1;
    public A_Star_Pathfinder A_Star;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var st = A_Star.st;
        this.gameObject.transform.position = EVA1.position + st;
        this.gameObject.transform.rotation = EVA1.rotation;// new Vector3(0f, EVA1.rotation.y, 0f);

    }
}
