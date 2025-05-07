using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ColorList",menuName = "Custom/ColorList")]
public class ColorList : ScriptableObject
{
    public List<Color> Colors = new List<Color>();
}
