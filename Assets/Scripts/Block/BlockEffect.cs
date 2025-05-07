using System.Collections;
using UnityEngine;

public class BlockEffect : MonoBehaviour
{
    [SerializeField] private float _destroyTime;

    private SpriteRenderer _spriteRenderer;
    
    private Color _currentColor;
    private Color _startColor;
    private Color _endColor;
    private Color _colorOffset;

    private Vector3 _startScale;
    private Vector3 _endScale;
    private Vector3 _scaleOffset;
    private Vector3 _currentScale;

    private void Awake() 
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Color color)
    {
        _currentColor = color;
        StartCoroutine(ScaleEffect());
        StartCoroutine(FadeEffect());
    }

    private IEnumerator ScaleEffect()
    {
        float timeElapsed = 0f;

        _startScale = Vector3.one * 0.6f;
        _endScale = Vector3.one * 1.2f;
        _scaleOffset = _endScale - _startScale;

        _currentScale = _startScale;
        transform.localScale = _currentScale;

        while (timeElapsed < 1f)
        {
            timeElapsed += Time.deltaTime;
            
            _currentScale = _startScale + timeElapsed * _scaleOffset;
            transform.localScale = _currentScale;
            
            yield return null;
        }
        Destroy(gameObject);
    }

    private IEnumerator FadeEffect()
    {
        float timeElapsed = 0f;

        _startColor = _currentColor;
        _startColor.a = 0.8f;

        _endColor = _currentColor;
        _endColor.a = 0.2f;

        _colorOffset = _endColor - _startColor;
        Color c = _startColor;
        _spriteRenderer.color = c;

        while (timeElapsed < 1f)
        {
            timeElapsed += Time.deltaTime;

            c = _startColor + timeElapsed * _colorOffset;
            _spriteRenderer.color = c;
            
            yield return null;
        }
    }
}
