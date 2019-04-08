using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabTool : MonoBehaviour
{
    public int SnapDistance = 4;
    public float LiftHeight = 2.0f;
    public float LiftSpeed = 0.1f;

    // the selected object
    private GameObject selectedObject;
    private GameObject sourceObject;

    private AnchorPoint bestSrcPoint;
    private AnchorPoint bestDstPoint;

    // movement offset
    private Vector3 offset;
    private float time;

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit hitInfo = new RaycastHit();
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
            {
                sourceObject = hitInfo.transform.gameObject;

                // create clone
                selectedObject = GameObject.Instantiate(sourceObject);
                offset = selectedObject.transform.position - MouseToWorldPosition();
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
            }
            else
            {
                // if connection not possible, destroy selection
                GameObject.Destroy(selectedObject);
            }
            selectedObject = null;
            time = 0.0f;
        }

        if (selectedObject != null && selectedObject.transform.hasChanged)
        {
            var pos = this.offset + MouseToWorldPosition();
            pos.y = 0.0f;
            selectedObject.transform.position = Vector3.Lerp(pos, pos + Vector3.up * LiftHeight, time += LiftSpeed);

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

    // checks if two anchors may be connected
    private bool CanConnect(in AnchorPoint src, in AnchorPoint dst)
    {
        // ignore invalid anchors
        if (src == null || dst == null)
        {
            return false;
        }

        // cannot connect to anchor that is already used
        if (!(src.Open && dst.Open))
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
            src.Open = false;
            dst.Open = false;
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
