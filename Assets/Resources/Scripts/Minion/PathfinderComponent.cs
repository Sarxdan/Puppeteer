using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class AStarNode
{
    public float FCost;
    public int FaceIndex;
    public meshFace Face;
    public AStarNode Parent;
    public AStarNode(float fCost, int faceIndex, meshFace Face, AStarNode parent)
    {
        this.FCost = fCost;
        this.FaceIndex = faceIndex;
        this.Parent = parent;
        this.Face = Face;
    }
}

public class PathfinderComponent : MonoBehaviour
{
    public float MovementSpeed = 1;
    public int MaxRecursionDepth = 20;
    public List<Vector3> path;
    private Transform navmeshTransform;
    private bool IsPathfinding = false;

    public float LegHeight;
    // Start is called before the first frame update

    private Rigidbody rigidBody;
    void Start()
    {
        this.rigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        
        if(IsPathfinding){
        for(int i = 0; i < this.path.Count-1; i++){
            Debug.DrawLine(this.path[i] + transform.up, this.path[i+1] + transform.up, Color.black, Time.deltaTime);
        }
            move();
        }

    }

    private void createOrUpdateNode(int faceIndex, meshFace face, Vector3 position, Vector3 start, Vector3 end, Dictionary<int, AStarNode> unEvaluatedNodes, List<int> faceFilter, AStarNode parent)
    {
        unEvaluatedNodes[faceIndex] = new AStarNode(Vector3.Distance(start, position) + Vector3.Distance(position,end),faceIndex, face, parent);
        faceFilter.Add(faceIndex);
    }

    private void move(){
        if(path.Count <= 0){
            IsPathfinding = false;
            return;
        }
        Vector3 deltaPos = path[0] - transform.position + transform.up * LegHeight + navmeshTransform.position;
        float distance = 0;
        if(deltaPos.magnitude < this.MovementSpeed * Time.deltaTime){
            distance = deltaPos.magnitude;
            this.path.RemoveAt(0);
        }else{
            distance = this.MovementSpeed * Time.deltaTime;
        }
        rigidBody.MovePosition(transform.position + deltaPos.normalized * this.MovementSpeed * Time.deltaTime );
    }

