using UnityEngine;

public class Switch : MonoBehaviour
{
    public GameObject[] Targets;

	void OnTriggerEnter2D(Collider2D col)
    {
        if (col.name == "BlobTrigger") {
            notifyTargets();
        }
    }

    void notifyTargets()
    {
	    foreach (var target in Targets) {
	        if (target != null) {
                target.SendMessage("OnSwitch");
            }
        }
    }
}
