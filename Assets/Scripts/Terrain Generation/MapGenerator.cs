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

	[Header("Tiling")]
	public bool repeating;
	public Vector2Int totalSize;

	public void GenerateMap()
	{
		float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);
		if (doErosion)
		{
			float[,] erosionMap = (float[,])noiseMap.Clone();

			
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
		if (repeating)
		{
			float[,] originalMap = (float[,])noiseMap.Clone();
			noiseMap = new float[totalSize.x, totalSize.y];

			for (int l = 0; l < totalSize.x; l++)
			{
				for (int c = 0; c < totalSize.y; c++)
				{
					// flip some patches to make it repeat
					int modifiedX = l;
					if ((l / (mapWidth)) % 2 == 1)
						modifiedX += (int)((mapWidth * 0.5f - l) * 2);
					int modifiedY = c;
					if ((c / (mapHeight)) % 2 == 1)
						modifiedY += (int)((mapHeight * 0.5f - c) * 2);


					noiseMap[l, c] = originalMap[Mod(modifiedX, mapWidth), Mod(modifiedY, mapHeight)];

					// fix seams
					if (l % mapWidth == 0 && l != 0 && l != mapWidth - 1)
						noiseMap[l, c] = (noiseMap[l - 1, c] + noiseMap[l + 1, c]);
					if (c % mapHeight == 0 && c != 0 && c != mapHeight - 1)
						noiseMap[l, c] = (noiseMap[l, c - 1] + noiseMap[l, c + 1]);
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

	int Mod(int a, int n) => (a % n + n) % n;
}

[System.Serializable]
public struct TerrainType
{
	public string name;
	public float height;
	public Color colour;
}