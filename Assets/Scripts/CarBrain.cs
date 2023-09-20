using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarBrain : MonoBehaviour
{
    public bool IamStone = false;
    //Стартовые значения позииции и вращения машины
    private Vector3 StartPos, StartRot;
    //Конечное значение позиции
    public Vector3 LastPos;
    // Значения которые определяют ускорение и коэфициент поворота(Аналог руля) машины
    [Range(0, 1f)]
    public float speed;
    [Range(-1f, 1f)]
    public float turnRange;
    public float TurnMult;
    public float SpeedMult;
    public float LiveTime;
    // Расчет приспособленности (Чем дальше и быстрее проедет машина тем она приспособленей. Значимость скорости и дальности можно регулировать с помощью коэфициентов *)
    public float fitnessRate;
    public float CarDistance;
    public float AverageSpeed;
    //*
    public float CarDistanceMul;
    public float AverageSpeedMul;
    // Датчики которые будет принимать нейросеть(По сути из центра машины откладываем 3 луча: в перед и еще 2 отклонены от него на 45 градусов в разные стороны. 
    // Их значениями будут их длины от стен, в которые они упираюются, до центра машины)
    public float forwardSensor;
    public float leftSensor;
    public float rightSensor;

    public int LayersNum = 2;
    public int NeuronPerLayer = 10;

    public NNClass brain;

    // Выполняется в самом начале перед всем остальным кодом
    private void Start()
    { 
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
        brain.Fitness = fitnessRate;

        // Считаем приспособленность
        FitnessCalc();
        // Запись позиции машины во время текущего фрейма
        LastPos = gameObject.transform.position;
        // Считываем датчики
        SensorsCalc();
        // Если в течении 25 секунд машина не делает видимого прогресса или достигает нужного прогресса за большой промежуток времени, то убиваем её
        if(LiveTime >= 25 && fitnessRate < 30 || LiveTime >= 300 && fitnessRate < 2000)
        {
            Stone();
        }
        if (!IamStone)
        {
            float[] Sensors = { leftSensor, forwardSensor, rightSensor };
            (float Spd,float Trn) = brain.NNRun(Sensors);
            speed = Spd;
            turnRange = Trn;
            LiveTime += Time.deltaTime;
        }
        // Двигаемся
        Moving();
    }
    // Проверка на столкновение со стеной
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Wall")
        {
            Stone();
        }
    }

    public void ResetCar(NNClass net)
    {
        gameObject.transform.GetChild(0).GetComponent<Renderer>().material.color = Color.green;
        gameObject.transform.GetChild(1).GetComponent<Renderer>().material.color = Color.green;
        IamStone = false;
        transform.position = StartPos;
        transform.eulerAngles = new Vector3(0,90,0);
        LastPos = StartPos;
        speed = 0f;
        turnRange = 0f;
        LiveTime = 0f;
        fitnessRate = 0f;
        CarDistance = 0f;
        AverageSpeed = 0f;
        brain = net;
        brain.LayersNum = LayersNum;
        brain.NeuronPerLayer = NeuronPerLayer;
    }

    private void Moving()
    {
        //Меняем поворот и скорость в зависимовсти от значений turnRange и speed соответственно 
        gameObject.transform.eulerAngles += new Vector3(0, turnRange * TurnMult * 90 * Time.deltaTime, 0);
        gameObject.transform.position += transform.TransformDirection(new Vector3(0, 0, speed * SpeedMult));
    }
    // Обработка столкновения
    public void Stone()
    {
        IamStone = true;
        // Остановка
        turnRange = 0;
        speed = 0;
        // Изменение цвета
        gameObject.transform.GetChild(0).GetComponent<Renderer>().material.color = Color.gray;
        gameObject.transform.GetChild(1).GetComponent<Renderer>().material.color = Color.gray;

        
       //brain.Fitness = fitnessRate;
        
    }
    private void FitnessCalc()
    {
        // Прибавляем к общему расстоянию расстояние между позицей теперешней и позицией которая была на прошлом фрейме
        CarDistance += Vector3.Distance(transform.position, LastPos);
        AverageSpeed = CarDistance / LiveTime;

        fitnessRate = CarDistance * CarDistanceMul + AverageSpeed * AverageSpeedMul;
    }
    private void SensorsCalc()
    {
        // Откладываем луч в перед и проверяем на столкновение со стеной. Если столкновение есть, то записываем расстояние от стены до машины в соответствующий сенсор
        // после поворачиваем луч и повторяем
        Ray CarRay = new Ray(transform.position, transform.forward);
        RaycastHit hit;
         Debug.DrawRay(transform.position, transform.forward, new Color(255, 0 ,0));
         Debug.DrawRay(transform.position, (transform.forward + transform.right), new Color(255, 0, 0));
         Debug.DrawRay(transform.position, (transform.forward - transform.right), new Color(255, 0, 0));
        if (Physics.Raycast(CarRay, out hit))
        {
            if(hit.collider.tag == "Wall")
            forwardSensor = hit.distance / 25;
        }
        CarRay.direction = (transform.forward + transform.right);
        if (Physics.Raycast(CarRay, out hit))
        {
            if (hit.collider.tag == "Wall")
                rightSensor = hit.distance / 25;
        }
        CarRay.direction = (transform.forward - transform.right);
        if (Physics.Raycast(CarRay, out hit))
        {
            if (hit.collider.tag == "Wall")
                leftSensor = hit.distance / 25;
        }
    }
}
