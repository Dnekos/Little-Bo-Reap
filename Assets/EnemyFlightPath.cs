using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFlightPath : MonoBehaviour
{
	//Struct to keep position, normal and tangent of a spline point
	[System.Serializable]
	public struct CatmullRomPoint
	{
		public Vector3 position;
		public Vector3 tangent;
		public Vector3 normal;

		public CatmullRomPoint(Vector3 position, Vector3 tangent, Vector3 normal)
		{
			this.position = position;
			this.tangent = tangent;
			this.normal = normal;
		}
	}

	CatmullRom catmullRom;
	[SerializeField] Transform[] points;
	public int resolution = 10;
	[SerializeField] private CatmullRomPoint[] splinePoints; //Generated spline points

	public CatmullRomPoint[] GetPoints()
	{
		if (splinePoints == null)
		{
			throw new System.NullReferenceException("Spline not Initialized!");
		}

		return splinePoints;
	}
	public Vector3 GetLerpPosition(float position)
	{
		int index = Mathf.FloorToInt(position);
		int endpoint = (index + 1) % splinePoints.Length;
		return Vector3.Lerp(splinePoints[index].position, splinePoints[endpoint].position, position % 1);
	}
	public Vector3 GetLerpTangent(float position)
	{
		int index = Mathf.FloorToInt(position);
		int endpoint = (index + 1) % splinePoints.Length;
		return Vector3.Lerp(splinePoints[index].tangent, splinePoints[endpoint].tangent, position % 1);
	}

	void SetSpline()
	{
		int pointsToCreate = resolution * points.Length;
		splinePoints = new CatmullRomPoint[pointsToCreate];

		Vector3 p0, p1; //Start point, end point
		Vector3 m0, m1; //Tangents

		// First for loop goes through each individual control point and connects it to the next, so 0-1, 1-2, 2-3 and so on
		int closedAdjustment = 0;
		for (int currentPoint = 0; currentPoint < points.Length - closedAdjustment; currentPoint++)
		{
			bool closedLoopFinalPoint = currentPoint == points.Length - 1;

			p0 = points[currentPoint].position;

			if (closedLoopFinalPoint)
			{
				p1 = points[0].position;
			}
			else
			{
				p1 = points[currentPoint + 1].position;
			}

			// m0
			if (currentPoint == 0) // Tangent M[k] = (P[k+1] - P[k-1]) / 2
			{
				m0 = p1 - points[points.Length - 1].position;
			}
			else
			{
				m0 = p1 - points[currentPoint - 1].position;
			}

			// m1

			if (currentPoint == points.Length - 1) //Last point case
			{
				m1 = points[(currentPoint + 2) % points.Length].position - p0;
			}
			else if (currentPoint == 0) //First point case
			{
				m1 = points[currentPoint + 2].position - p0;
			}
			else
			{
				m1 = points[(currentPoint + 2) % points.Length].position - p0;
			}


			m0 *= 0.5f; //Doing this here instead of  in every single above statement
			m1 *= 0.5f;

			float pointStep = 1.0f / resolution;

			if (closedLoopFinalPoint)//if ((currentPoint == points.Length - 2 && !closedLoop) || closedLoopFinalPoint) //Final point
			{
				pointStep = 1.0f / (resolution - 1);  // last point of last segment should reach p1
			}

			// Creates [resolution] points between this control point and the next
			for (int tesselatedPoint = 0; tesselatedPoint < resolution; tesselatedPoint++)
			{
				float t = tesselatedPoint * pointStep;

				CatmullRomPoint point = Evaluate(p0, p1, m0, m1, t);

				splinePoints[currentPoint * resolution + tesselatedPoint] = point;
			}
		}
	}

	#region calculating points
	//Evaluates curve at t[0, 1]. Returns point/normal/tan struct. [0, 1] means clamped between 0 and 1.
	public static CatmullRomPoint Evaluate(Vector3 start, Vector3 end, Vector3 tanPoint1, Vector3 tanPoint2, float t)
	{
		Vector3 position = CalculatePosition(start, end, tanPoint1, tanPoint2, t);
		Vector3 tangent = CalculateTangent(start, end, tanPoint1, tanPoint2, t);
		Vector3 normal = NormalFromTangent(tangent);

		return new CatmullRomPoint(position, tangent, normal);
	}

	//Calculates curve position at t[0, 1]
	public static Vector3 CalculatePosition(Vector3 start, Vector3 end, Vector3 tanPoint1, Vector3 tanPoint2, float t)
	{
		// Hermite curve formula:
		// (2t^3 - 3t^2 + 1) * p0 + (t^3 - 2t^2 + t) * m0 + (-2t^3 + 3t^2) * p1 + (t^3 - t^2) * m1
		Vector3 position = (2.0f * t * t * t - 3.0f * t * t + 1.0f) * start
			+ (t * t * t - 2.0f * t * t + t) * tanPoint1
			+ (-2.0f * t * t * t + 3.0f * t * t) * end
			+ (t * t * t - t * t) * tanPoint2;

		return position;
	}

	//Calculates tangent at t[0, 1]
	public static Vector3 CalculateTangent(Vector3 start, Vector3 end, Vector3 tanPoint1, Vector3 tanPoint2, float t)
	{
		// Calculate tangents
		// p'(t) = (6t² - 6t)p0 + (3t² - 4t + 1)m0 + (-6t² + 6t)p1 + (3t² - 2t)m1
		Vector3 tangent = (6 * t * t - 6 * t) * start
			+ (3 * t * t - 4 * t + 1) * tanPoint1
			+ (-6 * t * t + 6 * t) * end
			+ (3 * t * t - 2 * t) * tanPoint2;

		return tangent.normalized;
	}

	//Calculates normal vector from tangent
	public static Vector3 NormalFromTangent(Vector3 tangent)
	{
		return Vector3.Cross(tangent, Vector3.up).normalized / 2;
	}
	#endregion

	private void OnDrawGizmos()
	{
		SetSpline();
		if (splinePoints != null && splinePoints.Length > 0)
		{
			Gizmos.color = Color.white;
			for (int i = 0; i < splinePoints.Length; i++)
			{
				if (i == splinePoints.Length - 1)
				{
					Gizmos.DrawLine(splinePoints[i].position, splinePoints[0].position);
				}
				else if (i < splinePoints.Length - 1)
				{
					Gizmos.DrawLine(splinePoints[i].position, splinePoints[i + 1].position);
				}
			}
		}
		//else if (points.Length > 0)
		//	SetSpline();
	}
	private bool ValidatePoints()
	{
		if (splinePoints == null)
		{
			throw new System.NullReferenceException("Spline not initialized!");
		}
		return splinePoints != null;
	}
}
