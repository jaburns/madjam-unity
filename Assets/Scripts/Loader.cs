using UnityEngine;

public class Loader : MonoBehaviour
{
    public Material GooMaterial;
    public Color[] GooColors;

    int _colorIndex;

    void Start ()
    {
        BlobController.s_checkedPoint = null;
        GooMaterial.color = GooColors[_colorIndex];
    }

    public void OnClickStart()
    {
        Application.LoadLevel("Level");
    }

    public void OnRightArrow()
    {
        _colorIndex++;
        if (_colorIndex >= GooColors.Length) _colorIndex = 0;
        GooMaterial.color = GooColors[_colorIndex];
    }

    public void OnLeftArrow()
    {
        _colorIndex--;
        if (_colorIndex < 0) _colorIndex = GooColors.Length - 1;
        GooMaterial.color = GooColors[_colorIndex];
    }

    public void OnUpArrow()
    {
    }
    public void OnDownArrow()
    {
    }
}
