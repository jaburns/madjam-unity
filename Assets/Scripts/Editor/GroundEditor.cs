using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(Ground))]
public class GroundEditor : Editor
{
    enum NodeAction {
        Nothing,
        Insert,
        Delete
    }

    Ground targ { get { return target as Ground; } }

    NodeAction _actionRequest = NodeAction.Nothing;
    bool _hasDeleted;
    bool _editNodes = true;

    void OnSceneGUI()
    {
        ensureTargetInit();

        if (Event.current.type == EventType.mouseUp) {
            _hasDeleted = false;
        }

        drawGrid();

        Handles.color = Color.white;
        Handles.DrawLine(targ.transform.position + targ.Nodes[targ.Nodes.Count-1].AsVector3(), targ.transform.position + targ.Nodes[0].AsVector3());

        int? actionIndex = null;
        Vector2 actionPos = Vector2.zero;

        for (int i = 0; i < targ.Nodes.Count; ++i) {
            if (_editNodes) {
                if (!_hasDeleted) {
                    var oldPos = targ.transform.position + targ.Nodes[i].AsVector3();
                    var newPos = Handles.PositionHandle(targ.transform.position + targ.Nodes[i].AsVector3(), Quaternion.identity);
                    targ.Nodes[i] = newPos - targ.transform.position;

                    if ((oldPos - newPos).sqrMagnitude > 1e-9) {
                        actionIndex = i;
                        actionPos = oldPos;
                    }
                } else {
                    Handles.PositionHandle(targ.transform.position + targ.Nodes[i].AsVector3(), Quaternion.identity);
                }
            }
            if (i > 0) {
                Handles.DrawLine(targ.transform.position + targ.Nodes[i-1].AsVector3(), targ.transform.position + targ.Nodes[i].AsVector3());
            }
        }

        if (actionIndex.HasValue) {
            switch (_actionRequest) {
                case NodeAction.Insert:
                    targ.Nodes.Insert(actionIndex.Value, actionPos);
                    break;
                case NodeAction.Delete:
                    targ.Nodes.RemoveAt(actionIndex.Value);
                    _hasDeleted = true;
                    break;
            }
            _actionRequest = NodeAction.Nothing;
            // generate();
        }
    }

    void drawGrid()
    {
        Handles.color = Color.grey;
        for (float ix = -1000; ix < 1000; ix += 0.5f) {
            Handles.DrawLine(
                new Vector3(ix, -1000, 0),
                new Vector3(ix,  1000, 0)
            );
        }
        for (float iy = -1000; iy < 1000; iy += 0.5f) {
            Handles.DrawLine(
                new Vector3(-1000, iy, 0),
                new Vector3( 1000, iy, 0)
            );
        }
    }

    void ensureTargetInit()
    {
        if (targ.Nodes == null || targ.Nodes.Count < 3) {
            targ.Nodes = new List<Vector2> {
                new Vector2(-1, -1),
                new Vector2(0,  2),
                new Vector2(1, -1)
            };
        }
    }

    override public void OnInspectorGUI()
    {
        ensureTargetInit();

        _editNodes = EditorGUILayout.Toggle("Edit Nodes", _editNodes);

        if (_editNodes) {
            _actionRequest = (NodeAction)EditorGUILayout.EnumPopup("Action", _actionRequest);

            if (GUILayout.Button("Insert")) _actionRequest = NodeAction.Insert;
            if (GUILayout.Button("Delete")) _actionRequest = NodeAction.Delete;

            if (_actionRequest == NodeAction.Delete && targ.Nodes.Count < 4) {
                _actionRequest = NodeAction.Nothing;
            }
        }

        targ.StepSize = EditorGUILayout.FloatField("Step Size", targ.StepSize);
        targ.JaggySize = EditorGUILayout.FloatField("Jaggy Size", targ.JaggySize);
        targ.FlipNormals = EditorGUILayout.Toggle("Flip Normals", targ.FlipNormals);
        targ.UseJaggies = EditorGUILayout.Toggle("Use Jaggies", targ.UseJaggies);

        if (GUILayout.Button("Generate")) {
            generate();
        }
    }

    void snapNodesToGrid()
    {
        targ.transform.position = new Vector3(
            Mathf.Round(targ.transform.position.x / 0.5f) * 0.5f,
            Mathf.Round(targ.transform.position.y / 0.5f) * 0.5f,
            targ.transform.position.z
        );

        var nodes = targ.Nodes;
        for (int i = 0; i < nodes.Count; ++i) {
            nodes[i] = new Vector2(
                Mathf.Round(nodes[i].x / 0.5f) * 0.5f,
                Mathf.Round(nodes[i].y / 0.5f) * 0.5f
            );
        }
    }

    void generate()
    {
        snapNodesToGrid();

        var poly = targ.gameObject.EnsureComponent<PolygonCollider2D>();
        poly.points = targ.Nodes.ToArray();

        var vertices2D = targ.UseJaggies ? getVertices() : targ.Nodes.ToArray();

        var tr = new Triangulator(vertices2D);
        int[] indices = tr.Triangulate();

        var vertices = new Vector3[vertices2D.Length];
        for (int i=0; i<vertices.Length; i++) {
            vertices[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, 0);
        }

        var mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        var filter = targ.gameObject.EnsureComponent<MeshFilter>();
        filter.sharedMesh = mesh;

        targ.gameObject.EnsureComponent<MeshRenderer>();
    }

    Vector2[] getVertices()
    {
        var vertices = new List<Vector2>();

        for (int i = 1; i < targ.Nodes.Count; ++i) {
            vertices.AddRange(getJaggedLine(targ.Nodes[i-1], targ.Nodes[i]));
        }
        vertices.AddRange(getJaggedLine(targ.Nodes[targ.Nodes.Count-1], targ.Nodes[0]));

        return vertices.ToArray();
    }

    Vector2[] getJaggedLine(Vector2 a, Vector2 b)
    {
        var vertices = new List<Vector2> { a };

        var abDist = (b - a).magnitude;
        var stepCount = Mathf.FloorToInt(abDist / targ.StepSize);
        var stepSize = abDist / stepCount;

        var abUnit = (b - a).normalized;
        var normal = abUnit.Rotate(90 * (targ.FlipNormals ? 1 : -1));

        for (int i = 1; i < stepCount; ++i) {
            vertices.Add(a + stepSize * i * abUnit + normal * targ.JaggySize * Random.value);
        }

        return vertices.ToArray();
    }
}
