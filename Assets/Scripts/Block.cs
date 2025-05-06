using UnityEngine;
using UnityEngine.UI;

public class Block : MonoBehaviour
{
    public int ColorId;

    private void Awake()
    {
        GetComponent<Image>().color = GameplayManager.Instance.Colors[ColorId];
    }
}
