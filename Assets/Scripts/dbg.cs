using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dbg : MonoBehaviour {
    public static dbg instance = null;

    float x_ax = 0f;

    void Awake() {
        instance = this;
    }

    public float SideLength = 2f;
    public int tileCounter = 0;

    [System.Serializable]
    public class meshSide {
        public float height;
        public float width;
        public float distanseToCenter;

        public List<List<Material>> Mask = new List<List<Material>>();
        public int bPointIndex;

        public Vector3 normal;
        public Vector3 ort;
        public Vector3 zeroVert;
        public List<Vector3> vertMesh = new List<Vector3>();
    }

    [System.Serializable]
    public class tile {
        public GameObject logic_tile;
        public GameObject tile3d;
        public Vector3 scale;
        public int mass = 1;
        public List<meshSide> side;
    }

    public List<tile> Holls = new List<tile>();

    public void Start() {
        for (int i = 0; i < Holls.Count; i++) {
            tileTrace(Holls[i]);
            Holls[i].logic_tile.tag = "logTile";
        }
    }

    void debDrowPoint(Vector3 point) {
        float sc = 0.5f;
        Debug.DrawLine(point - sc * Vector3.right, point + sc * Vector3.right, Color.red, 10000);
        Debug.DrawLine(point - sc * Vector3.up, point + sc * Vector3.up, Color.red, 10000);
        Debug.DrawLine(point - sc * Vector3.forward, point + sc * Vector3.forward, Color.red, 10000);
    }

    void getMaterial(tile tileBuf, meshSide meshBuf) {

        GameObject inst = Instantiate(tileBuf.logic_tile);

        inst.transform.position = new Vector3(x_ax, 0, 0);
        x_ax += 3f;

        Vector3 ort1 = vectProd(meshBuf.normal, Vector3.up);

        ///найти нулевую вершину

        Vector3 bufVect = new Vector3();
        Vector3 tileCenter = tileBuf.logic_tile.GetComponent<Renderer>().bounds.center;

        ort1 = ort1.normalized;
        meshBuf.ort = ort1;

        for (int i = 0; i < meshBuf.vertMesh.Count; i++) {
            if (i == 0) {
                bufVect = meshBuf.vertMesh[i];
                meshBuf.bPointIndex = i;
                continue;
            }
            Vector3 bufDir = meshBuf.vertMesh[i] - bufVect;
            float prod = meshBuf.ort.x * bufDir.x + meshBuf.ort.y * bufDir.y + meshBuf.ort.z * bufDir.z;
            if (Mathf.Abs(prod) < 0.01f) {
                if (meshBuf.vertMesh[i].y < bufVect.y) {
                    bufVect = meshBuf.vertMesh[i];
                    meshBuf.bPointIndex = i;
                    continue;
                }
            }
            if (prod < 0) {
                bufVect = meshBuf.vertMesh[i];
                meshBuf.bPointIndex = i;
                continue;
            }
        }

        meshBuf.zeroVert = bufVect;

        float distBuf = 0f;

        for (int i = 0; i < meshBuf.vertMesh.Count; i++) {
            float distVectX = Mathf.Pow((meshBuf.vertMesh[i].x - meshBuf.vertMesh[meshBuf.bPointIndex].x), 2f);
            float distVectY = Mathf.Pow((meshBuf.vertMesh[i].y - meshBuf.vertMesh[meshBuf.bPointIndex].y), 2f);
            float distVectZ = Mathf.Pow((meshBuf.vertMesh[i].z - meshBuf.vertMesh[meshBuf.bPointIndex].z), 2f);
            float distVect = distVectX + distVectY + distVectZ;

            if (distBuf < distVect) {
                distBuf = distVect;
                meshBuf.width = Mathf.Round(Mathf.Sqrt(distVectX + distVectZ) * tileBuf.scale.y / SideLength);
                meshBuf.height = Mathf.Round(Mathf.Sqrt(distVectY) * tileBuf.scale.y / SideLength);
            }
        }

        Ray rayBuf;
        RaycastHit hit;

        var meshCollider = tileBuf.logic_tile.GetComponent<MeshCollider>();

        for (int i = 0; i < meshBuf.height; i++) {
            meshBuf.Mask.Add(new List<Material>());
            for (int j = 0; j < meshBuf.width; j++) {

                Vector3 start = meshBuf.zeroVert + ((float)i + 0.5f) * Vector3.up * SideLength + ((float)j + 0.5f) * ort1 * SideLength + 1.5f * meshBuf.normal + inst.transform.position;

                //debDrowPoint(start);

                rayBuf = new Ray(start, -meshBuf.normal);
                Physics.Raycast(rayBuf, out hit, 4f);

                if (hit.collider == null) {
                    Debug.DrawLine(start, start - 4f * meshBuf.normal, Color.green, 10000);
                    //Debug.Log("boba aboba");
                    continue;
                }

                Mesh mesh = meshCollider.sharedMesh;
                int materialInd = -1;
                int index = hit.triangleIndex;

                for (int t = 0; t < mesh.subMeshCount; t++) {
                    var bufTriangles = mesh.GetTriangles(t);
                    for (int k = 0; k < bufTriangles.Length; k++) {
                        if (index == bufTriangles[k]) {
                            materialInd = t;
                            break;
                        }
                    }
                    if (materialInd != -1) {
                        break;
                    }
                }

                Vector3[] vertices = mesh.vertices;
                int[] triangles = mesh.triangles;

                Vector3 p0 = vertices[triangles[index * 3 + 0]] * 100f + inst.transform.position;
                Vector3 p1 = vertices[triangles[index * 3 + 1]] * 100f + inst.transform.position;
                Vector3 p2 = vertices[triangles[index * 3 + 2]] * 100f + inst.transform.position;

                //Debug.Log(start);

                Debug.DrawLine(p0, p1, Color.green, 10000f);
                Debug.DrawLine(p0, p2, Color.green, 10000f);
                Debug.DrawLine(p1, p2, Color.green, 10000f);

                Debug.Log(tileBuf.logic_tile.GetComponent<Renderer>().sharedMaterials[materialInd]);
                
                meshBuf.Mask[i].Add(tileBuf.logic_tile.GetComponent<Renderer>().sharedMaterials[materialInd]);
            }
        }
        //Destroy(inst);
    }

    Vector3 getNormal(Vector3 p0, Vector3 p1, Vector3 p2) {
        Vector3 norm;

        Vector3 v1 = p1 - p0;
        Vector3 v2 = p2 - p0;

        norm.x = v1.y * v2.z - v1.z * v2.y;
        norm.y = v1.z * v2.x - v1.x * v2.z;
        norm.z = v1.x * v2.y - v1.y * v2.x;

        return norm;
    }

    Vector3 vectProd(Vector3 a, Vector3 b) {
        Vector3 outp = new Vector3();
        outp.x = a.y * b.z - a.z * b.y;
        outp.y = a.z * b.x - a.x * b.z;
        outp.z = a.x * b.y - a.y * b.x;
        return outp;
    }
    private static int comp(Vector3 a, Vector3 b) {
        if (a.y < b.y) {
            return -1;
        } else if (a.y == b.y) {
            if (a.x < b.x) {
                return -1;
            } else if (a.x == b.x) {
                if (a.z < b.z) {
                    return -1;
                } else if (a.z == b.z) {
                    return 0;
                }
            }
        }
        return 1;
    }


    void tileTrace(tile tileBuf) {
        GameObject inst = Instantiate(tileBuf.logic_tile);

        tileBuf.scale = tileBuf.tile3d.transform.localScale;

        var meshCollaider = tileBuf.logic_tile.GetComponent<MeshCollider>();
        Mesh mesh = meshCollaider.sharedMesh;
        Vector3 norm;

        float rayDistance = 4f;

        Vector3 endTrace;
        Vector3 tileCenter = tileBuf.logic_tile.GetComponent<Renderer>().bounds.center;

        RaycastHit hit;

        Ray rayTrace;
        for (float i = 0; i < 102; i++) {
            endTrace.x = Mathf.Sin(i / 51f * Mathf.PI) * rayDistance;
            endTrace.z = Mathf.Cos(i / 51f * Mathf.PI) * rayDistance;
            endTrace.y = 0f;

            rayTrace = new Ray(tileCenter + endTrace, -endTrace);

            Physics.Raycast(tileCenter + endTrace, -endTrace, out hit, rayDistance);

            if (hit.collider == null) {
                continue;
            }

            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            Vector3 p0 = vertices[triangles[hit.triangleIndex * 3 + 0]];
            Vector3 p1 = vertices[triangles[hit.triangleIndex * 3 + 1]];
            Vector3 p2 = vertices[triangles[hit.triangleIndex * 3 + 2]];

            norm = getNormal(tileBuf.scale.x * p0, tileBuf.scale.x * p1, tileBuf.scale.x * p2);



            if ((norm.x * endTrace.x) + (norm.y * endTrace.y) + (norm.z * endTrace.z) < 0) {
                norm = -norm;
            }

            //norm.y = 0f;
            norm = norm.normalized;

            bool f = true;
            bool f0 = true;
            bool f1 = true;
            bool f2 = true;

            int sideCount = tileBuf.side.Count - 1;

            for (int j = 0; (j < tileBuf.side.Count) && f; j++) {
                if ((tileBuf.side[j].normal - norm).magnitude <= 0.001f) {
                    f = false;
                    sideCount = j;

                }
            }

            if (f) {
                tileBuf.side.Add(new meshSide());
                sideCount++;
                tileBuf.side[sideCount].normal = norm;
                //Debug.Log(tileBuf.side[sideCount].normal);
                tileBuf.side[sideCount].distanseToCenter = Mathf.Abs((norm.x * tileCenter.x + norm.y * tileCenter.y + tileCenter.z * p0.z) - (norm.x * p0.x + norm.y * p0.y + norm.z * p0.z));
            }

            for (int j = 0; j < tileBuf.side[sideCount].vertMesh.Count; j++) {
                if ((p0 - tileBuf.side[sideCount].vertMesh[j]).magnitude <= 0.001f)
                    f0 = false;
                if ((p1 - tileBuf.side[sideCount].vertMesh[j]).magnitude <= 0.001f)
                    f1 = false;
                if ((p2 - tileBuf.side[sideCount].vertMesh[j]).magnitude <= 0.001f)
                    f2 = false;
            }

            if (f0)
                tileBuf.side[sideCount].vertMesh.Add(p0);
            if (f1)
                tileBuf.side[sideCount].vertMesh.Add(p1);
            if (f2)
                tileBuf.side[sideCount].vertMesh.Add(p2);
        }

        Destroy(inst);

        for (int i = 0; i < tileBuf.side.Count; i++) {
            getMaterial(tileBuf, tileBuf.side[i]);
        }
    }
}
