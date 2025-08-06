using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

/*
	Alien Trilogy Data Loader
	Load data directly from original Alien Trilogy files to use it in Unity
*/
public class AlienTrilogyMapLoader : MonoBehaviour
{
	public string levelPath = ""; // path to the .MAP file
	public string texturePath = ""; // path to the .B16 file
	public int textureSize = 256; // pixel dimensions
	public float scalingFactor = 0.01f; // scaling corrections
	public Material baseMaterial;

	// These store the mesh data for Unity
	private List<Vector3> meshVertices = new();
	private List<Vector2> meshUVs = new();
	private Dictionary<int, List<int>> meshTriangles = new();

	// Original vertex data from MAP0 before duplication
	private List<Vector3> originalVertices = new();

	// UV rectangles for each texture group
	private List<List<(int X, int Y, int Width, int Height)>> uvRects = new();
	
	// Texture image data list
	public List<Texture2D> imgData = new();

	/*
		Called once as soon as this script is loaded
	*/
	void Start()
	{
		Initiate();
	}

	/*
		Load the file from a given path and build the map in Unity
	*/
	private void Initiate()
	{
		// Build map textures
		BuildMapTextures();

		// Build map geometry
		BuildMapGeometry();

		// Build map mesh
		BuildMapMesh();
	}

	/*
		Load a specific byte-section from map data
	*/
	private List<BinaryReader> LoadSection(byte[] data, string sectionName)
	{
		MemoryStream ms = new MemoryStream(data);
		BinaryReader br = new BinaryReader(ms);
		List<BinaryReader> brList = new();

		string formTag = Encoding.ASCII.GetString(br.ReadBytes(4));
		int formSize = BitConverter.ToInt32(br.ReadBytes(4).Reverse().ToArray(), 0);
		string platform = Encoding.ASCII.GetString(br.ReadBytes(4));
		while (br.BaseStream.Position + 8 <= br.BaseStream.Length) // Parse chunks
		{
			string chunkName = Encoding.ASCII.GetString(br.ReadBytes(4));
			int chunkSize = BitConverter.ToInt32(br.ReadBytes(4).Reverse().ToArray(), 0);
			if (br.BaseStream.Position + chunkSize > br.BaseStream.Length) { break; }
			byte[] chunkData = br.ReadBytes(chunkSize);
			if ((chunkSize % 2) != 0) { br.BaseStream.Seek(1, SeekOrigin.Current); } // IFF padding to 2-byte alignment
			if (chunkName.StartsWith(sectionName))
			{
				Debug.Log(sectionName + " > " + chunkName + " > bytes: " + chunkData.Length);
				MemoryStream msChunkData = new MemoryStream(chunkData);
				BinaryReader brChunkData = new BinaryReader(msChunkData);
				brList.Add(brChunkData);
			}
		}

		return brList;
	}
	
	/*
		Build the map textures
	*/
	private void BuildMapTextures()
	{
		byte[] textureData = File.ReadAllBytes(texturePath);
		Debug.Log("Map texture bytes: " + textureData.Length);
		
		int imageId = ExtractIdFromGfxFilename(texturePath);
		
		// Read BX sections
		List<BinaryReader> brList = LoadSection(textureData, "BX");
		uvRects.Clear();
		
		// Read UV from texture groups BX00-BX04
		foreach (BinaryReader bxbr in brList)
		{
			List<(int X, int Y, int Width, int Height)> rects = new();
			int rectCount = bxbr.ReadInt16();
			for (int j = 0; j < rectCount; j++)
			{
				byte x = bxbr.ReadByte();
				byte y = bxbr.ReadByte();
				byte width = bxbr.ReadByte();
				byte height = bxbr.ReadByte();
				bxbr.ReadBytes(2); // unknown bytes
				rects.Add((x, y, width + 1, height + 1));
			}
			uvRects.Add(rects);
		}
		
		// Read TP sections
		List<BinaryReader> brImgList = LoadSection(textureData, "TP");
		List<BinaryReader> brPalList = LoadSection(textureData, "CL");
		Debug.Log("brPalList.Count = " + brPalList.Count);
		
		int imageIndex = 0;
		imgData.Clear();
		
		// Read texture image data from TP00-TP04
		foreach (BinaryReader tpbr in brImgList)
		{
			byte[] imageBytes = tpbr.ReadBytes((int)tpbr.BaseStream.Length);
			byte[] paletteData = brPalList[imageIndex].ReadBytes((int)brPalList[imageIndex].BaseStream.Length); // Embedded palette
			
			List<byte> palD = new();
			for(int p = 0; p < paletteData.Length; p++)
			{
				if(p >= 4)
				{
					palD.Add(paletteData[p]);
				}
			}
			paletteData = Convert16BitPaletteToRGB(palD.ToArray());
			
			Texture2D texture = RenderRaw8bppImageUnity(imageBytes, paletteData, textureSize, textureSize, imageId, imageIndex);
			texture.name = "Tex_" + imageIndex.ToString("D2");
			imgData.Add(texture);
			Debug.Log($"Loaded texture: {texture.width}x{texture.height}");
			imageIndex++;
		}
	}
	
