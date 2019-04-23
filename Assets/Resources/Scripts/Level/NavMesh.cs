using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class navmeshFace : System.Object
{
    public Vector3 aPos, bPos, cPos = new Vector3();
    public Vector3 Origin;

    public navmeshFace(Vector3 a, Vector3 b, Vector3 c)
    {

        this.aPos = a;
        this.bPos = b;
        this.cPos = c;

        this.Origin = (aPos + bPos + cPos)/3;

    }


    public bool hasVertex(Vector3 vertexPos)
    {
        return this.aPos == vertexPos || this.bPos == vertexPos || this.cPos == vertexPos;
    }

}


[ExecuteInEditMode]
public class NavMesh : MonoBehaviour
{
    public Mesh inputMesh;
    
    //Cache
    public navmeshFace[] faces;

    public bool bake;
    public bool draw;
    Quaternion rotation = Quaternion.Euler(0, 0, 0);

    private Mesh highlight;

    private void OnDrawGizmos()
    {
        if (bake)
        {
            rotation = Quaternion.Euler(0, 0, 0);
            bake = false;

            Vector3[] vertices = new Vector3[inputMesh.vertices.Length];

            for(int i = 0; i < inputMesh.vertices.Length; i++)
            {
                vertices[i] = rotation * inputMesh.vertices[i];
            }

            faces = new navmeshFace[(int)(inputMesh.triangles.Length / 3)];

            for(int i = 0; i < faces.Length; i++)
            {
                faces[i] = new navmeshFace(vertices[inputMesh.triangles[i*3]], vertices[inputMesh.triangles[i * 3 + 1]], vertices[inputMesh.triangles[i * 3 + 2]]); 
            }
            


        }
        if (draw)
        {
            if (inputMesh != null)
            {
                Gizmos.DrawWireMesh(inputMesh, transform.position, rotation * transform.rotation);
            }
            if(highlight != null)
            {
                Gizmos.DrawMesh(highlight, transform.position);
            }
        }
    }

    public navmeshFace getFaceFromEdge(Vector3 A, Vector3 B, List<navmeshFace> filter)
    {
        foreach(navmeshFace face in faces)
        {
            if (!filter.Contains(face))
            {
                if ((face.aPos == A || face.bPos == A || face.cPos == A) &&
                   (face.aPos == B || face.bPos == B || face.cPos == B))
                    return face;
            }
        }
        return null;
    }

    public navmeshFace getFaceFromPoint(Vector3 point)
    {
        List<navmeshFace> filteredFaces = new List<navmeshFace>();
        foreach (navmeshFace face in faces) {
            if (PointWithinFace(point, face))
            {
                return face;
            }
            else
            {
                filteredFaces.Add(face);
            }
        }

        return null;
    }

    public navmeshFace getClosestFace(Vector3 position, List<navmeshFace> filter)
    {
        float closestDistance = Mathf.Infinity;
        navmeshFace closestFace = null;
        for (int i = 0; i < faces.Length; i++)
        {
            if (!filter.Contains(faces[i]))
            {
                float distance = Vector3.Distance(faces[i].Origin + transform.position, position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestFace = faces[i];
                }
            }
        }


        return closestFace;

    }
    

    public bool PointWithinFace(Vector3 point, navmeshFace triangle)
    {
        bool A = SameSideOf(point, triangle.aPos, triangle.bPos, triangle.cPos);
        bool B = SameSideOf(point, triangle.bPos, triangle.aPos, triangle.cPos);
        bool C = SameSideOf(point, triangle.cPos, triangle.aPos, triangle.bPos);
        return A && B && C;
    }

    public bool SameSideOf(Vector3 pA, Vector3 pB, Vector3 A, Vector3 B)
    {
        //Returns whether pA and pB are on the same side of the line AB or not
        Vector3 crossA = Vector3.Cross(B - A, pA - A);
        Vector3 crossB = Vector3.Cross(B - A, pB - A);
        float dot = Vector3.Dot(crossA,crossB);
        return dot >= 0;

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
