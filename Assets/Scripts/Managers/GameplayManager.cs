using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GameplayManager : MonoBehaviour
{
    #region VARIABLES

    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private TMP_Text _difficultyText;
    [SerializeField] private AudioClip _pointClip;
    [SerializeField] private ColorList _colorList;
    [SerializeField] private float _spawnTime;
    [SerializeField] private float _difficultyLevelDuration = 12f;
    [SerializeField] private float _blockSpeedIncreasePerSecond = 0.0125f;
    [SerializeField] private float _maxBlockSpeedMultiplier = 2.2f;
    [SerializeField] private float _spawnTimeReductionPerSecond = 0.012f;
    [SerializeField] private float _minSpawnTime = 0.35f;
    [SerializeField] private FloatingBlock _floatingBlockPrefab;
    [SerializeField] private BlockEffect _blockEffect;
    [SerializeField] private AudioClip _gameOverClip;

    public static GameplayManager Instance;

    public UnityAction GameOver;

    private FloatingBlock _currentBlock;

    private float _score;
    private float _elapsedGameplayTime;
    private bool _hasGameFinished;
    private int _displayedDifficultyLevel;

    public List<Color> Colors => _colorList.Colors;

    public int CurrentDifficultyLevel
    {
        get
        {
            if (_difficultyLevelDuration <= 0f)
            {
                return 1;
            }

            return 1 + Mathf.FloorToInt(_elapsedGameplayTime / _difficultyLevelDuration);
        }
    }

    public float CurrentBlockSpeedMultiplier
    {
        get
        {
            float speedMultiplier = 1f + (_elapsedGameplayTime * _blockSpeedIncreasePerSecond);
            return Mathf.Min(speedMultiplier, _maxBlockSpeedMultiplier);
        }
    }

    public float CurrentSpawnInterval
    {
        get
        {
            float spawnInterval = _spawnTime - (_elapsedGameplayTime * _spawnTimeReductionPerSecond);
            return Mathf.Max(_minSpawnTime, spawnInterval);
        }
    }

    #endregion

    #region START

    private void Awake()
    {
        Instance = this;
        _hasGameFinished = false;
        GameManager.Instance.IsInitialized = true;

        _score = 0f;
        _elapsedGameplayTime = 0f;
        _displayedDifficultyLevel = 0;

        ResolveDifficultyText();
        UpdateScoreText();
        UpdateDifficultyText(true);

        StartCoroutine(SpawnBlock());
    }

    private void ResolveDifficultyText()
    {
        if (_difficultyText != null)
        {
            return;
        }

        GameObject levelTextObject = GameObject.Find("LevelText");
        if (levelTextObject != null)
        {
            _difficultyText = levelTextObject.GetComponent<TMP_Text>();
        }
    }

    #endregion

    #region UI

    private void UpdateScoreText()
    {
        if (_scoreText != null)
        {
            _scoreText.text = ((int)_score).ToString();
        }
    }

    private void UpdateDifficultyText(bool force = false)
    {
        if (_difficultyText == null)
        {
            return;
        }

        int currentDifficultyLevel = CurrentDifficultyLevel;
        if (!force && currentDifficultyLevel == _displayedDifficultyLevel)
        {
            return;
        }

        _displayedDifficultyLevel = currentDifficultyLevel;
        _difficultyText.text = $"LEVEL {currentDifficultyLevel}";
    }

    #endregion

    #region SCORE

    private void IncreaseScore()
    {
        _score++;
        SoundManager.Instance.PlaySound(_pointClip);
        UpdateScoreText();
    }

    #endregion

    #region BLOCKS

    private IEnumerator SpawnBlock()
    {
        FloatingBlock prevBlock = null;
        while (!_hasGameFinished)
        {
            FloatingBlock tempBlock = Instantiate(_floatingBlockPrefab, transform.position, Quaternion.identity);

            if (prevBlock == null)
            {
                _currentBlock = tempBlock;
                prevBlock = tempBlock;
            }
            else
            {
                prevBlock.NextBlock = tempBlock;
                prevBlock = tempBlock;
            }

            yield return new WaitForSeconds(CurrentSpawnInterval);
        }
    }

    #endregion

    #region GAME_LOGIC

    private void Update()
    {
        if (!_hasGameFinished)
        {
            _elapsedGameplayTime += Time.deltaTime;
            UpdateDifficultyText();
        }

        if (Input.GetMouseButtonDown(0) && !_hasGameFinished)
        {
            if (_currentBlock == null)
            {
                TriggerGameOver();
                return;
            }

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            if (!hit.collider || hit.collider.CompareTag("Obstacle"))
            {
                TriggerGameOver();
                return;
            }

            int currentBlockId = _currentBlock.ColorId;
            int clickedBlockId = hit.collider.gameObject.GetComponent<BlockButton>().ColorId;

            if (currentBlockId != clickedBlockId)
            {
                TriggerGameOver();
                return;
            }

            BlockEffect effect = Instantiate(_blockEffect, _currentBlock.transform.position, Quaternion.identity);
            effect.Initialize(Colors[currentBlockId]);

            FloatingBlock tempBlock = _currentBlock;
            if (_currentBlock.NextBlock != null)
            {
                _currentBlock = _currentBlock.NextBlock;
            }

            Destroy(tempBlock.gameObject);
            IncreaseScore();
        }
    }

    #endregion

    #region GAME_OVER

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
