using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;
public class GridInfo : MonoBehaviour
{

    public static GridInfo Instance;

    private BallData[,] gridData = new BallData[5, 5];
    private SerializableBallData[,] serializableBallData = new SerializableBallData[5, 5];
    private int spawnCounter = 1;

    private int timesRandom;
    
    private void Awake()
    {
        Instance = this;
    }
    public void SaveGrid()
    {
        SerializeData();

        var gridSaveData = JsonConvert.SerializeObject(serializableBallData);
        PlayerPrefs.SetString("GridData", gridSaveData);
    }

    public void SerializeData()
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (gridData[i, j] == null)
                {
                    continue;
                }
                serializableBallData[i, j] = new()
                {
                    Value = gridData[i, j].Value,
                    BallTransform = gridData[i, j].BallTransform
                };
            }
        }
    }
    public void RemoveBall(int x, int y)
    {
        gridData[x, y] = null;
        SaveGrid();
    }
    public void AddBall(BallData ballData)
    {
        gridData[ballData.BallTransform.x, ballData.BallTransform.y] = ballData;
        SaveGrid();
    }
    public bool BallExistsBelow(int x, int y)
    {
        if (y == 0)
            return true;

        return gridData[x, y - 1] != null;
    }
    public BallData[,] LoadGrid()
    {
        var data = PlayerPrefs.GetString("GridData");
        if(string.IsNullOrEmpty(data))
            return CreateNewGrid();
        serializableBallData = JsonConvert.DeserializeObject<SerializableBallData[,]>(data);
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                gridData[i, j] = new()
                {
                    Value = serializableBallData[i, j].Value,
                    BallTransform = serializableBallData[i, j].BallTransform,
                    Color = GlobalBallData.Instance.GetColorByValue(serializableBallData[i, j].Value)
                };
            }
        }

        return gridData;

    }

    public int GetNextBall(BallSpawner spawner, BallTransform lastRemoved)
    {
        if (!spawner.HasSolution())
        {
            List<int> topValues = new List<int>();

            for (int i = lastRemoved.x - 1; i < lastRemoved.x + 2; i++)
            {
                for (int j = lastRemoved.y - 1; j < lastRemoved.y + 2; j++)
                {
                    try
                    {
                        if (gridData[i, j] != null)
                            topValues.Add(gridData[i, j].Value);
                    }
                    catch (System.Exception e)
                    {
                        continue;
                    }
                }
            }

            if (timesRandom < 3)
                return topValues[Random.Range(0, topValues.Count)];

            timesRandom = 0;
            return topValues.Min();
        }


        timesRandom = 0;
        spawnCounter += Random.Range(1, 5);
        if (spawnCounter > 6)
            spawnCounter = 1;

        return (int)Mathf.Pow(2, spawnCounter);
    }

    public BallData[,] CreateNewGrid()
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                var Value = (int)Mathf.Pow(2, UnityEngine.Random.Range(1, 6));
                gridData[i, j] = new BallData()
                {
                    Value = Value,
                    BallTransform = new() { x = i, y = j },
                };
            }
        }

        return gridData;
    }

    public BallData[,] GetGrid()
    {
        if (PlayerPrefs.GetInt("PlayedBefore") == 1)
        {
            return LoadGrid();
        }
        else
        {
            PlayerPrefs.SetInt("PlayedBefore", 1);
            return CreateNewGrid();
        }
    }

    public BallData GetBallData(int x, int y)
    {
        if (x < 0 || y < 0)
            return null;

        if (x > 4 || y > 4)
            return null;

        return gridData[x, y];
    }


}

[System.Serializable]
public class SerializableBallData
{
    public int Value;
    public BallTransform BallTransform;
}