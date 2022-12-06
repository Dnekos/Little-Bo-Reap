using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{

	public enum DrawMode { NoiseMap, ColourMap };
	public DrawMode drawMode;

	public int mapWidth;
	public int mapHeight;
	public float noiseScale;

	
	public int octaves;
	[Range(0, 1)]
	public float persistance;
	public float lacunarity;

	public int seed;
	public Vector2 offset;

	public bool autoUpdate;

	public Terrain terra;

	[Header("Erosion")]
	public bool doErosion;
	[Range(0,1)]
	public float threshold;

	public void GenerateMap()
	{
		float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);
		if (doErosion)
		{
			float[,] erosionMap = noiseMap;


			List<Vector2> neighbors = new List<Vector2>();
			for (float nLine = -5; nLine < 5; nLine++)
				for (float nCol = -5; nCol < 5; nCol++)
					neighbors.Add(new Vector2( nLine, nCol));

			int changed = 0;
			for (int l = 5; l < mapWidth - 5; l++)
			{
				for (int c = 5; c < mapHeight - 5; c++)
				{
					float height = erosionMap[l,c];
					float limit = height - threshold;

					for (int i = 0; i < neighbors.Count; i++)
					{
						int nx = l + (int)neighbors[i].x;
						int ny = c + (int)neighbors[i].y;
						float nHeight = erosionMap[nx, ny];

						// is the neighbor below the threshold?
						if (nHeight < limit)
						{
							changed++;
							//std::cout << "height " << height <<" "<< nHeight << std::endl;

							// some of the height moves, from 0 to 1/# of neighbor of the threshold, depending on height difference
							float delta = (limit - nHeight) / threshold;
							if (delta > 2)
								delta = 2;
							float change = delta * threshold / ((neighbors.Count - 1) * 2);

							// write to the copy
							float neg_change = Mathf.Max(0.0f, noiseMap[l, c] - change);
							float pos_change = Mathf.Max(0.0f, noiseMap[nx, ny] + change);

							noiseMap[l, c] = neg_change;
							noiseMap[nx, ny] = pos_change;
						}
					}
				}
			}
		}

		terra.terrainData.SetHeights(0, 0, noiseMap);

		Color[] colourMap = new Color[mapWidth * mapHeight];
		MapDisplay display = FindObjectOfType<MapDisplay>();
		if (drawMode == DrawMode.NoiseMap)
		{
			display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
		}
		else if (drawMode == DrawMode.ColourMap)
		{
			display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
		}
	}

	void OnValidate()
	{
		if (mapWidth < 1)
		{
			mapWidth = 1;
		}
		if (mapHeight < 1)
		{
			mapHeight = 1;
		}
		if (lacunarity < 1)
		{
			lacunarity = 1;
		}
		if (octaves < 0)
		{
			octaves = 0;
		}
	}
}

[System.Serializable]
public struct TerrainType
{
	public string name;
	public float height;
	public Color colour;
}