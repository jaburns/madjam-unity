using UnityEngine;

public class Loader : MonoBehaviour
{
    public Material GooMaterial;
    public Color[] GooColors;
    public GameObject[] Brains;

    static public int BrainChoice = 0;

    int _colorIndex;

    void Start ()
    {
        BlobController.s_checkedPoint = null;
        Brains[1].SetActive(false);
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
        BrainChoice = 1 - BrainChoice;
        Brains[0].SetActive(!Brains[0].active);
        Brains[1].SetActive(!Brains[1].active);
    }

    public void OnDownArrow()
    {
        OnUpArrow();
    }
}
