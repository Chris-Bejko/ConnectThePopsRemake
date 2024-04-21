using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class StartPanel : MonoBehaviour
{
    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private Transform tap2PlayText;

    private void Awake()
    {
        AnimateText();
    }
    private async void AnimateText()
    {
        while (gameObject.activeInHierarchy)
        {
            tap2PlayText.DOBlendableLocalMoveBy(new Vector3(0, 20, 0), 0.4f);
            await Task.Delay(400);
            tap2PlayText.DOBlendableLocalMoveBy(new Vector3(0, -20, 0), 0.4f);
            await Task.Delay(400);
        }
    }
    public async void Remove()
    {
        audioSource.Play();
        transform.DOMoveX(transform.position.x + 1200, 0.3f);
        await Task.Delay(300);
        gameObject.SetActive(false);
    }

}
