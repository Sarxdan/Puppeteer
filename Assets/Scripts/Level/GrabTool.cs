using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabTool : MonoBehaviour
{
    // the maximum distance for snapping modules
    public int SnapDistance = 4;

    // the lift height when grabbing an object
    public float LiftHeight = 2.0f;

    // the lift speed when grabbing an object
    public float LiftSpeed = 0.1f;

    // determines if a module may be grabbed even if connected with multiple others
    public bool GrabInterconnected = false;

    private GameObject selectedObject;
    private GameObject sourceObject;
    private GameObject guideObject;

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
                    this.bestSrcPoint = anchor;
                    this.bestDstPoint = nearest;
                    Debug.DrawLine(nearest.transform.position, anchor.transform.position, Color.yellow);

                    // place guide object
                    var offset = anchor.transform.parent.position - anchor.transform.position;
                    guideObject.transform.SetPositionAndRotation(nearest.transform.position + offset, selectedObject.transform.rotation);
                }
            }
            guideObject.SetActive(this.CanConnect(bestSrcPoint, bestDstPoint));
        }
    }

    void Update()
    {
        if (selectedObject != null && selectedObject.transform.hasChanged)
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
                foreach (var anchor in anchors)
                {
                    numConnections += (anchor.Attachment != null ? 1 : 0);

                }

                if (numConnections <= 1 || GrabInterconnected)
                {
                    selectedObject = GameObject.Instantiate(sourceObject);
                    selectedObject.name = "SelectedObject";
                    offset = selectedObject.transform.position - MouseToWorldPosition();

                    // setup guide object
                    guideObject = GameObject.Instantiate(sourceObject);
                    guideObject.name = "GuideObject";

                    // remove anchors from guide
                    foreach (var anchor in guideObject.GetComponentsInChildren<AnchorPoint>())
                    {
                        GameObject.Destroy(anchor.gameObject);
                    }

                    var renderer = guideObject.GetComponent<Renderer>();
                    renderer.material.SetColor("_Color", Color.green);
                }
            }
        }

        if (Input.GetMouseButtonDown(1) && selectedObject != null)
        {
            selectedObject.transform.Rotate(0, 90, 0);
        }

        if (Input.GetMouseButtonUp(0) && selectedObject != null)
        {
            if (this.PerformConnection(bestSrcPoint, bestDstPoint))
            {
                // connection possible, move 
                GameObject.Destroy(sourceObject);
                selectedObject.name = sourceObject.name;
                selectedObject.transform.SetParent(transform);
            }
            else
            {
                // if connection not possible, destroy selection
                GameObject.Destroy(selectedObject);
            }
            GameObject.Destroy(guideObject);
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

        // check if anchors are too far apart  
        float dist = (dst.transform.position - src.transform.position).magnitude;
        if (dist > SnapDistance)
        {
            return false;
        }

        var srcDir = (src.transform.position - src.transform.parent.position).normalized;
        var dstDir = (dst.transform.position - dst.transform.parent.position).normalized;

        // only connect modules with correct anchor angles
        return -1.0f == Vector3.Dot(srcDir, dstDir);
    }

    private bool PerformConnection(in AnchorPoint src, in AnchorPoint dst)
    {
        if (CanConnect(src, dst))
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
        foreach (var target in targets)
        {
            if (target.transform.parent == anchor.transform.parent)
                continue;

            if (target.transform.parent == sourceObject.transform) // remove this statement after level builder is finished
                continue;

            float curDist = (anchor.transform.position - target.transform.position).sqrMagnitude;
            if (curDist < distance)
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
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
}
