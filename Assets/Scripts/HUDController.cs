using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using SimpleJSON;
public class HUDController : MonoBehaviour
{
    
    
    public TaskManager taskManager;//holds  "dataController"
    



    public int Task_index = 0;
    public int Procedure_index = 0;
    public int Step_index = 0;
    public int Substep_index = 0;

    public JSONNode current_T;
    public JSONNode current_P;
    public JSONNode current_S;
    public JSONNode current_SS;





    public JSONNode taskboard;
    public JSONNode suit_rep;
    public Transform procedureListParent;
    
    private bool proceduresActive; //are there still procedures todo?
                           
    private int proceduresCompleted;
    private int procedureIndex; //which procedure are we on

    public TextMesh time_canvas;
    public TextMesh Procedure_canvas;
    public TextMesh Step_canvas;
    public TextMesh ChecklistCanvas2;

    private List<GameObject> stepsGameObjects = new List<GameObject>();
   
   
    public Text completedProcedures;
    public Text procedureText;
    public Text stepDisplayText;


    // Start is called before the first frame update
    void Start()
    {
        taskboard = taskManager.taskboard;
        current_T = taskboard["tasks"][Task_index];
        current_P = taskboard["tasks"][Task_index]["children"][Procedure_index];
        current_S = taskboard["tasks"][Task_index]["children"][Procedure_index]["children"][Step_index];
        current_SS = taskboard["tasks"][Task_index]["children"][Procedure_index]["children"][Step_index]["children"];
        //var txb = ChecklistCanvas.GetComponent<Text>();
        //var ttl = ClickerCanvas.GetComponent<Text>();
        
}
    bool togg = true;
    public void toggle()
    {
        togg = !togg;
        //ChecklistCanvas.SetActive(togg);
        //ChecklistCanvas2.SetActive(togg);
    }
    
    public void next_step()
    {
        if (Step_index == current_P["childIds"][current_P["childIds"].Count - 1])
        {
            if (Procedure_index == current_T["childIds"][current_T["childIds"].Count - 1])
            {
                Task_index++;
                Procedure_index = taskboard["tasks"][Task_index]["childIds"][0];
                Step_index = taskboard["tasks"][Task_index]["childIds"][Procedure_index]["childIds"][0];
            }
            else
            {
                Procedure_index++;
                Step_index = taskboard["tasks"][Task_index]["childIds"][Procedure_index]["childIds"][0];
            }
        }
        else
        {
            Step_index++;
        }
    }
    public void previous_step()
    {
        if (Step_index == current_P["childIds"][0])
        {
            if (Procedure_index == current_T["childIds"][0])
            {
                Task_index--;
                Procedure_index = taskboard["tasks"][Task_index]["childIds"].Count - 1;
                Step_index = taskboard["tasks"][Task_index]["childIds"][Procedure_index]["childIds"].Count - 1;
            }
            else
            {
                Procedure_index++;
                Step_index = taskboard["tasks"][Task_index]["childIds"][Procedure_index]["childIds"].Count - 1;
            }
        }
        else
        {
            Step_index++;
        }
    }


    
    void Update()
    {
        taskboard = taskManager.taskboard;
        suit_rep = taskManager.suit_rep;
        current_T = taskboard["tasks"][Task_index];
        current_P = taskboard["tasks"][Task_index]["children"][Procedure_index];
        current_S = taskboard["tasks"][Task_index]["children"][Procedure_index]["children"][Step_index];
        current_SS = taskboard["tasks"][Task_index]["children"][Procedure_index]["children"][Step_index]["children"];
        //var procedure_text = Procedure_canvas.GetComponent<Text>();
        //var step_text = Step_canvas.GetComponent<Text>();
        //var time_text = time_canvas.GetComponent<Text>();
        var time_rem = suit_rep[0];

        //ttl.text = time_rem["t_oxygen"] + " "  + current_T["name"] + "- Current Step:  " + (Procedure_index + 1) + ". " + current_P["name"] + " - " + (Procedure_index + 1) + "." + (Step_index + 1) + ".  " + current_S["name"];
        //for (var ii = 0; ii < current_SS.Count; ii++)//  stp in current_SS)
        //{
        //var stp = current_SS[ii]["action_object"] + " " + current_SS[ii]["condition"];

        //            txb.text = txb.text + " \n " + stp;
        //Debug.Log(current_SS.Count);
        //      }
        //Debug.Log(procedure_text.text);
        Procedure_canvas.text = current_P["name"] + "\n\n\n\n\n";
        Step_canvas.text = current_S["name"] + "\n\n\n\n\n";
        time_canvas.text = time_rem["t_oxygen"];







    }




    void OnGUI()
    {
        //top left point of rectangle
        Vector3 boxPosHiLeftWorld = new Vector3(0.5f, 12, 0);
        //bottom right point of rectangle
        Vector3 boxPosLowRightWorld = new Vector3(1.5f, 0, 0);

        Vector3 boxPosHiLeftCamera = Camera.main.WorldToScreenPoint(boxPosHiLeftWorld);
        Vector3 boxPosLowRightCamera = Camera.main.WorldToScreenPoint(boxPosLowRightWorld);

        float width = boxPosHiLeftCamera.x - boxPosLowRightCamera.x;
        float height = boxPosHiLeftCamera.y - boxPosLowRightCamera.y;


        GUI.Box(new Rect(boxPosHiLeftCamera.x, Screen.height - boxPosHiLeftCamera.y, width, height), "", "highlightBox");
    }

} 
