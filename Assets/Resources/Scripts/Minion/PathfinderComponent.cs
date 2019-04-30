using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PathfinderComponent : MonoBehaviour
{
    public bool UseRootMotion;
    public float MovementSpeed = 50;
    public float RotationSpeed = 10;
    public int MaxRecursionDepth = 20;
    public List<AStarRoomNode> path;
    public List<Vector3> subPath;
    public bool HasPath = false;
    public Transform navmeshTransform;

    public Vector3 TransformRaycastOffset;

    public float LegHeight;

    public float InteractRayLength = 0.4f;

    //Unstuck
    public float MinVelocityThreshold;
    public float StuckTimeThreshold;
    public float UnstuckRadius;
    private Vector3 lastPosition;
    private float currentStuckTime = 0;

    public bool debug;
    private NavMesh previousNavmesh;

    private Rigidbody rigidBody;
    private Animator animController;
    void Start()
    {
        this.rigidBody = GetComponent<Rigidbody>();
        this.animController = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(HasPath){
            move();
            if((transform.position - lastPosition).magnitude/Time.deltaTime < MinVelocityThreshold){
                currentStuckTime += Time.deltaTime;
                if(currentStuckTime >= StuckTimeThreshold){
                    unstuck();
                    currentStuckTime = 0;
                }
            }else{
                currentStuckTime = 0;
            }
        }else{
            if(UseRootMotion)
                animController.SetBool("IsWalking", false);
        }
        lastPosition = transform.position;
    }

    //Method for helping stuck entities
    private void unstuck(){
        Debug.Log("Attempting to unstuck!");
        Vector3 unstuckPoint = transform.position + new Vector3(Random.Range(-UnstuckRadius, UnstuckRadius), transform.position.y, Random.Range(-UnstuckRadius, UnstuckRadius));
        MoveTo(unstuckPoint);
    }

    

    private void move(){

        
        if(this.path.Count <= 0 && this.subPath.Count <= 0){
            this.HasPath = false;
            if(UseRootMotion){
                animController.SetBool("IsWalking", false);
            }else{
                rigidBody.velocity = Vector3.zero;
            }
            return;
        }
        //If subpath is empty, create new subpath
        if(this.subPath.Count <= 0){
            
            RaycastHit hit;
            if(!Physics.Raycast(transform.position + TransformRaycastOffset, -Vector3.up, out hit, 4)){
                Debug.LogError("Object " + transform.name + " could not find the floor and therefore not pathfind. This is very bad and cannot happen");
                this.HasPath = false;
                return;
            }


            this.navmeshTransform = this.path[path.Count-1].Parent.RoomRef;

            Vector3 endPos = navmeshTransform.InverseTransformPoint(this.path[path.Count-1].EntrancePos);
            Vector3 startPos = navmeshTransform.InverseTransformPoint(hit.point);

            this.path.RemoveAt(path.Count-1);

            NavMesh navmesh = navmeshTransform.GetComponent<NavMesh>();
            if(debug){
                if(previousNavmesh != null) previousNavmesh.draw = false;
                previousNavmesh = navmesh;
                navmesh.draw = true;
            }

            if(navmesh == null){
                Debug.LogError("Object " + hit.transform.name + " does not have a NavMesh component! Plz fix!");
                this.HasPath = false;
                return;
            }
            

            //Runs A* pathfind on faces, saves last face (reversed linked list)
            AStarFaceNode startNode = aStarFacePathfind(startPos, endPos, navmesh);



            //If A* linked list contains at least two items, run funnel stringpull
            if(startNode != null && startNode.Parent != null){
                //Replaces lone point on start node with end
                Vector3[] commonEndNodes = getCommonNodes(startNode.Face, startNode.Parent.Face);
                if(commonEndNodes.Length == 2){
                    startNode.Face = new navmeshFace(commonEndNodes[0], commonEndNodes[1], endPos);
                    this.subPath.AddRange(funnelStringpull(startNode, endPos, startPos, new List<Vector3>(), 0));
                }
            }
            this.subPath.Add(endPos);
            Debug.Log("Subpath generated for room " + navmeshTransform);

        if(subPath.Count <= 0){
            Debug.LogError("Subpath was not created! This is very bad");
        }

        }else{
            //Does the actual movement
            Vector3 deltaPos = navmeshTransform.TransformPoint(subPath[0]) - transform.position + transform.up * LegHeight;
            float distance = 0;
            Quaternion goalRot = Quaternion.LookRotation(deltaPos, transform.up);

            if(debug){
                Debug.DrawRay(transform.position, transform.forward, Color.magenta, Time.deltaTime);
                Debug.DrawRay(transform.position, goalRot*transform.forward, Color.red, Time.deltaTime);
            }
            
            transform.rotation = Quaternion.RotateTowards(transform.rotation, goalRot, RotationSpeed);
            
            if(deltaPos.magnitude <= 0.1f){
                distance = deltaPos.magnitude;
                this.subPath.RemoveAt(0);
            }else{
                distance = this.MovementSpeed * Time.deltaTime;
            }
            //rigidBody.MovePosition(transform.position + transform.forward * this.MovementSpeed * Time.deltaTime );
            if(UseRootMotion){
                animController.SetBool("IsWalking", true);
            }else{
                rigidBody.velocity = transform.forward * this.MovementSpeed * Time.deltaTime;
            }
            
        }

        if(debug){
            for(int i = 0; i < this.subPath.Count - 1; i++){
                Debug.DrawLine(navmeshTransform.TransformPoint(this.subPath[i]), navmeshTransform.TransformPoint(this.subPath[i+1]), Color.magenta, Time.deltaTime);
            }

            if(this.subPath.Count > 0 ) Debug.DrawLine(transform.position, navmeshTransform.TransformPoint(this.subPath[0]), Color.magenta, Time.deltaTime);

            for(int i = 0; i < this.path.Count - 1; i++){
                Debug.DrawLine(this.path[i].EntrancePos, this.path[i+1].EntrancePos, Color.cyan, Time.deltaTime);
            }

            if(this.path.Count > 0 ) Debug.DrawLine(transform.position, this.path[0].EntrancePos, Color.cyan, Time.deltaTime);
        }
        
        RaycastHit doorHit;
        if(Physics.Raycast(transform.position, transform.forward, out doorHit, InteractRayLength)){
            DoorComponent doorComponent = doorHit.transform.GetComponent<DoorComponent>();
            if(doorComponent != null){
                doorComponent.OnInteractBegin(gameObject);
            }
        }

    }




    public void MoveTo(Vector3 endPos)
    {

        //Raycasts to floor on destination
        RaycastHit endHit;
        Transform endRoom = null;
        if(Physics.Raycast(endPos + TransformRaycastOffset, Vector3.down, out endHit))
        {
            endRoom = endHit.transform.parent;
            endPos = endHit.point;
        }else{
            return;
        }

        //Raycasts to floor on start
        RaycastHit startHit;
        Transform startRoom;
        if (Physics.Raycast(transform.position + TransformRaycastOffset, -transform.up, out startHit))
        {
            startRoom = startHit.transform.parent;
        }else{
            return;
        }

        //Clears previous path
        this.path.Clear();
        this.subPath.Clear();
        this.HasPath = false;

        //If a room was not hit, abort
        if(endRoom.GetComponent<NavMesh>() == null || startRoom.GetComponent<NavMesh>() == null){
            return;
        }

        //Creates room pathfind
        this.path = aStarRoomPathfind(startRoom, endRoom);

        //If roompath was not found, abort
        if(this.path == null){
            this.path = new List<AStarRoomNode>();
            return;
        }

        //Adds destination to roompath
        if(this.path.Count > 0){ 
            this.path.Insert(0, new AStarRoomNode(0,this.path[0].RoomRef, this.path[0], endPos));
        }else{
            this.path.Insert(0, new AStarRoomNode(0, startRoom, null, endPos));
            this.path[0].Parent = this.path[0];
        }

        HasPath = true;
    }

    #region room
    private List<AStarRoomNode> aStarRoomPathfind(Transform startRoom, Transform endRoom){
        List<AStarRoomNode> returnList = new List<AStarRoomNode>();
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

            //If destination is reached, follow room parents back and return as list
            if(currentNode.RoomRef == endRoom){
                while(currentNode.Parent != null){
                    returnList.Add(currentNode);
                    currentNode = currentNode.Parent;
                }
                return returnList;
            }

            unevaluatedNodes.Remove(currentNode);
            evaluatedNodes.Add(currentNode);

            foreach(AnchorPoint door in currentNode.RoomRef.GetComponent<DoorReferences>().doors){
                if(door.Connected && door.ConnectedTo != null){
                    Transform adjacentRoom = door.ConnectedTo.transform.parent.transform.parent;
                    if(!checkedRooms.Contains(adjacentRoom)){
                        unevaluatedNodes.Add(new AStarRoomNode(Vector3.Distance(startRoom.position, adjacentRoom.position) + Vector3.Distance(endRoom.position, adjacentRoom.position), adjacentRoom, currentNode, door.transform.position));
                        checkedRooms.Add(adjacentRoom);
                    }
                }
            }

        }
        return null;
        //If path was not found
        
    }

    [System.Serializable]
    public class AStarRoomNode
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

    #region face
    private AStarFaceNode aStarFacePathfind(Vector3 start, Vector3 end, NavMesh navmesh){

        List<AStarFaceNode> unevaluatedNodes = new List<AStarFaceNode>();
        List<AStarFaceNode> evaluatedNodes = new List<AStarFaceNode>();
        List<navmeshFace> faceFilter = new List<navmeshFace>();
        List<AStarFaceNode> facePath = new List<AStarFaceNode>();



        navmeshFace startFace = navmesh.getFaceFromPoint(start);
        navmeshFace endFace = navmesh.getFaceFromPoint(end);


        if(debug){
            Debug.DrawRay(startFace.Origin + navmeshTransform.position, navmeshTransform.up * 5, Color.green,5);
            Debug.DrawRay(endFace.Origin + navmeshTransform.position, navmeshTransform.up * 5, Color.red,5);
        }

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
            if (currentNode.Face == endFace){
                break;
            };

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

    #endregion

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
