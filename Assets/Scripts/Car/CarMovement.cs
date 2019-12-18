using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
	[SerializeField]
	private float speed = 0, direction = 0; //, averageSpeed = 0;
	[SerializeField]
	private float speedMultiplier = 1;
	private Rigidbody rb;


	public float Speed { get { return speed; } set { speed = value; } }
	public float Direction { get { return direction; } set { direction = value; } }
	//public float AverageSpeed { get { return averageSpeed; } set { averageSpeed = value; } }


	private void Awake()
	{
		rb = GetComponent<Rigidbody>();
	}

	private void OnDisable()
	{
		speed = 0;
		direction = 0;
	}

	private void Update()
	{
		//Drive();
	}

	private void FixedUpdate()
	{
		Drive();
		//PhysicsDrive();
	}

	private void Drive()
	{
		// Drive forward/backward:
		//transform.position += transform.forward * speed * Time.deltaTime*6;
		rb.velocity = transform.forward * speed * 5 * speedMultiplier;

		// Turn left/right:
		if(speed != 0) // Car can't turn while not moving.
		{
			if(speed < 0)
			{
				direction *= -1;
			}

			float x = transform.rotation.eulerAngles.x;
			float y = transform.rotation.eulerAngles.y;
			float z = transform.rotation.eulerAngles.z;
			transform.rotation = Quaternion.Euler(x, y + direction*3, z);
		}
	}
}
