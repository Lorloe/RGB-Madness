using System.Collections.Generic;
using UnityEngine;

public class FloatingBlock : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private List<Vector3> _spawnPos;
    [SerializeField] private float _holdTintStrength = 0.35f;

    [HideInInspector] public int ColorId;
    [HideInInspector] public FloatingBlock NextBlock;

    private SpriteRenderer _spriteRenderer;
    private Vector3 _baseScale;
    private Color _baseColor;
    private bool _hasGameFinished;

    public bool IsLongBlock { get; private set; }
    public float RequiredHoldDuration { get; private set; }

    private void Awake()
    {
        _hasGameFinished = false;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _baseScale = transform.localScale;
        int colorCount = GameplayManager.Instance.Colors.Count;
        ColorId = Random.Range(0, colorCount);

        transform.position = _spawnPos[Random.Range(0, _spawnPos.Count)];

        _baseColor = GameplayManager.Instance.Colors[ColorId];
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = _baseColor;
        }
    }

    public void InitializeLongBlock(bool isLongBlock, float holdDuration, float lengthMultiplier)
    {
        IsLongBlock = isLongBlock;
        RequiredHoldDuration = isLongBlock ? Mathf.Max(0.1f, holdDuration) : 0f;

        float clampedLengthMultiplier = Mathf.Max(1f, lengthMultiplier);
        transform.localScale = new Vector3(_baseScale.x, _baseScale.y * clampedLengthMultiplier, _baseScale.z);

        UpdateHoldProgress(0f);
    }

    public void UpdateHoldProgress(float progress01)
    {
        if (_spriteRenderer == null)
        {
            return;
        }

        if (!IsLongBlock)
        {
            _spriteRenderer.color = _baseColor;
            return;
        }

        float normalizedProgress = Mathf.Clamp01(progress01);
        _spriteRenderer.color = Color.Lerp(_baseColor, Color.white, normalizedProgress * _holdTintStrength);
    }

    private void FixedUpdate()
    {
        if (_hasGameFinished)
        {
            return;
        }

        transform.Translate(_moveSpeed * GameplayManager.Instance.CurrentBlockSpeedMultiplier * Time.fixedDeltaTime * Vector3.down);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle"))
        {
            GameplayManager.Instance.TriggerGameOver();
        }
    }

    private void OnMouseDown()
    {
        if (GameplayManager.Instance != null)
        {
            GameplayManager.Instance.HandleTapConnectClick(this);
        }
    }

    private void OnEnable()
    {
        GameplayManager.Instance.GameOver += GameOver;
    }

    private void OnDisable()
    {
        if (GameplayManager.Instance != null)
        {
            GameplayManager.Instance.GameOver -= GameOver;
        }
    }

    private void GameOver()
    {
        _hasGameFinished = true;
    }
}
