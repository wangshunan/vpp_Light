using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightTest : MonoBehaviour
{
    public float range = 30;
    public LayerMask cullingMask = -1;
    public Material material;

    private int mask = -1;
    private float distance = 0;
    private int segments = 360;

    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private Vector3[] vertexs;
    private int[] triangles;

    private void Start()
    {
        mask = 0 | cullingMask;

        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = material;

    }

    private void Update()
    {
        mask = 0 | cullingMask;

        vertexs = new Vector3[segments + 1];
        vertexs[0] = Vector3.zero;

        int count = 1;
        for (float i = -transform.localEulerAngles.z; i <= -transform.localEulerAngles.z + 360; i++)
        {
            Vector2 direction = new Vector2(Mathf.Sin(Mathf.Deg2Rad * i), Mathf.Cos(Mathf.Deg2Rad * i));

            RaycastHit2D hit = Physics2D.Raycast(transform.localPosition, direction, range, mask);

            distance = hit.collider == null ? range : hit.distance;
            Vector2 endPoint = new Vector2(transform.localPosition.x + distance * direction.x / direction.magnitude, transform.localPosition.y + distance * direction.y / direction.magnitude);
            endPoint = transform.InverseTransformPoint(endPoint);

            if (count <= segments)
                vertexs[count++] = endPoint;
        }

        triangles = new int[(segments + 1) * 3];
        int index = 0;

        for ( int i = 0; i < (segments + 1) * 3 - 3; i += 3 )
        {
            triangles[i] = 0;
            triangles[i + 1] = index + 1;

            if ( i == ( segments * 3 ) - 3)
            {
                triangles[i + 2] = 1;
            } else
            {
                triangles[i + 2] = index + 2;
            }

            index++;
        }

        mesh = new Mesh();
        mesh.vertices = vertexs;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;
    }

}