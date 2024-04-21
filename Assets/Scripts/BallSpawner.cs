using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    public List<BallTransform> placesToReplace;

    public Action OnBallRemoved;
    [SerializeField]
    private WorldRectTransforms worldRectTransforms;

    [SerializeField]
    private Ball ballPrefab;

    [SerializeField]
    private GridManager gridManager;

    [SerializeField]
    private Transform layout;
    
    private Ball[,] currentBallLayout = new Ball[5, 5];

    // Start is called before the first frame update
    void Start()
    {
        InitiateGrid();
    }

    private void InitiateGrid()
    {
        var grid = GridInfo.Instance.GetGrid();
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                var data = GlobalBallData.Instance.GetBallByValue(grid[i, j].Value.FloorPower2());
                var newData = new BallData
                {
                    Value = grid[i, j].Value,
                    BallTransform = new() { x = i, y = j },
                    Color = data.Color
                };
                Ball go = InstantiateSingleBall(i, j, newData);
                currentBallLayout[i, j] = go;
            }
        }
    }

    private Ball InstantiateSingleBall(int i, int j, BallData newData)
    {
        var go = BallPool.Instance.GetBall();
        go.transform.parent = layout;
        go.transform.position = worldRectTransforms.GetByPosition(i, j).rect.transform.position;
        go.Config(newData, gridManager, this, true);
        go.gameObject.SetActive(true);
        return go;
    }

    public Vector3 GetPosition(int x, int y)
    {
        return worldRectTransforms.GetByPosition(x, y).rect.transform.position;
    }

    public Ball GetBall(int x, int y)
    {
        return currentBallLayout[x, y];
    }
    public async Task OnGridChanged(List<BallTransform> ballTransformsRemoved, Ball createdBall)
    {
        var listOfXCoordinates = new List<int>();
        foreach (var ballTransform in ballTransformsRemoved)
        {
            if (listOfXCoordinates.Contains(ballTransform.x))
                continue;

            listOfXCoordinates.Add(ballTransform.x);
        }
        foreach (var ballTransform in ballTransformsRemoved)
        {
            currentBallLayout[ballTransform.x, ballTransform.y] = null;
            GridInfo.Instance.RemoveBall(ballTransform.x, ballTransform.y);
        }

        if (createdBall != null)
        {
            await createdBall.OnGridChanged();
        }


        foreach (var x in listOfXCoordinates)
        {
            for (int y = 0; y < 5; y++)
            {
                if (currentBallLayout[x, y] != null)
                {
                    await currentBallLayout[x, y].OnGridChanged();

                }
            }
        }

        foreach (var x in listOfXCoordinates)
        {
            for (int y = 0; y < 5; y++)
            {
                if (currentBallLayout[x, y] == null)
                {
                    var Value = GridInfo.Instance.GetNextBall(this, ballTransformsRemoved.Last());
                    var ballData = new BallData()
                    {
                        Value = Value,
                        BallTransform = new() { x = x, y = y },
                        Color = GlobalBallData.Instance.GetColorByValue(Value)
                    };
                    bool shouldPop = true;
                    if (y < 4)
                    {
                        if (currentBallLayout[x, y + 1] != null)
                            shouldPop = false;
                    }
                    await InstantiateBall(ballData, shouldPop);
                }
            }
        }

    }

    public void AddBall(Ball ball)
    {
        currentBallLayout[ball.GetData().BallTransform.x, ball.GetData().BallTransform.y] = ball;
        GridInfo.Instance.AddBall(ball.GetData());
    }
    public async Task<Ball> InstantiateBall(BallData ballData, bool pop = true, bool waitAnimation = false)
    {
        var go = BallPool.Instance.GetBall();
        go.transform.parent = layout;
        // go.transform.position = worldRectTransforms.GetByPosition(ballData.BallTransform.x, ballData.BallTransform.y).rect.transform.position;
        var data = GlobalBallData.Instance.GetBallByValue(ballData.Value.FloorPower2());
        var newData = new BallData
        {
            Value = ballData.Value,
            BallTransform = new() { x = ballData.BallTransform.x, y = ballData.BallTransform.y },
            Color = data.Color
        };
        go.Config(newData, gridManager, this, pop);
        go.gameObject.SetActive(true);
        AddBall(go);
        while (!go.AnimationDone && waitAnimation)
            await Task.Yield();

        return go;
    }

    public bool HasSolution()
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (currentBallLayout[i, j] == null)
                    continue;
                if (currentBallLayout[i, j].BallHasNeighbour()) ///We need at least one neighbor to have a solution
                    return true;
            }
        }

        return false;
    }

}

[System.Serializable]
public class WorldRectTransform
{
    public RectTransform rect;

    public BallTransform ballTransform;
}
[System.Serializable]
public class WorldRectTransforms
{
    public List<WorldRectTransform> worldRectTransforms;

    public WorldRectTransform GetByPosition(int x, int y)
    {
        foreach (var e in worldRectTransforms)
            if (e.ballTransform.x == x && e.ballTransform.y == y)
                return e;


        return null;
    }
}