using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TaskBoard
{
    public Task[] tasks;
}

[Serializable]
public class Task
{
    
    public string _id;
    public string name;
    //public string tools;
    //public string parent;
    //public string childIds;
    public Procedure[] children;
    public string graphics;
    public int[] childIds;

    /*
    public Task(string num, string _name, int[] p_list)
    {
        _id = num;
        name = _name;
        //procedures = gameObject.GetComponent<TaskManager>().Procedure_list.GetRange(p_list[0], p_list.Last() - 1);
        procedures = p_list;
    }

    */
}


[System.Serializable]
public class Procedure
{
    //individual steps for procedures
    //potential sub task object
    //cautionwarning

    /*
    {"tasks":
    [{"_id":"1","name":"Suit prep", ,"graphics":"dcmO2start.png","tools":"","procedure_interrupt":"","parent":"1","childIds":[1,2,3,4]
}
        "children":[
        
        
        {"_id":"1",
        "name":"Prepare UIA",
        "estimated_time":"00:03:00",
                "children":[
                                    {"_id":"2","action_object":"Check POWER EV-1 and 2 MEU LED's","condition":"OFF","caution":"","warning":"","graphics":"","tools":"","parent":"1"},
                                    {"_id":"3","action_object":"Check WATER SUPPLY EV-1 and 2","condition":"CLOSE","caution":"","warning":"","graphics":"","tools":"","parent":"1"},
                                    {"_id":"4","action_object":"Check OXYGEN EV-1 and 2","condition":"CLOSE","caution":"","warning":"","graphics":"","tools":"","parent":"1"}],
                {"_id":"2","object":"UIA O2 supply lines","action":"Depressurize","caution":"","warning":"","confirmation":"0,0,0", ,"graphics":"dcmO2deprs.png","tools":"","procedure_interrupt":"","parent":"1","childIds":[5,6,7,8]}
                        "children":[{"_id":"5","action_object":"Switch O2 Vent","condition":"OPEN","caution":"","warning":"","graphics":"","tools":"","parent":"2"},
                                    {"_id":"6","action_object":"When UIA supply press < 23psi","condition":"Proceed","caution":"","warning":"","graphics":"","tools":"","parent":"2"},
                                    {"_id":"7","action_object":"Switch O2 Vent","condition":"CLOSE","caution":"","warning":"","graphics":"","tools":"","parent":"2"},
                                    {"_id":"8","action_object":"Check O2 vent","condition":"CLOSE","caution":"","warning":"","graphics":"","tools":"","parent":"2"}],
     {"_id":"2","name":"Fill and dump UIA and SCU O2 lines","estimated_time":"00:05:00","children":[{"_id":"5","object":"EVA 1","action":"Depressurize","caution"
*/

    public string _id;
    public string name;
    public string estimated_time;
    public Step[] children;
    public string graphics;
    public string parent;
    public int[] childIds;
    /*
    public Procedure(int num, string _name, int[] steps, float _est_time)
    {
        _id = num;
        name = _name;
        //Steps = gameObject.GetComponent<TaskManager>().Step_list;
        est_time = _est_time;
        Steps = steps;
    }
    */
}


[System.Serializable]
public class Step
{

    /*
                  {"_id":"1",
                     "object":"Ensure appropriate start condition",
                     "action":"OFF/CLOSED","caution":"",
                     "warning":"",
                     "confirmation":"0,0,0",
                        "children":[{
     /*                                "_id":"1",
                                     "action_object":"Check POWER EV-1 and 2",
                                     "condition":"OFF","caution":"",
                                     "warning":"","graphics":"",
                                     "tools":"",
                                     "parent":"1"},
      
     */
    public string _id;
    public string name;
    public string action;
    public string caution;
    public string warning;
    public string conformation;
    public Substep[] children;
    public string graphics;
    public string tools;
    public string procedure_interrupt;
    public string parent;
    public int[] childIds;
    /*
    public Step(int num, string _name, int[] _substeps)
    {
        _ID = num;
        name = _name;
        substeps = _substeps;
    }
    */

}

[Serializable]
public class Substep
{
    public string _id;
    public string action_object;
    public string condition;
    public string caution;
    public string warning;
    public string graphics;
    public string tools;
    public string parent;


    /*                                "_id":"1",
                                    "action_object":"Check POWER EV-1 and 2",
                                    "condition":"OFF",
                                    "caution":"",
                                    "warning":"",
                                    "graphics":"",
                                    "tools":"",
                                    "parent":"1"},

    *

    public Substep(int num, string _name)
    {
        _id = num;
        name = _name;
    }
    */
}
