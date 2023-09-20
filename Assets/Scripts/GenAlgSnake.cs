using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using MathNet.Numerics.LinearAlgebra;
using System;

public class GenAlgSnake : MonoBehaviour
{
    // Проверка нажатия кнопки остановки алгоритма
    public bool SubmitClick = false;
    // Номер поколения
    public int GenNum = 0;
    // колличество особей 
    public int SamplesPerGeneration;
    // Колличество особей пееренесенных элитарной стратегией
    public int NumForElitist;
    // Суммарное значение функции приспособленности
    public float maxFitness = 0;
    public float SumFitness = 0;
    // Массивы с представителями текущего поколения
    List<SnakeBrain> CurSnakeGeneration = new List<SnakeBrain>();
    List<NNClass> CurNNGeneration = new List<NNClass>();
    List<NNClass> Parents = new List<NNClass>();
    // Текст для перегеноа в графики
    private string GrphTextGen = "";
    NNClass tempNN;
    NNClass tempNN1;
    NNClass MaxFitBrain;

    public bool CrossbreedingCheck, MutationCheck, ElitistCheck;
    public float MutatioChanse;
    public int StrategyChoose;



    // Задаём стартовые значения
    private void Start()
    {
        NumForElitist = SamplesPerGeneration;
        GenNum = 0;
        Time.timeScale = 0f;
        FillFirstGeneration();
    }
    // каждый фрейм проверяем оккаменели ли все осыби и если да то переходим на следующее поколение
    private void FixedUpdate()
    {

        if (CurSnakeGeneration[0].LiveTime > 10)
        {
            maxFitness = 0;
            for (int i = 0; i < SamplesPerGeneration; i++)
            {
                SumFitness += CurNNGeneration[i].Fitness;
                if (maxFitness < CurNNGeneration[i].Fitness)
                {
                    maxFitness = CurNNGeneration[i].Fitness;
                    MaxFitBrain = CurNNGeneration[i];
                    if (ElitistCheck)
                        NumForElitist = i;
                }
            }
            ResetGen();
        }
        // Проверяем Нажатие кнопки остановки алгоритма
        if (SubmitClick || GenNum > 300)
        {
            maxFitness = 0;
            for (int i = 0; i < SamplesPerGeneration; i++)
            {

                if (maxFitness < CurNNGeneration[i].Fitness)
                {
                    maxFitness = CurNNGeneration[i].Fitness;
                    MaxFitBrain = CurNNGeneration[i];
                }
            }
            MaxFitBrain.SaveNN();
            WriteInTxt();
            SubmitClick = false;
            SceneManager.LoadScene("MainMenu");
        }
    }
    // Заполнение Первого поколения заспавнеными машинами в сетях кторых выставлены рандомные веса
    private void FillFirstGeneration()
    {
        for (int i = 0; i < SamplesPerGeneration; i++)
        {
            SnakeBrain car = transform.GetChild(i).GetComponent<SnakeBrain>();
            NNClass NN = car.GetComponent<NNClass>();
            CurNNGeneration.Add(NN);
            CurSnakeGeneration.Add(car);
            Parents.Add(null);



        }

    }
    //Выбор родительского пула методом рулетки
    private void ChooseParentsByRoulette()
    {
        float[,] ChanseToBeParent = new float[SamplesPerGeneration, 3];
        ChanseToBeParent[0, 0] = 0f;
        ChanseToBeParent[0, 1] = ChanseToBeParent[0, 0] + ((CurNNGeneration[0].Fitness) / SumFitness);
        ChanseToBeParent[0, 2] = 0;

        for (int i = 1; i < SamplesPerGeneration; i++)
        {
            ChanseToBeParent[i, 0] = ChanseToBeParent[i - 1, 1];
            ChanseToBeParent[i, 1] = ChanseToBeParent[i, 0] + ((CurNNGeneration[i].Fitness) / SumFitness);
            ChanseToBeParent[0, 2] = 0;
        }
        for (int i = 0; i < SamplesPerGeneration; i++)
        {
            float dice = UnityEngine.Random.Range(0f, 1f);


            for (int j = 0; j < SamplesPerGeneration; j++)
            {

                if (dice >= ChanseToBeParent[j, 0] && dice <= ChanseToBeParent[j, 1])
                {
                    tempNN = CurSnakeGeneration[i].gameObject.AddComponent<NNClass>();
                    tempNN.InputsNum = CurNNGeneration[j].InputsNum;
                    tempNN.OutsNum = CurNNGeneration[j].OutsNum;
                    tempNN.Initialization(CurSnakeGeneration[i].LayersNum, CurSnakeGeneration[i].NeuronPerLayer);
                    for (int k = 0; k < tempNN.weights.Count; k++)
                    {
                        for (int x = 0; x < tempNN.weights[k].RowCount; x++)
                        {
                            for (int y = 0; y < tempNN.weights[k].ColumnCount; y++)
                            {
                                tempNN.weights[k][x, y] = CurNNGeneration[j].weights[k][x, y];
                            }
                        }
                    }
                    for (int k = 0; k < tempNN.biases.Count; k++)
                    {
                        tempNN.biases[k] = CurNNGeneration[j].biases[k];
                    }
                    tempNN.Num = CurNNGeneration[j].Num;
                    Parents[i] = tempNN;

                    break;
                }

            }
        }
    }
    //Турнирный метод (4 Осыби в каждом турнирном сегменте)
    private void ChooseParentsByTournament()
    {
        NNClass[] TournamentList = new NNClass[4];
        NNClass MAxByTrList;
        for (int i = 0; i < SamplesPerGeneration; i++)
        {
            TournamentList[0] = CurNNGeneration[UnityEngine.Random.Range(0, SamplesPerGeneration)];
            TournamentList[1] = CurNNGeneration[UnityEngine.Random.Range(0, SamplesPerGeneration)];
            //TournamentList[2] = CurNNGeneration[UnityEngine.Random.Range(0, SamplesPerGeneration)];
            //TournamentList[3] = CurNNGeneration[UnityEngine.Random.Range(0, SamplesPerGeneration)];
            MAxByTrList = TournamentList[0];
            for (int j = 1; j < 2; j++)
            {
                if (MAxByTrList.Fitness < TournamentList[j].Fitness)
                {
                    MAxByTrList = TournamentList[j];
                }
            }


            tempNN = CurSnakeGeneration[i].gameObject.AddComponent<NNClass>();
            tempNN.InputsNum = MAxByTrList.InputsNum;
            tempNN.OutsNum = MAxByTrList.OutsNum;
            tempNN.Initialization(CurSnakeGeneration[i].LayersNum, CurSnakeGeneration[i].NeuronPerLayer);
            for (int k = 0; k < tempNN.weights.Count; k++)
            {
                for (int x = 0; x < tempNN.weights[k].RowCount; x++)
                {
                    for (int y = 0; y < tempNN.weights[k].ColumnCount; y++)
                    {
                        tempNN.weights[k][x, y] = MAxByTrList.weights[k][x, y];
                    }
                }
            }
            for (int k = 0; k < tempNN.biases.Count; k++)
            {
                tempNN.biases[k] = MAxByTrList.biases[k];
            }
            tempNN.Num = MAxByTrList.Num;
            Parents[i] = tempNN;
        }
    }
    // ВЫборка по ранг(Проходит только первая половина осыбей со своии дубликатами)
    private void ChooseParentsByRank()
    {
        CurNNGeneration.Sort((x, y) => x.Fitness.CompareTo(y.Fitness));
        for (int i = 0; i < SamplesPerGeneration / 2; i++)
        {
            tempNN = CurSnakeGeneration[SamplesPerGeneration - 1 - i].gameObject.AddComponent<NNClass>();
            tempNN1 = CurSnakeGeneration[i].gameObject.AddComponent<NNClass>();

            tempNN.InputsNum = CurNNGeneration[SamplesPerGeneration - 1 - i].InputsNum;
            tempNN.OutsNum = CurNNGeneration[SamplesPerGeneration - 1 - i].OutsNum;
            tempNN1.InputsNum = CurNNGeneration[SamplesPerGeneration - 1 - i].InputsNum;
            tempNN1.OutsNum = CurNNGeneration[SamplesPerGeneration - 1 - i].OutsNum;

            tempNN.Initialization(CurSnakeGeneration[i].LayersNum, CurSnakeGeneration[i].NeuronPerLayer);
            tempNN1.Initialization(CurSnakeGeneration[i].LayersNum, CurSnakeGeneration[i].NeuronPerLayer);
            for (int k = 0; k < tempNN.weights.Count; k++)
            {
                for (int x = 0; x < tempNN.weights[k].RowCount; x++)
                {
                    for (int y = 0; y < tempNN.weights[k].ColumnCount; y++)
                    {
                        tempNN.weights[k][x, y] = CurNNGeneration[SamplesPerGeneration - 1 - i].weights[k][x, y];
                        tempNN1.weights[k][x, y] = CurNNGeneration[SamplesPerGeneration - 1 - i].weights[k][x, y];
                    }
                }
            }
            for (int k = 0; k < tempNN.biases.Count; k++)
            {
                tempNN.biases[k] = CurNNGeneration[SamplesPerGeneration - 1 - i].biases[k];
                tempNN1.biases[k] = CurNNGeneration[SamplesPerGeneration - 1 - i].biases[k];
            }
            tempNN.Num = CurNNGeneration[SamplesPerGeneration - 1 - i].Num;
            tempNN1.Num = CurNNGeneration[SamplesPerGeneration - 1 - i].Num;
            Parents[i] = tempNN;
            Parents[SamplesPerGeneration - 1 - i] = tempNN1;
        }
    }

