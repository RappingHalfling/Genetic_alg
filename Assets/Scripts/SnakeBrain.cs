using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeBrain : MonoBehaviour
{
  
    //Стартовые значения позииции и вращения машины
    private Vector3 StartPos, StartRot;
    //Конечное значение позиции
    GameObject [] Bones = new GameObject[11];
    [Range(-1f, 1f)]
    public float[] turnRange = new float[10];
    public float TurnMult;
    
    public float LiveTime;
    // Расчет приспособленности (Чем дальше и быстрее проедет машина тем она приспособленей. Значимость скорости и дальности можно регулировать с помощью коэфициентов *)
    public float fitnessRate;
    public float SnakeDistance;
    public float AverageSpeed;
    //*
    public float SnakeDistanceMul;
    public float AverageSpeedMul;
    // Датчики которые будет принимать нейросеть(По сути из центра машины откладываем 3 луча: в перед и еще 2 отклонены от него на 45 градусов в разные стороны. 
    // Их значениями будут их длины от стен, в которые они упираюются, до центра машины)
    public float forwardSensor;
    public float leftSensor;
    public float rightSensor;
    public float upSensor;
    public float downSensor;
    public float backSensor;
    public float[] Sensors = new float[10];
    public int LayersNum = 2;
    public int NeuronPerLayer = 10;

    public NNClass brain;

    // Выполняется в самом начале перед всем остальным кодом
    private void Start()
    {
        for (int i = 0; i < Bones.Length; i++)
            Bones[i] = transform.GetChild(i).gameObject;
        // Назначаем стартовые значения позиции и вращения
        StartPos = gameObject.transform.position;
        StartRot = gameObject.transform.eulerAngles;
        brain = GetComponent<NNClass>();
        brain.Initialization(LayersNum, NeuronPerLayer);
        brain.LayersNum = LayersNum;
        brain.NeuronPerLayer = NeuronPerLayer;
        brain.RandomizeWeights();
    }
    //Выполняется в определенный малый промежуток времени в зависимости фреймрейта
    private void FixedUpdate()
    {

       
        FitnessCalc();
       
        float[] res = brain.NNSnakeRun(turnRange);

        for (int i = 0; i < 10; i++)
            turnRange[i] = res[i];
        LiveTime += Time.deltaTime;
        brain.Fitness = fitnessRate;
        
        // Двигаемся
        Moving();
    }    

    public void ResetSnake(NNClass net)
    {               
        transform.position = StartPos;
        transform.eulerAngles = StartRot;
        for (int i = 0; i < turnRange.Length; i++)
            turnRange[i] = 0f;
        for (int i = 0; i < Bones.Length; i++)
        {
            Bones[i].transform.localPosition = new Vector3(-i * 1.05f, 0, 0);
            Bones[i].transform.localEulerAngles = new Vector3(0, -180, 0);
        }
        LiveTime = 0f;
        fitnessRate = 0f;
        SnakeDistance = 0f;
        AverageSpeed = 0f;
        brain = net;
        brain.LayersNum = LayersNum;
        brain.NeuronPerLayer = NeuronPerLayer;
    }

    private void Moving()
    {
        for (int i = 1; i < Bones.Length; i++)
        {
            JointSpring spr = Bones[i].GetComponent<HingeJoint>().spring;
            spr.targetPosition = turnRange[i - 1] * TurnMult;
            Bones[i].GetComponent<HingeJoint>().spring = spr;
               
        }

    }
    // Обработка столкновения

    private void FitnessCalc()
    {
        // Прибавляем к общему расстоянию расстояние между позицей теперешней и позицией которая была на прошлом фрейме
        SnakeDistance = Bones[0].transform.position.x;
        AverageSpeed = SnakeDistance / LiveTime;

        fitnessRate = 1000 + SnakeDistance * SnakeDistanceMul + AverageSpeed * AverageSpeedMul;
    }
   
}
