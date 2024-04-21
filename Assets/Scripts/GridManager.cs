using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class GridManager : MonoBehaviour
{
    [SerializeField]
    private Ball PreviewBall;


    [SerializeField]
    private float yOffset;

    [SerializeField]
    private UILineRenderer LineRenderer;

    [SerializeField]
    private BallSpawner ballSpawner;


    [SerializeField]
    private AudioClip click;

    [SerializeField]
    private AudioClip[] popSounds;

    [SerializeField]
    private AudioSource audioSource;

    private List<Ball> SelectedBalls = new();
    private bool Hovered;
    private BallData previewBallData;
    private List<Vector2> Points = new();

    public bool TouchIsOn;

    public void OnPointerEntered(Ball ball, RectTransform transform)
    {
        if (!TouchIsOn)
            return;

        Hovered = true;
        var ballData = ball.GetData();
        //First case, no balls selected
        if (SelectedBalls.Count == 0)
        {
            SelectedBalls.Add(ball);
            previewBallData = new()
            {
                Value = ballData.Value,
                BallTransform = new() { x = -1, y = -1 },
                Color = ballData.Color
            };
            LineRenderer.color = ballData.Color;
            RemoveAllLines();
            ball.ScaleUp();
            PlayClickSound();
        }
        //Second case, player tries to deselect the last ball
        else if (SelectedBalls.ContainsBall(ball) && !SelectedBalls[^1].GetData().Equals(ballData))
        {
            if (!SelectedBalls[^2].GetData().Equals(ballData))
                return;

            var lastSelectedBall = SelectedBalls[SelectedBalls.Count - 1].GetData();
            if (IsNeighbour(ballData.BallTransform, lastSelectedBall.BallTransform))
            {
                RemoveLastLine();
                previewBallData.Value -= lastSelectedBall.Value;
                ball.ScaleUp();
                SelectedBalls[SelectedBalls.Count - 1].ScaleDown();
                SelectedBalls.RemoveAt(SelectedBalls.Count - 1);
                PlayClickSound();
            }
        }
        else if (!SelectedBalls.ContainsBall(ball) && CanBallBeSelected(ballData, SelectedBalls[SelectedBalls.Count - 1].GetData()))
        {
            DrawLine(SelectedBalls[^1].GetComponent<RectTransform>(), transform);
            SelectedBalls.Add(ball);
            var lastBallSelected = SelectedBalls[SelectedBalls.Count - 1].GetData();
            ball.ScaleUp();
            previewBallData.Value += lastBallSelected.Value;
            PlayClickSound();
        }
        PreviewBall.Config(previewBallData, this, ballSpawner);
    }

    private void PlayClickSound()
    {
        audioSource.clip = click;
        audioSource.pitch = 1 + SelectedBalls.Count * 0.1f;
        audioSource.Play();
        Haptics.HapticFeedback("DRAG_START");
    }

    private void PlayPopSound()
    {
        audioSource.clip = popSounds[Random.Range(0, popSounds.Length)];
        audioSource.pitch = 1;
        audioSource.Play();
    }
    public float GetPitch()
    {
        return SelectedBalls.Count * 0.1f;
    }
    private void Update()
    {
        TouchIsOn = Input.GetMouseButton(0);
        PreviewBall.gameObject.SetActive(TouchIsOn && Hovered);
        if (!TouchIsOn)
        {
            CheckRelease();
            Hovered = false;
            foreach (var e in SelectedBalls)
            {
                e.ScaleDown();
            }
            SelectedBalls.Clear();

            RemoveAllLines();
        }
    }


    private async void CheckRelease()
    {
        if (SelectedBalls.Count <= 1)
            return;


        Haptics.HapticFeedback("LONG_PRESS");
        var position = SelectedBalls.Last().GetData().BallTransform;
        List<BallTransform> ballsThatWillBeRemoved = new();
        Ball lastBall = null;

        BallData newBallData = new BallData
        {
            Value = previewBallData.Value,
            BallTransform = position,
            Color = previewBallData.Color
        };
        foreach (var e in SelectedBalls)
        {
            if (e.GetData().BallTransform.Equals(position))
            {
                lastBall = e;
                continue;
            }
            e.GoToTarget(position.x, position.y);
            ballsThatWillBeRemoved.Add(e.GetData().BallTransform);
        }

        await Task.Delay(133);
        var ball = await ballSpawner.InstantiateBall(newBallData, true, true);
        PlayPopSound();
        await ball.PopAfterRelease(lastBall);
        SelectedBalls.Clear();
        await ballSpawner.OnGridChanged(ballsThatWillBeRemoved, ball);
    }
    private bool CanBallBeSelected(BallData thisBall, BallData previousBall)
    {
        return IsNeighbour(thisBall.BallTransform, previousBall.BallTransform) &&
        thisBall.Value == previousBall.Value &&
        !thisBall.BallTransform.Equals(previousBall.BallTransform);
    }


    private bool IsNeighbour(BallTransform thisBall, BallTransform previousBall)
    {
        return Mathf.Abs(previousBall.x - thisBall.x) <= 1 && Mathf.Abs(previousBall.y - thisBall.y) <= 1;
    }

    private void DrawLine(RectTransform start, RectTransform end)
    {
        if (Points.Contains(start.localPosition) && Points.Contains(end.localPosition))
            return;

        Vector2 startPos = new(start.localPosition.x, start.localPosition.y + yOffset);
        Vector2 endPos = new(end.localPosition.x, end.localPosition.y + yOffset);
        Points.Add(startPos);
        Points.Add(endPos);
        LineRenderer.Points = Points.ToArray();
    }

    private void RemoveLastLine()
    {
        Points.RemoveAt(Points.Count - 1);

        if (Points.Count > 1)
            Points.RemoveAt(Points.Count - 2);
        LineRenderer.Points = Points.ToArray();
    }

    private void RemoveAllLines()
    {
        Points = new();
        LineRenderer.Points = Points.ToArray();
    }
}
