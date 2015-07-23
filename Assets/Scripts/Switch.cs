using UnityEngine;

public class Switch : MonoBehaviour
{
    public bool Heavy;
    public bool TargetIsGravity;
    public GameObject[] Targets;

    void OnTriggerEnter2D(Collider2D col)
    {
        var sh = col.GetComponent<SwitchHitter>();
        if (sh == null) return;

        if (Heavy == sh.Heavy) {
            if (TargetIsGravity) {
                GravitySetting.SwitchGravity();
            } else {
                foreach (var target in Targets) {
                    if (target != null) {
                        target.SendMessage("OnSwitch");
                    }
                }
            }
            Destroy(gameObject);
        }
    }
}
