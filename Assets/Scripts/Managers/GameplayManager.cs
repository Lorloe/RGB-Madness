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
    [SerializeField] private float _minLongBlockChance = 0.12f;
    [SerializeField] private float _maxLongBlockChance = 0.24f;
    [SerializeField] private int _minNormalBlocksBetweenLong = 2;
    [SerializeField] private int _maxNormalBlocksBetweenLong = 4;
    [SerializeField] private float _longBlockMinHoldDuration = 0.45f;
    [SerializeField] private float _longBlockMaxHoldDuration = 0.95f;
    [SerializeField] private float _longBlockMinLengthMultiplier = 1.7f;
    [SerializeField] private float _longBlockMaxLengthMultiplier = 2.5f;
    [SerializeField] private FloatingBlock _floatingBlockPrefab;
    [SerializeField] private BlockEffect _blockEffect;
    [SerializeField] private AudioClip _gameOverClip;

    public static GameplayManager Instance;

    public UnityAction GameOver;

    private FloatingBlock _currentBlock;
    private FloatingBlock _activeHoldBlock;

    private float _score;
    private float _elapsedGameplayTime;
    private float _currentHoldTime;
    private bool _hasGameFinished;
    private bool _isHoldingLongBlock;
    private int _displayedDifficultyLevel;
    private int _normalBlocksBeforeNextLongAllowed;

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
        _isHoldingLongBlock = false;
        GameManager.Instance.IsInitialized = true;

        _score = 0f;
        _elapsedGameplayTime = 0f;
        _currentHoldTime = 0f;
        _displayedDifficultyLevel = 0;
        _normalBlocksBeforeNextLongAllowed = GetRandomLongBlockSpacing();

        UpdateScoreText();
        UpdateDifficultyText(true);
        StartCoroutine(SpawnBlock());
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
            ConfigureSpawnedBlock(tempBlock);

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

    private void ConfigureSpawnedBlock(FloatingBlock block)
    {
        if (_normalBlocksBeforeNextLongAllowed > 0)
        {
            _normalBlocksBeforeNextLongAllowed--;
            block.InitializeLongBlock(false, 0f, 1f);
            return;
        }

        float longBlockChance = Random.Range(
            Mathf.Clamp01(Mathf.Min(_minLongBlockChance, _maxLongBlockChance)),
            Mathf.Clamp01(Mathf.Max(_minLongBlockChance, _maxLongBlockChance)));

        if (Random.value > longBlockChance)
        {
            block.InitializeLongBlock(false, 0f, 1f);
            return;
        }

        float holdT = Random.value;
        float holdDuration = Mathf.Lerp(_longBlockMinHoldDuration, _longBlockMaxHoldDuration, holdT);
        float lengthMultiplier = Mathf.Lerp(_longBlockMinLengthMultiplier, _longBlockMaxLengthMultiplier, holdT);
        block.InitializeLongBlock(true, holdDuration, lengthMultiplier);
        _normalBlocksBeforeNextLongAllowed = GetRandomLongBlockSpacing();
    }

    private int GetRandomLongBlockSpacing()
    {
        int minSpacing = Mathf.Max(0, Mathf.Min(_minNormalBlocksBetweenLong, _maxNormalBlocksBetweenLong));
        int maxSpacing = Mathf.Max(minSpacing, Mathf.Max(_minNormalBlocksBetweenLong, _maxNormalBlocksBetweenLong));
        return Random.Range(minSpacing, maxSpacing + 1);
    }

    private void ResolveCurrentBlock()
    {
        if (_currentBlock == null)
        {
            TriggerGameOver();
            return;
        }

        BlockEffect effect = Instantiate(_blockEffect, _currentBlock.transform.position, Quaternion.identity);
        effect.Initialize(Colors[_currentBlock.ColorId]);

        FloatingBlock clearedBlock = _currentBlock;
        _currentBlock = _currentBlock.NextBlock;

        if (_activeHoldBlock == clearedBlock)
        {
            ResetHoldState();
        }

        Destroy(clearedBlock.gameObject);
        IncreaseScore();
    }

    #endregion

    #region INPUT

    private void Update()
    {
        if (_hasGameFinished)
        {
            return;
        }

        _elapsedGameplayTime += Time.deltaTime;
        UpdateDifficultyText();

        if (_isHoldingLongBlock)
        {
            HandleLongBlockHold();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            HandlePressStarted();
        }
    }

    private void HandlePressStarted()
    {
        if (_currentBlock == null)
        {
            TriggerGameOver();
            return;
        }

        if (!TryGetHoveredButton(out BlockButton hoveredButton))
        {
            TriggerGameOver();
            return;
        }

        if (hoveredButton.ColorId != _currentBlock.ColorId)
        {
            TriggerGameOver();
            return;
        }

        if (!_currentBlock.IsLongBlock)
        {
            ResolveCurrentBlock();
            return;
        }

        _isHoldingLongBlock = true;
        _activeHoldBlock = _currentBlock;
        _currentHoldTime = 0f;
        _activeHoldBlock.UpdateHoldProgress(0f);
    }

    private void HandleLongBlockHold()
    {
        if (_activeHoldBlock == null || _activeHoldBlock != _currentBlock)
        {
            ResetHoldState();
            return;
        }

        if (Input.GetMouseButtonUp(0))
        {
            TriggerGameOver();
            return;
        }

        if (!Input.GetMouseButton(0))
        {
            return;
        }

        if (!TryGetHoveredButton(out BlockButton hoveredButton) || hoveredButton.ColorId != _activeHoldBlock.ColorId)
        {
            TriggerGameOver();
            return;
        }

        _currentHoldTime += Time.deltaTime;
        _activeHoldBlock.UpdateHoldProgress(_currentHoldTime / _activeHoldBlock.RequiredHoldDuration);

        if (_currentHoldTime >= _activeHoldBlock.RequiredHoldDuration)
        {
            ResolveCurrentBlock();
        }
    }

    private bool TryGetHoveredButton(out BlockButton blockButton)
    {
        blockButton = null;

        if (Camera.main == null)
        {
            return false;
        }

        Vector3 pointerPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 pointerPosition2D = new Vector2(pointerPosition.x, pointerPosition.y);
        RaycastHit2D hit = Physics2D.Raycast(pointerPosition2D, Vector2.zero);

        if (!hit.collider || hit.collider.CompareTag("Obstacle"))
        {
            return false;
        }

        return hit.collider.TryGetComponent(out blockButton);
    }

    private void ResetHoldState()
    {
        if (_activeHoldBlock != null)
        {
            _activeHoldBlock.UpdateHoldProgress(0f);
        }

        _isHoldingLongBlock = false;
        _activeHoldBlock = null;
        _currentHoldTime = 0f;
    }

    #endregion

    #region GAME_OVER

    public void TriggerGameOver()
    {
        if (_hasGameFinished)
        {
            return;
        }

        ResetHoldState();
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
