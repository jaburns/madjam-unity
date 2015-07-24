using UnityEngine;

public class DeathZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag != "AnimalTrigger") return;
        Application.LoadLevel("Level");
    }
}
