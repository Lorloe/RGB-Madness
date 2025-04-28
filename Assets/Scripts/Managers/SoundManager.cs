using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource _effectAudioSource;

    public static SoundManager Instance;

    private void Awake() 
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }
        else
            Destroy(gameObject);
    }


    public void PlaySound(AudioClip clip)
    {
        _effectAudioSource.PlayOneShot(clip);
    }
}
