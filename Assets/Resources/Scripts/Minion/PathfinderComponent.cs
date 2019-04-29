using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PathfinderComponent : MonoBehaviour
{
    public float MovementSpeed = 1;
    public int MaxRecursionDepth = 20;
    public List<Vector3> path;
    public List<Vector3> subPath;
    public bool HasPath = false;
    private Transform navmeshTransform;

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
        if(HasPath){
            move();
        }
    }

    

    private void move(){

        
        if(this.path.Count <= 0 && this.subPath.Count <= 0){
            this.HasPath = false;
            return;
        }

        //If subpath is empty, create new subpath
        if(this.subPath.Count <= 0){
            //Pops end position
            Vector3 endPos = this.path[path.Count-1];
            this.path.RemoveAt(path.Count-1);

            RaycastHit hit;
            if(!Physics.Raycast(transform.position, -Vector3.up, out hit, 4)){
                Debug.LogError("Object " + transform.name + " could not find the floor and therefore not pathfind. This is very bad and cannot happen");
                this.HasPath = false;
                return;
            }
            NavMesh navmesh = hit.transform.root.GetComponent<NavMesh>();
            if(navmesh == null){
                Debug.LogError("Object " + hit.transform.name + " does not have a NavMesh component! Plz fix!");
                this.HasPath = false;
                return;
            }

            //Runs A* pathfind on faces, saves last face (reversed linked list)
            AStarFaceNode startNode = aStarFacePathfind(hit.point, endPos, navmesh);
            navmeshTransform = hit.transform;

            //If A* linked list contains at least two items, run funnel stringpull
            if(startNode != null && startNode.Parent != null){
                //Replaces lone point on start node with end
                Vector3[] commonEndNodes = getCommonNodes(startNode.Face, startNode.Parent.Face);
                if(commonEndNodes.Length == 2){
                    startNode.Face = new navmeshFace(commonEndNodes[0], commonEndNodes[1], endPos);
                    this.subPath.AddRange(funnelStringpull(startNode, endPos, hit.point, new List<Vector3>(), 0));
                    this.subPath.Add(navmeshTransform.InverseTransformPoint(endPos));
                }
            }

            for(int i = 0; i < this.subPath.Count -1; i++){
                Debug.DrawLine(this.subPath[i], this.subPath[i+1], Color.green, 3);
            }

        }else{
            //Does the actual movement
            Vector3 deltaPos = navmeshTransform.TransformPoint(subPath[0]) - transform.position + transform.up * LegHeight;
            float distance = 0;
            
            if(deltaPos.magnitude < this.MovementSpeed * Time.deltaTime){
                distance = deltaPos.magnitude;
                this.subPath.RemoveAt(0);
            }else{
                distance = this.MovementSpeed * Time.deltaTime;
            }
            rigidBody.MovePosition(transform.position + deltaPos.normalized * this.MovementSpeed * Time.deltaTime );
        }
    }


    public void MoveTo(Vector3 endPos, Transform endRoom)
    {

        RaycastHit hit;
        Transform startRoom;
        if (Physics.Raycast(transform.position, -transform.up, out hit))
        {
            startRoom = hit.transform.root;
        }else{
            return;
        }

        this.path = aStarRoomPathfind(startRoom, endRoom);
        this.path.Insert(0,endPos);

        for(int i = 0; i < this.path.Count-1; i++){
            Debug.DrawLine(this.path[i], this.path[i+1], Color.cyan, 3);
        }

        HasPath = true;
    }

    #region room
    private List<Vector3> aStarRoomPathfind(Transform startRoom, Transform endRoom){
        List<Vector3> returnList = new List<Vector3>();
        List<AStarRoomNode> unevaluatedNodes = new List<AStarRoomNode>();
        List<AStarRoomNode> evaluatedNodes = new List<AStarRoomNode>();
        List<Transform> checkedRooms = new List<Transform>();
        AStarRoomNode currentNode = null;

        unevaluatedNodes.Add(new AStarRoomNode(Vector3.Distance(endRoom.position, startRoom.position), startRoom, null, new Vector3()));
        checkedRooms.Add(startRoom);

        while(unevaluatedNodes.Count > 0){
            float lowestFCost = Mathf.Infinity;
            currentNode = null;
            foreach(AStarRoomNode node in unevaluatedNodes)
            {
                if(node.FCost < lowestFCost)
                {
                    lowestFCost = node.FCost;
                    currentNode = node;
                }
            }

            if(currentNode.RoomRef == endRoom){
                break;
            }

            unevaluatedNodes.Remove(currentNode);
            evaluatedNodes.Add(currentNode);

            foreach(AnchorPoint door in currentNode.RoomRef.GetComponent<DoorReferences>().doors){
                if(door.Connected){
                    Transform adjacentRoom = door.ConnectedTo.transform.parent.transform.parent;
                    if(!checkedRooms.Contains(adjacentRoom)){
                        unevaluatedNodes.Add(new AStarRoomNode(Vector3.Distance(startRoom.position, adjacentRoom.position) + Vector3.Distance(endRoom.position, adjacentRoom.position), adjacentRoom, currentNode, door.transform.position));
                        checkedRooms.Add(adjacentRoom);
                        Debug.DrawRay(adjacentRoom.position, Vector3.up * 10, Color.red, 3);
                    }
                }
            }

        }
        
        while(currentNode.Parent != null){
            returnList.Add(currentNode.EntrancePos);
            currentNode = currentNode.Parent;
        }
        return returnList;
    }

    private class AStarRoomNode
    {
        public float FCost;
        public Transform RoomRef;
        public AStarRoomNode Parent;
        public Vector3 EntrancePos;

        public AStarRoomNode(float fCost, Transform roomRef, AStarRoomNode parent, Vector3 entrancePos){
            this.FCost = fCost;
            this.RoomRef = roomRef;
            this.Parent = parent;
            this.EntrancePos = entrancePos;
        }
    }

    

    #endregion

    #region navmesh
    private AStarFaceNode aStarFacePathfind(Vector3 start, Vector3 end, NavMesh navmesh){

        List<AStarFaceNode> unevaluatedNodes = new List<AStarFaceNode>();
        List<AStarFaceNode> evaluatedNodes = new List<AStarFaceNode>();
        List<navmeshFace> faceFilter = new List<navmeshFace>();
        List<AStarFaceNode> facePath = new List<AStarFaceNode>();
        this.subPath = new List<Vector3>();


        navmeshFace startFace = navmesh.getFaceFromPoint(start);
        navmeshFace endFace = navmesh.getFaceFromPoint(end);

        if(startFace == null || endFace == null) return null;

        createOrUpdateNode(startFace, end, start, unevaluatedNodes, faceFilter, null);

        AStarFaceNode currentNode = null;

        while (unevaluatedNodes.Count > 0)
        {
            //Fetches unevaluated node with lowest FCost
            float lowestFCost = Mathf.Infinity;
            foreach(AStarFaceNode node in unevaluatedNodes)
            {
                if(node.FCost < lowestFCost)
                {
                    lowestFCost = node.FCost;
                    currentNode = node;
                }
            }

            //Moves node to evaluated list
            unevaluatedNodes.Remove(currentNode);
            evaluatedNodes.Add(currentNode);

            //Check if current face is destination
            if (currentNode.Face == endFace) break;

            //Fetches adjacent faces, filtering already fetched faces
            navmeshFace newFace1 = navmesh.getFaceFromEdge(currentNode.Face.aPos, currentNode.Face.bPos, faceFilter);
            navmeshFace newFace2 = navmesh.getFaceFromEdge(currentNode.Face.aPos, currentNode.Face.cPos, faceFilter);
            navmeshFace newFace3 = navmesh.getFaceFromEdge(currentNode.Face.bPos, currentNode.Face.cPos, faceFilter);

            //If edge has a face, add it as a A* node
            if (newFace1 != null)
            {
                createOrUpdateNode(newFace1,  end, start, unevaluatedNodes, faceFilter, currentNode);
            }

            if (newFace2 != null)
            {
                createOrUpdateNode(newFace2, end, start, unevaluatedNodes, faceFilter, currentNode);
            }

            if (newFace3 != null)
            {
                createOrUpdateNode(newFace3, end, start, unevaluatedNodes, faceFilter, currentNode);
            }
        }
        return currentNode;

    }
    private void createOrUpdateNode(navmeshFace face, Vector3 start, Vector3 end, List<AStarFaceNode> unEvaluatedNodes, List<navmeshFace> faceFilter, AStarFaceNode parent)
    {
        if(faceFilter.Contains(face)){
            unEvaluatedNodes.Find(x => x.Face == face);
        }else{
            unEvaluatedNodes.Add(new AStarFaceNode(Vector3.Distance(start, face.Origin) + Vector3.Distance(face.Origin, end), face, parent));
            faceFilter.Add(face);
        }
    }

    private List<Vector3> funnelStringpull(AStarFaceNode currentNode, Vector3 origin, Vector3 end, List<Vector3> nodeList, int recursionIndex){
        recursionIndex ++;
        if(recursionIndex>MaxRecursionDepth || currentNode.Parent == null) return nodeList;

        //Last added left and right
        Vector3 lastLeft = getLeft(currentNode, origin);
        Vector3 lastRight = getRight(currentNode, origin);

        //
        Vector3 leftPortal = lastLeft;
        Vector3 rightPortal = lastRight;

        AStarFaceNode lastRightNode = currentNode.Parent;
        AStarFaceNode lastLeftNode = currentNode.Parent;

        currentNode = currentNode.Parent;
        //Backtraces through parents and adds faces to facepath list
        while(currentNode.Parent != null) 
        {
            AStarFaceNode nextNode = currentNode.Parent;
            //Defines next AStarNode

            //If next face does not contain current left vert
            if (!nextNode.Face.hasVertex(leftPortal))
            {
                //Moves left portal to next node point
                leftPortal = getLeft(nextNode, rightPortal);

                //Calculates (lastRight x leftPortal) * (Up) from origin
                Vector3 leftRightCross = Vector3.Cross(lastRight - origin, leftPortal - origin);
                float leftRightDot = Vector3.Dot(leftRightCross, Vector3.up);

                //Checks if left portal has crossed right, marks right and recurses from there if so
                if (leftRightDot < 0)
                {
                    nodeList.Insert(0,lastRight);
                    return funnelStringpull(getLastOccurance(lastLeftNode,lastRight), lastRight, end, nodeList, recursionIndex);
                }
                
                //Calculates (leftportal x lastLeft) * (Up) from origin
                Vector3 newOldCross = Vector3.Cross(leftPortal - origin, lastLeft - origin);
                float newOldDot = Vector3.Dot(newOldCross, Vector3.up);

                //Checks if new cone is smaller than previous. If so, narrows the cone to rightportal
                if(newOldDot > 0){
                    lastLeft = leftPortal;
                    lastRightNode = nextNode;
                    if(currentNode.Face.hasVertex(lastRight))lastLeftNode = nextNode;
                }
            }
            else //Next face has left vert, but not right
            {
                //Moves right portal to next node point
                rightPortal = getRight(nextNode, leftPortal);

                //Calculates (rightPortal x lastLeft) * (Up) from origin
                Vector3 leftRightCross = Vector3.Cross(rightPortal - origin, lastLeft - origin);
                float leftRightDot = Vector3.Dot(leftRightCross, Vector3.up);

                //Checks if right portal has crossed left, marks left and recurses from there if so
                if (leftRightDot < 0)
                {
                    nodeList.Insert(0,lastLeft);
                    return funnelStringpull(getLastOccurance(lastRightNode,lastLeft), lastLeft, end, nodeList, recursionIndex);
                }

                //Calculates (rightPortal x lastRight) * (Up) from origin
                Vector3 newOldCross = Vector3.Cross(lastRight - origin, rightPortal - origin);
                float newOldDot = Vector3.Dot(newOldCross, Vector3.up);

                //Checks if new cone is smaller than previous. If so, narrows the cone to leftPortal
                if(newOldDot > 0){
                    lastRight = rightPortal;
                    lastLeftNode = nextNode;
                    if(currentNode.Face.hasVertex(lastLeft))lastRightNode = nextNode;
                }
            }
            currentNode = nextNode;
            

        }

        Vector3 endLeftCross = Vector3.Cross(end - origin, lastLeft - origin);
        Vector3 rightEndCross = Vector3.Cross(lastRight - origin, end - origin);

        if(Vector3.Dot(endLeftCross, Vector3.up) < 0){
            nodeList.Insert(0,lastLeft);
            return funnelStringpull(getLastOccurance(lastRightNode,lastLeft), lastLeft, end, nodeList, recursionIndex);
        }else if(Vector3.Dot(rightEndCross,Vector3.up) < 0){
            nodeList.Insert(0,lastRight);
            return funnelStringpull(getLastOccurance(lastLeftNode,lastRight), lastRight, end, nodeList, recursionIndex);
        }


        return nodeList;
    }
    class AStarFaceNode
    {
        public float FCost;
        public navmeshFace Face;
        public AStarFaceNode Parent;
        public AStarFaceNode(float fCost, navmeshFace Face, AStarFaceNode parent)
        {
            this.FCost = fCost;
            this.Parent = parent;
            this.Face = Face;
        }
    }
    private List<T> listSum<T>(List<T> A, List<T> B)
    {
        List<T> ret = new List<T>();
        ret.AddRange(A);
        ret.AddRange(B);
        return ret;
    }

    private Vector3 getLeft(AStarFaceNode node, Vector3 origin)
    {
        return getLeftOrRight(node, origin, 1);
    }
    private Vector3 getRight(AStarFaceNode node, Vector3 origin)
    {
        return getLeftOrRight(node, origin, -1);
    }

    private AStarFaceNode getLastOccurance(AStarFaceNode node, Vector3 point){
        if(node.Parent != null && node.Parent.Face.hasVertex(point)){
            return getLastOccurance(node.Parent, point);
        }else{
            return node;
        }
    }
    private Vector3 getLeftOrRight(AStarFaceNode node, Vector3 origin, int inverse)
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


    private Vector3 getLonelyNode(navmeshFace A, navmeshFace B)
    {
        if (!B.hasVertex(A.aPos)) return A.aPos;
        if (!B.hasVertex(A.bPos)) return A.bPos;
        if (!B.hasVertex(A.cPos)) return A.cPos;
        return new Vector3();
    }

    private Vector3[] getCommonNodes(navmeshFace A, navmeshFace B)
    {
        List<Vector3> ret = new List<Vector3>();
        if (B.hasVertex(A.aPos)) ret.Add(A.aPos);
        if (B.hasVertex(A.bPos)) ret.Add(A.bPos);
        if (B.hasVertex(A.cPos)) ret.Add(A.cPos);
        return ret.ToArray();
    }
    #endregion

}
