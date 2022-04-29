using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;

public enum BlockType
{
    Air, Grass, Stone, RedClay, Sand, DeepStone, Dirt, Snow, GreyStone
}

public enum Biome
{
    Plains, Grasslands, Hills, Mountains, Desert, Dunes, Plateau, Mesa, SnowyPlains, SnowyGrasslands, SnowyHills, SnowyMountains, Caves
}

public class Cube
{
    public BlockType[] materials;

    public float[] surfaceValues;

    public Vector3[] vertices;

    public List<int> meshVertices;

    public Cube(BlockType[] val, float[] surfaceVal, Vector3[] vert)
    {
        materials = val;
        surfaceValues = surfaceVal;
        vertices = vert;
    }

    public Cube(CubeStruct c)
    {
        materials = new BlockType[] { c.materials, c.materials1, c.materials2, c.materials3, c.materials4, c.materials5, c.materials6, c.materials7 };
        surfaceValues = new float[] { c.surfaceValues, c.surfaceValues1, c.surfaceValues2, c.surfaceValues3, c.surfaceValues4, c.surfaceValues5, c.surfaceValues6, c.surfaceValues7 };
        vertices = new Vector3[] { c.vertices, c.vertices1, c.vertices2, c.vertices3, c.vertices4, c.vertices5, c.vertices6, c.vertices7 };
    }
}

public class ChunkManager : MonoBehaviour
{
    public int m_chunkSize = 32;

    int m_numCubes;

    public float m_size = 1;

    public float m_maxDepth = 10f;

    public Biome m_biome = Biome.Mountains;

    NativeArray<Vector3> m_CubeCoords;

    NativeArray<CubeStruct> m_CubeStructs;

    CalculateCube m_CalculateCube;

    JobHandle m_JobHandle;


    Dictionary<Vector3Int, Cube[,,]> chunks;
    public HashSet<Vector2Int> horizontalChunks;

    // Start is called before the first frame update
    void Start()
    {
        chunks = new Dictionary<Vector3Int, Cube[,,]>();
        horizontalChunks = new HashSet<Vector2Int>();
        m_numCubes = m_chunkSize * m_chunkSize * m_chunkSize;
        m_CubeCoords = new NativeArray<Vector3>(m_numCubes, Allocator.Persistent);
        m_CubeStructs = new NativeArray<CubeStruct>(m_numCubes, Allocator.Persistent);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public Cube[,,] GetChunk(Vector3 coord)
    {
        var chunkCoord = WorldToChunk(coord);
        if (!chunks.ContainsKey(chunkCoord))
        {
            GenerateChunk(chunkCoord);
        }
        return chunks[chunkCoord];
    }

    public Cube[,,] CheckGetChunk(Vector3Int coord)
    {
        if (!chunks.ContainsKey(coord))
        {
            return null;
        }
        return chunks[coord];
    }

    public bool CheckChunk(Vector3Int coord)
    {
        return chunks.ContainsKey(coord);
    }

    public Vector3Int WorldToChunk(Vector3 coord)
    {
        float ratio = m_chunkSize * m_size;
        return new Vector3Int(Mathf.FloorToInt(coord.x / ratio), Mathf.FloorToInt(coord.y / ratio), Mathf.FloorToInt(coord.z / ratio));
    }

    public Vector3 ChunkToWorld(Vector3Int coord)
    {
        float ratio = m_chunkSize * m_size;
        return new Vector3(coord.x * ratio, coord.y * ratio, coord.z * ratio);
    }

    void GenerateChunk(Vector3Int chunkCoord)
    {
        float startTime = Time.realtimeSinceStartup;

        Vector3 chunkEdge = ChunkToWorld(chunkCoord);
        // if there is no chunk at coord, generate chunk
        if (!chunks.ContainsKey(chunkCoord))
        {
            Cube[,,] chunk = new Cube[m_chunkSize, m_chunkSize, m_chunkSize];

            for (int x = 0; x < m_chunkSize; x++)
            {
                for (int y = 0; y < m_chunkSize; y++)
                {
                    for (int z = 0; z < m_chunkSize; z++)
                    {
                        m_CubeCoords[x + y * m_chunkSize + z * m_chunkSize * m_chunkSize] =
                            new Vector3(chunkEdge.x + x * m_size, chunkEdge.y + y * m_size, chunkEdge.z + z * m_size);
                    }
                }
            }

            ScheduleCube();

            m_JobHandle.Complete();

            CubeStruct[] cubeStructs = new CubeStruct[m_numCubes];
            m_CalculateCube.cubeStructs.CopyTo(cubeStructs);

            for (int x = 0; x < m_chunkSize; x++)
            {
                for (int y = 0; y < m_chunkSize; y++)
                {
                    for (int z = 0; z < m_chunkSize; z++)
                    {
                        chunk[x, y, z] = new Cube(cubeStructs[x + y * m_chunkSize + z * m_chunkSize * m_chunkSize]);
                    }
                }
            }

            horizontalChunks.Add(new Vector2Int(chunkCoord.x, chunkCoord.z));
            chunks.Add(chunkCoord, chunk);

            // Debug.Log(((Time.realtimeSinceStartup - startTime) * 1000f) + "ms");
        }
    }

    void ScheduleCube()
    {
        m_CalculateCube = new CalculateCube()
        {
            cubeCoords = m_CubeCoords,
            cubeStructs = m_CubeStructs,
            size = m_size,
            chunkSize = m_chunkSize,
            maxDepth = m_maxDepth,
            biomeType = m_biome
        };

        m_JobHandle = m_CalculateCube.Schedule(m_CubeCoords.Length, 1);
    }

    void OnDestroy()
    {
        m_CubeCoords.Dispose();
        m_CubeStructs.Dispose();
    }
}

public struct CubeStruct
{
    public BlockType materials;
    public BlockType materials1;
    public BlockType materials2;
    public BlockType materials3;
    public BlockType materials4;
    public BlockType materials5;
    public BlockType materials6;
    public BlockType materials7;

