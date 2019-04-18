using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class meshFace : System.Object
{
    public int a, b, c = -1;
    public Vector3 aPos, bPos, cPos = new Vector3();
    public Vector3 Origin;
    public meshFace(int a, int b, int c, Vector3[] positionRefs)
    {
        this.a = a;
        this.b = b;
        this.c = c;

        this.aPos = positionRefs[a];
        this.bPos = positionRefs[b];
        this.cPos = positionRefs[c];
    }

    public bool hasVertex(int vertexIndex)
    {
        return this.a == vertexIndex || this.b == vertexIndex || this.c == vertexIndex;
    }

    public bool hasVertex(Vector3 vertexPos)
    {
        return this.aPos == vertexPos || this.bPos == vertexPos || this.cPos == vertexPos;
    }

    public int this[int i]
    {
        get { return i == 0 ? this.a : i == 1 ? this.b : i == 2 ? this.c : -1; }
        set {} //This will unlikely be used
    }
}


[ExecuteInEditMode]
public class NavMesh : MonoBehaviour
{
    public Mesh inputMesh;
    
    //Cache
    public Vector3[] vertices;
    public meshFace[] faces;
    public Vector3[] faceOrigins;

    public bool bake;
    public bool draw;
    Quaternion rotation = Quaternion.Euler(-90, 0, 0);

    private Mesh highlight;

    private void OnDrawGizmos()
    {
        if (bake)
        {
            bake = false;

            vertices = new Vector3[inputMesh.vertices.Length];

            for(int i = 0; i < inputMesh.vertices.Length; i++)
            {
                vertices[i] = rotation * inputMesh.vertices[i];
            }

            faceOrigins = new Vector3[(int)(inputMesh.triangles.Length/3)];
            faces = new meshFace[(int)(inputMesh.triangles.Length / 3)];

            for(int i = 0; i < faces.Length; i++)
            {
                faces[i] = new meshFace(inputMesh.triangles[i*3], inputMesh.triangles[i * 3 + 1], inputMesh.triangles[i * 3 + 2], vertices); 
                faceOrigins[i] = (vertices[faces[i].a] + vertices[faces[i].b] + vertices[faces[i].c])/3;
            }
            


        }
        if (draw)
        {
            if (inputMesh != null)
            {
                Gizmos.DrawWireMesh(inputMesh, transform.position, rotation);
            }
            if(highlight != null)
            {
                Gizmos.DrawMesh(highlight, transform.position);
            }
        }
    }

    

    public int getFaceFromEdge(int A, int B, List<int> filter)
    {
        for(int i = 0; i < faces.Length; i++)
        {
            if (!filter.Contains(i))
            {
                if ((faces[i].a == A || faces[i].b == A || faces[i].c == A) &&
                   (faces[i].a == B || faces[i].b == B || faces[i].c == B))
                    return i;
            }
        }
        return -1;
    }

    public int getFaceFromPoint(Vector3 point)
    {
        List<int> filteredFaces = new List<int>();
        int closestFace = 0;
        for (int i = 0; i < 10; i++) {
            closestFace = getClosestFace(point, filteredFaces);
            if (PointWithinFace(point, faces[closestFace]))
            {
                Debug.Log("Found closest face!");
                return closestFace;
            }
            else
            {
                filteredFaces.Add(closestFace);
            }
        }
        Mesh faceMesh = new Mesh();
        faceMesh.vertices = new Vector3[]{
            faces[closestFace].aPos,
            faces[closestFace].bPos,
            faces[closestFace].cPos
        };
        faceMesh.triangles = new int[] { 0, 1, 2 };

        faceMesh.normals = new Vector3[]
        {
            Vector3.up,
            Vector3.up,
            Vector3.up
        };

        highlight = faceMesh;

        Debug.DrawRay(faceOrigins[closestFace] + transform.position, Vector3.up * 3, Color.blue, Time.deltaTime);
        Debug.Log("Did not find closest face!");
        return 0;
    }

    public int getClosestFace(Vector3 position, List<int> filter)
    {
        float closestDistance = Mathf.Infinity;
        int closestFace = -1;
        for (int i = 0; i < faceOrigins.Length; i++)
        {
            if (!filter.Contains(i))
            {
                float distance = Vector3.Distance(faceOrigins[i] + transform.position, position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestFace = i;
                }
            }
        }


        return closestFace;

    }
    

    public bool PointWithinFace(Vector3 point, meshFace triangle)
    {
        return (
                SameSideOf(point, triangle.aPos + transform.position, triangle.bPos + transform.position, triangle.cPos) &&
                SameSideOf(point, triangle.bPos + transform.position, triangle.aPos + transform.position, triangle.cPos) &&
                SameSideOf(point, triangle.cPos + transform.position, triangle.aPos + transform.position, triangle.bPos));
    }

    public bool SameSideOf(Vector3 p, Vector3 C, Vector3 A, Vector3 B)
    {
        //Returns whether pA and pB are on the same side of the line AB or not
        Vector3 crossA = Vector3.Cross(B - A, p - A);
        Vector3 crossB = Vector3.Cross(B - A, C - A);
        return Vector3.Dot(crossA,crossB) >= 0;
    }

    private RaycastHit[] recursiveRaycast(Ray ray, float length, int layerMask)
    {
       
        return performRecursiveRaycast(new List<RaycastHit>(), ray, length, layerMask).ToArray();
    }

    private List<RaycastHit> performRecursiveRaycast(List<RaycastHit> hitList, Ray ray, float lengthRemaining, int layerMask)
    {
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, lengthRemaining))
        {
            lengthRemaining -= hit.distance;
            hitList.Add(hit);
            Ray nextRay = new Ray(hit.point + ray.direction*.01f, ray.direction);
            return performRecursiveRaycast(hitList, nextRay, lengthRemaining, layerMask);
        }
        else
        {
            return hitList;
        }
        
    }
}
