using System.Collections.Generic;
using UnityEngine;

public class FloatingBlock : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private List<Vector3> _spawnPos;

    [HideInInspector]
    public int ColorId;

    [HideInInspector]
    public FloatingBlock NextBlock;

    private bool _hasGameFinished;

    private void Awake() 
    {
        _hasGameFinished = false;
        transform.position = _spawnPos[Random.Range(0, _spawnPos.Count)];
        int colorCount = GameplayManager.Instance.Colors.Count;
        ColorId = Random.Range(0, colorCount);

        GetComponent<SpriteRenderer>().color = GameplayManager.Instance.Colors[ColorId];
    }

    private void FixedUpdate() 
    {
        if (_hasGameFinished) return;
        transform.Translate(_moveSpeed * Time.fixedDeltaTime * Vector3.down);    
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("Obstacle"))
        {
            GameplayManager.Instance.TriggerGameOver();
        }
    }

    private void OnEnable() 
    {
        GameplayManager.Instance.GameOver += GameOver; // public UnityAction GameOver trong GameplayManager.cs;
    }

    private void OnDisable() 
    {
        GameplayManager.Instance.GameOver -= GameOver; // public UnityAction GameOver trong GameplayManager.cs;
    }

    private void GameOver()
    {
        _hasGameFinished = true;
        // GameplayManager.Instance.TriggerGameOver();
    }
}
