using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabTool : MonoBehaviour
{
    public int SnapDistance = 4;
    public float LiftHeight = 2.0f;
    public float LiftSpeed = 0.1f;
    public bool GrabInterconnected = false;

    // the selected object
    private GameObject selectedObject;
    private GameObject sourceObject;

    private AnchorPoint bestSrcPoint;
    private AnchorPoint bestDstPoint;

    // movement offset
    private Vector3 offset;
    private float time;

    void FixedUpdate()
    {
        if (selectedObject != null && selectedObject.transform.hasChanged)
        {
            var anchors = selectedObject.GetComponentsInChildren<AnchorPoint>();
            float bestDist = Mathf.Infinity;

            foreach (var anchor in anchors)
            {
                var nearest = this.FindNearestAnchor(anchor, ref bestDist);
                if (this.CanConnect(anchor, nearest))
                {
                    // TODO: cleanup
                    this.bestSrcPoint = anchor;
                    this.bestDstPoint = nearest;
                    Debug.DrawLine(nearest.transform.position, anchor.transform.position, Color.yellow);
                }
            }
        }
    }

    void Update()
    {
        if(selectedObject != null && selectedObject.transform.hasChanged)
        {
            var pos = this.offset + MouseToWorldPosition();
            pos.y = 0.0f;
            selectedObject.transform.position = Vector3.Lerp(pos, pos + Vector3.up * LiftHeight, time += LiftSpeed);
        }

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hitInfo = new RaycastHit();
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
            {
                sourceObject = hitInfo.transform.gameObject;

                var anchors = sourceObject.GetComponentsInChildren<AnchorPoint>();
                int numConnections = 0;
                foreach(var anchor in anchors)
                {
                    numConnections += (anchor.Attachment != null ? 1 : 0);
                }

                if (numConnections <= 1 || GrabInterconnected)
                {
                    selectedObject = GameObject.Instantiate(sourceObject);
                    selectedObject.name = "SelectedObject";
                    offset = selectedObject.transform.position - MouseToWorldPosition();
                }
            }
        }

        if(Input.GetMouseButtonDown(1) && selectedObject != null)
        {
            // rotate by 90deg
            selectedObject.transform.Rotate(0, 90, 0);
        }

        if (Input.GetMouseButtonUp(0) && selectedObject != null)
        {
            if (this.PerformConnection(bestSrcPoint, bestDstPoint))
            {
                GameObject.Destroy(sourceObject);
                selectedObject.name = sourceObject.name;
                selectedObject.transform.SetParent(transform);
            }
            else
            {
                // if connection not possible, destroy selection
                GameObject.Destroy(selectedObject);
            }
            selectedObject = null;
            time = 0.0f;
        }
    }

    // checks if two anchors may be connected
    private bool CanConnect(in AnchorPoint src, in AnchorPoint dst)
    {
        // ignore invalid anchors
        if (src == null || dst == null)
        {
            return false;
        }

        // cannot connect to anchor that is already used
        if (src.Attachment || dst.Attachment)
        {
            return false;
        }

        float dist = (dst.transform.position - src.transform.position).magnitude;
        // anchors are too far apart  
        if(dist > SnapDistance)
        {
            return false;
        }

        var srcDir = (src.transform.position - src.transform.parent.position).normalized;
        var dstDir = (dst.transform.position - dst.transform.parent.position).normalized;

        return -1.0f == Vector3.Dot(srcDir, dstDir);
    }

    private bool PerformConnection(in AnchorPoint src, in AnchorPoint dst)
    {
        if(CanConnect(src, dst))
        {
            src.transform.parent.position = dst.transform.position + src.transform.parent.position - src.transform.position;
            src.Attachment = dst;
            dst.Attachment = src;
            return true;
        }
        return false;
    }

    private List<AnchorPoint> GetAllAnchors()
    {
        var result = new List<AnchorPoint>();
        var objects = GameObject.FindGameObjectsWithTag("Connectable");
        foreach (var obj in objects)
        {
            result.AddRange(obj.GetComponentsInChildren<AnchorPoint>());
        }
        return result;
    }

    private AnchorPoint FindNearestAnchor(in AnchorPoint anchor, ref float distance)
    {
        var targets = this.GetAllAnchors();
        AnchorPoint result = null;
        foreach(var target in targets)
        {
            if (target.transform.parent == anchor.transform.parent)
                continue;

            if(target.transform.parent == sourceObject.transform) // remove this statement after level builder is finished
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
