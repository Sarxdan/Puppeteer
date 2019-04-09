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

    private GameObject selectedObject;
    private GameObject sourceObject;
    private GameObject guideObject;

    private AnchorPoint bestSrcPoint;
    private AnchorPoint bestDstPoint;

    // movement offset
    private Vector3 offset;
    private float time;

    void Update()
    {
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

        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hitInfo = new RaycastHit();
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo))
            {
                sourceObject = hitInfo.transform.gameObject;

                if (true) // TODO verify that a path from start to goal still exists
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
            if (this.CanConnect(bestSrcPoint, bestDstPoint))
            {
                // connection possible, move source object
                var offset = bestDstPoint.transform.position + bestSrcPoint.transform.parent.position - bestSrcPoint.transform.position;
                sourceObject.transform.SetPositionAndRotation(offset, selectedObject.transform.rotation);
            }
            GameObject.Destroy(selectedObject);
            GameObject.Destroy(guideObject);
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

    private AnchorPoint FindNearestAnchor(in AnchorPoint anchor, ref float distance)
    {
        var targets = new List<AnchorPoint>();
        var objects = GameObject.FindGameObjectsWithTag("Connectable");
        foreach (var obj in objects)
        {
            targets.AddRange(obj.GetComponentsInChildren<AnchorPoint>());
        }
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
