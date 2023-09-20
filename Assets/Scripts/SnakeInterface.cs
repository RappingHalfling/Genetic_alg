using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SnakeInterface : MonoBehaviour
{
    private GameObject SampInfo;
    private int SampleCount;
    private GameObject GenController;
    private Transform StatsPanel;
    // Start is called before the first frame update
    void Start()
    {
        GenController = GameObject.FindWithTag("GenController");
        SampInfo = transform.GetChild(0).GetChild(4).GetChild(0).GetChild(0).GetChild(0).gameObject;
        SampleCount = GenController.GetComponent<GenAlgSnake>().SamplesPerGeneration;
        StatsPanel = gameObject.transform.GetChild(0);
        DrawInfoBlock();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        DrawInfo();
        StatsPanel.GetChild(0).GetComponentInChildren<Text>().text = "Generation#" + GenController.GetComponent<GenAlgSnake>().GenNum;
        StatsPanel.GetChild(5).GetComponentInChildren<Text>().text = "Time Scale: " + Time.timeScale;
    }
    public void DrawInfoBlock()
    {
        for (int i = 1; i < SampleCount; i++)
        {
            GameObject go = Instantiate(SampInfo.transform.GetChild(0).gameObject);
            go.transform.SetParent(SampInfo.transform);
            go.transform.localEulerAngles = Vector3.zero;
            go.transform.localScale = SampInfo.transform.GetChild(0).localScale;
            go.transform.localPosition = new Vector3(SampInfo.transform.GetChild(0).localPosition.x, SampInfo.transform.GetChild(0).localPosition.y - 24 * i, SampInfo.transform.GetChild(0).localPosition.z);
        }
    }
    public void DrawInfo()
    {
        for (int i = 0; i < SampleCount; i++)
        {

            Text curText = SampInfo.transform.GetChild(i).GetComponent<Text>();
            SnakeBrain Car = GenController.transform.GetChild(i).GetComponent<SnakeBrain>();
            curText.text = "№" + i + "| Fitness:" + Car.fitnessRate;            
            curText.color = new Color(0, 0, 0);
        }
    }
}