    public void MoveTo(Vector3 end, NavMesh navmesh)
    {
        
        RaycastHit hit;
        Vector3 start;
        if (Physics.Raycast(transform.position, -transform.up, out hit))
        {
            NavMesh navMesh = hit.transform.GetComponent<NavMesh>();
            if (navMesh != null)
            {
                start = hit.point;
                navmeshTransform = navMesh.transform;
            }else{
                return;
            }
        }else{
            return;
        }

        //Vector3 end = new Vector3(-10,0,-10);

        Dictionary<int,AStarNode> unEvaluatedNodes = new Dictionary<int, AStarNode>();
        Dictionary<int, AStarNode> evaluatedNodes = new Dictionary<int, AStarNode>();
        List<int> faceFilter = new List<int>();
        List<AStarNode> facePath = new List<AStarNode>();
        
        this.path = new List<Vector3>();

        Debug.DrawRay(start, Vector3.up * 5, Color.red, 10);
        Debug.DrawRay(end, Vector3.up * 5, Color.green, 10);

        int startFaceIndex = navmesh.getFaceFromPoint(start);

        createOrUpdateNode(startFaceIndex, navmesh.faces[startFaceIndex], navmesh.faceOrigins[startFaceIndex], end, start, unEvaluatedNodes, faceFilter, null);

        AStarNode currentNode = null;

        while (unEvaluatedNodes.Count > 0)
        {
            //Fetches unevaluated node with lowest FCost
            float lowestFCost = Mathf.Infinity;
            foreach(AStarNode node in unEvaluatedNodes.Values)
            {
                if(node.FCost < lowestFCost)
                {
                    lowestFCost = node.FCost;
                    currentNode = node;
                }
            }

            //Moves node to evaluated list
            unEvaluatedNodes.Remove(currentNode.FaceIndex);
            evaluatedNodes[currentNode.FaceIndex] = currentNode;

            //Check if current face is destination
            if (navmesh.PointWithinFace(end, navmesh.faces[currentNode.FaceIndex])) break;

            //Fetches adjacent faces, filtering already fetched faces
            int newFace1 = navmesh.getFaceFromEdge(currentNode.Face.a, currentNode.Face.b, faceFilter);
            int newFace2 = navmesh.getFaceFromEdge(currentNode.Face.a, currentNode.Face.c, faceFilter);
            int newFace3 = navmesh.getFaceFromEdge(currentNode.Face.b, currentNode.Face.c, faceFilter);

            //If edge has a face, add it as a A* node
            if (newFace1 != -1)
            {
                createOrUpdateNode(newFace1, navmesh.faces[newFace1], navmesh.faceOrigins[newFace1], end, start, unEvaluatedNodes, faceFilter, currentNode);
            }

            if (newFace2 != -1)
            {
                createOrUpdateNode(newFace2, navmesh.faces[newFace2], navmesh.faceOrigins[newFace2], end, start, unEvaluatedNodes, faceFilter, currentNode);
            }

            if (newFace3 != -1)
            {
                createOrUpdateNode(newFace3, navmesh.faces[newFace3], navmesh.faceOrigins[newFace3], end, start, unEvaluatedNodes, faceFilter, currentNode);
            }
            

        }
        this.path.Add(start);
        this.path.AddRange(funnelPathfind(currentNode, start, new List<Vector3>(), 0));

        this.path.Add(end);
        for(int i = 0; i < this.path.Count-1; i++){
            Debug.DrawLine(this.path[i] + transform.up, this.path[i+1] + transform.up, Color.black, Time.deltaTime);
        }

        
        IsPathfinding = true;
    }

    private List<Vector3> funnelPathfind(AStarNode currentNode, Vector3 origin, List<Vector3> nodeList, int recursionIndex){
        recursionIndex ++;
        if(recursionIndex>MaxRecursionDepth || currentNode.Parent == null) return nodeList;

        float debugLineTime = 5;
        Debug.DrawRay(origin, -Vector3.up, Color.yellow, debugLineTime);
        Vector3 lastRight = getRight(currentNode, origin);
        Vector3 lastLeft = getLeft(currentNode, origin);
        Vector3 right = lastRight;
        Vector3 left = lastLeft;

        AStarNode lastRightNode = currentNode.Parent;
        AStarNode lastLeftNode = currentNode.Parent;
        Debug.DrawRay(lastRight, Vector3.up, Color.red, debugLineTime);
        Debug.DrawRay(lastLeft, -Vector3.up, Color.green, debugLineTime);

        currentNode = currentNode.Parent;
        //Backtraces through parents and adds faces to facepath list
        while (currentNode.Parent != null)
        {
            AStarNode nextNode = currentNode.Parent;
            Debug.DrawRay(currentNode.Face.Origin, Vector3.up*4, Color.cyan, debugLineTime);
            
            Vector3 newVert = new Vector3();

            //If next node does not contain current right, 
            if (!nextNode.Face.hasVertex(right))
            {
                newVert = getRight(nextNode, left);
                right = newVert;
                Debug.DrawRay(newVert, Vector3.up, Color.red,debugLineTime);

                Vector3 leftRightCross = Vector3.Cross(newVert - origin, lastLeft - origin);
                float leftRightDot = Vector3.Dot(leftRightCross, Vector3.up);

                if (leftRightDot > 0.0f)
                {
                    nodeList.Insert(0,lastLeft);
                    return funnelPathfind(getLastOccurance(lastLeftNode,lastLeft), lastLeft, nodeList, recursionIndex);
                }
                
                Vector3 newOldCross = Vector3.Cross(newVert - origin, lastRight - origin);
                float newOldDot = Vector3.Dot(newOldCross, Vector3.up);

                if(newOldDot > 0){
                    lastRight = newVert;
                    lastRightNode = nextNode;
                    if(currentNode.Face.hasVertex(lastLeft))lastLeftNode = nextNode;
                }else{
                    Debug.DrawLine(lastRight + Vector3.up, origin, Color.red, debugLineTime);
                    Debug.DrawLine(newVert + Vector3.up, origin, Color.yellow, debugLineTime);
                }
            }
            else
            {
                newVert = getLeft(nextNode, right);
                left = newVert;
                Debug.DrawRay(newVert, Vector3.up, Color.green, debugLineTime);

                Vector3 leftRightCross = Vector3.Cross(lastRight - origin, newVert - origin);
                float leftRightDot = Vector3.Dot(leftRightCross, Vector3.up);

                if (leftRightDot > 0.0f)
                {
                    nodeList.Insert(0,lastRight);
                    return funnelPathfind(getLastOccurance(lastRightNode,lastRight), lastRight, nodeList, recursionIndex);
                }

                Vector3 newOldCross = Vector3.Cross(lastLeft - origin, newVert - origin);
                float newOldDot = Vector3.Dot(newOldCross, Vector3.up);

                if(newOldDot > 0){
                    lastLeft = newVert;
                    lastLeftNode = nextNode;
                    if(currentNode.Face.hasVertex(lastRight))lastRightNode = nextNode;
                }else{
                    //Debug.DrawLine(lastLeft + Vector3.up, origin, Color.green, debugLineTime);
                    Debug.DrawLine(newVert + Vector3.up, origin, Color.yellow, debugLineTime);
                }
            }
            currentNode = nextNode;
            

        }

        return nodeList;
    }

