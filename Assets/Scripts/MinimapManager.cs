using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapManager : MonoBehaviour
{

    public Transform Eagle;
    public Transform Eagle_f;
    private float vel = 0.0f;

    public void MapUp()
    {
        Eagle.position = Eagle.position + new Vector3(0f, 0f, 10f);
    }

    public void MapDown()
    {
        Eagle.position = Eagle.position + new Vector3(0f, 0f, -10f);
    }

    public void MapLeft()
    {
        Eagle.position = Eagle.position + new Vector3(-10f, 0f, 0f);
    }

    public void MapRight()
    {
        Eagle.position = Eagle.position + new Vector3(10f, 0f, 0f);
    }
    public void MapIn()
    {
        Eagle.position = Eagle.position + new Vector3(0f, -10f, 0f);
    }
    public void MapOut()
    {
        Eagle.position = Eagle.position + new Vector3(10f, 10f, 0f);
    }


    public Transform EVA1;
    public float disp = 10f;// new Vector3(10f,75f,0f);
    public float zoom = 0f;
    public A_Star_Pathfinder A_Star;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var ach = A_Star.anchor;
        
        var eag_rot = Quaternion.Euler(90f, EVA1.rotation.eulerAngles.y, 0f);
        //eag_rot.eulerAngles.y = EVA1.rotation.eulerAngles.y;
        Eagle.gameObject.transform.rotation = eag_rot;// new Vector3(0f, EVA1.rotation.y, 0f);
        Eagle_f.gameObject.transform.rotation = eag_rot;// new Vector3(0f, EVA1.rotation.y, 0f);

        var eag_pos = EVA1.position + ach;// + disp;
        eag_pos.y = zoom * 10f + 128f;
        var eag_pos2 = EVA1.position + ach;// + disp;
        eag_pos2.y = -5 * 10f + 128f;

        Eagle.gameObject.transform.position = eag_pos;
        Eagle.transform.Translate(Vector3.up * disp);

        Eagle_f.gameObject.transform.position = eag_pos2;
        Eagle_f.transform.Translate(Vector3.up * disp);



    }
}
