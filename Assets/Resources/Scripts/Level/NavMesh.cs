using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * AUTHOR:
 * Ludvig Björk Förare
 * 
 * DESCRIPTION:
 * A container class for holding and generating a navmesh from a given mesh.
 * Also contains some navmesh operations 
 *
 * CODE REVIEWED BY:
 * 
 * 
 * CLEANED
 */

//Class for defining triangles in navmesh
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

    //Returns if face has vertex 'vertexPos'
    public bool hasVertex(Vector3 vertexPos)
    {
        return this.aPos == vertexPos || this.bPos == vertexPos || this.cPos == vertexPos;
    }
}

[ExecuteInEditMode]
public class NavMesh : MonoBehaviour
{
    //Mesh to base navmesh upon
    public Mesh inputMesh;
    
    //Cached faces
    [HideInInspector]
    public navmeshFace[] Faces;

    public Vector3 NavmeshOffset;

    //Optional rotation offset to fix mismatching coordinate systems
    Quaternion rotation = Quaternion.Euler(0, 0, 0);

    private Mesh highlight;

    private void OnDrawGizmos()
    {
        if (inputMesh != null)
        {
            Gizmos.DrawWireMesh(inputMesh, transform.position + transform.rotation*NavmeshOffset, rotation * transform.rotation);
        }
    }

    //Fetches a face that contains edge AB that is not included in filter
    public navmeshFace getFaceFromEdge(Vector3 A, Vector3 B, List<navmeshFace> filter)
    {
        foreach(navmeshFace face in Faces)
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

    //Fetches face closest to point (according to face origin)
    //Should return which face a point is within but that shit broke ATM
    public navmeshFace getFaceFromPoint(Vector3 point)
    {
        return getClosestFace(point, new List<navmeshFace>());

        //Disabled
        List<navmeshFace> filteredFaces = new List<navmeshFace>();
        foreach (navmeshFace face in Faces) {
            if (PointWithinFace(point, face))
            {
                return face;
            }
            else
            {
                filteredFaces.Add(face);
            }
        }
    }

    //Gets face with closest origin to point that is not included in filter
    public navmeshFace getClosestFace(Vector3 position, List<navmeshFace> filter)
    {
        float closestDistance = Mathf.Infinity;
        navmeshFace closestFace = null;
        for (int i = 0; i < Faces.Length; i++)
        {
            if (!filter.Contains(Faces[i]))
            {
                float distance = Vector3.Distance(Faces[i].Origin, position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestFace = Faces[i];
                }
            }
        }
        return closestFace;
    }
    
    //Returns true if point is within face
    public bool PointWithinFace(Vector3 point, navmeshFace triangle)
    {
        bool A = SameSideOf(point, triangle.aPos, triangle.bPos, triangle.cPos);
        bool B = SameSideOf(point, triangle.bPos, triangle.aPos, triangle.cPos);
        bool C = SameSideOf(point, triangle.cPos, triangle.aPos, triangle.bPos);
        return A && B && C;
    }

    //Returns true if pA and pB are on the same side of line AB
    public bool SameSideOf(Vector3 pA, Vector3 pB, Vector3 A, Vector3 B)
    {
        Vector3 crossA = Vector3.Cross(B - A, pA - A);
        Vector3 crossB = Vector3.Cross(B - A, pB - A);
        float dot = Vector3.Dot(crossA,crossB);
        return dot >= 0;
    }

    public void BakeNavmesh()
    {
        //Fetches vertex data from mesh
        Vector3[] vertices = new Vector3[inputMesh.vertices.Length];

        //Rotates vertices
        for(int i = 0; i < inputMesh.vertices.Length; i++)
        {
            vertices[i] = rotation * inputMesh.vertices[i] + NavmeshOffset;
        }

        //Saves faces from mesh
        Faces = new navmeshFace[(int)(inputMesh.triangles.Length / 3)];
        for(int i = 0; i < Faces.Length; i++)
        {
            Faces[i] = new navmeshFace(vertices[inputMesh.triangles[i*3]], vertices[inputMesh.triangles[i * 3 + 1]], vertices[inputMesh.triangles[i * 3 + 2]]); 
        }
    }

    public void ClearNavmesh(){
        Faces = null;
    }
}