    private List<T> listSum<T>(List<T> A, List<T> B)
    {
        List<T> ret = new List<T>();
        ret.AddRange(A);
        ret.AddRange(B);
        return ret;
    }

    private Vector3 getRight(AStarNode node, Vector3 origin)
    {
        return getLeftOrRight(node, origin, 1);
    }
    private Vector3 getLeft(AStarNode node, Vector3 origin)
    {
        return getLeftOrRight(node, origin, -1);
    }

    private AStarNode getLastOccurance(AStarNode node, Vector3 point){
        if(node.Parent != null && node.Parent.Face.hasVertex(point)){
            return getLastOccurance(node.Parent, point);
        }else{
            return node;
        }
    }
    private Vector3 getLeftOrRight(AStarNode node, Vector3 origin, int inverse)
    {
        //If someone knows how to do this in one line plz call me ;*
        Vector3 A = new Vector3();
        Vector3 B = new Vector3();
        Vector3 C = new Vector3();


        if(origin == node.Face.aPos)
        {
            A = node.Face.aPos;
            B = node.Face.bPos;
            C = node.Face.cPos;
        }
        else if(origin == node.Face.bPos)
        {
            A = node.Face.bPos;
            B = node.Face.cPos;
            C = node.Face.aPos;
        }
        else if(origin == node.Face.cPos)
        {
            A = node.Face.cPos;
            B = node.Face.bPos;
            C = node.Face.aPos;
        }else{
            
            return getLeftOrRight(node, getLonelyNode(node.Face, node.Parent.Face), inverse);
        }

        Vector3 normal = Vector3.Cross(B-A, C-A);
        float dot = Vector3.Dot(normal, transform.up);
        Vector3 DB = Vector3.Normalize(C - (B+C)/2);

        return dot * inverse > 0 ? C : B;
    }


    private Vector3 getLonelyNode(meshFace A, meshFace B)
    {
        if (!B.hasVertex(A.a)) return A.aPos;
        if (!B.hasVertex(A.b)) return A.bPos;
        if (!B.hasVertex(A.c)) return A.cPos;
        return new Vector3();
    }
    
}
