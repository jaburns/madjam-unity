using UnityEngine;

public class Switch : MonoBehaviour
{
    public bool TargetIsGravity;
    public GameObject[] Targets;

	void OnTriggerEnter2D(Collider2D col)
    {
        if (col.name == "BlobTrigger") {
            if (TargetIsGravity) {
                GravitySetting.SwitchGravity();
            } else {
                foreach (var target in Targets) {
                    if (target != null) {
                        target.SendMessage("OnSwitch");
                    }
                }
            }
        }
    }
}
