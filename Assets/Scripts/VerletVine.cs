using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Point
{
	public Vector3 position, prevPosition;
	public bool locked;
	public void SetBoth(Vector3 pos)
	{
		position = pos;
		prevPosition = pos;
	}
	public Point(bool loc = false)
	{
		position = Vector3.zero;
		prevPosition = Vector3.zero;
		locked = loc;
	}
}

public class Stick
{
	public Point pointA, pointB;
	public float length;
	public Stick(Point a, Point b, float len = 0)
	{
		pointA = a;
		pointB = b;
		length = len;
	}
}



public class VerletVine : MonoBehaviour
{
	[Header("Verlet Integration"), SerializeField] int numIterations = 10;
	[SerializeField, Range(2,100)] int numPoints = 5;
	[SerializeField] float segmentLength = 1;

	[HideInInspector]
	public List<Point> points;
	List<Stick> sticks;

	[SerializeField] LineRenderer vine;

	[Header("Simulation")]
	[SerializeField] bool inRuntime = false;
	[SerializeField, Tooltip("deltatime when running in the inspector")] public float InspectorTimeStep = 0.15f;

	public void Simulate(float dt)
	{
		if (points == null || sticks == null || points.Count <= 0 || sticks.Count <= 0)
			Init();

		points[0].SetBoth(transform.position);
		points[points.Count - 1].SetBoth(transform.GetChild(0).position);

		// logic provided by https://www.youtube.com/watch?v=PGk0rnyTa1U
		foreach (Point p in points)
		{
			if (!p.locked)
			{
				Vector3 positionBeforeUpdate = p.position;
				p.position += p.position - p.prevPosition;
				p.position += Physics.gravity * dt * dt;
				p.prevPosition = positionBeforeUpdate;
			}
		}
		for (int i = 0; i < numIterations; i++)
		{
			foreach (Stick stick in sticks)
			{
				Vector3 stickCenter = (stick.pointA.position + stick.pointB.position) * 0.5f;
				Vector3 stickDir = (stick.pointA.position - stick.pointB.position).normalized;
				if (!stick.pointA.locked)
					stick.pointA.position = stickCenter + stickDir * stick.length * 0.5f;
				if (!stick.pointB.locked)
					stick.pointB.position = stickCenter - stickDir * stick.length * 0.5f;

			}
		}

		// update line render
		vine.positionCount = points.Count;
		for (int i = 0; i < points.Count; i++)
			vine.SetPosition(i, points[i].position);
	}

	public void Init()
	{
		vine = GetComponent<LineRenderer>();

		// create lists
		points = new List<Point>();
		sticks = new List<Stick>();

		// create points and sticks
		points.Add(new Point(true)); // end points are manually moved to transforms, so are locked
		for (int i = 1; i < numPoints - 1; i++)
			points.Add(new Point());

		points.Add(new Point(true)); // end points are manually moved to transforms, so are locked
		for (int i = 0; i < numPoints - 1; i++)
			sticks.Add(new Stick(points[i], points[i + 1], segmentLength));

		// set default positions
		float countInverse = 1 / (float)points.Count;
		for (int i = 0; i < numPoints; i++)
			points[i].SetBoth(Vector3.Lerp(transform.position, transform.GetChild(0).position, i * countInverse));
	}

	public void CleanUp()
	{
		if (points != null)
		{
			points.Clear();
			points = null;
		}
		if (sticks != null)
		{
			sticks.Clear();
			sticks = null;
		}
	}

	private void Start()
	{
		if (inRuntime)
			Init();
	}
	private void Update()
	{
		if (inRuntime)
			Simulate(Time.deltaTime);
	}
}