    public float surfaceValues;
    public float surfaceValues1;
    public float surfaceValues2;
    public float surfaceValues3;
    public float surfaceValues4;
    public float surfaceValues5;
    public float surfaceValues6;
    public float surfaceValues7;

    public Vector3 vertices;
    public Vector3 vertices1;
    public Vector3 vertices2;
    public Vector3 vertices3;
    public Vector3 vertices4;
    public Vector3 vertices5;
    public Vector3 vertices6;
    public Vector3 vertices7;

    public CubeStruct(BlockType m, BlockType m1, BlockType m2, BlockType m3, BlockType m4, BlockType m5, BlockType m6, BlockType m7,
        float s, float s1, float s2, float s3, float s4, float s5, float s6, float s7,
        Vector3 v, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 v5, Vector3 v6, Vector3 v7)
    {
        materials = m; materials1 = m1; materials2 = m2; materials3 = m3; materials4 = m4; materials5 = m5; materials6 = m6; materials7 = m7;
        surfaceValues = s; surfaceValues1 = s1; surfaceValues2 = s2; surfaceValues3 = s3; surfaceValues4 = s4; surfaceValues5 = s5; surfaceValues6 = s6; surfaceValues7 = s7;
        vertices = v; vertices1 = v1; vertices2 = v2; vertices3 = v3; vertices4 = v4; vertices5 = v5; vertices6 = v6; vertices7 = v7;
    }
}

//[BurstCompile]
public struct CalculateCube : IJobParallelFor
{
    public NativeArray<Vector3> cubeCoords;

    public NativeArray<CubeStruct> cubeStructs;

    public float size;

    public int chunkSize;

    public float maxDepth;

    public Biome biomeType;

