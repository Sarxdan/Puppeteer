// using System.Collections;
// using System.Collections.Generic;
// using Mirror;
// using UnityEngine;

// public class ItemGrabTool : NetworkBehaviour
// {

//     private LevelBuilder level;
// 	// The maximum distance for snapping modules
// 	public int SnapDistance = 10;
// 	// Maximum raycast ray length
// 	public float RaycastDistance = 500;
// 	// The lift height when grabbing an object
// 	public float LiftHeight = 3.0f;
// 	// The lift speed when grabbing an object
// 	public float LiftSpeed = 50.0f;

// 	// enables camera movement using mouse scroll
// 	public bool EnableMovement = true;

// 	private GameObject sourceObject;
// 	private GameObject selectedObject;
// 	private GameObject guideObject;

// 	private TrapSnapPoint bestDstPoint;

// 	private TrapComponent lastHit;
// 	private Vector3 grabOffset = new Vector3();

// 	private Vector3 localPlayerMousePos;

// 	// Original parent node used for updating tree when dropping without snapping to something.
// 	private RoomTreeNode firstParentNode;

//     // Start is called before the first frame update
//     void Start()
//     {
//         level = GetComponent<LevelBuilder>();
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         if (isLocalPlayer)
//             ClientUpdate();
//         if (isServer)
//         {
//             if (selectedObject != null)
//                 ServerUpdate();
//         }
//     }

//     private void ClientUpdate()
//     {
//         if (selectedObject != null)
//         {
//             RaycastHit hit;
//             int layermask = 1 << LayerMask.NameToLayer("Puppeteer Interact");
//             if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),out hit, RaycastDistance, layermask, QueryTriggerInteraction.Collide))
//             {
//                 GameObject hitObject = hit.transform.gameObject;

//                 TrapComponent trapComponent = hitObject.GetComponent<TrapComponent>();
//                 if (trapComponent != null)
//                 {
//                     if (trapComponent != lastHit)
//                     {
//                         if (lastHit != null)
//                         {
//                             lastHit.OnRaycastExit();
//                         }
//                         lastHit = trapComponent;
//                         if(!lastHit.Placed)
//                         {
//                             lastHit.OnRaycastEnter();
//                         }
//                     }
//                     if (Input.GetButtonDown("Fire"))
// 					{
// 						if (!trapComponent.Placed)
// 						{
// 							Pickup(hitObject);
// 						}
// 					}
//                 }
//                 else
//                 {
//                     lastHit.OnRaycastExit();
//                     lastHit = null;
//                 }
//             }
//             else
// 			{
// 				// If raycast doesn't hit any objects
// 				if (lastHit != null)
// 				{
// 					lastHit.OnRaycastExit();
// 					lastHit = null;
// 				}
// 			}
//         }
//         else
// 		{
// 			CmdUpdateMousePos(MouseToWorldPosition());

// 			if (Input.GetButtonUp("Fire"))
// 			{
// 				Drop();
// 			}
// 			else
// 			{
// 				if (Input.GetButtonDown("Rotate"))
// 				{
// 					// Rotate room around its own up-axis

// 					selectedObject.transform.RotateAround(selectedObject.transform.position, selectedObject.transform.up, 90);
// 					CmdRotate(selectedObject.transform.rotation);
// 				}
// 				if (!isServer)
// 				{
// 					ClientUpdatePositions();
// 				}

// 			}
//         }
//     }
//     [Command]
//     public void CmdRotate(Quaternion rot)
//     {
//         selectedObject.transform.rotation = rot;
//     }
//     [Command]
//     public void CmdUpdateMousePos(Vector3 pos)
//     {
//         localPlayerMousePos = pos;
//     }
//     private void ServerUpdate()
//     {
//         ServerUpdatePositions();
//     }
//     private void Pickup(GameObject pickupTrap)
//     {
//         if (!isServer)
//         {
//             sourceObject = pickupTrap;

//             selectedObject  = Instantiate(sourceObject);
//             selectedObject.name =  "SelectedObject";

            
// 			guideObject = Instantiate(sourceObject);
// 			guideObject.name = "GuideObject";

// 			grabOffset = sourceObject.transform.position - MouseToWorldPosition();
//         }

