using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;
using Unity.Android.Types;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class GameView : SingletonBehaviour<GameView>
{
    [Header("pages")]
    [SerializeField] RectTransform IngameRect;
    [SerializeField] RectTransform EndgameRect;

    [Header("Ingame")]    
    [SerializeField] Button _captureButton;
    [SerializeField] Button _pauseButton;

    [Space(10)]
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI lifeText;
    [SerializeField] TextMeshProUGUI judgeText;

    [Space(10)]
    [SerializeField] Slider lifeSlider_left;
    [SerializeField] Slider lifeSlider_right;

    [Space(10)]
    [Tooltip("텍스트가 페이드아웃 되는데 걸리는 시간")]
    [SerializeField] float judgeFadeTime = 1f;


    [Header("Endgame")]
    [SerializeField] TextMeshProUGUI endGameScoreText;
    [SerializeField] Button retryButton;
    [SerializeField] Button homeButton;

    public Button CaptureButton => _captureButton;
    public Button RetryButton => retryButton;
    public Button HomeButton => homeButton;
    private int maxLife;

    public void SetMaxLife(int maxLife)
    {
        this.maxLife = maxLife;
    }

    public void SetLife(float cur)
    {
        lifeText.text = $"{((int)cur)} / {maxLife}";
        lifeSlider_right.value = cur / maxLife;
        lifeSlider_left.value = cur / maxLife;
    }

    public void SetScore(int score)
    {
        scoreText.text = score.ToString();
    }

    private async UniTask FadeJudgeText(CancellationToken token)
    {
        float alpha = 1f;
        while(alpha > 0f && !token.IsCancellationRequested)
        {
            await UniTask.Yield();
            alpha -= judgeFadeTime * Time.deltaTime;
            judgeText.alpha = alpha;
        }
    }

    public void SetGame(bool isGameEnd)
    {
        IngameRect.gameObject.SetActive(!isGameEnd);
        EndgameRect.gameObject.SetActive(isGameEnd);
    }

    public void SetEndGameRect(int score)
    {
        endGameScoreText.text = $"SCORE : {score}";
    }
}
