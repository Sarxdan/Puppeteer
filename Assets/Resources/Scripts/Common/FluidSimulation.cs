using UnityEngine;

/*
 * AUTHOR:
 * Anton Jonsson
 * 
 * DESCRIPTION:
 * Script used to simulate a fluid moving in a cylinder.
 * 
 * CODE REVIEWED BY:
 * Philip Stenmark (15/05-2019)
 * 
 * 
 * CLEANED
 */
//CLEANED

public class FluidSimulation : MonoBehaviour
{
	// The actual fluid meshes that are rotated and scaled to create the effect.
	public GameObject FluidTop;
	public GameObject FluidBottom;

	// Original scale and rotation values of the objects.
	private Vector3 topScale;
	private Vector3 bottomScale;
	private Vector3 topRotation;
	private Vector3 bottomRotation;

	// Height of object in Unity units. Might be tricky to find but found no way of automatically gathering it.
	public float TopHeight;
	public float BottomHeight;

	// Degrees of tilt for fluid at standard scale.
	public float FluidTiltAmount;

	// Scale of top slanted fluid in local y-axis.
	private float currentTopScale = 0.2f;
	// Maximum allowed scale of top fluid object.
	public float MaxTiltScale = 3;

	// Liquid amounts used to calculate percentage of liquid left.
	public float MaxLiquidAmount = 100;
	public float CurrentLiquidAmount = 100;

	public float SploshAmount = 0.007f;
	public float SploshSlowdown = 0.7f;
	private float rotateAcceleration = 0;
	private float rotateSpeed = 0;

	public float MovementSplosh = 5.0f;
	public float RotationSplosh = 0.07f;
	public float ScaleLerpAmount = 0.1f;

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

		UpdateRotationAndTilt();
		UpdatePositionAndScale();
	}

	void FixedUpdate()
	{
		UpdateRotationAndTilt();
		UpdatePositionAndScale();
	}

	private void UpdateRotationAndTilt()
	{
		Vector3 deltaPos = lastPosition - transform.position;
		lastPosition = transform.position;
		float deltaYrot = lastYRotation - transform.eulerAngles.y;
		lastYRotation = transform.eulerAngles.y;
		while (deltaYrot > 180)
			deltaYrot -= 360;
		while (deltaYrot < -180)
			deltaYrot += 360;
		// Project the up vector + the movement vector + the rotation vector of the fluid onto x/z plane by removing y-coord:
		Vector2 leanVector = new Vector2(gameObject.transform.up.x, gameObject.transform.up.z) + new Vector2(deltaPos.x, deltaPos.z) * MovementSplosh + new Vector2(transform.right.x, transform.right.z) * deltaYrot * RotationSplosh;
		// Calculate angle of vector relative to positive z.
		float angle = Vector2.SignedAngle(leanVector, new Vector2(0, 1));
		float targetAngle = angle - transform.eulerAngles.y;
		float angleDiff = Mathf.DeltaAngle(FluidTop.transform.localEulerAngles.y, targetAngle);

		while (angleDiff > 180)
			angleDiff -= 360;
		while (angleDiff < -180)
			angleDiff += 360;

		rotateAcceleration += angleDiff * SploshAmount;
		rotateAcceleration *= SploshSlowdown;
		rotateAcceleration = Mathf.Clamp(rotateAcceleration, -1, 1);

		rotateSpeed += rotateAcceleration;

		// Slow speed down when speed is moving away from target in order to prevent a perfect pendulum.
		if (Mathf.Abs(rotateSpeed + angleDiff) < Mathf.Abs(rotateSpeed))
		{
			rotateSpeed *= SploshSlowdown;
		}

		FluidTop.transform.localEulerAngles = new Vector3(0, FluidTop.transform.localEulerAngles.y + rotateSpeed + topRotation.y, 0);
		FluidBottom.transform.localEulerAngles = new Vector3(0, FluidTop.transform.localEulerAngles.y + bottomRotation.y, 0);

		// Calculate length of lean vector to get amount of fluid tilt.
		// When the length of the lean vector is 1, the fluid is tilted 90 degrees.
		float fluidTiltAngle = Mathf.Asin(leanVector.magnitude);
		currentTopScale = Mathf.Tan(fluidTiltAngle) * topScale.y / (Mathf.Deg2Rad * FluidTiltAmount);

		if (currentTopScale > MaxTiltScale || float.IsNaN(currentTopScale))
		{
			currentTopScale = MaxTiltScale;
		}
		else if (currentTopScale < Mathf.Epsilon)
		{
			currentTopScale = Mathf.Epsilon;
		}
	}

	private void UpdatePositionAndScale()
	{
		// Calculate how much the bottom fluid has to move out of the way to make room for the top fluid as percentage of original scale.
		float splashOffset = (currentTopScale / topScale.y);
		// Calculate how much the total liquid has to sink as percentage of original amount.
		float amountOffset = CurrentLiquidAmount / MaxLiquidAmount;
		// Update positions of Liquid parts using offsets.
		FluidTop.transform.localPosition = new Vector3(0, amountOffset * BottomHeight, 0);
		// Update Scale of Liquid parts using offsets
		if (amountOffset != 0)
		{
			FluidTop.transform.localScale = Vector3.Lerp(FluidTop.transform.localScale, new Vector3(topScale.x, currentTopScale, topScale.z), ScaleLerpAmount);
		}
		else
		{
			// If there is no liquid left, scale of top should always be 0 (Or as close as possible)
			FluidTop.transform.localScale = new Vector3(topScale.x, Mathf.Epsilon, topScale.z);
		}
		FluidBottom.transform.localScale = new Vector3(bottomScale.x, amountOffset * bottomScale.y, bottomScale.z);
	}
}
