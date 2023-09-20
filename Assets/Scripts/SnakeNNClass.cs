using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using Newtonsoft.Json;
using System;
using System.IO;

public class SnakeNNClass : MonoBehaviour
{
    //Матрица входных значений
    public Matrix<float> Inputs = Matrix<float>.Build.Dense(1, 3);
    //Матрица выходных значений
    public Matrix<float> Outs = Matrix<float>.Build.Dense(1, 2);
    //Лист матриц весов
    public List<Matrix<float>> weights = new List<Matrix<float>>();
    //Лист матриц скрытых слоёв
    List<Matrix<float>> Hidens = new List<Matrix<float>>();
    //Лист Биасов
    public List<float> biases = new List<float>();
    public float Fitness = 0;

    public int LayersNum;
    public int NeuronPerLayer;
    public string Num;
    public string CrossBreedLog;

    private float temp;

    public void Initialization(int HidensLNum, int HidenNeuronsPerLNum)
    {
        // Очищаем Старые значения
        Inputs.Clear();
        Outs.Clear();
        weights.Clear();
        Hidens.Clear();
        biases.Clear();
        // Инициализируем как будет выглядеть первая матрица весов
        weights.Add(Matrix<float>.Build.Dense(3, HidenNeuronsPerLNum));
        biases.Add(UnityEngine.Random.Range(-1f, 1f));

        Hidens.Add(Matrix<float>.Build.Dense(1, HidenNeuronsPerLNum)); // - Попутно инициализируем Матрицы(Строки) Скрытых слоёв (*)
        // Инициализируем как будут выглядеть промежуточные матрицы весов
        for (int i = 1; i < HidensLNum; i++)
        {
            Hidens.Add(Matrix<float>.Build.Dense(1, HidenNeuronsPerLNum)); // - (*)

            weights.Add(Matrix<float>.Build.Dense(HidenNeuronsPerLNum, HidenNeuronsPerLNum));
            biases.Add(UnityEngine.Random.Range(-1f, 1f));
        }
        // Инициализируем как будет выглядеть последняя матрицы весов
        weights.Add(Matrix<float>.Build.Dense(HidenNeuronsPerLNum, 2));
        biases.Add(UnityEngine.Random.Range(-1f, 1f));

        //Забиваем рандомные значения в матрицы весов(Для Тестов!)
        // RandomizeWeights();

    }

    public void RandomizeWeights()
    {
        for (int i = 0; i < weights.Count; i++)
        {
            for (int x = 0; x < weights[i].RowCount; x++)
            {
                for (int y = 0; y < weights[i].ColumnCount; y++)
                {
                    weights[i][x, y] = UnityEngine.Random.Range(-1f, 1f);
                }
            }
        }
    }
    //PointsOfCrossWeight двойной массив размера [weights.Count,2](1 - индекс матрицы веса , 2- х и у координата точки скрещивания)
    public void CrossbreedWithOtherBrain(NNClass otherBrain, int[,] PointsOfCrossWeight, int PointOfCrossBiases)
    {
        int x;
        for (int i = 0; i < weights.Count; i++)
        {
            x = 0;
            while (x < PointsOfCrossWeight[i, 0])
            {
                for (int y = 0; y < weights[i].ColumnCount; y++)
                {
                    weights[i][x, y] = otherBrain.weights[i][x, y];
                }
                x++;
            }
            for (int y = 0; y < PointsOfCrossWeight[i, 1]; y++)
            {
                weights[i][x, y] = otherBrain.weights[i][x, y];
            }
        }
        for (int i = 0; i < PointOfCrossBiases; i++)
        {
            biases[i] = otherBrain.biases[i];
        }


    }
    public void Mutate(float MutationRate)
    {

        for (int i = 0; i < weights.Count; i++)
        {
            int x = 0;
            while (x < weights[i].RowCount)
            {
                for (int y = 0; y < weights[i].ColumnCount; y++)
                {
                    float dice = UnityEngine.Random.Range(0f, 1f);
                    if (dice < MutationRate)
                    {
                        weights[i][x, y] = UnityEngine.Random.Range(-1f, 1f);
                    }
                }
                x++;
            }

        }
    }
    //запускаем сеть
    public (float, float) NNRun(float SL, float SF, float SR)
    {
        //Забиваем значения датчиков в 1 слой  
        Inputs[0, 0] = SL;
        Inputs[0, 1] = SF;
        Inputs[0, 2] = SR;
        // прогоняем все значения через гиперболический тангенс для получения значенийй от -1 до 1
        Inputs = Inputs.PointwiseTanh();
        // Вычисляем 1 спрятаный слой
        Hidens[0] = (Inputs * weights[0] + biases[0]).PointwiseTanh();
        //Вычисляем оставшиеся
        for (int i = 1; i < Hidens.Count; i++)
        {
            Hidens[i] = (Hidens[i - 1] * weights[i] + biases[i]).PointwiseTanh();
        }
        // Вычисляем последний слой(Результат)
        Outs = (Hidens[Hidens.Count - 1] * weights[weights.Count - 1] + biases[biases.Count - 1]).PointwiseTanh();
        //Возвращаем результат
        return ((1 / (1 + Mathf.Exp(-Outs[0, 0]))), (float)System.Math.Tanh(Outs[0, 1]));
    }

    public void SaveNN()
    {
        string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string ID = DateTime.Now.Ticks.ToString();
        if (!System.IO.Directory.Exists(path + "\\Brains\\CarBrain_" + ID))
        {
            Directory.CreateDirectory(path + "\\Brains\\CarBrain_" + ID);
        }
        path = path + "\\Brains\\CarBrain_" + ID;
        string json = JsonConvert.SerializeObject(weights.ToArray());
        File.WriteAllText(path + "\\weights" + ".txt", json);
        json = JsonConvert.SerializeObject(biases.ToArray());
        File.WriteAllText(path + "\\biases" + ".txt", json);
    }
}
