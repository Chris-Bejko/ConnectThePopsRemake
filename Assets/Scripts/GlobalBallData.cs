using System.Collections.Generic;
using UnityEngine;

public class GlobalBallData : MonoBehaviour
{
    public static GlobalBallData Instance;

    [SerializeField]
    private List<BallData> BallData;

    private void Awake()
    {
        Instance = this;
    }
    public BallData GetBallByValue(int value)
    {
        foreach (var e in BallData)
        {
            var exponent = value.GetExponentBase2();
            var comparison = exponent;
            var secondDigit = int.Parse(comparison.ToString()[^1].ToString());
            comparison = secondDigit;
        
            if (secondDigit == 0)
            {
                comparison = 10;
            }

            if (e.Value == comparison)
                return e;
        }

        return null;
    }

    public Color GetColorByValue(int value)
    {
        return GetBallByValue(value).Color;
    }
}
