using UnityEngine;

public class DeathZone : MonoBehaviour
{
	void OnTriggerEnter2D(Collider2D col)
    {
        if (col.name == "BlobTrigger") {
            Application.LoadLevel(0);
        }
    }
}