    public void Execute(int i)
    {
        var cubeCoord = cubeCoords[i];

        float x = cubeCoord.x;
        float y = cubeCoord.y;
        float z = cubeCoord.z;

        (float, BlockType) p = GetBiomeData(x, y, z);
        float surfaceVal = p.Item1;
        BlockType blockType = p.Item2;

        p = GetBiomeData(x + size, y, z);
        float surfaceVal1 = p.Item1;
        BlockType blockType1 = p.Item2;

        p = GetBiomeData(x + size, y, z + size);
        float surfaceVal2 = p.Item1;
        BlockType blockType2 = p.Item2;

        p = GetBiomeData(x, y, z + size);
        float surfaceVal3 = p.Item1;
        BlockType blockType3 = p.Item2;

        p = GetBiomeData(x, y + size, z);
        float surfaceVal4 = p.Item1;
        BlockType blockType4 = p.Item2;

        p = GetBiomeData(x + size, y + size, z);
        float surfaceVal5 = p.Item1;
        BlockType blockType5 = p.Item2;

        p = GetBiomeData(x + size, y + size, z + size);
        float surfaceVal6 = p.Item1;
        BlockType blockType6 = p.Item2;

        p = GetBiomeData(x, y + size, z + size);
        float surfaceVal7 = p.Item1;
        BlockType blockType7 = p.Item2;

        surfaceVal = Mathf.Clamp(surfaceVal, -maxDepth, maxDepth);
        surfaceVal1 = Mathf.Clamp(surfaceVal1, -maxDepth, maxDepth);
        surfaceVal2 = Mathf.Clamp(surfaceVal2, -maxDepth, maxDepth);
        surfaceVal3 = Mathf.Clamp(surfaceVal3, -maxDepth, maxDepth);
        surfaceVal4 = Mathf.Clamp(surfaceVal4, -maxDepth, maxDepth);
        surfaceVal5 = Mathf.Clamp(surfaceVal5, -maxDepth, maxDepth);
        surfaceVal6 = Mathf.Clamp(surfaceVal6, -maxDepth, maxDepth);
        surfaceVal7 = Mathf.Clamp(surfaceVal7, -maxDepth, maxDepth);

        Vector3 vertex = new Vector3(x, y, z);
        Vector3 vertex1 = new Vector3(x + size, y, z);
        Vector3 vertex2 = new Vector3(x + size, y, z + size);
        Vector3 vertex3 = new Vector3(x, y, z + size);
        Vector3 vertex4 = new Vector3(x, y + size, z);
        Vector3 vertex5 = new Vector3(x + size, y + size, z);
        Vector3 vertex6 = new Vector3(x + size, y + size, z + size);
        Vector3 vertex7 = new Vector3(x, y + size, z + size);

        cubeStructs[i] = new CubeStruct(blockType, blockType1, blockType2, blockType3, blockType4, blockType5, blockType6, blockType7,
            surfaceVal, surfaceVal1, surfaceVal2, surfaceVal3, surfaceVal4, surfaceVal5, surfaceVal6, surfaceVal7,
            vertex, vertex1, vertex2, vertex3, vertex4, vertex5, vertex6, vertex7);
    }

    (float, BlockType) Plains(float x, float y, float z, float noise, float size)
    {
        x *= size;
        y *= size;
        z *= size;

        float depth = -y;
        BlockType block = BlockType.Air;

        if (depth > 0)
        {
            if (y > -3f)
            {
                block = BlockType.Grass;
            }
            else
            {
                block = BlockType.Dirt;
            }
        }

        return (depth, block);
    }

    (float, BlockType) Grasslands(float x, float y, float z, float noise, float size)
    {
        x *= size;
        y *= size;
        z *= size;

        float depth = -y + (noise * 5) * (Perlin.Noise(x * 0.05f, z * 0.05f) * 0.5f + 0.5f);
        float depth2 = depth - 3;
        BlockType block = BlockType.Air;

        if (depth > 0)
        {
            block = BlockType.Grass;
            if (depth2 > 0)
            {
                block = BlockType.Dirt;
            }
        }

        return (depth, block);
    }

    (float, BlockType) Hills(float x, float y, float z, float noise, float size)
    {
        x *= size;
        y *= size;
        z *= size;

        float depth = -y + (5 + noise * 10f) * (Perlin.Noise(x * 0.06f, z * 0.06f) * 0.5f + 0.5f);
        float depth2 = depth - 3;
        BlockType block = BlockType.Air;

        if (depth > 0)
        {
            block = BlockType.Grass;
            if (depth2 > 0)
            {
                block = BlockType.Dirt;
            }
        }

        return (depth, block);
    }

