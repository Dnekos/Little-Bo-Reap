using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SheepBezier : SheepHolder
{

	[Header("Speed")]
	[SerializeField] int SheepBars = 2;
	[SerializeField] float lerpSpeed;

	[Header("Sounds")]
	[SerializeField] protected FMODUnity.EventReference placeSound;

	[Header("Math")]
	[SerializeField] float height = 4;
	[SerializeField] float width = 2;

	[Header("Curve Points")]
	[SerializeField] Transform P0;
	[SerializeField] Transform P1, P2;

	[Header("Shown for Debug Visibility")]
	float SheepRadius = 1;
	[SerializeField]  int layerCount = 0;
	[SerializeField] bool SaveToDatabase = false;
	//[SerializeField]  Vector3 sheepPlacement;

	[Header("Settings")]
	[SerializeField]
	float delay = 0.2f;
	[SerializeField, Tooltip("How many times to try placeing a sheep before raising the height")]
	int PlacementGuesses = 40;
	[SerializeField, Tooltip("How much of a sheep's radius is the height raised when a layer is filled")]
	float HeightStep = 0.01f;

	[Header("Collider")]
	[SerializeField] float ColliderHorzBuffer = 0.7f;
	[SerializeField] float ColliderVertBuffer = 0.7f;

	[SerializeField] MeshCollider col;
	Mesh mesh;
	public List<Vector3> newVertices = new List<Vector3>();
	public List<int> newTriangles = new List<int>();


	// components

	private void Start()
	{
		col.sharedMesh = null;

		containedSheep = new List<Transform>();
		mesh = new Mesh();
		newVertices = new List<Vector3>();
		newTriangles = new List<int>();
	}

	private void Update()
	{
		for (float i = 0; i < 1; i+=0.01f)
		{
			Vector3 point = CalcCurvePoint(i);
			Debug.DrawLine(point, point + Differentiate(i), Color.red, 0.1f);
			Debug.DrawLine(point, point +  DoubleDerivative(i), Color.blue, 0.1f);
			Debug.DrawLine(point, point + Vector3.Cross(Differentiate(i), DoubleDerivative(i)), Color.cyan, 0.1f);

			Debug.DrawLine(point,  CalcCurvePoint(i+0.01f), Color.green, 0.1f);
		}
	}

	#region Curves
	Vector3 CalcCurvePoint(float t)
	{
		return P0.position * (1 - t) * (1 - t) + P1.position * 2 * (1 - t) * t + P2.position * t * t;
	}
	Vector3 Differentiate(float t, float h = 1e-6f)
	{
		return (P0.position * -2 * (1-t) + P1.position * 2 * (-2 * t + 1) + P2.position * 2 * t).normalized;

	}
	Vector3 DoubleDerivative(float t, float h = 1e-6f)
	{
		return (P0.position * 2 + P1.position * -4 + P2.position * 2).normalized;

	}
	#endregion

	public override void RemoveSheep()
	{
		if (col.sharedMesh != null)
		{
			newTriangles.Clear();
			newVertices.Clear();
			mesh.Clear();

			col.sharedMesh = null;
		}

		StopAllCoroutines();

		base.RemoveSheep();
	}

	public void RemoveAllSheep()
	{
		for (int i = containedSheep.Count - 1; i >= 0; i--)
		{
			containedSheep[i].GetComponent<PlayerSheepAI>().EndConstruct(false);
		}
		containedSheep.Clear();
	}

	public void RemoveAll(bool StopCoroutine = true)
	{
		col.sharedMesh = null;
		newTriangles.Clear();
		newVertices.Clear();
		mesh.Clear();

		layerCount = 0;
		CurveT = 0;
		if (StopCoroutine)
			StopAllCoroutines();
	}

	#region Adding Sheep
	public override void Interact()
	{
		base.Interact();
		StopAllCoroutines();
		StartCoroutine(AddAllSheep(WorldState.instance.player.GetComponent<PlayerSheepAbilities>().sheepFlocks[(int)SheepTypes.BUILD].activeSheep, delay));
	}
	IEnumerator AddAllSheep(List<PlayerSheepAI> flock, float delay)
	{
		RemoveAll(false);
		RemoveAllSheep();

		yield return new WaitForSeconds(delay);
		
		for (int i = 0; i < flock.Count; i++)
		{
			if (flock[i].GetComponent<SphereCollider>() != null)
				SheepRadius = flock[i].GetComponent<SphereCollider>().radius * flock[i].transform.lossyScale.x;
			else if (flock[i].GetComponent<CapsuleCollider>() != null)
				SheepRadius = flock[i].GetComponent<CapsuleCollider>().radius * flock[i].transform.lossyScale.x;

			// set height if this is the first sheep
			if (containedSheep.Count == 0)
			{
				Debug.Log("meh");
				CurveT = 0;// + SheepRadius;
				FMODUnity.RuntimeManager.StudioSystem.setParameterByName("ConstructCompletion", CurveT);

				AddQuad();
				ConnectCap();
			}

			// add the little guy
			AddSheep(flock[i].transform);

			// delay if the sheep increment is right (if bars is two it does sheep 2 at a time)
			if (i % SheepBars == 0 && delay > 0)
				yield return new WaitForSeconds(delay);

			// if the curve is finished, stop counting
			if (CurveT >= 1)
			{
				break;
			}
				
		}
		ConnectCap();
		SetMesh();
#if UNITY_EDITOR
		if (SaveToDatabase)
		{
			AssetDatabase.CreateAsset(mesh, "Assets/BridgeMesh.mesh");
		}
#endif
	}

	void AddSheep(Transform newSheep)
	{
		float RandomCount = 0;

		int SheepChecked = 0;

		// TEMP, FIX LATER, SHOULD BE USING SPHERES IN BOX EQUATION NOW
		float radius = 0.5f * width;

		// math estimating the likely amount of sheep needed to check
		float V_c = Mathf.PI * radius * radius * SheepRadius * 3f;
		float V_s = 1.333f * Mathf.PI * SheepRadius * SheepRadius * SheepRadius;
		int sheeptocheck = Mathf.CeilToInt( 0.7f * V_c / V_s);

		while (CurveT <= 1)
		{
			// make a guess at a good spot to place sheep, within collider bounds
			Vector3 down = DoubleDerivative(CurveT);
			Vector3 right = Vector3.Cross(down, Differentiate(CurveT));
			Vector3 sheepPlacement = Random.Range(height * -0.5f, height * 0.5f) * down + Random.Range(width * -0.5f, width * 0.5f) * right + CalcCurvePoint(CurveT); 

			// check if the spot is filled
			bool Filled = false;
			for (int i = containedSheep.Count - 1; i >= Mathf.Max(0, containedSheep.Count - sheeptocheck); i--) 
			{

				SheepChecked++;

				// check the position, using sphere intersections
				Filled = Filled || SheepIntersection(containedSheep[i].GetComponent<PlayerSheepAI>().constructPos, sheepPlacement);

				// stop checking if we found a filled spot
				if (Filled)
					break;
			}
			if (!Filled) // if empty, place sheep there
			{
				containedSheep.Add(newSheep);
				Debug.Log("Sheep " + containedSheep.Count +" took " + RandomCount + " tries and checked "+ SheepChecked+" sheep");

				// set state of AI
				newSheep.GetComponent<PlayerSheepAI>()?.DoConstruct(sheepPlacement);

				StartCoroutine(LerpSheep(newSheep, sheepPlacement));

				return;
			}
			else if (RandomCount < PlacementGuesses)
				// if filled, try again
				RandomCount++;
			else
			{
				// if done as many checks as allowed, increase height
				RandomCount = 0;
				CurveT += HeightStep * SheepRadius;
				layerCount++;

				FMODUnity.RuntimeManager.StudioSystem.setParameterByName("ConstructCompletion", CurveT);

				AddQuad();
				ConnectTriangles();
				SetMesh();
			}
		}
		if (RandomCount != 0)
			Debug.Log("failed to place sheep :( took "+ RandomCount);
	}

	IEnumerator LerpSheep(Transform newSheep, Vector3 SheepPlacement)
	{
		Vector3 oldpos = newSheep.position;

		float t = 0;
		do
		{
			if (newSheep == null)
				yield break;

			t += Time.deltaTime * lerpSpeed;
			newSheep.position = Vector3.Lerp(oldpos, SheepPlacement, t);
			yield return new WaitForEndOfFrame();

		} while (t < 1 && newSheep != null);

		if (newSheep != null)
		{
			newSheep.position = SheepPlacement;
			newSheep.eulerAngles = Random.insideUnitSphere * 360;
			FMODUnity.RuntimeManager.PlayOneShotAttached(placeSound, newSheep.gameObject);

		}

	}

	// TODO: if collider on final sheep ends up being a capsule, will need new math for calculating displacement
	bool ContainsSheep(Transform sheep, Vector3 pos, float centerDisplacement)
	{
		float radius = (centerDisplacement == SheepRadius) ? 
			SheepRadius : // if the checking sheep is at the same height, dont do the math (else it gives NaN)
			Mathf.Sqrt((SheepRadius * SheepRadius) - (centerDisplacement * centerDisplacement)); // Pythagorean, gets radius of circle in a sphere's slice
		return Mathf.Pow(pos.x - sheep.position.x, 2) + Mathf.Pow(pos.z - sheep.position.z, 2) < radius * radius; // equation to see if point is in circle
	}

	bool SheepIntersection(Vector3 checkingPos, Vector3 newSheepPos)
	{
		return Vector3.Distance(checkingPos, newSheepPos) < (SheepRadius + SheepRadius);
	}
	#endregion

	#region Mesh Collider
	void AddQuad()
	{
		// make a guess at a good spot to place sheep, within collider bounds
		Vector3 down = DoubleDerivative(CurveT);
		Vector3 right = Vector3.Cross(down, Differentiate(CurveT));
		Vector3 center = CalcCurvePoint(CurveT) - transform.position;

		// bottom left
		newVertices.Add(center + down * height * (-ColliderVertBuffer - SheepRadius) + right * (-ColliderHorzBuffer - SheepRadius) * height);

		// bottom right
		newVertices.Add(center + down * height * (-ColliderVertBuffer - SheepRadius) + right * (ColliderHorzBuffer + SheepRadius) * height);

		// top left
		newVertices.Add(center + down * height * (ColliderVertBuffer + SheepRadius) + right * (-ColliderHorzBuffer - SheepRadius) * height);

		// top right
		newVertices.Add(center + down * height * (ColliderVertBuffer + SheepRadius) + right * (ColliderHorzBuffer + SheepRadius) * height);
	}
	void ConnectCap()
	{
		// c = current bot left
		int c = newVertices.Count - 4;

		// left side
		newTriangles.Add(c    );
		newTriangles.Add(c + 2);
		newTriangles.Add(c + 1);
		newTriangles.Add(c + 2);
		newTriangles.Add(c + 3);
		newTriangles.Add(c + 1);
		newTriangles.Add(c + 1);
		newTriangles.Add(c + 2);
		newTriangles.Add(c    );
		newTriangles.Add(c + 1);
		newTriangles.Add(c + 3);
		newTriangles.Add(c + 2);
	}

	void ConnectTriangles()
	{
		// c = current bot left
		int c = newVertices.Count - 4;

		// left side
		newTriangles.Add(c + 2);
		newTriangles.Add(c - 4);
		newTriangles.Add(c    );
		newTriangles.Add(c - 2);
		newTriangles.Add(c - 4);
		newTriangles.Add(c + 2);

		// top
		newTriangles.Add(c + 3);
		newTriangles.Add(c - 2);
		newTriangles.Add(c + 2);
		newTriangles.Add(c + 3);
		newTriangles.Add(c - 1);
		newTriangles.Add(c - 2);

		// right
		newTriangles.Add(c - 3);
		newTriangles.Add(c - 1);
		newTriangles.Add(c + 3);
		newTriangles.Add(c + 1);
		newTriangles.Add(c - 3);
		newTriangles.Add(c + 3);

		// bottom
		newTriangles.Add(c - 4);
		newTriangles.Add(c - 3);
		newTriangles.Add(c + 1);
		newTriangles.Add(c - 4);
		newTriangles.Add(c + 1);
		newTriangles.Add(c    );
	}
	void SetMesh()
	{
		mesh.Clear();
		mesh.vertices = newVertices.ToArray();
		mesh.triangles = newTriangles.ToArray();
		mesh.Optimize();
		mesh.RecalculateBounds();

		mesh.RecalculateNormals();

		col.sharedMesh = mesh;
	}
	#endregion
}
