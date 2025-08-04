using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using UnityEngine;

/*
	Alien Trilogy Data Loader
	Load data directly from original Alien Trilogy files to use it in Unity
*/
public class AlienTrilogyMapLoader : MonoBehaviour
{
	public string levelPath = ""; // path to the .MAP file
	public string texturePath = ""; // path to the .B16 file
	public float texSize = 256f;
	public float scalingFactor = 0.01f; // scaling corrections
	public Material baseMaterial;
	public List<Texture2D> textureTest = new();

	// These store the mesh data for Unity
	private List<Vector3> meshVertices = new();
	private List<Vector2> meshUVs = new();
	private Dictionary<int, List<int>> meshTriangles = new();

	// Original vertex data from MAP0 before duplication
	private List<Vector3> originalVertices = new();

	// UV rectangles for each texture group
	private List<List<(int X, int Y, int Width, int Height)>> uvRects = new();

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
		byte[] mapTextureData = File.ReadAllBytes(texturePath);
		Debug.Log("Map texture bytes: " + mapTextureData.Length);
		BuildMapTextures(mapTextureData);

		// Build map geometry
		byte[] mapFileData = File.ReadAllBytes(levelPath);
		Debug.Log("Map geometry bytes: " + mapFileData.Length);
		BuildMapGeometry(mapFileData);

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
	private void BuildMapTextures(byte[] data)
	{
		// Read BX section
		List<BinaryReader> brList = LoadSection(data, "BX");
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
	}

	/*
		Build the map geometry and prepare mesh data (vertices, uvs, triangles)
	*/
	private void BuildMapGeometry(byte[] data)
	{
		// Read MAP0 section
		List<BinaryReader> map0brList = LoadSection(data, "MAP0");

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
			mat.mainTexture = textureTest[sub.Key];
			SetMaterialRenderingMode(mat, RenderingMode.Cutout);
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
	
	public enum RenderingMode
	{
		Opaque,
		Cutout,
		Fade,
		Transparent
	}

	public void SetMaterialRenderingMode(Material material, RenderingMode mode)
	{
		switch (mode)
		{
			case RenderingMode.Opaque:
				material.SetFloat("_Mode", 0);
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				material.SetInt("_ZWrite", 1);
				material.DisableKeyword("_ALPHATEST_ON");
				material.DisableKeyword("_ALPHABLEND_ON");
				material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = -1;
				break;

			case RenderingMode.Cutout:
				material.SetFloat("_Mode", 1);
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				material.SetInt("_ZWrite", 1);
				material.EnableKeyword("_ALPHATEST_ON");
				material.DisableKeyword("_ALPHABLEND_ON");
				material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = 2450;
				break;

			case RenderingMode.Fade:
				material.SetFloat("_Mode", 2);
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				material.SetInt("_ZWrite", 0);
				material.DisableKeyword("_ALPHATEST_ON");
				material.EnableKeyword("_ALPHABLEND_ON");
				material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = 3000;
				break;

			case RenderingMode.Transparent:
				material.SetFloat("_Mode", 3);
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				material.SetInt("_ZWrite", 0);
				material.DisableKeyword("_ALPHATEST_ON");
				material.DisableKeyword("_ALPHABLEND_ON");
				material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = 3000;
				break;
		}
	}

}
