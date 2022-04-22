using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class EventLog : MonoBehaviour
{
    private static EventLog singleton;
    private static List<string> log;

    private Text display_log;
    private Scrollbar scroll;

    private int log_index;
    private int window;
    // Start is called before the first frame update
    void Start()
    {
        if(singleton != null){
            Destroy(gameObject);
        }
        log = new List<string>();
        singleton = this;
        window = 10;
        log_index = 0;
        scroll = GetComponentInChildren<Scrollbar>();
        display_log = GetComponentInChildren<Text>();
        UpdateScrollbar();
        UpdateLogDisplay();
    }

    public void ChangeWindow(){
        int new_index = (int)(scroll.value * (log.Count - 1));
        if(new_index != log_index){
            log_index = new_index;
            UpdateLogDisplay();
        }
    }

    void UpdateScrollbar(){
        int steps = (log.Count>0)?log.Count - 1:1;
        float size = ((float)window)/steps;
        scroll.size = size;
        scroll.numberOfSteps = steps;
    }

    void UpdateLogDisplay(){
        StringBuilder sb = new StringBuilder();
        // start at latest - window index, unless if it is out of bounds, else start earlier
        for(int i = (log_index + window<log.Count)?window:(log.Count - log_index); i > 0; i--){
            sb.AppendFormat("\n{0}", log[log.Count-(i + log_index)]);
        }
        display_log.text = sb.ToString();
    }

    public static void Log(string newEntry){
        log.Add(newEntry);
        singleton.UpdateScrollbar();
        singleton.UpdateLogDisplay();
    }
}
