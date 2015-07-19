using UnityEngine;
using System.Collections.Generic;

public class Ground : MonoBehaviour
{
    public List<Vector2> Nodes;
    public float JaggySize = 0.5f;
    public float StepSize = 1.5f;
    public bool FlipNormals;
}