    (float, BlockType) Mountains(float x, float y, float z, float noise, float size)
    {
        x *= size;
        y *= size;
        z *= size;

        float depth = -y + 60 * Mathf.InverseLerp(-0.75f, 0.75f, Perlin.Fbm(x * 0.04f, z * 0.04f, 6));
        float depth2 = -y + (noise * 4);
        float depth3 = depth2 - 3;
        BlockType block = BlockType.Air;

        if (depth2 > 0)
        {
            block = BlockType.Grass;
            if (depth3 > 0)
            {
                block = BlockType.Dirt;
            }
        }

        if (depth > 0)
        {
            block = BlockType.Stone;
        }

        return (Mathf.Max(depth, depth2), block);
    }

    (float, BlockType) Desert(float x, float y, float z, float noise, float size)
    {
        x *= size;
        y *= size;
        z *= size;

        float depth = -y;
        BlockType block = BlockType.Air;

        if (depth > 0)
        {
            block = BlockType.Sand;
        }

        return (depth, block);
    }

    (float, BlockType) Dunes(float x, float y, float z, float noise, float size)
    {
        x *= size;
        y *= size;
        z *= size;

        float depth = -y + (16 + noise * 9) * (Perlin.Noise(x * 0.05f, z * 0.05f) * 0.5f + 0.5f);
        BlockType block = BlockType.Air;

        if (depth > 0)
        {
            block = BlockType.Sand;
        }

        return (depth, block);
    }

    (float, BlockType) Plateau(float x, float y, float z, float noise, float size)
    {
        x *= size;
        y *= size;
        z *= size;

        float depth = -y + Mathf.Clamp((16 + noise * 8) * (Perlin.Noise(x * 0.06f, z * 0.06f) * 0.5f + 0.5f), 0f, 8 + noise * 5);
        BlockType block = BlockType.Air;

        if (depth > 0)
        {
            block = BlockType.Sand;
        }

        return (depth, block);
    }

    (float, BlockType) Mesa(float x, float y, float z, float noise, float size)
    {
        x *= size;
        y *= size;
        z *= size;

        float height = 40f * Mathf.InverseLerp(-0.75f, 0.75f, Perlin.Fbm(x * 0.04f, z * 0.04f, 6));
        float depth = -y + Mathf.Ceil(Mathf.Clamp(height, 0, 15 + noise * 9));
        float depth2 = -y + (noise * 4);
        BlockType block = BlockType.Air;

        if (depth2 > 0)
        {
            block = BlockType.Sand;
        }

        if (depth > 0)
        {
            block = BlockType.RedClay;
        }

        return (Mathf.Max(depth, depth2), block);
    }

    (float, BlockType) SnowyPlains(float x, float y, float z, float noise, float size)
    {
        x *= size;
        y *= size;
        z *= size;

        float depth = -y + 3f;
        BlockType block = BlockType.Air;

        if (depth > 0)
        {
            if (y > 0f)
            {
                block = BlockType.Snow;
            }
            else if (y > -3f)
            {
                block = BlockType.Grass;
            }
            else
            {
                block = BlockType.Dirt;
            }
        }

        return (depth, block);
    }

    (float, BlockType) SnowyGrasslands(float x, float y, float z, float noise, float size)
    {
        x *= size;
        y *= size;
        z *= size;

        float depth = -y + (noise * 5) * (Perlin.Noise(x * 0.05f, z * 0.05f) * 0.5f + 0.5f) + 3f;
        float depth2 = depth - 3;
        float depth3 = depth - 6;
        BlockType block = BlockType.Air;

        if (depth > 0)
        {
            block = BlockType.Snow;
            if (depth2 > 0)
            {
                block = BlockType.Grass;
            }
            if (depth3 > 0)
            {
                block = BlockType.Dirt;
            }
        }

        return (depth, block);
    }

    (float, BlockType) SnowyHills(float x, float y, float z, float noise, float size)
    {
        x *= size;
        y *= size;
        z *= size;

        float depth = -y + (5 + noise * 10f) * (Perlin.Noise(x * 0.06f, z * 0.06f) * 0.5f + 0.5f) + 3f;
        float depth2 = depth - 3;
        float depth3 = depth - 6;
        BlockType block = BlockType.Air;

        if (depth > 0)
        {
            block = BlockType.Snow;
            if (depth2 > 0)
            {
                block = BlockType.Grass;
            }
            if (depth3 > 0)
            {
                block = BlockType.Dirt;
            }
        }

        return (depth, block);
    }

