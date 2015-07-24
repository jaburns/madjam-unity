using UnityEngine;

public class Loader : MonoBehaviour
{
    void Start ()
    {
        BlobController.s_checkedPoint = null;
        Application.LoadLevel("Level");
    }
}
