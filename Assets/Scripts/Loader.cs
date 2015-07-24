using UnityEngine;

public class Loader : MonoBehaviour
{
    void Start ()
    {
        BlobController.s_checkedPoint = null;
    }

    public void OnClickStart()
    {
        Application.LoadLevel("Level");
    }
}
