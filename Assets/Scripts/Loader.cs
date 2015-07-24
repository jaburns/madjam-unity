using UnityEngine;

public class Loader : MonoBehaviour
{
    void Start ()
    {
        BlobController.s_checkedPoint = null;
    }

    public void OnClickStart()
    {
        Debug.Log("hi");
        Application.LoadLevel("Level");
    }
}
