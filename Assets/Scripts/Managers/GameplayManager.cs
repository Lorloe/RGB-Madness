using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GameplayManager : MonoBehaviour
{
    #region START

    [SerializeField] private ColorList _colorList;
    public List<Color> Colors => _colorList.Colors;

    private bool _hasGameFinished;
    public static GameplayManager Instance;

    // public List<Color> Colors;

    private void Awake()
    {
        Instance = this;

        // Debug.Log("GameManager.Instance null? " + (GameManager.Instance == null));
        // Debug.Log("scoreText null? " + (_scoreText == null));

        _hasGameFinished = false;
        GameManager.Instance.IsInitialized = true;

        _score = 0;
        _scoreText.text = ((int)_score).ToString(); // ép kiểu float về int

       StartCoroutine(SpawnBlock()); 
    }    

    #endregion

    #region SCORE

    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private AudioClip _pointClip;

    private float _score;

    private void UpdateSCoreText()
    {
        _score++;
        SoundManager.Instance.PlaySound(_pointClip);
        _scoreText.text = ((int)_score).ToString(); // ép kiểu float về int
    }

    #endregion

    #region BLOCKS
    
    [SerializeField] private float _spawnTime;
    [SerializeField] private FloatingBlock _floatingBlockPrefab;

    private FloatingBlock _currentBlock;

    private IEnumerator SpawnBlock()
    {
        FloatingBlock _prevBlock = null;
        while (!_hasGameFinished)
        {
            var tempBlock = Instantiate(_floatingBlockPrefab, transform.position, Quaternion.identity);

            if (_prevBlock == null)
            {
                _currentBlock = tempBlock;
                _prevBlock = tempBlock;
            }
            else
            {
                _prevBlock.NextBlock = tempBlock;
                _prevBlock = tempBlock;
            }

            yield return new WaitForSeconds(_spawnTime);
        }
    }
 
    #endregion

    #region GAME_LOGIC



    #endregion

    #region GAME_OVER

    [SerializeField] private AudioClip _gameOverClip;

    public UnityAction GameOver;

    public void TriggerGameOver()
    {
        GameOver?.Invoke();
        SoundManager.Instance.PlaySound(_gameOverClip);
        _hasGameFinished = true;
        GameManager.Instance.CurrentScore = (int)_score;
        StartCoroutine(HandleGameOver());
    }

    private IEnumerator HandleGameOver()
    {
        yield return new WaitForSeconds(3f);
        GameManager.Instance.GoToMainMenu();
    }
    
    #endregion
}
