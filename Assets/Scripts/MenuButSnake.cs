using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuButSnake : MonoBehaviour
{
    private GenAlgSnake GenController;
    // Start is called before the first frame update
    void Start()
    {
        GenController = GameObject.FindWithTag("GenController").GetComponent<GenAlgSnake>();
    }


    // Update is called once per frame
    void OnMouseDown()
    {

        switch (gameObject.name)
        {            
            case "SetSubmitBut":
                Time.timeScale = 1f;
                transform.parent.parent.parent.GetComponent<SnakeSetting>().Check = true;
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
