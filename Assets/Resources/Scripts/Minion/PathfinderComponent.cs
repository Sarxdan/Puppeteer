using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PathfinderComponent : NetworkBehaviour
{

    [Header("Movement settings")]
    public bool UseRootMotion;
    public float MovementSpeed = 50;
    public float RotationSpeed = 10;
    public float LegHeight;

    [HideInInspector]
    public Vector3 Velocity;

    [Header("Pathfinding settings")]
    public int MaxRecursionDepth = 20;
    [HideInInspector]
    public bool HasPath = false;
    public Vector3 TransformRaycastOffset;
    public float NodeArrivalMargin;
    
    private Transform navmeshTransform;
    private List<AStarRoomNode> worldPath;
    private List<Vector3> roomPath;
    public bool PathfindDebug;



    [Header("Obstacle avoidance settings")]
    //All-that-aren't-minions stuff
    public float RaycastAvoidDistance;

    public float RaycastAvoidAngle;
    public float RaycastAvoidWeight;

    private Quaternion rightRayRotation;
    private Quaternion leftRayRotation;

    private Vector3 raycastAvoidVector;

    //Minion stuff
    public float MinionAvoidDistance;
    public float MinionAvoidWeight;

    private Vector3 minionAvoidVector;
    private static int layerMask;

    public bool ObstacleAvoidDebug;

    [Header("Unstuck settings")]
    public float MinVelocityThreshold;
    public float StuckTimeThreshold;
    public float UnstuckRadius;
    private Vector3 lastPosition;
    private float currentStuckTime = 0;
    

    [Header("Misc. settings")]
    public float DoorInteractRange;
    //References
    private Rigidbody rigidBody;
    private Animator animController;
    [Range(0,1)]
    public float ForceSmoothingValue;

    public float ClampValues;

    void Start()
    {
        layerMask = ~(1 << LayerMask.NameToLayer("Puppeteer Interact"));
        if(!isServer)
        {
            this.enabled = false;
            return;
        }
        this.rigidBody = GetComponent<Rigidbody>();
        this.animController = GetComponent<Animator>();
        this.worldPath = new List<AStarRoomNode>();
        this.roomPath = new List<Vector3>();
        rightRayRotation =  Quaternion.AngleAxis(RaycastAvoidAngle,Vector3.up);
        leftRayRotation = Quaternion.AngleAxis(-RaycastAvoidAngle, Vector3.up);
    }

    // Update is called once per frame
    void Update()
    {
        //Updates object velocity
        Velocity = (transform.position - lastPosition)/Time.deltaTime;
        lastPosition = transform.position;

        if(HasPath)
        {
            avoidanceCheck();
            doorInteraction();
            updatePath();
            performMove();
            stuckCheck();
        }
        else if(UseRootMotion)
        {
            animController.SetBool("Moving", false);
        }
    }

    

    public void Stop()
    {
        this.worldPath.Clear();
        this.roomPath.Clear();
        this.HasPath = false;
        this.animController.SetBool("Moving", false);
    }

    public void MoveTo(Vector3 endPos)
    {
        if(!isServer) return;

        RaycastHit startHit = new RaycastHit();
        RaycastHit endHit = new RaycastHit();
        try{
            //Raycasts to floor on destination
            if(!Physics.Raycast(endPos + TransformRaycastOffset, Vector3.down, out endHit))
                return;

            //Raycasts to floor on start
            if (!Physics.Raycast(transform.position + TransformRaycastOffset, -transform.up, out startHit))
                return;

            //Clears previous path
            Stop();

            Transform endRoom = endHit.transform.GetComponentInParent<NavMesh>().transform;
            Transform startRoom = startHit.transform.GetComponentInParent<NavMesh>().transform;
            //If a room was not hit, abort
            if(endRoom == null || startRoom == null)
            {
                return;
            }

            
            //Creates room pathfind
            this.worldPath = aStarRoomPathfind(startRoom, endRoom);

            //If roompath was not found, abort
            if(this.worldPath == null)
            {
                this.worldPath = new List<AStarRoomNode>();
                return;
            }

            //Adds destination to roompath
            if(this.worldPath.Count > 0)
            { 
                this.worldPath.Insert(0, new AStarRoomNode(0,this.worldPath[0].RoomRef, this.worldPath[0], endPos));
            }
            else
            {
                this.worldPath.Insert(0, new AStarRoomNode(0, startRoom, null, endPos));
                this.worldPath[0].Parent = this.worldPath[0];
            }

            HasPath = true;
        }
        catch(System.NullReferenceException e)
        {
            Debug.LogWarning("Pathfinder error: Raycast hit something that wasn't a room! Start: " + startHit.transform.name + "end: " + endHit.transform.name);
            if(startHit.transform.name == "LevelReturn") GetComponent<StateMachine>().Despawn();
            
            Stop();
        }
    }
    
    //Method for helping stuck entities
    private void stuckCheck()
    {
        if(Velocity.magnitude < MinVelocityThreshold)
        {

            currentStuckTime += Time.deltaTime;
            if(currentStuckTime >= StuckTimeThreshold)
            {
                Vector3 unstuckPoint = transform.position + new Vector3(Random.Range(-UnstuckRadius, UnstuckRadius), transform.position.y, Random.Range(-UnstuckRadius, UnstuckRadius));
                MoveTo(unstuckPoint);
                currentStuckTime = 0;
            }
        }
        else
        {
            currentStuckTime = 0;
        }
    }
    
    private void avoidanceCheck()
    {
        Transform closestMinion = null;
        float closestMinionDistance = Mathf.Infinity;
        foreach(StateMachine minion in EnemySpawner.AllMinions)
        {
            Vector3 localPos = transform.InverseTransformPoint(minion.transform.position);
            if(localPos.z <= 0) continue;
                if(localPos.magnitude < closestMinionDistance)
                {
                    closestMinion = minion.transform;
                    closestMinionDistance = localPos.magnitude;
                }

        }

        if(closestMinionDistance < MinionAvoidDistance){
            minionAvoidVector = minionAvoidVector * (1-ForceSmoothingValue) + ((transform.position - closestMinion.position).normalized/closestMinionDistance) *ForceSmoothingValue;
        }
        else
        {
            minionAvoidVector = minionAvoidVector * (1-ForceSmoothingValue) + Vector3.zero * ForceSmoothingValue;
        }


        RaycastHit hit;
        Vector3 newRaycastAvoidVector = Vector3.zero;

        if(Physics.Raycast(transform.position + TransformRaycastOffset, transform.forward, out hit, RaycastAvoidDistance, layerMask) &&
        hit.transform.tag != "Enemy" &&
        hit.transform.tag != "Player")
        {
            newRaycastAvoidVector += hit.normal / hit.distance;
        }



        if(Physics.Raycast(transform.position + TransformRaycastOffset, rightRayRotation * transform.forward, out hit, RaycastAvoidDistance, layerMask) &&
        hit.transform.tag != "Enemy" &&
        hit.transform.tag != "Player")
        {
            newRaycastAvoidVector += hit.normal / hit.distance;
        }

        if(Physics.Raycast(transform.position + TransformRaycastOffset, leftRayRotation * transform.forward, out hit, RaycastAvoidDistance, layerMask) &&
        hit.transform.tag != "Enemy" &&
        hit.transform.tag != "Player")
        {
            newRaycastAvoidVector += hit.normal / hit.distance;
        }

        raycastAvoidVector = raycastAvoidVector * (1-ForceSmoothingValue) + newRaycastAvoidVector * 0.3f * ForceSmoothingValue;
    }

    private void doorInteraction()
    {
        if(Physics.Raycast(transform.position + TransformRaycastOffset, transform.forward, out RaycastHit hit, DoorInteractRange, layerMask))
        {
            if(hit.transform.tag.Equals("Door")){
                DoorComponent door = hit.transform.GetComponent<DoorComponent>();
                if(!door.Open){
                    door.OnInteractBegin(gameObject);
                }
            }
        }
    }

    //Rebuilds path if neccecary
    private void updatePath()
    {
        //If both path and subpath are empty, stop pathfinding
        if(this.worldPath.Count <= 0 && this.roomPath.Count <= 0)
        {
            this.HasPath = false;
            if(UseRootMotion)
            {
                animController.SetBool("Moving", false);
            }
            else
            {
                rigidBody.velocity = Vector3.zero;
            }
            return;
        }

        //If subpath is empty, create new subpath
        if(this.roomPath.Count == 0)
        {
            //Raycasts to floor
            RaycastHit hit;
            if(!Physics.Raycast(transform.position + TransformRaycastOffset, -Vector3.up, out hit, 4))
            {
                Debug.LogError("Pathfind error: Agent " + transform.name + " could not find the floor and therefore not pathfind. This is very bad and cannot happen");
                this.HasPath = false;
                return;
            }

            //Fetches room transform from worldpath
            this.navmeshTransform = this.worldPath[worldPath.Count-1].Parent.RoomRef;

            //Defines start + end
            Vector3 endPos = navmeshTransform.InverseTransformPoint(this.worldPath[worldPath.Count-1].EntrancePos);
            Vector3 startPos = navmeshTransform.InverseTransformPoint(hit.point);

            //Removes node from worldpath
            this.worldPath.RemoveAt(worldPath.Count-1);

            //Fetches navmesh data
            NavMesh navmesh = navmeshTransform.GetComponent<NavMesh>();

            //Safety check
            if(navmesh == null)
            {
                Debug.LogError("Object " + hit.transform.name + " does not have a NavMesh component! Plz fix!");
                this.HasPath = false;
                return;
            }
            

            //Runs A* pathfind on faces, saves last face (reversed linked list)
            AStarFaceNode startNode = aStarFacePathfind(startPos, endPos, navmesh);

            //If A* linked list contains at least two items, run funnel stringpull
            if(startNode != null && startNode.Parent != null)
            {
                //Replaces lone point on start node with end
                Vector3[] commonEndNodes = getCommonNodes(startNode.Face, startNode.Parent.Face);
                if(commonEndNodes.Length == 2)
                {
                    startNode.Face = new navmeshFace(commonEndNodes[0], commonEndNodes[1], endPos);
                    this.roomPath.AddRange(funnelStringpull(startNode, endPos, startPos, new List<Vector3>(), 0));
                }
            }

            this.roomPath.Add(endPos);

            if(roomPath.Count <= 0)
            {
                Debug.LogError("Subpath was not created! This is very bad");
            }

        }
    }

    private void performMove()
    {
        if(this.roomPath.Count != 0)
        {
            //Does the actual movement
            Vector3 deltaPos = navmeshTransform.TransformPoint(roomPath[0]) - transform.position + transform.up * LegHeight;
            float distance = 0;
            Vector3 forceSum = deltaPos.normalized + minionAvoidVector * MinionAvoidWeight + raycastAvoidVector * RaycastAvoidWeight;
            forceSum.y = 0;
            Quaternion goalRot = Quaternion.LookRotation(forceSum, transform.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, goalRot, RotationSpeed);
            
            if(ObstacleAvoidDebug)
            {
                Debug.DrawRay(transform.position, forceSum, Color.green);
                Debug.DrawRay(transform.position, deltaPos, Color.yellow);
                Debug.DrawRay(transform.position, minionAvoidVector * MinionAvoidWeight, Color.red);
                Debug.DrawRay(transform.position, raycastAvoidVector * RaycastAvoidWeight, Color.cyan);
                
            }


            if(deltaPos.magnitude <= NodeArrivalMargin)
            {
                distance = deltaPos.magnitude;
                this.roomPath.RemoveAt(0);
            }
            else
            {
                distance = this.MovementSpeed * Time.deltaTime;
            }
            //rigidBody.MovePosition(transform.position + transform.forward * this.MovementSpeed * Time.deltaTime );
            if(UseRootMotion)
            {
                animController.SetBool("Moving", true);
            }
            else
            {
                rigidBody.velocity = transform.forward * this.MovementSpeed * Time.deltaTime;
            }
            
        }

        //Draws current path if debug is on
        if(PathfindDebug)
        {
            for(int i = 0; i < this.roomPath.Count - 1; i++)
            {
                Debug.DrawLine(navmeshTransform.TransformPoint(this.roomPath[i]), navmeshTransform.TransformPoint(this.roomPath[i+1]), Color.magenta, Time.deltaTime);
            }

            if(this.roomPath.Count > 0 ) Debug.DrawLine(transform.position, navmeshTransform.TransformPoint(this.roomPath[0]), Color.magenta, Time.deltaTime);

            for(int i = 0; i < this.worldPath.Count - 1; i++)
            {
                Debug.DrawLine(this.worldPath[i].EntrancePos, this.worldPath[i+1].EntrancePos, Color.cyan, Time.deltaTime);
            }

            if(this.worldPath.Count > 0 ) Debug.DrawLine(transform.position, this.worldPath[worldPath.Count-1].EntrancePos, Color.cyan, Time.deltaTime);
        }
        

    }




    

    #region room
    private List<AStarRoomNode> aStarRoomPathfind(Transform startRoom, Transform endRoom)
    {
        List<AStarRoomNode> returnList = new List<AStarRoomNode>();
        List<AStarRoomNode> unevaluatedNodes = new List<AStarRoomNode>();
        List<AStarRoomNode> evaluatedNodes = new List<AStarRoomNode>();
        List<Transform> checkedRooms = new List<Transform>();
        AStarRoomNode currentNode = null;

        unevaluatedNodes.Add(new AStarRoomNode(Vector3.Distance(endRoom.position, startRoom.position), startRoom, null, new Vector3()));
        checkedRooms.Add(startRoom);

        while(unevaluatedNodes.Count > 0)
        {
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
            if(currentNode.RoomRef == endRoom)
            {
                while(currentNode.Parent != null)
                {
                    returnList.Add(currentNode);
                    currentNode = currentNode.Parent;
                }
                return returnList;
            }

            unevaluatedNodes.Remove(currentNode);
            evaluatedNodes.Add(currentNode);

            foreach(AnchorPoint door in currentNode.RoomRef.GetComponent<DoorReferences>().doors)
            {
                if(door.Connected && door.ConnectedTo != null)
                {
                    Transform adjacentRoom = door.ConnectedTo.transform.GetComponentInParent<DoorReferences>().transform; //Ugly but secure way to get room transform
                    if(!checkedRooms.Contains(adjacentRoom))
                    {
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

        public AStarRoomNode(float fCost, Transform roomRef, AStarRoomNode parent, Vector3 entrancePos)
        {
            this.FCost = fCost;
            this.RoomRef = roomRef;
            this.Parent = parent;
            this.EntrancePos = entrancePos;
        }
    }

    

    #endregion

    #region navmesh

    #region face
    private AStarFaceNode aStarFacePathfind(Vector3 start, Vector3 end, NavMesh navmesh)
    {

        List<AStarFaceNode> unevaluatedNodes = new List<AStarFaceNode>();
        List<AStarFaceNode> evaluatedNodes = new List<AStarFaceNode>();
        List<navmeshFace> faceFilter = new List<navmeshFace>();
        List<AStarFaceNode> facePath = new List<AStarFaceNode>();



        navmeshFace startFace = navmesh.getFaceFromPoint(start);
        navmeshFace endFace = navmesh.getFaceFromPoint(end);


        if(PathfindDebug)
        {
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
            if (currentNode.Face == endFace)
            {
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
        if(faceFilter.Contains(face))
        {
            unEvaluatedNodes.Find(x => x.Face == face);
        }
        else
        {
            unEvaluatedNodes.Add(new AStarFaceNode(Vector3.Distance(start, face.Origin) + Vector3.Distance(face.Origin, end), face, parent));
            faceFilter.Add(face);
        }
    }

    #endregion

    private List<Vector3> funnelStringpull(AStarFaceNode currentNode, Vector3 origin, Vector3 end, List<Vector3> nodeList, int recursionIndex)
    {
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

            if(PathfindDebug) Debug.DrawRay(navmeshTransform.TransformPoint(currentNode.Face.Origin), Vector3.up, Color.white, 2);


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
                if(newOldDot > 0)
                {
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
                if(newOldDot > 0)
                {
                    lastRight = rightPortal;
                    lastLeftNode = nextNode;
                    if(currentNode.Face.hasVertex(lastLeft))lastRightNode = nextNode;
                }
            }
            currentNode = nextNode;
            
        }

        Vector3 endLeftCross = Vector3.Cross(end - origin, lastLeft - origin);
        Vector3 rightEndCross = Vector3.Cross(lastRight - origin, end - origin);

        if(Vector3.Dot(endLeftCross, Vector3.up) < 0)
        {
            nodeList.Insert(0,lastLeft);
            return funnelStringpull(getLastOccurance(lastRightNode,lastLeft), lastLeft, end, nodeList, recursionIndex);
        }
        else if(Vector3.Dot(rightEndCross,Vector3.up) < 0)
        {
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

    private AStarFaceNode getLastOccurance(AStarFaceNode node, Vector3 point)
    {
        if(node.Parent != null && node.Parent.Face.hasVertex(point))
        {
            return getLastOccurance(node.Parent, point);
        }
        else
        {
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
        }
        else{
            
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
