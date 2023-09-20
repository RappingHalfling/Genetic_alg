using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Setting : MonoBehaviour
{
    GenAlg Controller;
    GameObject Menu;
    public bool Check = false;
    private void Start()
    {
        Controller = GameObject.FindWithTag("GenController").GetComponent<GenAlg>();
        Menu = gameObject.transform.GetChild(0).GetChild(0).gameObject;

    }
    private void FixedUpdate()
    {
        if (Check)
        {
            Controller.CrossbreedingCheck = Menu.transform.GetChild(2).GetComponent<Toggle>().isOn;
            Controller.MutationCheck = Menu.transform.GetChild(3).GetComponent<Toggle>().isOn;
            Controller.ElitistCheck = Menu.transform.GetChild(6).GetComponent<Toggle>().isOn;

            Controller.MutatioChanse = Convert.ToSingle(Menu.transform.GetChild(5).GetComponent<InputField>().text);
            Controller.StrategyChoose = Menu.transform.GetChild(0).GetComponent<Dropdown>().value;
            gameObject.SetActive(false);
        }
    }
}
