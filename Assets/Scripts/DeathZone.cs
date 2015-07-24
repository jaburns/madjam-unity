using UnityEngine;

public class DeathZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag != "AnimalTrigger") return;
        GameObject.Find ("GameOver").GetComponent<TriggeredUI>().Show ();
        Controls.Instance.Enabled = false;
    }

    void FixedUpdate()
    {
        if (!Controls.Instance.Enabled && Controls.IsDown(Controls.Instance.Retry)) {
            Controls.Instance.Enabled = true;
            Application.LoadLevel("Level");
        }
    }
}
