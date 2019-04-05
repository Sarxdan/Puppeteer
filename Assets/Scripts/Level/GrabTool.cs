using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabTool : MonoBehaviour
{
    public int SnapDistance = 4;
    public float LiftHeight = 2.0f;
    public float LiftSpeed = 0.1f;

    // contains all available anchors
    private List<AnchorPoint> targets;

    // the selected object
    private GameObject selectedObject;

    private AnchorPoint bestSrcPoint;
    private AnchorPoint bestDstPoint;
    private AnchorPoint lastDstPoint;
    private Vector3 lastPosition;
    private Quaternion lastRotation;

    // movement offset
    private Vector3 offset;
    private float time;

    void Start()
    {
        targets = new List<AnchorPoint>();

        var objects = GameObject.FindGameObjectsWithTag("Connectable");
        foreach(var obj in objects)
        {
            targets.AddRange(obj.GetComponentsInChildren<AnchorPoint>());
        }
    }

    void Update()
    {
        if(selectedObject != null && selectedObject.transform.hasChanged)
        {
            var pos = this.offset + MouseToWorldPosition();
            pos.y = 0.0f;
            selectedObject.transform.position = Vector3.Lerp(pos, pos + Vector3.up * LiftHeight, time += LiftSpeed);

            var anchors = selectedObject.GetComponentsInChildren<AnchorPoint>();
            float bestDist = Mathf.Infinity;

            foreach(var anchor in anchors)
            {
                var nearest = this.FindNearestAnchor(anchor, ref bestDist);

                if(nearest != null && this.CanConnect(anchor, nearest))
                {
                    this.bestSrcPoint = anchor;
                    this.bestDstPoint = nearest;
                    Debug.DrawLine(anchor.transform.position, nearest.transform.position, Color.cyan);
                }
            }

            if(this.CanConnect(bestSrcPoint, bestDstPoint))
            {
                Debug.DrawLine(bestSrcPoint.transform.position, bestDstPoint.transform.position, Color.yellow);
            }
        }

        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit hitInfo = new RaycastHit();
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
            {
                selectedObject = hitInfo.transform.gameObject;
                offset = selectedObject.transform.position - MouseToWorldPosition();

                lastPosition = selectedObject.transform.position;
                lastRotation = selectedObject.transform.rotation;
            }
        }

        if(Input.GetMouseButtonDown(1) && selectedObject != null)
        {
            selectedObject.transform.Rotate(0, 90, 0);
        }

        if (Input.GetMouseButtonUp(0) && selectedObject != null)
        {
            if(this.CanConnect(bestSrcPoint, bestDstPoint))
            {
                var offset = bestDstPoint.transform.position + bestSrcPoint.transform.parent.position - bestSrcPoint.transform.position;
                offset.y = 0.0f;
                selectedObject.transform.position = offset;
            }
            else
            {
                selectedObject.transform.SetPositionAndRotation(lastPosition, lastRotation);
            }
            selectedObject = null;
            time = 0.0f;
        }
    }

    // checks if two anchors may be connected
    private bool CanConnect(in AnchorPoint src, in AnchorPoint dst)
    {
        if (src == null || dst == null)
            return false;

        // cannot connect to anchor that is already used
        if (!dst.Open)
            return false;

        var srcDir = (src.transform.position - src.transform.parent.position).normalized;
        var dstDir = (dst.transform.position - dst.transform.parent.position).normalized;

        float dist = (dst.transform.position - src.transform.position).magnitude;

        return Vector3.Dot(srcDir, dstDir) == -1 && dist < SnapDistance;
    }

    private AnchorPoint FindNearestAnchor(in AnchorPoint anchor, ref float distance)
    {
        AnchorPoint result = null;
        foreach(var target in targets)
        {
            if (target.transform.parent == anchor.transform.parent)
                continue;

            float curDist = (anchor.transform.position - target.transform.position).sqrMagnitude;
            if(curDist < distance)
            {
                result = target;
                distance = curDist;
            }
        }
        return result;
    }

    private Vector3 MouseToWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.WorldToScreenPoint(selectedObject.transform.position).z;
        // convert to world position
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
}
