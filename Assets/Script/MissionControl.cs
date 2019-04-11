using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissionControl : MonoBehaviour
{

    private float MissionTimeElapsed;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        MissionTimeElapsed += Time.deltaTime;
    }

    private string GetTE()
    {


        return Mathf.Floor(MissionTimeElapsed / 60) + ":" + Mathf.Floor(MissionTimeElapsed % 60);
    }

    public void OnGUI()
    {
        GUI.Label(new Rect(10, 300, 250, 20), "Time elpsed: " + GetTE());

    }

}
