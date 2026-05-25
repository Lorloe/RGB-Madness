using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class GameplayManager : MonoBehaviour
{
    private static readonly string[] GameplayLaneActionNames = { "A", "S", "D", "J", "K", "L" };

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
    private readonly InputAction[] _laneActions = new InputAction[6];
    private InputActionMap _playerActionMap;

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

        InitializeInputActions();
        UpdateScoreText();
        UpdateDifficultyText(true);
        StartCoroutine(SpawnBlock());
    }

    private void OnEnable()
    {
        _playerActionMap?.Enable();
    }

    private void OnDisable()
    {
        _playerActionMap?.Disable();
    }

    private void InitializeInputActions()
    {
        InputActionAsset projectWideActions = InputSystem.actions;
        if (projectWideActions == null)
        {
            Debug.LogError("Project-wide Input System actions are not configured.");
            return;
        }

        _playerActionMap = projectWideActions.FindActionMap("Player", false);
        if (_playerActionMap == null)
        {
            Debug.LogError("Input action map 'Player' was not found in project-wide actions.");
            return;
        }

        for (int i = 0; i < GameplayLaneActionNames.Length; i++)
        {
            _laneActions[i] = _playerActionMap.FindAction(GameplayLaneActionNames[i], false);
            if (_laneActions[i] == null)
            {
                Debug.LogError($"Input action '{GameplayLaneActionNames[i]}' was not found in map 'Player'.");
            }
        }

        _playerActionMap.Enable();
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

        // Old touch/raycast input was replaced by project-wide Input System actions.
        if (TryGetPressedLane(out int laneIndex))
        {
            HandlePressStarted(laneIndex);
        }
    }

    private void HandlePressStarted(int laneIndex)
    {
        if (_currentBlock == null)
        {
            TriggerGameOver();
            return;
        }

        if (laneIndex != _currentBlock.ColorId)
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

        int expectedLaneIndex = _activeHoldBlock.ColorId;
        if (expectedLaneIndex < 0 || expectedLaneIndex >= _laneActions.Length)
        {
            TriggerGameOver();
            return;
        }

        if (WasWrongLanePressed(expectedLaneIndex))
        {
            TriggerGameOver();
            return;
        }

        InputAction expectedLaneAction = _laneActions[expectedLaneIndex];
        if (expectedLaneAction == null || expectedLaneAction.WasReleasedThisFrame())
        {
            TriggerGameOver();
            return;
        }

        if (!expectedLaneAction.IsPressed())
        {
            return;
        }

        _currentHoldTime += Time.deltaTime;
        _activeHoldBlock.UpdateHoldProgress(_currentHoldTime / _activeHoldBlock.RequiredHoldDuration);

        if (_currentHoldTime >= _activeHoldBlock.RequiredHoldDuration)
        {
            ResolveCurrentBlock();
        }
    }

    private bool TryGetPressedLane(out int laneIndex)
    {
        for (int i = 0; i < _laneActions.Length; i++)
        {
            InputAction laneAction = _laneActions[i];
            if (laneAction != null && laneAction.WasPressedThisFrame())
            {
                laneIndex = i;
                return true;
            }
        }

        laneIndex = -1;
        return false;
    }

    private bool WasWrongLanePressed(int expectedLaneIndex)
    {
        for (int i = 0; i < _laneActions.Length; i++)
        {
            if (i == expectedLaneIndex)
            {
                continue;
            }

            InputAction laneAction = _laneActions[i];
            if (laneAction != null && laneAction.WasPressedThisFrame())
            {
                return true;
            }
        }

        return false;
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
