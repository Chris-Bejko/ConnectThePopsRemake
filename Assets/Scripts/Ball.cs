using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;

public class Ball : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField]
    private Image[] images;

    [SerializeField]
    private bool IsPreviewBall;
    [SerializeField]
    private Image image, innerImage, ring;

    [SerializeField]
    private TMP_Text valueText;

    [SerializeField]
    private GridManager gridManager;

    [SerializeField]
    private BallData ballData;
    [SerializeField]
    private BallSpawner ballSpawner;
    [SerializeField]
    private AudioClip click, popSound;

    [SerializeField]
    private AudioSource audioSource;
    
    Vector2 originalScale;

    private bool Hovering;

    public bool AnimationDone { get; internal set; }
    public bool FallDone = true;


    private void Awake()
    {
        FallDone = true;
        originalScale = transform.localScale;
    }
    public void Config(BallData ballData, GridManager gridManager, BallSpawner ballSpawner, bool pop = true)
    {
        Hovering = false;
        AnimationDone = false;
        this.gridManager = gridManager;
        this.ballData = ballData;
        this.ballSpawner = ballSpawner;

        if (!IsPreviewBall)
        {
            if (pop)
                PopAnimation();
            else
                FallAnimation();
        }

        if (IsPreviewBall)
        {
            SetText();
            SetColor();
        }
        else
        {
            this.ballData.Value = ballData.Value.FloorPower2();
            SetText();
            SetColor();
        }

    }

    private void SetText()
    {
        if (ballData.Value <= 0)
            return;

        if(ballData.Value <= 1024)
        {
            valueText.gameObject.SetActive(false);
            return;
        }
        valueText.gameObject.SetActive(true);
        var exponent = ballData.Value.FloorPower2().GetExponentBase2();
        var lastDigit = int.Parse(exponent.ToString()[^1].ToString());
        valueText.text = Mathf.Pow(2, lastDigit).ToString();

        if (exponent <= 9)
            return;

        if (exponent <= 20)
        {
            valueText.text += "K";
        }
        else if (exponent <= 30)
        {
            valueText.text += "M";
        }

    }

    private void SetColor()
    {
        ring.gameObject.SetActive(ballData.Value >= 1025);
        image.sprite = GlobalBallData.Instance.GetSpriteByValue(ballData.Value.FloorPower2());
        // image.color = GlobalBallData.Instance.GetColorByValue(ballData.Value.FloorPower2());
        // innerImage.color = GlobalBallData.Instance.GetColorByValue(ballData.Value.FloorPower2());
        innerImage.sprite = GlobalBallData.Instance.GetSpriteByValue(ballData.Value.FloorPower2());
    }
    public async Task OnGridChanged()
    {
        List<BallTransform> BallsRemoved = new();
        BallTransform currentPositionCheck = new() { x = ballData.BallTransform.x, y = ballData.BallTransform.y };
        BallData newBallData = new()
        {
            BallTransform = new() { x = currentPositionCheck.x, y = currentPositionCheck.y },
            Value = ballData.Value,
            Color = ballData.Color,
        };
        bool changed = false;
        while (!GridInfo.Instance.BallExistsBelow(currentPositionCheck.x, currentPositionCheck.y))
        {
            BallsRemoved.Add(ballData.BallTransform);
            newBallData = new()
            {
                BallTransform = new() { x = currentPositionCheck.x, y = currentPositionCheck.y - 1 },
                Value = ballData.Value,
                Color = ballData.Color,
            };
            currentPositionCheck = new() { x = currentPositionCheck.x, y = currentPositionCheck.y - 1 };
            changed = true;
        }

        if (changed)
        {
            Config(newBallData, gridManager, ballSpawner, false);
            ballSpawner.AddBall(this);
            await ballSpawner.OnGridChanged(BallsRemoved, null);
        }
    }

    private void Update()
    {
        if (IsPreviewBall)
            return;

        if (gridManager.TouchIsOn && Hovering)
        {
            gridManager.OnPointerEntered(this, GetComponent<RectTransform>());
        }

    }

    public void ScaleUp()
    {
        transform.DOScale(originalScale.x + 0.1f, 0.1f);
    }

    public void ScaleDown()
    {
        transform.DOScale(originalScale.x, 0.1f);
    }

    public async Task PopAfterRelease(Ball lastBall)
    {
        transform.localScale = originalScale / 2;
        transform.position = ballSpawner.GetPosition(ballData.BallTransform.x, ballData.BallTransform.y);
        transform.DOScale(originalScale.x + 0.3f, 0.03333f);
        lastBall.transform.DOScale(lastBall.transform.localScale / 2, 0.03333f);
        lastBall.Disable();
        await Task.Delay(33);
        transform.DOScale(originalScale.x, 0.05f);
        await Task.Delay(50);
        AnimationDone = true;
    }
    public async void PopAnimation()
    {
        transform.localScale = Vector2.zero;

        if (ballData.BallTransform.y > 0)
        {
            bool shouldWait = true;

            while (shouldWait)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (ballSpawner.GetBall(i, ballData.BallTransform.y - 1) == null)
                        continue;
                    if (!ballSpawner.GetBall(i, ballData.BallTransform.y - 1).FallDone)
                    {
                        shouldWait = true;
                        break;
                    }

                    shouldWait = false;
                }
                await Task.Yield();
            }
        }

        transform.position = ballSpawner.GetPosition(ballData.BallTransform.x, ballData.BallTransform.y);
        transform.DOScale(originalScale, 0.15f);
        AnimationDone = true;
    }

    public async void FallAnimation()
    {
        if (ballData.BallTransform.y == 4)
            return;

        FallDone = false;
        var position = ballSpawner.GetPosition(ballData.BallTransform.x, ballData.BallTransform.y);
        transform.DOMove(position, 0.15f);

        await Task.Delay(150);

        DoSquash();
        AnimationDone = true;
        FallDone = true;
    }

    private async void DoSquash()
    {
        transform.DOScaleY(0.8f * originalScale.x, 0.05f);
        await Task.Delay(50);
        transform.DOScaleY(originalScale.x, 0.1f);
        await Task.Delay(100);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        Hovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Hovering = false;
    }


    public bool BallHasNeighbour()
    {
        for (int i = ballData.BallTransform.x - 1; i <= ballData.BallTransform.x + 1; i++)
        {
            if (i < 0)
                continue;
            for (int j = ballData.BallTransform.y - 1; j <= ballData.BallTransform.y + 1; j++)
            {
                if (j < 0)
                    continue;

                if (i == ballData.BallTransform.x && j == ballData.BallTransform.y)
                    continue;

                if (GridInfo.Instance.GetBallData(i, j) == null)
                    return false;
                if (GridInfo.Instance.GetBallData(i, j).Value == ballData.Value)
                    return true;
            }
        }

        return false;
    }

    public BallData GetData()
    {
        return ballData;
    }

    public async void GoToTarget(int x, int y)
    {
        var position = ballSpawner.GetPosition(x, y);
        transform.DOMove(position, 0.13333f);
        await Task.Delay(133);
        Disable();
    }

    public void Disable()
    {
        gameObject.SetActive(false);
    }
}

[System.Serializable]
public class BallTransform
{
    public int y;
    public int x;

    public bool Equals(BallTransform obj)
    {
        return obj.x == x && obj.y == y;
    }
}

[System.Serializable]
public class BallData
{
    public int Value;
    public BallTransform BallTransform;

    public Color Color;

    public Sprite sprite;

}


