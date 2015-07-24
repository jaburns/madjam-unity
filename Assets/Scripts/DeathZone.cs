using UnityEngine;

public class DeathZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag != "AnimalTrigger") return;
		GameObject.Find ("GameOver").GetComponent<TriggeredUI>().Show ();
        
		// TODO: Application.LoadLevel("Level");
    }
}
