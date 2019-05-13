using UnityEngine;

/*
 * AUTHOR:
 * Anton Jonsson
 * 
 * DESCRIPTION:
 * Script used to simulate a fluid moving in a cylinder.
 * 
 * CODE REVIEWED BY:
 * 
 */

public class FluidSimulation : MonoBehaviour
{
	public GameObject FluidTop;
	public GameObject FluidBottom;

	// Original scale and rotation values of the objects
	private Vector3 topScale;
	private Vector3 bottomScale;
	private Vector3 topRotation;
	private Vector3 bottomRotation;

	private Rigidbody parentRigidBody;

	// Height of objects in Unity units. Might be tricky to find but found no way of automatically gathering it.
	public float topHeight;
	public float bottomHeight;

	// Degrees of tilt for fluid at standard scale
	public float fluidTiltAmount;

	// Scale of top slanted fluid in local y-axis.
	private float currentTopScale = 0.2f;
	public float MaxTiltScale = 3;

	// Liquid amounts used to calculate percentage of liquid left.
	public float MaxLiquidAmount = 100;
	public float CurrentLiquidAmount = 0;

	public float sploshAmount = 0.007f;
	public float sploshSlowdown = 0.7f;
	private float rotateAcceleration = 0;
	private float rotateSpeed = 0;

	public float MovementSplosh = 0.5f;

	private Vector3 lastPosition;
	private float lastYRotation;

	void Start()
	{
		lastPosition = transform.position;
		lastYRotation = transform.eulerAngles.y;

		// Save the original scale values
		topScale = FluidTop.transform.localScale;
		bottomScale = FluidBottom.transform.localScale;
		topRotation = FluidTop.transform.localEulerAngles;
		bottomRotation = FluidBottom.transform.localEulerAngles;
		parentRigidBody = GetComponentInParent<Rigidbody>();

		UpdateRotationAndTilt();
		UpdatePositionAndScale();
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		UpdateRotationAndTilt();
		UpdatePositionAndScale();
	}

	private void UpdateRotationAndTilt()
	{
		Vector3 deltaPos = lastPosition - transform.position;
		lastPosition = transform.position;
		// Project the up vector + the movement vector of the fluid onto x/z plane by removing y-coord:
		Vector2 leanVector = new Vector2(gameObject.transform.up.x, gameObject.transform.up.z) + new Vector2(deltaPos.x, deltaPos.z) * MovementSplosh;
		// Calculate angle of vector relative to positive z.
		float angle = Vector2.SignedAngle(leanVector, new Vector2(0, 1));
		float targetAngle = angle - transform.eulerAngles.y;
		float angleDiff = Mathf.DeltaAngle(FluidTop.transform.localEulerAngles.y, targetAngle);

		while (angleDiff > 180)
			angleDiff -= 360;
		while (angleDiff < -180)
			angleDiff += 360;

		rotateAcceleration += angleDiff * sploshAmount;
		rotateAcceleration *= sploshSlowdown;

		if (rotateAcceleration >= 1)
		{
			rotateAcceleration = 1;
		}
		else if (rotateAcceleration <= -1)
		{
			rotateAcceleration = -1;
		}

		rotateSpeed += rotateAcceleration;

		if (parentRigidBody != null)
		{
			rotateSpeed += parentRigidBody.angularVelocity.y;
		}
		else
		{
			parentRigidBody = GetComponentInParent<Rigidbody>();
		}


		if (Mathf.Abs(rotateSpeed + angleDiff) < Mathf.Abs(rotateSpeed))
		{
			rotateSpeed *= sploshSlowdown;
		}

		FluidTop.transform.localEulerAngles = new Vector3(0, FluidTop.transform.localEulerAngles.y + rotateSpeed, 0);
		FluidBottom.transform.localEulerAngles = new Vector3(0, FluidTop.transform.localEulerAngles.y + bottomRotation.y, 0);

		// Calculate lentgh of lean vector to get amount of fluid tilt.
		// When the length of the lean vector is 1, the fluid is tilted 90 degrees.
		float fluidTiltAngle = Mathf.Asin(leanVector.magnitude);
		currentTopScale = Mathf.Tan(fluidTiltAngle) * topScale.y / (Mathf.Deg2Rad * fluidTiltAmount);

		if (currentTopScale > MaxTiltScale)
		{
			currentTopScale = MaxTiltScale;
		}
		else if (currentTopScale < 0.0001f || float.IsNaN(currentTopScale))
		{
			currentTopScale = 0.0001f;
		}
	}

	private void UpdatePositionAndScale()
	{
		// Calculate how much the bottom fluid has to move out of the way to make room for the top fluid as percentage of original scale.
		float splashOffset = (currentTopScale / topScale.y);
		// Calculate what percentage of its original scale the bottom fluid needs to be to make room for top fluid.
		float bottomScalePercentage = splashOffset * (topHeight / 2) / bottomHeight;
		float currentBottomScale = bottomScale.y - bottomScalePercentage * bottomScale.y;
		// Calculate how much the total liquid has to sink as percentage of original amount.
		float amountOffset = CurrentLiquidAmount / MaxLiquidAmount;
		// Update positions of Liquid parts using offsets.
		FluidTop.transform.localPosition = new Vector3(0, amountOffset * bottomHeight, 0);
		// Update Scale of Liquid parts using offsets
		if (amountOffset != 0)
		{
			FluidTop.transform.localScale = new Vector3(topScale.x, currentTopScale, topScale.z);
		}
		else
		{
			FluidTop.transform.localScale = new Vector3(topScale.x, 0.0001f, topScale.z);
		}
		FluidBottom.transform.localScale = new Vector3(bottomScale.x, currentBottomScale + amountOffset * bottomScale.y - bottomScale.y, bottomScale.z);
	}
}
