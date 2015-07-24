using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    const float RADIUS = 5;

    BlobController _blob;

    void Start()
    {
        _blob = FindObjectOfType<BlobController>();
    }

    void FixedUpdate()
    {
        if ((_blob.transform.position - transform.position).sqrMagnitude < RADIUS*RADIUS) {
            BlobController.s_checkedPoint = transform.position;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.5f);
        Gizmos.DrawWireSphere(transform.position, RADIUS);
    }
}
