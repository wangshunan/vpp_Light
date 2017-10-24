using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 頂点情報
public class verts
{
    public float angle { get; set; }
    public int location { get; set; }
    public Vector3 pos { get; set; }
    public bool endpoint { get; set; }
}

public class LightTest2 : MonoBehaviour
{

    // ライトマテリア
    public Material lightMaterial;

    [HideInInspector]
    public PolygonCollider2D[] allMeshes; // 半径内全メッシュの情報

    [HideInInspector]
    public List<verts> allVertices = new List<verts>(); // メッシュを描くvertex

    [SerializeField]
    public float lightRadius = 20f; // ライトの半径

    [Range(4, 360)]
    public int lightSegments = 8; // ライトメッシュの形

    public LayerMask layer;

    Mesh lightMesh;

    // ---------------------- //
    // ライトray角度
    static bool hasInstanced = false;
    public static float[] SenArray;
    public static float[] CosArray;

    private void Start()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshRenderer renderer = GetComponent<MeshRenderer>();

        renderer.sharedMaterial = lightMaterial;

        lightMesh = new Mesh();
        meshFilter.mesh = lightMesh;
        lightMesh.name = "Light Mesh";
        lightMesh.MarkDynamic();

        LightVertAngle();
    }

    private void Update()
    {
        GetAllMeshes();
        SetLine();
        renderLightMesh();
    }


    void GetAllMeshes()
    {
        Collider2D[] allColl2D = Physics2D.OverlapCircleAll(transform.position, lightRadius, layer);
        allMeshes = new PolygonCollider2D[allColl2D.Length];

        for (int i = 0; i < allColl2D.Length; i++)
        {
            allMeshes[i] = (PolygonCollider2D)allColl2D[i];
        }
    }


    void SetLine()
    {
        bool sortAngles = false;
        bool lows = false;
        bool his = false;

        allVertices.Clear();

        List<verts> tempVerts = new List<verts>();

        for (int i = 0; i < allMeshes.Length; i++)
        {
            tempVerts.Clear();
            PolygonCollider2D mesh = allMeshes[i];

            lows = false;
            his = false;

            //　障害物のメッシュの頂点からライトまでの計算
            if (((1 << mesh.transform.gameObject.layer) & layer) != 0)
            {
                for (int j = 0; j < mesh.GetTotalPointCount(); j++)
                {
                    verts vert = new verts();
                    Vector3 worldPoint = mesh.transform.TransformPoint(mesh.points[j]);

                    RaycastHit2D ray = Physics2D.Raycast(transform.position, worldPoint - transform.position, lightRadius, layer);

                    if (ray)
                    {
                        vert.pos = ray.point;
                        if ((worldPoint.sqrMagnitude >= ray.point.sqrMagnitude - 0.15f) &&
                             worldPoint.sqrMagnitude <= ray.point.sqrMagnitude + 0.15f)
                        {
                            vert.endpoint = true;
                        }

                    }
                    else
                    {
                        vert.pos = worldPoint;
                        vert.endpoint = true;
                    }


                    //　相対位置に変更と角度計算
                    vert.pos = transform.InverseTransformPoint(vert.pos);
                    vert.angle = getVectorAngle(true, vert.pos.x, vert.pos.y);

                    // 頂点の位置
                    if (vert.angle < 0f)
                    {
                        lows = true;
                    }


                    if (vert.angle > 2f)
                    {
                        his = true;
                    }

                    // 半径内の頂点を記録する
                    if (vert.pos.sqrMagnitude <= lightRadius * lightRadius)
                    {
                        tempVerts.Add(vert);
                        Debug.DrawLine(transform.position, transform.TransformPoint(vert.pos), Color.white);
                    }

                    if (sortAngles == false)
                    {
                        sortAngles = true;
                    }

                }

            }

            //　メッシュ頂点の位置を判断
            if (tempVerts.Count > 0)
            {
                sortList(tempVerts); // 角度を大きいから小さいに並ぶ

                int posLowAngle = 0;
                int posHighAngle = 0;

                // 頂点がライトの第三、第四象限に存在する場合の調整
                if (his && lows)
                {
                    float lowestAngle = -1f;　// 右
                    float highestAngle = tempVerts[0].angle; // 左

                    for (int k = 0; k < tempVerts.Count; k++)
                    {
                        if (tempVerts[k].angle < 1f && tempVerts[k].angle > lowestAngle)
                        {
                            lowestAngle = tempVerts[k].angle;
                            posLowAngle = k;
                        }

                        if (tempVerts[k].angle > 2f && tempVerts[k].angle < highestAngle)
                        {
                            highestAngle = tempVerts[k].angle;
                            posHighAngle = k;
                        }
                    }
                }
                else
                {
                    posLowAngle = 0;
                    posHighAngle = tempVerts.Count - 1;
                }

                tempVerts[posLowAngle].location = 1; // 右
                tempVerts[posHighAngle].location = -1; // 左

                allVertices.AddRange(tempVerts);

                // メッシュ二つ頂点に当たったRayの延長
                for (int r = 0; r < 2; r++)
                {
                    Vector3 fromRay = new Vector3();
                    bool isEndpoint = false;

                    //　0はlow,1はhight
                    if (r == 0)
                    {
                        fromRay = transform.TransformPoint(tempVerts[posLowAngle].pos);
                        isEndpoint = tempVerts[posLowAngle].endpoint;
                    }
                    else
                    {
                        fromRay = transform.TransformPoint(tempVerts[posHighAngle].pos);
                        isEndpoint = tempVerts[posHighAngle].endpoint;
                    }

                    // もし頂点に当たったら
                    if (isEndpoint)
                    {
                        Vector2 from = fromRay;
                        Vector2 dir = (from - (Vector2)transform.position);

                        float endPointRayOffset = 0.001f;

                        // endPointのオフセット
                        from += (dir * endPointRayOffset);

                        RaycastHit2D rayCont = Physics2D.Raycast(from, dir, lightRadius, layer);
                        Vector3 hitPoint;

                        if (rayCont)
                        {
                            hitPoint = rayCont.point;
                        }
                        else
                        {
                            Vector2 newDir = transform.InverseTransformVector(dir);
                            hitPoint = transform.TransformPoint(newDir.normalized * lightRadius);
                        }

                        if ((hitPoint - transform.position).sqrMagnitude > (lightRadius * lightRadius))
                        {
                            dir = transform.InverseTransformDirection(dir);
                            hitPoint = transform.TransformPoint(dir.normalized * lightRadius);
                        }

                        Debug.DrawLine(from, hitPoint, Color.green);

                        // 相対位置と角度
                        verts newVert = new verts();
                        newVert.pos = transform.InverseTransformPoint(hitPoint);
                        newVert.angle = getVectorAngle(true, newVert.pos.x, newVert.pos.y);
                        allVertices.Add(newVert);
                    }

                }
            }

        }

        // ---------------------------------- //
        // ライト照射範囲の頂点を生成
        // ---------------------------------- //

        int theta = 0;
        // ライトメッシュの頂点数
        int vertAmout = 360 / lightSegments;

        for (int i = 0; i < lightSegments; i++)
        {
            theta = vertAmout * i;
            if (theta == 360) theta = 0;

            // ライト照射範囲の頂点情報
            verts vert = new verts();

            vert.pos = new Vector3(SenArray[theta], CosArray[theta], 0);
            vert.angle = getVectorAngle(true, vert.pos.x, vert.pos.y);
            vert.pos *= lightRadius;
            vert.pos += transform.position;

            RaycastHit2D ray = Physics2D.Raycast(transform.position, vert.pos - transform.position, lightRadius, layer);

            if (!ray)
            {
                vert.pos = transform.InverseTransformPoint(vert.pos);
                allVertices.Add(vert);
            }
        }

        // 角度順に再排列
        if (sortAngles == true)
        {
            sortList(allVertices);
        }

        // 二つの頂点が同じ方向の時の調整
        float rangeAngleComparision = 0.00001f;
        for (int i = 0; i < allVertices.Count - 1; i++)
        {

            verts before = allVertices[i];
            verts after = allVertices[i + 1];

            // 調整するかどうかの判定
            if (before.angle >= after.angle - rangeAngleComparision && before.angle <= after.angle + rangeAngleComparision)
            {
                // 右
                if (after.location == -1)
                { 

                    if (before.pos.sqrMagnitude > after.pos.sqrMagnitude)
                    {
                        allVertices[i] = after;
                        allVertices[i + 1] = before;
                    }
                }

                // 左
                if (before.location == 1)
                { 
                    if (before.pos.sqrMagnitude < after.pos.sqrMagnitude)
                    {

                        allVertices[i] = after;
                        allVertices[i + 1] = before;
                    }
                }

            }
        }
    }

    // メッシュ生成
    void renderLightMesh()
    {
        Vector3[] iniVerticesMeshLight = new Vector3[allVertices.Count + 1];
        iniVerticesMeshLight[0] = Vector3.zero;

        for (int i = 0; i < allVertices.Count; i++)
        {
            iniVerticesMeshLight[i + 1] = allVertices[i].pos;
        }

        lightMesh.Clear();
        lightMesh.vertices = iniVerticesMeshLight;


        Vector2[] uvs = new Vector2[iniVerticesMeshLight.Length];

        for (int i = 0; i < iniVerticesMeshLight.Length; i++)
        {
            uvs[i] = new Vector2(iniVerticesMeshLight[i].x, iniVerticesMeshLight[i].y);
        }

        lightMesh.uv = uvs;

        int idx = 0;
        int[] triangles = new int[allVertices.Count * 3];

        for (int i = 0; i < allVertices.Count * 3; i += 3)
        {
            triangles[0] = 0;
            triangles[i + 1] = idx + 1;

            // 最後の頂点の処理
            if (i == (allVertices.Count * 3) - 3)
            {
                triangles[i + 2] = 1;
            }
            else
            {
                triangles[i + 2] = idx + 2;
            }

            idx++;
        }

        lightMesh.triangles = triangles;
        lightMesh.RecalculateNormals();
        lightMesh.RecalculateBounds();

        GetComponent<Renderer>().sharedMaterial = lightMaterial;

    }

    // ベクトル角度計算
    float getVectorAngle(bool pseudo, float x, float y)
    {
        float ang = 0;
        if (pseudo == true)
        {
            ang = pseudoAngle(x, y);
        }
        else
        {
            ang = Mathf.Atan2(y, x);
        }
        return ang;
    }

    float pseudoAngle(float dx, float dy)
    {
        float ax = Mathf.Abs(dx);
        float ay = Mathf.Abs(dy);
        float p = dy / (ax + ay);
        if (dx < 0)
        {
            p = 2 - p;

        }
        return p;
    }

    void sortList(List<verts> lista)
    {
        lista.Sort((item1, item2) => (item2.angle.CompareTo(item1.angle)));
    }


    static void LightVertAngle()
    {

        if (hasInstanced == false)
        {
            SenArray = new float[360];
            CosArray = new float[360];

            for (int i = 0; i < 360; i++)
            {
                SenArray[i] = Mathf.Sin(i * Mathf.Deg2Rad);
                CosArray[i] = Mathf.Cos(i * Mathf.Deg2Rad);
            }

            hasInstanced = true;
        }
    }
}
