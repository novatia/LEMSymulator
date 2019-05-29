using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void LEMGroundSimulationClick()
    {
        SceneManager.LoadScene("LEMGroundSimulation");
    }

    public void LEMLandingSimulationClick()
    {
        SceneManager.LoadScene("LEMLandingSimulation");
    }

    public void QuitClick()
    {
        Application.Quit();
    }
}
