using System.Collections;
using TMPro;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private TMP_Text _highScoreText;
    [SerializeField] private TMP_Text _newScoreText;
    [SerializeField] private AudioClip _clickedClip;
    [SerializeField] private float _animationTime = 1f;
    [SerializeField] private AnimationCurve _speedCurve;

    private void Awake() 
    {
        if (GameManager.Instance != null)
        {
            StartCoroutine(ShowScore());
        }
        else
        {
            _newScoreText.gameObject.SetActive(false);
            _scoreText.gameObject.SetActive(false);
            _highScoreText.text = GameManager.Instance.HighScore.ToString();
        }
    }

    // public để có thể gọi từ Unity Event OnClick trong Button 
    public void ClickedPlay()
    {
        SoundManager.Instance.PlaySound(_clickedClip);
        GameManager.Instance.GoToGamePlay();
    }

    private IEnumerator ShowScore()
    {
        int tempScore = 0;
        _scoreText.text = tempScore.ToString();

        int currentScore = GameManager.Instance.CurrentScore;
        int highScore = GameManager.Instance.HighScore;

        if (currentScore > highScore)
        {
            _newScoreText.gameObject.SetActive(true);
            GameManager.Instance.HighScore = currentScore;
        }
        else
        {
            _newScoreText.gameObject.SetActive(false);
        }
        _highScoreText.text = GameManager.Instance.HighScore.ToString();

        float speed = 1 / _animationTime;
        float timeElapsed = 0f;

        while (timeElapsed < _animationTime)
        {
            timeElapsed += speed * Time.deltaTime;
            tempScore = (int)(_speedCurve.Evaluate(timeElapsed) * currentScore);
            _scoreText.text = tempScore.ToString();
            yield return null;
        }

        tempScore = currentScore;
        _scoreText.text = tempScore.ToString();
    }
}
