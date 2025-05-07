using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GameplayManager : MonoBehaviour
{
    #region VARIABLES
    
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private AudioClip _pointClip;
    [SerializeField] private ColorList _colorList;
    [SerializeField] private float _spawnTime;
    [SerializeField] private FloatingBlock _floatingBlockPrefab;
    [SerializeField] private BlockEffect _blockEffect;
    [SerializeField] private AudioClip _gameOverClip;

    public static GameplayManager Instance;

    public UnityAction GameOver;
    
    private FloatingBlock _currentBlock;
    
    private float _score;
    private bool _hasGameFinished;

    public List<Color> Colors => _colorList.Colors;
    // public List<Color> Colors;
    
    #endregion

    #region START

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

    private void UpdateSCoreText()
    {
        _score++;
        SoundManager.Instance.PlaySound(_pointClip);
        _scoreText.text = ((int)_score).ToString(); // ép kiểu float về int
    }

    #endregion

    #region BLOCKS

    private IEnumerator SpawnBlock()
    {
        FloatingBlock prevBlock = null;
        while (!_hasGameFinished)
        {
            var tempBlock = Instantiate(_floatingBlockPrefab, transform.position, Quaternion.identity);

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

            yield return new WaitForSeconds(_spawnTime);
        }
    }
 
    #endregion

    #region GAME_LOGIC

    private void Update() 
    {
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
            
            var t = Instantiate(_blockEffect, _currentBlock.gameObject.transform.position, Quaternion.identity);
            t.Initialize(Colors[currentBlockId]);

            var tempBlock = _currentBlock;

            if (_currentBlock.NextBlock != null)
            {
                _currentBlock = _currentBlock.NextBlock;
            }
            Destroy(tempBlock.gameObject);
            UpdateSCoreText();
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
