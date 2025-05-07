using UnityEngine;
using UnityEngine.UI;

public class BlockButton : MonoBehaviour
{
    public int ColorId;

    private void Awake()
    {
        GetComponent<Image>().color = GameplayManager.Instance.Colors[ColorId];
    }
}