	/*
		Extract map id
	*/
	public int ExtractIdFromGfxFilename(string fullPath)
	{
		string fileName = Path.GetFileName(fullPath); // e.g. "111GFX.B16"
		
		// Match exactly 3 digits at the start of the filename
		var match = Regex.Match(fileName, @"^\d{3}");
		
		if (match.Success && int.TryParse(match.Value, out int id))
		{
			return id;
		}

		return 0; // Fallback if no match or parse fails
	}
	
	/*
		Create 16-bit RGB palette
	*/
	public byte[] Convert16BitPaletteToRGB(byte[] rawPalette)
	{
		if (rawPalette == null || rawPalette.Length < 2) 
			throw new ArgumentException("Palette data is missing or too short.");

		int colorCount = rawPalette.Length / 2;
		byte[] rgbPalette = new byte[256 * 3]; // max 256 colors RGB

		for (int i = 0; i < colorCount && i < 256; i++)
		{
			// Read 16-bit color (little endian)
			ushort color = (ushort)((rawPalette[i * 2 + 1] << 8) | rawPalette[i * 2]);

			int r5 = color & 0x1F;
			int g5 = (color >> 5) & 0x1F;
			int b5 = (color >> 10) & 0x1F;

			// Convert 5-bit color to 8-bit using bit replication
			byte r8 = (byte)((r5 << 3) | (r5 >> 2));
			byte g8 = (byte)((g5 << 3) | (g5 >> 2));
			byte b8 = (byte)((b5 << 3) | (b5 >> 2));

			rgbPalette[i * 3 + 0] = r8;
			rgbPalette[i * 3 + 1] = g8;
			rgbPalette[i * 3 + 2] = b8;
		}

		return rgbPalette;
	}

	/*
		Returns transparent palette indices based on map id and image index
	*/
	private int[] GetTransparencyValues(int id, int index)
	{
		int[] values = null;

		switch (id)
		{
			case 111:
			case 113:
			case 114:
			case 115:
			case 121:
				switch (index)
				{
					case 0:
					case 2:
					case 4:
						values = new int[] { 255 };
						break;
					case 1:
					case 3:
						values = new int[] { 0 };
						break;
				}
				break;

			case 112:
				switch (index)
				{
					case 0:
						return values;
					case 1:
					case 3:
						values = new int[] { 0 };
						break;
					case 2:
					case 4:
						values = new int[] { 255 };
						break;
				}
				break;

			case 122:
			case 213:
				switch (index)
				{
					case 0:
						values = new int[] { 255 };
						break;
					case 1:
					case 2:
					case 3:
					case 4:
						return values;
				}
				break;

			case 131:
			case 211:
			case 212:
			case 231:
			case 232:
			case 242:
			case 243:
			case 262:
			case 331:
			case 361:
			case 391:
			case 901:
			case 906:
			case 907:
				return values;

			case 141:
			case 155:
			case 161:
			case 162:
			case 263:
			case 311:
			case 352:
			case 353:
			case 381:
			case 902:
			case 903:
			case 908:
			case 909:
				switch (index)
				{
					case 0:
					case 1:
					case 2:
					case 3:
						return values;
					case 4:
						values = new int[] { 255 };
						break;
				}
				break;

			case 154:
			case 321:
			case 322:
			case 323:
			case 324:
			case 325:
				switch (index)
				{
					case 0:
					case 4:
						values = new int[] { 255 };
						break;
					case 1:
					case 2:
					case 3:
						return values;
				}
				break;

			case 222:
				switch (index)
				{
					case 0:
					case 1:
					case 3:
					case 4:
						return values;
					case 2:
						values = new int[] { 255 };
						break;
				}
				break;

			case 351:
			case 371:
				switch (index)
				{
					case 0:
					case 1:
					case 3:
						return values;
					case 2:
					case 4:
						values = new int[] { 255 };
						break;
				}
				break;

			case 900:
				switch (index)
				{
					case 0:
						values = new int[] { 255 };
						break;
					case 1:
					case 3:
						values = new int[] { 0 };
						break;
					case 2:
					case 4:
						return values;
				}
				break;

			case 904:
			case 905:
				switch (index)
				{
					case 0:
						values = new int[] { 255 };
						break;
					case 1:
					case 2:
					case 3:
						return values;
					case 4:
						values = new int[] { 255 };
						break;
				}
				break;

			default:
				break;
		}

		return values;
	}

