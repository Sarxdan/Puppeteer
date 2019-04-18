using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AStarNode
{
    public float gCost;
    public AStarNode(float gCost, int meshIndex)
    {
        this.gCost = gCost;
    }
}

[ExecuteInEditMode]
public class PathfinderComponent : MonoBehaviour
{
    public float MovementSpeed = 1;
    List<Vector3> path;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit))
        {
            NavMesh navMesh = hit.transform.GetComponent<NavMesh>();
            if (navMesh != null)
            {
                Debug.DrawRay(transform.position, Vector3.up * 10, Color.green, 1);
                navMesh.getFaceFromPoint(hit.point);
            }
        }
    }

    void MoveTo(Vector3 position)
    {
        int startFace = 0;
        Vector3 end = new Vector3(10,0,10);

        List<AStarNode> unEvaluatedNodes = new List<AStarNode>();
        List<AStarNode> evaluatedNodes = new List<AStarNode>();
        List<AStarNode> facePath = new List<AStarNode>();



        while (true)
        {
            AStarNode currentNode = null;
            float lowestGCost = Mathf.Infinity;
            foreach(AStarNode node in unEvaluatedNodes)
            {
                if(node.gCost < lowestGCost)
                {
                    lowestGCost = node.gCost;
                    currentNode = node;
                }
            }
            unEvaluatedNodes.Remove(currentNode);
            evaluatedNodes.Add(currentNode);
        }

    }
}
