using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;

public class ErosionManagerJobs : MonoBehaviour
{

//    [SerializeField]
//    ChunkManager chunkManager;

//    [SerializeField]
//    MarchingCubes marchingCubes;

//    public float m_erosionValue = 0.01f;

//    public int m_droplets = 50;

//    public float m_erosionThreshold = 10f;

//    NativeArray<DensityData> m_DensityData;

//    // Start is called before the first frame update
//    void Start()
//    {
//        UpdateDroplets();
//    }

//    public void UpdateDroplets()
//    {
//        m_DensityData = new NativeArray<DensityData>(m_droplets, Allocator.Persistent);
//    }

//    // Update is called once per frame
//    void Update()
//    {

//    }

//    public void Erosion()
//    {
//        UpdateDroplets();
//        Debug.Log("erosion running");
//        List<Vector2Int> chunkCoords = new List<Vector2Int>(chunkManager.horizontalChunks);
//        foreach (var coord in chunkCoords)
//        {
//            int j = 10;
//            while (!chunkManager.CheckChunk(new Vector3Int(coord.x, j, coord.y)))
//            {
//                j--;
//                if (j < -10)
//                {
//                    break;
//                }
//            }
//            // if chunk level less than -10, go to next xz chunk column
//            if (j < -10)
//            {
//                continue;
//            }

//            // run fluid sim on chunk
//            for (int i = 0; i < m_droplets; i++)
//            {
//                int x = Random.Range(0, 32);
//                int z = Random.Range(0, 32);
//                int w = Random.Range(4, 8);

//                // create DensityData object representing droplet
//                Vector3Int chc = new Vector3Int(coord.x, j, coord.y);
//                Vector3Int cuc = new Vector3Int(x, 31, z);
//                DensityData dd = new DensityData(chc, cuc, w, 0);
//                m_DensityData[i] = dd;

//                ErosionHelper(coord.x, j, coord.y, x, 31, z, w, 0, 0);
//            }


//        }
//    }


//}

//public struct DensityData
//{
//    public Vector3Int chunkCoord;

//    public Vector3Int cubeCoord;

//    public int cubeVert;

//    public float density;

//    public DensityData(Vector3Int chc, Vector3Int cuc, int w, float d)
//    {
//        chunkCoord = chc;
//        cubeCoord = cuc;
//        cubeVert = w;
//        density = d;
//    }
//}

//public struct CalculateErosion : IJob
//{
//    public NativeArray<DensityData> densityData;

//    public float erosionValue;

//    public float erosionThreshold;



//    public void Execute()
//    {

//    }

//    void ErosionHelper(int i, int j, int k, int x, int y, int z, int w, float erosionAmount, int iteration)
//    {
//        Vector3Int chunkCoord = new Vector3Int(i, j, k);
//        Cube[,,] chunk = chunkManager.CheckGetChunk(chunkCoord);
//        if (chunk == null)
//        {
//            return;
//        }
//        // if reach chunk limit return
//        if (i < -10)
//        {
//            return;
//        }

//        Cube cube = chunk[x, y, z];
//        // if cube vertex is Air move downwards
//        if (cube.materials[w] == BlockType.Air)
//        {
//            if (w >= 4)
//            {
//                ErosionHelper(i, j, k, x, y, z, w - 4, erosionAmount, 0);
//                return;
//            }
//            else
//            {
//                if (y == 0)
//                {
//                    ErosionHelper(i, j - 1, k, x, 31, z, w + 4, erosionAmount, 0);
//                }
//                else
//                {
//                    ErosionHelper(i, j, k, x, y - 1, z, w + 4, erosionAmount, 0);
//                }
//            }
//        }
//        else // if not Air check if adjacent is Air
//        {
//            Mesh mesh = marchingCubes.meshes[chunkCoord].GetComponent<MeshFilter>().mesh;
//            Vector3[] normals = mesh.normals;
//            // go through vertex indices of cube and get upwards facing vectors
//            Vector3 normalAverage = Vector3.zero;
//            int normalNum = 0;
//            foreach (int index in cube.meshVertices)
//            {
//                Vector3 normal = normals[index];
//                if (normal.y > 0)
//                {
//                    normalAverage += normal;
//                    normalNum++;
//                }
//            }
//            normalAverage /= normalNum;
//            // if normal is close to vertical then deposit droplet
//            if (Vector3.Angle(Vector3.up, normalAverage) < erosionThreshold)
//            {
//                float e = erosionAmount / 4.0f;
//                if (w >= 4)
//                {
//                    ChangeDensity(i, j, k, x, y, z, 4, e);
//                    ChangeDensity(i, j, k, x, y, z, 5, e);
//                    ChangeDensity(i, j, k, x, y, z, 6, e);
//                    ChangeDensity(i, j, k, x, y, z, 7, e);
//                }
//                else
//                {
//                    ChangeDensity(i, j, k, x, y, z, 0, e);
//                    ChangeDensity(i, j, k, x, y, z, 1, e);
//                    ChangeDensity(i, j, k, x, y, z, 2, e);
//                    ChangeDensity(i, j, k, x, y, z, 3, e);
//                }
//                return;
//            }
//            else
//            {
//                // remove density and choose direction to move droplet based on normal
//                if (iteration > 3)
//                {
//                    float e = erosionAmount / 4.0f;
//                    if (w >= 4)
//                    {
//                        ChangeDensity(i, j, k, x, y, z, 4, e);
//                        ChangeDensity(i, j, k, x, y, z, 5, e);
//                        ChangeDensity(i, j, k, x, y, z, 6, e);
//                        ChangeDensity(i, j, k, x, y, z, 7, e);
//                    }
//                    else
//                    {
//                        ChangeDensity(i, j, k, x, y, z, 0, e);
//                        ChangeDensity(i, j, k, x, y, z, 1, e);
//                        ChangeDensity(i, j, k, x, y, z, 2, e);
//                        ChangeDensity(i, j, k, x, y, z, 3, e);
//                    }
//                    return;
//                }
//                else
//                {
//                    ChangeDensity(i, j, k, x, y, z, w, -erosionValue);
//                }
//                float direction = Vector3.SignedAngle(Vector3.forward, new Vector3(normalAverage.x, 0, normalAverage.z), Vector3.up);
//                if (direction > -22.5f && direction <= 22.5f)
//                {
//                    if (z == 31)
//                    {
//                        if (chunkManager.CheckGetChunk(new Vector3Int(i, j, k + 1)) != null)
//                        {
//                            ErosionHelper(i, j, k + 1, x, y, 0, w, erosionAmount + erosionValue, iteration + 1);
//                        }
//                    }
//                    else
//                    {
//                        ErosionHelper(i, j, k, x, y, z + 1, w, erosionAmount + erosionValue, iteration + 1);
//                    }
//                }
//                else if (direction > 22.5f && direction <= 67.5f)
//                {
//                    if (z == 31 && x == 31)
//                    {
//                        if (chunkManager.CheckGetChunk(new Vector3Int(i + 1, j, k + 1)) != null)
//                        {
//                            ErosionHelper(i + 1, j, k + 1, 0, y, 0, w, erosionAmount + erosionValue, iteration + 1);
//                        }
//                    }
//                    else if (z == 31)
//                    {
//                        if (chunkManager.CheckGetChunk(new Vector3Int(i, j, k + 1)) != null)
//                        {
//                            ErosionHelper(i, j, k + 1, x + 1, y, 0, w, erosionAmount + erosionValue, iteration + 1);
//                        }
//                    }
//                    else if (x == 31)
//                    {
//                        if (chunkManager.CheckGetChunk(new Vector3Int(i + 1, j, k)) != null)
//                        {
//                            ErosionHelper(i + 1, j, k, 0, y, z + 1, w, erosionAmount + erosionValue, iteration + 1);
//                        }
//                    }
//                    else
//                    {
//                        ErosionHelper(i, j, k, x + 1, y, z + 1, w, erosionAmount + erosionValue, iteration + 1);
//                    }
//                }
//                else if (direction > 67.5f && direction <= 112.5f)
//                {
//                    if (x == 31)
//                    {
//                        if (chunkManager.CheckGetChunk(new Vector3Int(i + 1, j, k)) != null)
//                        {
//                            ErosionHelper(i + 1, j, k, 0, y, z, w, erosionAmount + erosionValue, iteration + 1);
//                        }
//                    }
//                    else
//                    {
//                        ErosionHelper(i, j, k, x + 1, y, z, w, erosionAmount + erosionValue, iteration + 1);
//                    }
//                }
//                else if (direction > 112.5f && direction <= 157.5f)
//                {
//                    if (z == 0 && x == 31)
//                    {
//                        if (chunkManager.CheckGetChunk(new Vector3Int(i + 1, j, k - 1)) != null)
//                        {
//                            ErosionHelper(i + 1, j, k - 1, 0, y, 31, w, erosionAmount + erosionValue, iteration + 1);
//                        }
//                    }
//                    else if (z == 0)
//                    {
//                        if (chunkManager.CheckGetChunk(new Vector3Int(i, j, k - 1)) != null)
//                        {
//                            ErosionHelper(i, j, k - 1, x + 1, y, 31, w, erosionAmount + erosionValue, iteration + 1);
//                        }
//                    }
//                    else if (x == 31)
//                    {
//                        if (chunkManager.CheckGetChunk(new Vector3Int(i + 1, j, k)) != null)
//                        {
//                            ErosionHelper(i + 1, j, k, 0, y, z - 1, w, erosionAmount + erosionValue, iteration + 1);
//                        }
//                    }
//                    else
//                    {
//                        ErosionHelper(i, j, k, x + 1, y, z - 1, w, erosionAmount + erosionValue, iteration + 1);
//                    }
//                }
//                else if (direction > 157.5f || direction <= -157.5f)
//                {
//                    if (z == 0)
//                    {
//                        if (chunkManager.CheckGetChunk(new Vector3Int(i, j, k - 1)) != null)
//                        {
//                            ErosionHelper(i, j, k - 1, x, y, 31, w, erosionAmount + erosionValue, iteration + 1);
//                        }
//                    }
//                    else
//                    {
//                        ErosionHelper(i, j, k, x, y, z - 1, w, erosionAmount + erosionValue, iteration + 1);
//                    }
//                }
//                else if (direction > -157.5f && direction <= -112.5f)
//                {
//                    if (z == 0 && x == 0)
//                    {
//                        if (chunkManager.CheckGetChunk(new Vector3Int(i - 1, j, k - 1)) != null)
//                        {
//                            ErosionHelper(i - 1, j, k - 1, 31, y, 31, w, erosionAmount + erosionValue, iteration + 1);
//                        }
//                    }
//                    else if (z == 0)
//                    {
//                        if (chunkManager.CheckGetChunk(new Vector3Int(i, j, k - 1)) != null)
//                        {
//                            ErosionHelper(i, j, k - 1, x - 1, y, 31, w, erosionAmount + erosionValue, iteration + 1);
//                        }
//                    }
//                    else if (x == 0)
//                    {
//                        if (chunkManager.CheckGetChunk(new Vector3Int(i - 1, j, k)) != null)
//                        {
//                            ErosionHelper(i - 1, j, k, 31, y, z - 1, w, erosionAmount + erosionValue, iteration + 1);
//                        }
//                    }
//                    else
//                    {
//                        ErosionHelper(i, j, k, x - 1, y, z - 1, w, erosionAmount + erosionValue, iteration + 1);
//                    }
//                }
//                else if (direction > -112.5f && direction <= -67.5f)
//                {
//                    if (x == 0)
//                    {
//                        if (chunkManager.CheckGetChunk(new Vector3Int(i - 1, j, k)) != null)
//                        {
//                            ErosionHelper(i - 1, j, k, 31, y, z, w, erosionAmount + erosionValue, iteration + 1);
//                        }
//                    }
//                    else
//                    {
//                        ErosionHelper(i, j, k, x - 1, y, z, w, erosionAmount + erosionValue, iteration + 1);
//                    }
//                }
//                else
//                {
//                    if (z == 31 && x == 0)
//                    {
//                        if (chunkManager.CheckGetChunk(new Vector3Int(i - 1, j, k + 1)) != null)
//                        {
//                            ErosionHelper(i - 1, j, k + 1, 31, y, 0, w, erosionAmount + erosionValue, iteration + 1);
//                        }
//                    }
//                    else if (z == 31)
//                    {
//                        if (chunkManager.CheckGetChunk(new Vector3Int(i, j, k + 1)) != null)
//                        {
//                            ErosionHelper(i, j, k + 1, x - 1, y, 0, w, erosionAmount + erosionValue, iteration + 1);
//                        }
//                    }
//                    else if (x == 0)
//                    {
//                        if (chunkManager.CheckGetChunk(new Vector3Int(i - 1, j, k)) != null)
//                        {
//                            ErosionHelper(i - 1, j, k, 31, y, z + 1, w, erosionAmount + erosionValue, iteration + 1);
//                        }
//                    }
//                    else
//                    {
//                        ErosionHelper(i, j, k, x - 1, y, z + 1, w, erosionAmount + erosionValue, iteration + 1);
//                    }
//                }
//            }
//        }
//    }

//    BlockType TurnToAir(Cube cube, int index)
//    {
//        BlockType block = cube.materials[index];

//        if (cube.surfaceValues[index] <= 0)
//        {
//            block = BlockType.Air;
//        }
//        return block;
//    }

//    void ChangeNeighbor(int val, float density, int i, int j, int k, int x, int y, int z)
//    {
//        Cube[,,] chunk = chunkManager.CheckGetChunk(new Vector3Int(i, j, k));
//        if (chunk != null)
//        {
//            chunk[x, y, z].surfaceValues[val] += density;
//            chunk[x, y, z].materials[val] = TurnToAir(chunk[x, y, z], val);
//        }
//    }

//    void ChangeDensity(int i, int j, int k, int x, int y, int z, int w, float density)
//    {
//        Cube[,,] chunk = chunkManager.CheckGetChunk(new Vector3Int(i, j, k));
//        if (chunk == null)
//        {
//            return;
//        }
//        Cube cube = chunk[x, y, z];
//        cube.surfaceValues[w] += density;
//        cube.materials[w] = TurnToAir(cube, w);

//        switch (w)
//        {
//            case 0:
//                int val = 1;
//                if (x > 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x - 1, y, z);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i - 1, j, k, 31, y, z);
//                }

//                val = 4;
//                if (y > 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x, y - 1, z);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i, j - 1, k, x, 31, z);
//                }

//                val = 3;
//                if (z > 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x, y, z - 1);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i, j, k - 1, x, y, 31);
//                }

//                val = 5;
//                if (x > 0 && y > 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x - 1, y - 1, z);
//                }
//                else if (x > 0 && y == 0)
//                {
//                    ChangeNeighbor(val, density, i, j - 1, k, x - 1, 31, z);
//                }
//                else if (x == 0 && y > 0)
//                {
//                    ChangeNeighbor(val, density, i - 1, j, k, 31, y - 1, z);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i - 1, j - 1, k, 31, 31, z);
//                }

//                val = 2;
//                if (x > 0 && z > 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x - 1, y, z - 1);
//                }
//                else if (x > 0 && z == 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k - 1, x - 1, y, 31);
//                }
//                else if (x == 0 && z > 0)
//                {
//                    ChangeNeighbor(val, density, i - 1, j, k, 31, y, z - 1);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i - 1, j, k - 1, 31, y, 31);
//                }

//                val = 7;
//                if (y > 0 && z > 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x, y - 1, z - 1);
//                }
//                else if (y > 0 && z == 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k - 1, x, y - 1, 31);
//                }
//                else if (y == 0 && z > 0)
//                {
//                    ChangeNeighbor(val, density, i, j - 1, k, x, 31, z - 1);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i, j - 1, k - 1, x, 31, 31);
//                }

//                val = 6;
//                if (x > 0 && y > 0 && z > 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x - 1, y - 1, z - 1);
//                }
//                if (x > 0 && y > 0 && z == 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k - 1, x - 1, y - 1, 31);
//                }
//                if (x > 0 && y == 0 && z > 0)
//                {
//                    ChangeNeighbor(val, density, i, j - 1, k, x - 1, 31, z - 1);
//                }
//                if (x == 0 && y > 0 && z > 0)
//                {
//                    ChangeNeighbor(val, density, i - 1, j, k, 31, y - 1, z - 1);
//                }
//                if (x > 0 && y == 0 && z == 0)
//                {
//                    ChangeNeighbor(val, density, i, j - 1, k - 1, x - 1, 31, 31);
//                }
//                if (x == 0 && y > 0 && z == 0)
//                {
//                    ChangeNeighbor(val, density, i - 1, j, k - 1, 31, y - 1, 31);
//                }
//                if (x == 0 && y == 0 && z > 0)
//                {
//                    ChangeNeighbor(val, density, i - 1, j - 1, k, 31, 31, z - 1);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i - 1, j - 1, k - 1, 31, 31, 31);
//                }

//                break;
//            case 1:
//                val = 0;
//                if (x < 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x + 1, y, z);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i + 1, j, k, 0, y, z);
//                }

//                val = 5;
//                if (y > 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x, y - 1, z);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i, j - 1, k, x, 31, z);
//                }

//                val = 2;
//                if (z > 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x, y, z - 1);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i, j, k - 1, x, y, 31);
//                }

//                val = 4;
//                if (x < 31 && y > 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x + 1, y - 1, z);
//                }
//                else if (x < 31 && y == 0)
//                {
//                    ChangeNeighbor(val, density, i, j - 1, k, x + 1, 31, z);
//                }
//                else if (x == 31 && y > 0)
//                {
//                    ChangeNeighbor(val, density, i + 1, j, k, 0, y - 1, z);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i + 1, j - 1, k, 0, 31, z);
//                }

//                val = 3;
//                if (x < 31 && z > 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x + 1, y, z - 1);
//                }
//                else if (x < 31 && z == 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k - 1, x + 1, y, 31);
//                }
//                else if (x == 31 && z > 0)
//                {
//                    ChangeNeighbor(val, density, i + 1, j, k, 0, y, z - 1);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i + 1, j, k - 1, 0, y, 31);
//                }

//                val = 6;
//                if (y > 0 && z > 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x, y - 1, z - 1);
//                }
//                else if (y > 0 && z == 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k - 1, x, y - 1, 31);
//                }
//                else if (y == 0 && z > 0)
//                {
//                    ChangeNeighbor(val, density, i, j - 1, k, x, 31, z - 1);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i, j - 1, k - 1, x, 31, 31);
//                }

//                val = 7;
//                if (x < 31 && y > 0 && z > 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x + 1, y - 1, z - 1);
//                }
//                else if (x < 31 && y > 0 && z == 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k - 1, x + 1, y - 1, 31);
//                }
//                else if (x < 31 && y == 0 && z > 0)
//                {
//                    ChangeNeighbor(val, density, i, j - 1, k, x + 1, 31, z - 1);
//                }
//                else if (x == 31 && y > 0 && z > 0)
//                {
//                    ChangeNeighbor(val, density, i + 1, j, k, 0, y - 1, z - 1);
//                }
//                else if (x < 31 && y == 0 && z == 0)
//                {
//                    ChangeNeighbor(val, density, i, j - 1, k - 1, x + 1, 31, 31);
//                }
//                else if (x == 31 && y > 0 && z == 0)
//                {
//                    ChangeNeighbor(val, density, i + 1, j, k - 1, 0, y - 1, 31);
//                }
//                else if (x == 31 && y == 0 && z > 0)
//                {
//                    ChangeNeighbor(val, density, i + 1, j - 1, k, 0, 31, z - 1);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i + 1, j - 1, k - 1, 0, 31, 31);
//                }

//                break;
//            case 2:
//                val = 3;
//                if (x < 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x + 1, y, z);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i + 1, j, k, 0, y, z);
//                }

//                val = 6;
//                if (y > 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x, y - 1, z);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i, j - 1, k, x, 31, z);
//                }

//                val = 1;
//                if (z < 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x, y, z + 1);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i, j, k + 1, x, y, 0);
//                }

//                val = 7;
//                if (x < 31 && y > 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x + 1, y - 1, z);
//                }
//                else if (x < 31 && y == 0)
//                {
//                    ChangeNeighbor(val, density, i, j - 1, k, x + 1, 31, z);
//                }
//                else if (x == 31 && y > 0)
//                {
//                    ChangeNeighbor(val, density, i + 1, j, k, 0, y - 1, z);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i + 1, j - 1, k, 0, 31, z);
//                }

//                val = 0;
//                if (x < 31 && z < 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x + 1, y, z + 1);
//                }
//                else if (x < 31 && z == 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k + 1, x + 1, y, 0);
//                }
//                else if (x == 31 && z < 31)
//                {
//                    ChangeNeighbor(val, density, i + 1, j, k, 0, y, z + 1);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i + 1, j, k + 1, 0, y, 0);
//                }

//                val = 5;
//                if (y > 0 && z < 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x, y - 1, z + 1);
//                }
//                else if (y > 0 && z == 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k + 1, x, y - 1, 0);
//                }
//                else if (y == 0 && z < 31)
//                {
//                    ChangeNeighbor(val, density, i, j - 1, k, x, 31, z + 1);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i, j - 1, k + 1, x, 31, 0);
//                }

//                val = 4;
//                if (x < 31 && y > 0 && z < 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x + 1, y - 1, z + 1);
//                }
//                else if (x < 31 && y > 0 && z == 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k + 1, x + 1, y - 1, 0);
//                }
//                else if (x < 31 && y == 0 && z < 31)
//                {
//                    ChangeNeighbor(val, density, i, j - 1, k, x + 1, 31, z + 1);
//                }
//                else if (x == 31 && y > 0 && z < 31)
//                {
//                    ChangeNeighbor(val, density, i + 1, j, k, 0, y - 1, z + 1);
//                }
//                else if (x < 31 && y == 0 && z == 31)
//                {
//                    ChangeNeighbor(val, density, i, j - 1, k + 1, x + 1, 31, 0);
//                }
//                else if (x == 31 && y > 0 && z == 31)
//                {
//                    ChangeNeighbor(val, density, i + 1, j, k + 1, 0, y - 1, 0);
//                }
//                else if (x == 31 && y == 0 && z < 31)
//                {
//                    ChangeNeighbor(val, density, i + 1, j - 1, k, 0, 31, z + 1);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i + 1, j - 1, k + 1, 0, 31, 0);
//                }

//                break;
//            case 3:
//                val = 2;
//                if (x > 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x - 1, y, z);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i - 1, j, k, 31, y, z);
//                }

//                val = 7;
//                if (y > 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x, y - 1, z);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i, j - 1, k, x, 31, z);
//                }

//                val = 0;
//                if (z < 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x, y, z + 1);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i, j, k + 1, x, y, 0);
//                }

//                val = 6;
//                if (x > 0 && y > 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x - 1, y - 1, z);
//                }
//                else if (x > 0 && y == 0)
//                {
//                    ChangeNeighbor(val, density, i, j - 1, k, x - 1, 31, z);
//                }
//                else if (x == 0 && y > 0)
//                {
//                    ChangeNeighbor(val, density, i - 1, j, k, 31, y - 1, z);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i - 1, j - 1, k, 31, 31, z);
//                }

//                val = 1;
//                if (x > 0 && z < 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x - 1, y, z + 1);
//                }
//                else if (x > 0 && z == 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k + 1, x - 1, y, 0);
//                }
//                else if (x == 0 && z < 31)
//                {
//                    ChangeNeighbor(val, density, i - 1, j, k, 31, y, z + 1);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i - 1, j, k + 1, 31, y, 0);
//                }

//                val = 4;
//                if (y > 0 && z < 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x, y - 1, z + 1);
//                }
//                else if (y > 0 && z == 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k + 1, x, y - 1, 0);
//                }
//                else if (y == 0 && z < 31)
//                {
//                    ChangeNeighbor(val, density, i, j - 1, k, x, 31, z + 1);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i, j - 1, k + 1, x, 31, 0);
//                }

//                val = 5;
//                if (x > 0 && y > 0 && z < 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x - 1, y - 1, z + 1);
//                }
//                else if (x > 0 && y > 0 && z == 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k + 1, x - 1, y - 1, 0);
//                }
//                else if (x > 0 && y == 0 && z < 31)
//                {
//                    ChangeNeighbor(val, density, i, j - 1, k, x - 1, 31, z + 1);
//                }
//                else if (x == 0 && y > 0 && z < 31)
//                {
//                    ChangeNeighbor(val, density, i - 1, j, k, 31, y - 1, z + 1);
//                }
//                else if (x > 0 && y == 0 && z == 31)
//                {
//                    ChangeNeighbor(val, density, i, j - 1, k + 1, x - 1, 31, 0);
//                }
//                else if (x == 0 && y > 0 && z == 31)
//                {
//                    ChangeNeighbor(val, density, i - 1, j, k + 1, 31, y - 1, 0);
//                }
//                else if (x == 0 && y == 0 && z < 31)
//                {
//                    ChangeNeighbor(val, density, i - 1, j - 1, k, 31, 31, z + 1);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i - 1, j - 1, k + 1, 31, 31, 0);
//                }

//                break;
//            case 4:
//                val = 5;
//                if (x > 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x - 1, y, z);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i - 1, j, k, 31, y, z);
//                }

//                val = 0;
//                if (y < 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x, y + 1, z);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i, j + 1, k, x, 0, z);
//                }

//                val = 7;
//                if (z > 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x, y, z - 1);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i, j, k - 1, x, y, 31);
//                }

//                val = 1;
//                if (x > 0 && y < 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x - 1, y + 1, z);
//                }
//                else if (x > 0 && y == 31)
//                {
//                    ChangeNeighbor(val, density, i, j + 1, k, x - 1, 0, z);
//                }
//                else if (x == 0 && y < 31)
//                {
//                    ChangeNeighbor(val, density, i - 1, j, k, 31, y + 1, z);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i - 1, j + 1, k, 31, 0, z);
//                }

//                val = 6;
//                if (x > 0 && z > 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x - 1, y, z - 1);
//                }
//                else if (x > 0 && z == 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k - 1, x - 1, y, 31);
//                }
//                else if (x == 0 && z > 0)
//                {
//                    ChangeNeighbor(val, density, i - 1, j, k, 31, y, z - 1);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i - 1, j, k - 1, 31, y, 31);
//                }

//                val = 3;
//                if (y < 31 && z > 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x, y + 1, z - 1);
//                }
//                else if (y < 31 && z == 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k - 1, x, y + 1, 31);
//                }
//                else if (y == 31 && z > 0)
//                {
//                    ChangeNeighbor(val, density, i, j + 1, k, x, 0, z - 1);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i, j + 1, k - 1, x, 0, 31);
//                }

//                val = 2;
//                if (x > 0 && y < 31 && z > 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x - 1, y + 1, z - 1);
//                }
//                else if (x > 0 && y < 31 && z == 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k - 1, x - 1, y + 1, 31);
//                }
//                else if (x > 0 && y == 31 && z > 0)
//                {
//                    ChangeNeighbor(val, density, i, j + 1, k, x - 1, 0, z - 1);
//                }
//                else if (x == 0 && y < 31 && z > 0)
//                {
//                    ChangeNeighbor(val, density, i - 1, j, k, 31, y + 1, z - 1);
//                }
//                else if (x > 0 && y == 31 && z == 0)
//                {
//                    ChangeNeighbor(val, density, i, j + 1, k - 1, x - 1, 0, 31);
//                }
//                else if (x == 0 && y < 31 && z == 0)
//                {
//                    ChangeNeighbor(val, density, i - 1, j, k - 1, 31, y + 1, 31);
//                }
//                else if (x == 0 && y == 31 && z > 0)
//                {
//                    ChangeNeighbor(val, density, i - 1, j + 1, k, 31, 0, z - 1);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i - 1, j + 1, k - 1, 31, 0, 31);
//                }

//                break;
//            case 5:
//                val = 4;
//                if (x < 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x + 1, y, z);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i + 1, j, k, 0, y, z);
//                }

//                val = 1;
//                if (y < 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x, y + 1, z);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i, j + 1, k, x, 0, z);
//                }

//                val = 6;
//                if (z > 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x, y, z - 1);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i, j, k - 1, x, y, 31);
//                }

//                val = 0;
//                if (x < 31 && y < 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x + 1, y + 1, z);
//                }
//                else if (x < 31 && y == 31)
//                {
//                    ChangeNeighbor(val, density, i, j + 1, k, x + 1, 0, z);
//                }
//                else if (x == 31 && y < 31)
//                {
//                    ChangeNeighbor(val, density, i + 1, j, k, 0, y + 1, z);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i + 1, j + 1, k, 0, 0, z);
//                }

//                val = 7;
//                if (x < 31 && z > 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x + 1, y, z - 1);
//                }
//                else if (x < 31 && z == 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k - 1, x + 1, y, 31);
//                }
//                else if (x == 31 && z > 0)
//                {
//                    ChangeNeighbor(val, density, i + 1, j, k, 0, y, z - 1);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i + 1, j, k - 1, 0, y, 31);
//                }

//                val = 2;
//                if (y < 31 && z > 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x, y + 1, z - 1);
//                }
//                else if (y < 31 && z == 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k - 1, x, y + 1, 31);
//                }
//                else if (y == 31 && z > 0)
//                {
//                    ChangeNeighbor(val, density, i, j + 1, k, x, 0, z - 1);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i, j + 1, k - 1, x, 0, 31);
//                }

//                val = 3;
//                if (x < 31 && y < 31 && z > 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x + 1, y + 1, z - 1);
//                }
//                else if (x < 31 && y < 31 && z == 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k - 1, x + 1, y + 1, 31);
//                }
//                else if (x < 31 && y == 31 && z > 0)
//                {
//                    ChangeNeighbor(val, density, i, j + 1, k, x + 1, 0, z - 1);
//                }
//                else if (x == 31 && y < 31 && z > 0)
//                {
//                    ChangeNeighbor(val, density, i + 1, j, k, 0, y + 1, z - 1);
//                }
//                else if (x < 31 && y == 31 && z == 0)
//                {
//                    ChangeNeighbor(val, density, i, j + 1, k - 1, x + 1, 0, 31);
//                }
//                else if (x == 31 && y < 31 && z == 0)
//                {
//                    ChangeNeighbor(val, density, i + 1, j, k - 1, 0, y + 1, 31);
//                }
//                else if (x == 31 && y == 31 && z > 0)
//                {
//                    ChangeNeighbor(val, density, i + 1, j + 1, k, 0, 0, z - 1);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i + 1, j + 1, k - 1, 0, 0, 31);
//                }

//                break;
//            case 6:
//                val = 7;
//                if (x < 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x + 1, y, z);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i + 1, j, k, 0, y, z);
//                }

//                val = 2;
//                if (y < 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x, y + 1, z);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i, j + 1, k, x, 0, z);
//                }

//                val = 5;
//                if (z < 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x, y, z + 1);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i, j, k + 1, x, y, 0);
//                }

//                val = 3;
//                if (x < 31 && y < 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x + 1, y + 1, z);
//                }
//                else if (x < 31 && y == 31)
//                {
//                    ChangeNeighbor(val, density, i, j + 1, k, x + 1, 0, z);
//                }
//                else if (x == 31 && y < 31)
//                {
//                    ChangeNeighbor(val, density, i + 1, j, k, 0, y + 1, z);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i + 1, j + 1, k, 0, 0, z);
//                }

//                val = 4;
//                if (x < 31 && z < 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x + 1, y, z + 1);
//                }
//                else if (x < 31 && z == 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k + 1, x + 1, y, 0);
//                }
//                else if (x == 31 && z < 31)
//                {
//                    ChangeNeighbor(val, density, i + 1, j, k, 0, y, z + 1);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i + 1, j, k + 1, 0, y, 0);
//                }

//                val = 1;
//                if (y < 31 && z < 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x, y + 1, z + 1);
//                }
//                else if (y < 31 && z == 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k + 1, x, y + 1, 0);
//                }
//                else if (y == 31 && z < 31)
//                {
//                    ChangeNeighbor(val, density, i, j + 1, k, x, 0, z + 1);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i, j + 1, k + 1, x, 0, 0);
//                }

//                val = 0;
//                if (x < 31 && y < 31 && z < 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x + 1, y + 1, z + 1);
//                }
//                else if (x < 31 && y < 31 && z == 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k + 1, x + 1, y + 1, 0);
//                }
//                else if (x < 31 && y == 31 && z < 31)
//                {
//                    ChangeNeighbor(val, density, i, j + 1, k, x + 1, 0, z + 1);
//                }
//                else if (x == 31 && y < 31 && z < 31)
//                {
//                    ChangeNeighbor(val, density, i + 1, j, k, 0, y + 1, z + 1);
//                }
//                else if (x < 31 && y == 31 && z == 31)
//                {
//                    ChangeNeighbor(val, density, i, j + 1, k + 1, x + 1, 0, 0);
//                }
//                else if (x == 31 && y < 31 && z == 31)
//                {
//                    ChangeNeighbor(val, density, i + 1, j, k + 1, 0, y + 1, 0);
//                }
//                else if (x == 31 && y == 31 && z < 31)
//                {
//                    ChangeNeighbor(val, density, i + 1, j + 1, k, 0, 0, z + 1);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i + 1, j + 1, k + 1, 0, 0, 0);
//                }

//                break;
//            case 7:
//                val = 6;
//                if (x > 0)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x - 1, y, z);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i - 1, j, k, 31, y, z);
//                }

//                val = 3;
//                if (y < 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x, y + 1, z);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i, j + 1, k, x, 0, z);
//                }

//                val = 4;
//                if (z < 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x, y, z + 1);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i, j, k + 1, x, y, 0);
//                }

//                val = 2;
//                if (x > 0 && y < 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x - 1, y + 1, z);
//                }
//                else if (x > 0 && y == 31)
//                {
//                    ChangeNeighbor(val, density, i, j + 1, k, x - 1, 0, z);
//                }
//                else if (x == 0 && y < 31)
//                {
//                    ChangeNeighbor(val, density, i - 1, j, k, 31, y + 1, z);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i - 1, j + 1, k, 31, 0, z);
//                }

//                val = 5;
//                if (x > 0 && z < 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x - 1, y, z + 1);
//                }
//                else if (x > 0 && z == 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k + 1, x - 1, y, 0);
//                }
//                else if (x == 0 && z < 31)
//                {
//                    ChangeNeighbor(val, density, i - 1, j, k, 31, y, z + 1);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i - 1, j, k + 1, 31, y, 0);
//                }

//                val = 0;
//                if (y < 31 && z < 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x, y + 1, z + 1);
//                }
//                else if (y < 31 && z == 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k + 1, x, y + 1, 0);
//                }
//                else if (y == 31 && z < 31)
//                {
//                    ChangeNeighbor(val, density, i, j + 1, k, x, 0, z + 1);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i, j + 1, k + 1, x, 0, 0);
//                }

//                val = 1;
//                if (x > 0 && y < 31 && z < 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k, x - 1, y + 1, z + 1);
//                }
//                else if (x > 0 && y < 31 && z == 31)
//                {
//                    ChangeNeighbor(val, density, i, j, k + 1, x - 1, y + 1, 0);
//                }
//                else if (x > 0 && y == 31 && z < 31)
//                {
//                    ChangeNeighbor(val, density, i, j + 1, k, x - 1, 0, z + 1);
//                }
//                else if (x == 0 && y < 31 && z < 31)
//                {
//                    ChangeNeighbor(val, density, i - 1, j, k, 31, y + 1, z + 1);
//                }
//                else if (x > 0 && y == 31 && z == 31)
//                {
//                    ChangeNeighbor(val, density, i, j + 1, k + 1, x - 1, 0, 0);
//                }
//                else if (x == 0 && y < 31 && z == 31)
//                {
//                    ChangeNeighbor(val, density, i - 1, j, k + 1, 31, y + 1, 0);
//                }
//                else if (x == 0 && y == 31 && z < 31)
//                {
//                    ChangeNeighbor(val, density, i - 1, j + 1, k, 31, 0, z + 1);
//                }
//                else
//                {
//                    ChangeNeighbor(val, density, i - 1, j + 1, k + 1, 31, 0, 0);
//                }

//                break;
//        }
//    }
}