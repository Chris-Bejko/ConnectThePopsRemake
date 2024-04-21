using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{

    public static bool ContainsBall(this List<Ball> ballDatas, Ball ballData)
    {
        foreach (Ball ball in ballDatas)
        {
            if (ball.GetData().Equals(ballData.GetData()))
                return true;
        }

        return false;
    }


    public static int FloorPower2(this int x)
    {
        if (x < 1)
        {
            return 1;
        }
        return (int)Mathf.Pow(2, (int)Mathf.Log(x, 2));
    }

     public static int GetExponentBase2(this int N)
    {
        int iteration = 0;
        int number = 0;
        number += N;
        while (number / 2 > 0 && number % 2 == 0)
        {
            number /= 2;
            iteration++;
        }

        return iteration;
    }
}