using System.Windows;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.IO;
using SimpleJSON;


//main class task manager, initialze link to json url and update the incoming data
public class TaskManager : MonoBehaviour
{       //datacontroller

    public string j_url = "http://nova-eva-support-cutee2020.herokuapp.com/api/simulation/state";
    public string t_url = "https://nova-eva-support-cutee2020.herokuapp.com/api/utils/procedures";

    public telemetry telemet;

    public int TaskUpadateRate = 720;
    public int TelemetryUpadateRate = 720;

    int i = 0;

    public JSONNode taskboard;
    public JSONNode suit_rep;


    // start funtuin
    void Start()
    {

        //backup json
        //jdat = "{ \"tasks\":[{\"_id\":\"1\",\"name\":\"Suit prep\",\"children\":[{\"_id\":\"1\",\"name\":\"Prepare UIA\",\"estimated_time\":\"00:03:00\",\"children\":[{\"_id\":\"1\",\"object\":\"Ensure appropriate start condition\",\"action\":\"OFF/CLOSED\",\"caution\":\"\",\"warning\":\"\",\"confirmation\":\"0,0,0\",\"children\":[{\"_id\":\"1\",\"action_object\":\"Check POWER EV-1 and 2\",\"condition\":\"OFF\",\"caution\":\"\",\"warning\":\"\",\"graphics\":\"\",\"tools\":\"\",\"parent\":\"1\"},{\"_id\":\"2\",\"action_object\":\"Check POWER EV-1 and 2 MEU LED's\",\"condition\":\"\",\"caution\":\"\",\"warning\":\"\",\"graphics\":\"\",\"tools\":\"\",\"parent\":\"1\"},{\"_id\":\"3\",\"action_object\":\"Check WATER SUPPLY EV-1 and 2\",\"condition\":\"CLOSE\",\"caution\":\"\",\"warning\":\"\",\"graphics\":\"\",\"tools\":\"\",\"parent\":\"1\"},{\"_id\":\"4\",\"action_object\":\"Check OXYGEN EV-1 and 2\",\"condition\":\"CLOSE\",\"caution\":\"\",\"warning\":\"\",\"graphics\":\"\",\"tools\":\"\",\"parent\":\"1\"}],\"graphics\":\"dcmO2start.png\",\"tools\":\"\",\"procedure_interrupt\":\"\",\"parent\":\"1\",\"childIds\":[1,2,3,4]},{\"_id\":\"2\",\"object\":\"UIA O2 supply lines\",\"action\":\"Depressurize\",\"caution\":\"\",\"warning\":\"\",\"confirmation\":\"0,0,0\",\"children\":[{\"_id\":\"5\",\"action_object\":\"Switch O2 Vent\",\"condition\":\"OPEN\",\"caution\":\"\",\"warning\":\"\",\"graphics\":\"\",\"tools\":\"\",\"parent\":\"2\"},{\"_id\":\"6\",\"action_object\":\"When UIA supply press < 23psi\",\"condition\":\"Proceed\",\"caution\":\"\",\"warning\":\"\",\"graphics\":\"\",\"tools\":\"\",\"parent\":\"2\"},{\"_id\":\"7\",\"action_object\":\"Switch O2 Vent\",\"condition\":\"CLOSE\",\"caution\":\"\",\"warning\":\"\",\"graphics\":\"\",\"tools\":\"\",\"parent\":\"2\"},{\"_id\":\"8\",\"action_object\":\"Check O2 vent\",\"condition\":\"CLOSE\",\"caution\":\"\",\"warning\":\"\",\"graphics\":\"\",\"tools\":\"\",\"parent\":\"2\"}],\"graphics\":\"dcmO2deprs.png\",\"tools\":\"\",\"procedure_interrupt\":\"\",\"parent\":\"1\",\"childIds\":[5,6,7,8]},{\"_id\":\"3\",\"object\":\"O2 vent\",\"action\":\"CLOSE\",\"caution\":\"\",\"warning\":\"\",\"confirmation\":\"1,0,0\",\"children\":\"\",\"graphics\":\"dcmFinish.png\",\"tools\":\"\",\"procedure_interrupt\":\"\",\"parent\":\"1\"},{\"_id\":\"4\",\"object\":\"EVA 1\",\"action\":\"Pressurize\",\"caution\":\"\",\"warning\":\"Repeat steps ID[this & next] two times for N2 purge\",\"confirmation\":\"0,0,0\",\"children\":[{\"_id\":\"9\",\"action_object\":\"Switch OXYGEN EV-1\",\"condition\":\"OPEN\",\"caution\":\"\",\"warning\":\"\",\"graphics\":\"\",\"tools\":\"\",\"parent\":\"4\"},{\"_id\":\"10\",\"action_object\":\"When UIA supply press > 3000 psi and stable\",\"condition\":\"Proceed\",\"caution\":\"\",\"warning\":\"\",\"graphics\":\"\",\"tools\":\"\",\"parent\":\"4\"},{\"_id\":\"11\",\"action_object\":\"Switch OXYGEN EV-1\",\"condition\":\"CLOSE\",\"caution\":\"\",\"warning\":\"\",\"graphics\":\"\",\"tools\":\"\",\"parent\":\"4\"}],\"graphics\":\"dumpPress.png\",\"tools\":\"\",\"procedure_interrupt\":\"\",\"parent\":\"1\",\"childIds\":[9,10,11]}],\"graphics\":\"dcmO2.png\",\"parent\":\"1\",\"childIds\":[1,2,3,4]},{\"_id\":\"2\",\"name\":\"Fill and dump UIA and SCU O2 lines\",\"estimated_time\":\"00:05:00\",\"children\":[{\"_id\":\"5\",\"object\":\"EVA 1\",\"action\":\"Depressurize\",\"caution\":\"\",\"warning\":\"Repeat steps ID[last & this] two times for N2 purge\",\"confirmation\":\"0,0,0\",\"children\":[{\"_id\":\"12\",\"action_object\":\"Switch O2 Vent\",\"condition\":\"Open\",\"caution\":\"\",\"warning\":\"\",\"graphics\":\"\",\"tools\":\"\",\"parent\":\"5\"},{\"_id\":\"13\",\"action_object\":\"When UIA supply press < 23 psi and stable\",\"condition\":\"Proceed\",\"caution\":\"\",\"warning\":\"\",\"graphics\":\"\",\"tools\":\"\",\"parent\":\"5\"}],\"graphics\":\"dumpDepress.png\",\"tools\":\"\",\"procedure_interrupt\":\"\",\"parent\":\"2\",\"childIds\":[12,13]}],\"graphics\":\"dcmO2.png\",\"parent\":\"1\",\"childIds\":[5]},{\"_id\":\"3\",\"name\":\"Final pressurization\",\"estimated_time\":\"00:15:00\",\"children\":[],\"graphics\":\"dcmO2.png\",\"parent\":\"1\",\"childIds\":[11,12]},{\"_id\":\"4\",\"name\":\"Preparation for donning\",\"estimated_time\":\"01:15:00\",\"children\":[],\"graphics\":\"alkDon.png\",\"parent\":\"1\",\"childIds\":[13,14,15]}],\"graphics\":\"suitPreEVA.png\",\"childIds\":[1,2,3,4]},{\"_id\":\"2\",\"name\":\"Rover repair\",\"children\":[],\"graphics\":\"roverIsoExplode.png\",\"childIds\":[5,6,7,8,9,10,11]},{\"_id\":\"3\",\"name\":\"Science task\",\"children\":[],\"graphics\":\"geoSurvey.png\",\"childIds\":[5,6,12,13,14,15,16,17,18,19]}]}";

        //starts a frame independent json request 
        StartCoroutine(GetRequest1(t_url));
        StartCoroutine(GetRequest2(j_url));
    }
    public GameObject load;


    //update function
    void Update()
    {
        //if tasks are retereived, deactivate loading screen
        if (taskboard != null)
        {
            load.SetActive(false);
        }

        //request updates
        //Debug.Log(taskboard == null);
        if (i % TaskUpadateRate == 0)
        {
            StartCoroutine(GetRequest1(t_url));
        }

        if (i % TelemetryUpadateRate == 0)
        {
            StartCoroutine(GetRequest2(j_url));
        }
        i++;
    }

    //request json function
    public string jdat;
    IEnumerator GetRequest1(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;


            if (webRequest.isNetworkError)
            {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
                jdat = File.ReadAllText(Application.dataPath + "/task_backup.json");

            }
            else
            {
                Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                jdat = webRequest.downloadHandler.text;
                taskboard = JSON.Parse(jdat);
            }
        }
    }

    public string jdat2;
    IEnumerator GetRequest2(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;


            if (webRequest.isNetworkError)
            {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
                jdat = File.ReadAllText(Application.dataPath + "/task_backup.json");

            }
            else
            {
                Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                jdat2 = webRequest.downloadHandler.text;
                suit_rep = JSON.Parse(jdat2);
            }
        }
    }
}