    private void CrossbreedingAndMutation()
    {
        int LayersNum = CurSnakeGeneration[0].LayersNum;
        int NeuronPerLayer = CurSnakeGeneration[0].NeuronPerLayer;
        switch (StrategyChoose)
        {
            case 0:
                ChooseParentsByRoulette();
                break;
            case 1:
                ChooseParentsByTournament();
                break;
            case 2:
                ChooseParentsByRank();
                break;
        }

        CurNNGeneration.Clear();
        int i = SamplesPerGeneration - 1;




        int dice1 = 0;
        int dice2 = 0;
        int[,] PointsForWeights = new int[LayersNum + 1, 2];
        while (i > 0)
        {
            dice1 = UnityEngine.Random.Range(0, i + 1);
            dice2 = UnityEngine.Random.Range(0, i + 1);
            while (dice2 == dice1)
            {
                dice2 = UnityEngine.Random.Range(0, i + 1);
            }

            if (CrossbreedingCheck)
            {
                PointsForWeights[0, 0] = UnityEngine.Random.Range(0, 3);
                if (PointsForWeights[0, 0] == 0)
                {
                    PointsForWeights[0, 1] = UnityEngine.Random.Range(1, NeuronPerLayer);
                }
                else if (PointsForWeights[0, 0] == 2)
                {
                    PointsForWeights[0, 1] = UnityEngine.Random.Range(0, NeuronPerLayer - 1);
                }
                else
                {
                    PointsForWeights[0, 1] = UnityEngine.Random.Range(0, NeuronPerLayer);
                }



                for (int x = 1; x < LayersNum; x++)
                {

                    PointsForWeights[x, 0] = UnityEngine.Random.Range(0, NeuronPerLayer);
                    if (PointsForWeights[x, 0] == 0)
                    {
                        PointsForWeights[x, 1] = UnityEngine.Random.Range(1, NeuronPerLayer);
                    }
                    else if (PointsForWeights[x, 0] == NeuronPerLayer - 1)
                    {
                        PointsForWeights[x, 1] = UnityEngine.Random.Range(0, NeuronPerLayer - 1);
                    }
                    else
                    {
                        PointsForWeights[x, 1] = UnityEngine.Random.Range(0, NeuronPerLayer);
                    }

                }


                PointsForWeights[LayersNum, 1] = UnityEngine.Random.Range(0, 2);
                if (PointsForWeights[LayersNum, 1] == 0)
                {
                    PointsForWeights[LayersNum, 0] = UnityEngine.Random.Range(1, NeuronPerLayer);
                }
                else if (PointsForWeights[LayersNum, 1] == 1)
                {
                    PointsForWeights[LayersNum, 0] = UnityEngine.Random.Range(0, NeuronPerLayer - 1);
                }
                else
                {
                    PointsForWeights[LayersNum, 0] = UnityEngine.Random.Range(0, NeuronPerLayer);
                }

                tempNN = gameObject.AddComponent<NNClass>();
                tempNN.InputsNum = Parents[dice1].InputsNum;
                tempNN.OutsNum = Parents[dice1].OutsNum;
                tempNN.Initialization(LayersNum, NeuronPerLayer);
                for (int j = 0; j < tempNN.weights.Count; j++)
                {
                    for (int x = 0; x < tempNN.weights[j].RowCount; x++)
                    {
                        for (int y = 0; y < tempNN.weights[j].ColumnCount; y++)
                        {
                            tempNN.weights[j][x, y] = Parents[dice1].weights[j][x, y];
                        }
                    }
                }
                for (int j = 0; j < tempNN.biases.Count; j++)
                {
                    tempNN.biases[j] = Parents[dice1].biases[j];
                }
                tempNN.Num = Parents[dice1].Num;
                int BiasesDice = UnityEngine.Random.Range(0, LayersNum + 2);
                Parents[dice1].CrossbreedWithOtherBrain(Parents[dice2], PointsForWeights, BiasesDice);
                Parents[dice2].CrossbreedWithOtherBrain(tempNN, PointsForWeights, BiasesDice);
                DestroyImmediate(tempNN);
            }
            if (MutationCheck)
            {
                Parents[dice1].Mutate(MutatioChanse);
                Parents[dice2].Mutate(MutatioChanse);
            }

            Parents.RemoveAt(dice1);
            if (dice2 > dice1)
            {
                Parents.RemoveAt(dice2 - 1);
            }
            else
            {
                Parents.RemoveAt(dice2);
            }
            Parents.Add(null);
            Parents.Add(null);
            CurNNGeneration.Add(null);
            CurNNGeneration.Add(null);
            i -= 2;

        }

        if (i == 0)
        {
            if (MutationCheck)
            {
                Parents[0].Mutate(MutatioChanse);
            }

            Parents.Remove(Parents[0]);
            Parents.Add(null);
            CurNNGeneration.Add(null);
        }
        for (int j = 0; j < SamplesPerGeneration; j++)
        {
            if (j != NumForElitist)
                DestroyImmediate(CurSnakeGeneration[j].gameObject.GetComponent<NNClass>());
            else
                DestroyImmediate(CurSnakeGeneration[j].gameObject.GetComponents<NNClass>()[1]);
        }
    }

    // Высчитываем среднеее значение приспособленности по поколению производим формирование нового поколения и заменяем сети старого поколения сетями нового поколения
    private void ResetGen()
    {
        float AvgFitness;

        CrossbreedingAndMutation();;

        for (int i = 0; i < SamplesPerGeneration; i++)
        {

            CurNNGeneration[i] = CurSnakeGeneration[i].gameObject.GetComponent<NNClass>();
            CurSnakeGeneration[i].ResetSnake(CurNNGeneration[i]);

        }
        AvgFitness = SumFitness / SamplesPerGeneration;
        GrphTextGen += GenNum + "/" + AvgFitness + "/" + maxFitness + "\r\n";
        AvgFitness = 0;
        SumFitness = 0;
        GenNum++;
    }

    private void WriteInTxt()
    {
        string filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\Grph.txt"
;
        FileStream fileStream = File.Open(filePath, FileMode.Create);
        StreamWriter output = new StreamWriter(fileStream);
        output.Write(GrphTextGen);
        output.Close();
    }


}