    (float, BlockType) SnowyMountains(float x, float y, float z, float noise, float size)
    {
        x *= size;
        y *= size;
        z *= size;

        float depth = -y + 60f * (Perlin.Fbm(x * 0.04f, z * 0.04f, 6) * 0.5f + 0.5f);
        float depth2 = -y + (noise * 6);
        float depth3 = depth2 - 1;
        float depth4 = depth3 - 3;
        BlockType block = BlockType.Air;

        float rand = 1 + noise * 4;
        if (depth2 > 0)
        {
            block = BlockType.Snow;
            if (depth3 > 0)
            {
                block = BlockType.Grass;
                if (depth4 > 0)
                {
                    block = BlockType.Dirt;
                }
            }
        }

        if (depth > 0)
        {
            block = BlockType.Stone;
            if (y > 20 + rand * 5)
            {
                block = BlockType.Snow;
            }
        }

        return (Mathf.Max(depth, depth2), block);
    }

    (float, BlockType) Caves(float x, float y, float z, float noise, float size)
    {
        x *= size;
        y *= size;
        z *= size;

        float depth = Perlin.Noise(x * 0.15f, y * 0.15f, z * 0.15f) * 10f - y + noise * 10;

        //if (y >= 0 || y < -10)
        //{
        //    depth = -y + noise * 10;
        //}
        BlockType block = BlockType.Air;

        if (depth > 0)
        {
            block = BlockType.DeepStone;
        }

        return (depth, block);
    }

    (float, BlockType) GetBiomeData(float x, float y, float z)
    {
        float altitude = Perlin.Noise(x * 0.02f + 3.2f, z * 0.02f - 6.3f) * 0.5f + 0.5f;
        float temperature = Perlin.Noise(x * 0.01f, z * 0.01f) * 0.5f + 0.5f;
        float precipitation = Perlin.Noise(x * 0.015f + 4.5f, z * 0.015f + 7.1f) * 0.5f + 0.5f;

        Biome biome = GetBiome(altitude, temperature, precipitation);

        switch (biomeType)
        {
            case Biome.Plains:
                return Plains(x, y, z, altitude, size);
            case Biome.Grasslands:
                return Grasslands(x, y, z, altitude, size);
            case Biome.Hills:
                return Hills(x, y, z, altitude, size);
            case Biome.Mountains:
                return Mountains(x, y, z, altitude, size);
            case Biome.Desert:
                return Desert(x, y, z, altitude, size);
            case Biome.Dunes:
                return Dunes(x, y, z, altitude, size);
            case Biome.Plateau:
                return Plateau(x, y, z, altitude, size);
            case Biome.Mesa:
                return Mesa(x, y, z, altitude, size);
            case Biome.SnowyPlains:
                return SnowyPlains(x, y, z, altitude, size);
            case Biome.SnowyGrasslands:
                return SnowyGrasslands(x, y, z, altitude, size);
            case Biome.SnowyHills:
                return SnowyHills(x, y, z, altitude, size);
            case Biome.SnowyMountains:
                return SnowyMountains(x, y, z, altitude, size);
            case Biome.Caves:
                return Caves(x, y, z, altitude, size);
        }
        return Plains(x, y, z, altitude, size);
    }

    Biome GetBiome(float altitude, float temperature, float precipitation)
    {
        if (temperature > 0.65f)
        {
            if (altitude > 0.75f)
            {
                return Biome.Mesa;
            }
            else if (altitude > 0.5f)
            {
                return Biome.Plateau;
            }
            else if (altitude > 0.25f)
            {
                return Biome.Dunes;
            }
            else
            {
                return Biome.Desert;
            }
        }
        else if (temperature > 0.35f)
        {
            if (altitude > 0.75f)
            {
                return Biome.Mountains;
            }
            else if (altitude > 0.5f)
            {
                return Biome.Hills;
            }
            else if (altitude > 0.25f)
            {
                return Biome.Grasslands;
            }
            else
            {
                return Biome.Plains;
            }
        }
        else
        {
            if (altitude > 0.75f)
            {
                return Biome.SnowyMountains;
            }
            else if (altitude > 0.5f)
            {
                return Biome.SnowyHills;
            }
            else if (altitude > 0.25f)
            {
                return Biome.SnowyGrasslands;
            }
            else
            {
                return Biome.SnowyPlains;
            }
        }
    }
}