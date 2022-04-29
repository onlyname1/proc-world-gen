using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManagerOld : MonoBehaviour
{
    public int m_chunkSize = 32;
    public float m_size = 1;
    public float maxDepth = 10f;

    Dictionary<Vector3Int, Cube[,,]> chunks;
    public HashSet<Vector2Int> horizontalChunks;

    // Start is called before the first frame update
    void Start()
    {
        chunks = new Dictionary<Vector3Int, Cube[,,]>();
        horizontalChunks = new HashSet<Vector2Int>();
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

    float GroundDepth(float x, float y, float z)
    {
        if (y > 10)
        {
            return -1;
        }
        else
        {
            return 1;
        }
    }

    float PerlinDepth(float x, float y, float z)
    {
        // if (y < 10)
        // {
        //     return Perlin.Noise(x * 0.15f, y * 0.15f, z * 0.15f);
        // }
        // else
        // {
        //     return -y;
        // }

        return -y + Mathf.Clamp(20f * Perlin.Fbm(x * 0.1f, z * 0.1f, 6) * Perlin.Noise(x * 0.005f, z * 0.005f), -20, 50);
    }

    float GenerateDepth(float x, float y, float z)
    {
        float depth = 0;
        switch (2)
        {
            case 1:
                depth = GroundDepth(x, y, z);
                break;
            case 2:
                depth = PerlinDepth(x, y, z);
                break;
            case -1:
                depth = GroundDepth(x, y, z);
                break;
        }

        return depth;
    }

    BlockType GenerateBlock(float depth, float x, float y, float z)
    {
        if (depth > 0)
        {
            float temperature = Perlin.Fbm(x * 0.15f, z * 0.15f, 6);
            float noise = Perlin.Noise(x * 0.3f, y * 0.3f, z * 0.3f);

            if (y > 10)
            {
                if (temperature > 0)
                {
                    return BlockType.Stone;
                }
                else
                {
                    return BlockType.Snow;
                }
            }
            else if (y > 5)
            {
                return BlockType.Stone;
            }
            else if (y > -10)
            {
                if (temperature > 0)
                {
                    return BlockType.Grass;
                }
                else
                {
                    return BlockType.Dirt;
                }
            }
            else
            {
                if (noise < 0)
                {
                    return BlockType.Dirt;
                }
                else
                {
                    return BlockType.DeepStone;
                }
            }
        }
        else
        {
            return BlockType.Air;
        }
    }

    (float, BlockType) Plains(float x, float y, float z, float noise)
    {
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

    (float, BlockType) Grasslands(float x, float y, float z, float noise)
    {
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

    (float, BlockType) Hills(float x, float y, float z, float noise)
    {
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

    (float, BlockType) Mountains(float x, float y, float z, float noise)
    {
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

    (float, BlockType) Desert(float x, float y, float z, float noise)
    {
        float depth = -y;
        BlockType block = BlockType.Air;

        if (depth > 0)
        {
            block = BlockType.Sand;
        }

        return (depth, block);
    }

    (float, BlockType) Dunes(float x, float y, float z, float noise)
    {
        float depth = -y + (noise * 5) * (Perlin.Noise(x * 0.05f, z * 0.05f) * 0.5f + 0.5f);
        BlockType block = BlockType.Air;

        if (depth > 0)
        {
            block = BlockType.Sand;
        }

        return (depth, block);
    }

    (float, BlockType) Plateau(float x, float y, float z, float noise)
    {
        float depth = -y + Mathf.Clamp((16 + noise * 8) * (Perlin.Noise(x * 0.06f, z * 0.06f) * 0.5f + 0.5f), 0f, 8 + noise * 5);
        BlockType block = BlockType.Air;

        if (depth > 0)
        {
            block = BlockType.Sand;
        }

        return (depth, block);
    }

    (float, BlockType) Mesa(float x, float y, float z, float noise)
    {
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

    (float, BlockType) SnowyPlains(float x, float y, float z, float noise)
    {
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

    (float, BlockType) SnowyGrasslands(float x, float y, float z, float noise)
    {
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

    (float, BlockType) SnowyHills(float x, float y, float z, float noise)
    {
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

    (float, BlockType) SnowyMountains(float x, float y, float z, float noise)
    {
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

    (float, BlockType) GetBiomeData(float x, float y, float z)
    {
        float altitude = Perlin.Noise(x * 0.02f + 3.2f, z * 0.02f - 6.3f) * 0.5f + 0.5f;
        float temperature = Perlin.Noise(x * 0.01f, z * 0.01f) * 0.5f + 0.5f;
        float precipitation = Perlin.Noise(x * 0.015f + 4.5f, z * 0.015f + 7.1f) * 0.5f + 0.5f;

        Biome biome = GetBiome(altitude, temperature, precipitation);

        switch (Biome.Mountains)
        {
            case Biome.Plains:
                return Plains(x, y, z, altitude);
            case Biome.Grasslands:
                return Grasslands(x, y, z, altitude);
            case Biome.Hills:
                return Hills(x, y, z, altitude);
            case Biome.Mountains:
                return Mountains(x, y, z, altitude);
            case Biome.Desert:
                return Desert(x, y, z, altitude);
            case Biome.Dunes:
                return Dunes(x, y, z, altitude);
            case Biome.Plateau:
                return Plateau(x, y, z, altitude);
            case Biome.Mesa:
                return Mesa(x, y, z, altitude);
            case Biome.SnowyPlains:
                return SnowyPlains(x, y, z, altitude);
            case Biome.SnowyGrasslands:
                return SnowyGrasslands(x, y, z, altitude);
            case Biome.SnowyHills:
                return SnowyHills(x, y, z, altitude);
            case Biome.SnowyMountains:
                return SnowyMountains(x, y, z, altitude);
        }
        return Plains(x, y, z, altitude);
    }

    Cube MakeCube2(float x, float y, float z, float size)
    {
        float[] surfaceVals = new float[8];
        BlockType[] vals = new BlockType[8];

        (float, BlockType) p = GetBiomeData(x, y, z);
        surfaceVals[0] = p.Item1;
        vals[0] = p.Item2;

        p = GetBiomeData(x + size, y, z);
        surfaceVals[1] = p.Item1;
        vals[1] = p.Item2;

        p = GetBiomeData(x + size, y, z + size);
        surfaceVals[2] = p.Item1;
        vals[2] = p.Item2;

        p = GetBiomeData(x, y, z + size);
        surfaceVals[3] = p.Item1;
        vals[3] = p.Item2;

        p = GetBiomeData(x, y + size, z);
        surfaceVals[4] = p.Item1;
        vals[4] = p.Item2;

        p = GetBiomeData(x + size, y + size, z);
        surfaceVals[5] = p.Item1;
        vals[5] = p.Item2;

        p = GetBiomeData(x + size, y + size, z + size);
        surfaceVals[6] = p.Item1;
        vals[6] = p.Item2;

        p = GetBiomeData(x, y + size, z + size);
        surfaceVals[7] = p.Item1;
        vals[7] = p.Item2;

        for (int i = 0; i < 8; i++)
        {
            surfaceVals[i] = Mathf.Clamp(surfaceVals[i], -maxDepth, maxDepth);
        }

        Vector3[] vert = new Vector3[8];
        vert[0] = new Vector3(x, y, z);
        vert[1] = new Vector3(x + size, y, z);
        vert[2] = new Vector3(x + size, y, z + size);
        vert[3] = new Vector3(x, y, z + size);
        vert[4] = new Vector3(x, y + size, z);
        vert[5] = new Vector3(x + size, y + size, z);
        vert[6] = new Vector3(x + size, y + size, z + size);
        vert[7] = new Vector3(x, y + size, z + size);

        return new Cube(vals, surfaceVals, vert);
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

    Cube MakeCube(float x, float y, float z, float size)
    {
        float[] surfaceVals = new float[8];
        surfaceVals[0] = GenerateDepth(x, y, z);
        surfaceVals[1] = GenerateDepth(x + size, y, z);
        surfaceVals[2] = GenerateDepth(x + size, y, z + size);
        surfaceVals[3] = GenerateDepth(x, y, z + size);
        surfaceVals[4] = GenerateDepth(x, y + size, z);
        surfaceVals[5] = GenerateDepth(x + size, y + size, z);
        surfaceVals[6] = GenerateDepth(x + size, y + size, z + size);
        surfaceVals[7] = GenerateDepth(x, y + size, z + size);

        BlockType[] vals = new BlockType[8];
        vals[0] = GenerateBlock(surfaceVals[0], x, y, z);
        vals[1] = GenerateBlock(surfaceVals[1], x + size, y, z);
        vals[2] = GenerateBlock(surfaceVals[2], x + size, y, z + size);
        vals[3] = GenerateBlock(surfaceVals[3], x, y, z + size);
        vals[4] = GenerateBlock(surfaceVals[4], x, y + size, z);
        vals[5] = GenerateBlock(surfaceVals[5], x + size, y + size, z);
        vals[6] = GenerateBlock(surfaceVals[6], x + size, y + size, z + size);
        vals[7] = GenerateBlock(surfaceVals[7], x, y + size, z + size);

        Vector3[] vert = new Vector3[8];
        vert[0] = new Vector3(x, y, z);
        vert[1] = new Vector3(x + size, y, z);
        vert[2] = new Vector3(x + size, y, z + size);
        vert[3] = new Vector3(x, y, z + size);
        vert[4] = new Vector3(x, y + size, z);
        vert[5] = new Vector3(x + size, y + size, z);
        vert[6] = new Vector3(x + size, y + size, z + size);
        vert[7] = new Vector3(x, y + size, z + size);

        return new Cube(vals, surfaceVals, vert);
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
                        chunk[x, y, z] = MakeCube2(chunkEdge.x + x * m_size, chunkEdge.y + y * m_size, chunkEdge.z + z * m_size, m_size);
                    }
                }
            }

            Debug.Log(((Time.realtimeSinceStartup - startTime) * 1000f) + "ms");
            horizontalChunks.Add(new Vector2Int(chunkCoord.x, chunkCoord.z));
            chunks.Add(chunkCoord, chunk);
        }
    }
}
