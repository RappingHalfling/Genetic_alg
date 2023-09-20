using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuButtonsSCR : MonoBehaviour
{
    private GenAlg GenController;
    // Start is called before the first frame update
    void Start()
    {
        if(SceneManager.GetActiveScene().name != "MainMenu")
            GenController = GameObject.FindWithTag("GenController").GetComponent<GenAlg>();
    }

   
    // Update is called once per frame
    void OnMouseDown()
    {

        switch (gameObject.name)
        {
            case "FirstSampleBut":
                SceneManager.LoadScene("FirstSample");
                break;
            case "SecondSampleBut":
                SceneManager.LoadScene("SecondSample");
                break;
            case "ThirdSampleBut":
                SceneManager.LoadScene("ThirdSample");
                break;
            case "SetSubmitBut":
                Time.timeScale = 1f;
                transform.parent.parent.parent.GetComponent<Setting>().Check = true;
                break;
            case "TimeMinusBut":
                Time.timeScale -= 0.5f;
                break;
            case "TimePlusBut":
                Time.timeScale += 0.5f;
                break;
            case "SubmitBut":
                GenController.SubmitClick = true;
                Time.timeScale += 0;
                break;
        }

    }
}