	/*
		Create 8-bit texture from 16-bit palette, with transparency based on palette indices for given index
	*/
	private Texture2D RenderRaw8bppImageUnity(
		byte[] pixelData, 
		byte[] rgbPalette, 
		int width, 
		int height, 
		int mapId,
		int imageIndex)
	{
		int[] transparentValues = GetTransparencyValues(mapId, imageIndex); // get transparency for this image

		int numColors = rgbPalette.Length / 3; // Number of colors in palette
		Color32[] pixels = new Color32[width * height];
		Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				int srcIndex = y * width + x;
				if (srcIndex >= pixelData.Length)
					continue;

				byte colorIndex = pixelData[srcIndex];
				Color32 color;

				// Defensive: if palette data is incomplete or index out of range, fallback magenta
				if (colorIndex < numColors && (colorIndex * 3 + 2) < rgbPalette.Length)
				{
					byte r = rgbPalette[colorIndex * 3];
					byte g = rgbPalette[colorIndex * 3 + 1];
					byte b = rgbPalette[colorIndex * 3 + 2];
					color = new Color32(r, g, b, 255);
				}
				else
				{
					color = new Color32(255, 0, 255, 255); // Magenta fallback
				}

				// Handle transparency: if pixel color index is in transparentValues, make fully transparent
				if (transparentValues != null && transparentValues.Contains(colorIndex))
				{
					color.a = 0;  // Fully transparent
				}

				// Vertical flip: write pixels from bottom to top
				int flippedY = height - 1 - y;
				int dstIndex = flippedY * width + x;
				pixels[dstIndex] = color;
			}
		}

		texture.SetPixels32(pixels);
		texture.Apply();

		return texture;
	}
	
	/*
		Build the map geometry and prepare mesh data (vertices, uvs, triangles)
	*/
	private void BuildMapGeometry()
	{
		byte[] mapData = File.ReadAllBytes(levelPath);
		Debug.Log("Map geometry bytes: " + mapData.Length);
		
		// Read MAP0 section
		List<BinaryReader> map0brList = LoadSection(mapData, "MAP0");

		meshVertices.Clear();
		meshUVs.Clear();
		meshTriangles.Clear();

		originalVertices.Clear();

		foreach (BinaryReader map0br in map0brList)
		{
			// Read number of vertices
			ushort vertCount = map0br.ReadUInt16();
			Debug.Log("Number of vertices: " + vertCount);

			// Read number of quads
			ushort quadCount = map0br.ReadUInt16();
			Debug.Log("Number of quads: " + quadCount);

			// Skip unused bytes
			map0br.BaseStream.Seek(32, SeekOrigin.Current);

			// Read original vertices
			originalVertices.Clear();
			for (int i = 0; i < vertCount; i++)
			{
				try
				{
					short x = map0br.ReadInt16();
					short y = map0br.ReadInt16();
					short z = map0br.ReadInt16();
					map0br.ReadBytes(2); // unknown bytes
					originalVertices.Add(new Vector3(x, y, z));
				}
				catch (Exception e)
				{
					Debug.LogError($"Failed at vertex index {i}: {e}");
					break;
				}
			}
			
			Debug.Log("originalVertices.Count = " + originalVertices.Count);

			// Read and process quads, duplicating vertices & uvs per quad
			for (int i = 0; i < quadCount; i++)
			{
				int a = 0;
				int b = 0;
				int c = 0;
				int d = 0;
				try
				{
					a = map0br.ReadInt32();
					b = map0br.ReadInt32();
					c = map0br.ReadInt32();
					d = map0br.ReadInt32();
					ushort texIndex = map0br.ReadUInt16();
					byte flags = map0br.ReadByte();
					byte other = map0br.ReadByte();

					// Determine texture group based on texIndex
					int localIndex = texIndex;
					int texGroup = 0;
					for (int t = 0; t < uvRects.Count; t++)
					{
						int count = uvRects[t].Count;
						if (localIndex < count)
						{
							texGroup = t;
							break;
						}
						localIndex -= count;
					}
					
					Vector3 vA, vB, vC, vD;
					bool issueFound = false;
					
					// Validate vertex indices
					if (a < 0 || b < 0 || c < 0 || d < 0 ||
						a >= originalVertices.Count || b >= originalVertices.Count ||
						c >= originalVertices.Count || d >= originalVertices.Count)
					{
						Debug.LogWarning($"Invalid quad indices at quad {i}: {a}, {b}, {c}, {d}, {flags}");
						d = a; // Triangle instead of quad
						issueFound = true;
					}

					// Get quad vertices positions
					vA = originalVertices[a];
					vB = originalVertices[b];
					vC = originalVertices[c];
					vD = originalVertices[d];

					// Get UV rectangle for texture
					var rect = uvRects[texGroup][localIndex];

					// Calculate UV coords (Unity uses bottom-left origin for UV)
					float texSize = (float)textureSize;
					float uMin = rect.X / texSize;
					float vMin = 1f - (rect.Y + rect.Height) / texSize;
					float uMax = (rect.X + rect.Width) / texSize;
					float vMax = 1f - rect.Y / texSize;

					Vector2[] baseUvs = new Vector2[]
					{
						new Vector2(uMin, vMin), // A
						new Vector2(uMax, vMin), // B
						new Vector2(uMax, vMax), // C
						new Vector2(uMin, vMax)  // D
					};

					Vector2[] quadUvs;

					// Adjust UVs based on quad flags
					switch (flags)
					{
						case 2:
							// Flip texture 180 degrees
							quadUvs = new Vector2[] { baseUvs[1], baseUvs[0], baseUvs[3], baseUvs[2] };
							break;

						case 11:
							// Special triangle case: repeat D's UV for the 4th vertex
							quadUvs = new Vector2[] { baseUvs[0], baseUvs[2], baseUvs[3], baseUvs[3] };
							break;

						default:
							// Default UV mapping
							quadUvs = baseUvs;
							break;
					}
					
					// Adjust UVs based on triangles
					if(issueFound)
					{
						quadUvs = new Vector2[] { baseUvs[0], baseUvs[2], baseUvs[3], baseUvs[3] };
					}

					// Add duplicated vertices for this quad
					int baseIndex = meshVertices.Count;

					meshVertices.Add(vA);
					meshVertices.Add(vB);
					meshVertices.Add(vC);
					meshVertices.Add(vD);

					// Add UVs for each vertex
					meshUVs.AddRange(quadUvs);

					// Add triangles for this quad (two triangles)
					if (!meshTriangles.TryGetValue(texGroup, out List<int> tris))
					{
						tris = new List<int>();
						meshTriangles[texGroup] = tris;
					}

					tris.Add(baseIndex + 0);
					tris.Add(baseIndex + 1);
					tris.Add(baseIndex + 2);

					tris.Add(baseIndex + 2);
					tris.Add(baseIndex + 3);
					tris.Add(baseIndex + 0);
				}
				catch (Exception e)
				{
					Debug.LogWarning($"Failed at quad index {i}:  {a}, {b}, {c}, {d} - {e}");
					break;
				}
			}
		}
	}

	/*
		Build the map mesh in Unity from duplicated vertices/uvs and triangles
	*/
	private void BuildMapMesh()
	{
		Mesh mesh = new Mesh();
		mesh.name = "Mesh";

		mesh.vertices = meshVertices.ToArray();
		mesh.uv = meshUVs.ToArray();

		mesh.subMeshCount = uvRects.Count;

		Material[] materials = new Material[mesh.subMeshCount];
		foreach (var sub in meshTriangles)
		{
			mesh.SetTriangles(sub.Value, sub.Key);

			Material mat = new Material(baseMaterial);
			mat.name = "Mat_" + sub.Key;
			mat.color = Color.white;
			mat.mainTexture = imgData[sub.Key];
			materials[sub.Key] = mat;
		}

		mesh.RecalculateNormals();
		mesh.RecalculateBounds();

		GameObject child = new GameObject("Map");
		child.transform.parent = this.transform;
		child.transform.localPosition = Vector3.zero;
		child.transform.localRotation = Quaternion.identity;
		child.transform.localScale = Vector3.one;

		var mf = child.AddComponent<MeshFilter>();
		mf.mesh = mesh;

		var mr = child.AddComponent<MeshRenderer>();
		mr.materials = materials;

		// Un-mirror
		child.transform.localScale = new Vector3(1f, 1f, -1f) * scalingFactor;

		Debug.Log("mesh.subMeshCount = " + mesh.subMeshCount);
	}

}