//         CmdUpdateMousePos(MouseToWorldPosition());
//         CmdPickup(pickupTrap);
//     }

//     [Command]
//     public void CmdPickup(GameObject pickupTrap)
//     {
//         sourceObject = pickupTrap;

// 		selectedObject = Instantiate(sourceObject);
// 		selectedObject.name = "SelectedObject";

// 		guideObject = Instantiate(sourceObject);
// 		guideObject.name = "GuideObject";

// 		grabOffset = sourceObject.transform.position - localPlayerMousePos;

//         if (!isLocalPlayer)
//         {
//             foreach (MeshRenderer renderer in selectedObject.GetComponentsInChildren<MeshRenderer>())
//             {
//                 renderer.enabled = false;
//             }
// 			foreach (MeshRenderer renderer in guideObject.GetComponentsInChildren<MeshRenderer>())
// 			{
// 				renderer.enabled = false;
// 			}
//         }
//     }

//     private void Drop()
//     {
//         CmdDrop();
// 		if (!isServer)
// 		{
// 			Destroy(selectedObject);
// 			selectedObject = null;
			
//             guideObject.name = "Placed Trap";
//             guideObject.GetComponent<TrapComponent>().Placed = true;
// 			guideObject = null;
// 		}
//     }

//     [Command]
//     public void CmdDrop()
//     {
//         Destroy(selectedObject);
// 		selectedObject = null;
			
//         guideObject.name = "Placed Trap";
//         guideObject.GetComponent<TrapComponent>().Placed = true;
// 		guideObject = null;
//     }

//     private void  ClientUpdatePositions()
//     {
//         Vector3  newPosition = MouseToWorldPosition() + grabOffset;
//         selectedObject.transform.position = Vector3.Lerp(selectedObject.transform.position, new Vector3(newPosition.x, LiftHeight,newPosition.z), LiftSpeed * Time.deltaTime);
//     }

//     private void ServerUpdatePositions()
//     {
//         Vector3 newPosition = localPlayerMousePos + grabOffset;
//         selectedObject.transform.position = Vector3.Lerp(selectedObject.transform.position, new Vector3(newPosition.x, LiftHeight, newPosition.z), LiftSpeed *  Time.deltaTime);

//         float bestDist = Mathf.Infinity;
//         bestDstPoint = null;

//         var nearestPoint = FindNearestFreePoint(selectedObject.transform, ref bestDist);
//         if (nearestPoint != null)
//         {
//             bestDstPoint = nearestPoint;

//             Debug.DrawLine(bestDstPoint.transform.position, selectedObject.transform.position, Color.yellow);
//         }

//         if (bestDstPoint != null)
//         {
//             RpcUpdateGuide(new TransformStruct(selectedObject.transform.position - (selectedObject.transform.position - bestDstPoint.transform.position), selectedObject.transform.rotation));
// 			guideObject.transform.position = selectedObject.transform.position - (selectedObject.transform.position - bestDstPoint.transform.position);
// 			guideObject.transform.rotation = selectedObject.transform.rotation;
//         }
//         else
//         {
//         	RpcUpdateGuide(new TransformStruct(sourceObject.transform.position, sourceObject.transform.rotation));
// 			guideObject.transform.position = sourceObject.transform.position;
// 			guideObject.transform.rotation = sourceObject.transform.rotation;
//         }
//     }

//     [ClientRpc]
//     public void RpcUpdateGuide(TransformStruct target)
//     {
//         if (isLocalPlayer)
//         {
//             guideObject.transform.position = target.Position;
//             gameObject.transform.rotation = target.Rotation;
//         }
//     }
//     private  TrapSnapPoint FindNearestFreePoint(in Transform heldTrap, ref float bestDist)
//     {
//         List<TrapSnapPoint> trapSnapPoints = new List<TrapSnapPoint>();
//         var rooms = level.GetRooms();
//         foreach (var room in rooms)
//         {
//             var trapSnapContainers = room.GetComponent<SnapPointContainer>().TrapSnapPoint;
//             TrapSnapPoint result = null;
//             foreach (var snapPoint in trapSnapContainers)
//             {
//                 if (!CanBePlaced(heldTrap, snapPoint))
//                     continue;
//             }


//         }
//     }
// }
