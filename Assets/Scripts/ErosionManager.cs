using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;

public struct ErosionData
{
    public int i;
    public int j;
    public int k;
    public int x;
    public int y;
    public int z;
    public int w;
    public float erosionAmount;
    public bool stopped;

    public ErosionData(int i, int j, int k, int x, int y, int z, int w, float erosionAmount, bool stopped)
    {
        this.i = i;
        this.j = j;
        this.k = k;
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
        this.erosionAmount = erosionAmount;
        this.stopped = stopped;
    }
}

public class ErosionManager : MonoBehaviour
{

    [SerializeField]
    ChunkManager chunkManager;
    [SerializeField]
    MarchingCubes marchingCubes;
    public float erosionValue = 0.01f;
    public int droplets = 50;
    public int refreshThreshold = 10;
    public float erosionThreshold = 10f;
    public BlockType depositMaterial = BlockType.Grass;
    List<(Vector3Int, Vector3Int)> origins;

    // Start is called before the first frame update
    void Start()
    {
        origins = new List<(Vector3Int, Vector3Int)>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void RunErosion()
    {
        int times = droplets / refreshThreshold;
        int remainder = droplets % refreshThreshold;
        for (int k = 0; k < times; k++)
        {
            Erosion(refreshThreshold);
            marchingCubes.TestMesh();
        }

        Erosion(remainder);
        marchingCubes.TestMesh();
    }

    public void Erosion(int times)
    {
        Debug.Log("erosion running");
        List<Vector2Int> chunkCoords = new List<Vector2Int>(chunkManager.horizontalChunks);
        foreach (var coord in chunkCoords)
        {
            int j = 10;
            while (!chunkManager.CheckChunk(new Vector3Int(coord.x, j, coord.y)))
            {
                j--;
                if (j < -10)
                {
                    break;
                }
            }
            // if chunk level less than -10, go to next xz chunk column
            if (j < -10)
            {
                continue;
            }

            // run fluid sim on chunk
            for (int i = 0; i < times; i++)
            {
                int x = Random.Range(0, 32);
                int z = Random.Range(0, 32);
                int w = Random.Range(4, 8);
                origins.Clear();
                ErosionData erosionData = ErosionHelper(coord.x, j, coord.y, x, 31, z, w, 0);
                int iterations = 0;
                while (!erosionData.stopped && iterations < 1000)
                {
                    erosionData = ErosionHelper(erosionData.i, erosionData.j, erosionData.k, erosionData.x, erosionData.y, erosionData.z, erosionData.w, erosionData.erosionAmount);
                    iterations++;
                }
            }
        }
    }

    void SpreadDensity(int i, int j, int k, int x, int y, int z, float erosionAmount)
    {
        float e = erosionAmount / (9f * 8f);

        for (int index = 0; index < 8; index++)
        {
            ChangeDensity(i, j, k, x, y, z, index, e);
        }

        if (z == 31)
        {
            if (chunkManager.CheckGetChunk(new Vector3Int(i, j, k + 1)) != null)
            {
                for (int index = 0; index < 8; index++)
                {
                    ChangeDensity(i, j, k + 1, x, y, 0, index, e);
                }
            }
        }
        else
        {
            for (int index = 0; index < 8; index++)
            {
                ChangeDensity(i, j, k, x, y, z + 1, index, e);
            }
        }

        if (z == 31 && x == 31)
        {
            if (chunkManager.CheckGetChunk(new Vector3Int(i + 1, j, k + 1)) != null)
            {
                for (int index = 0; index < 8; index++)
                {
                    ChangeDensity(i + 1, j, k + 1, 0, y, 0, index, e);
                }
            }
        }
        else if (z == 31)
        {
            if (chunkManager.CheckGetChunk(new Vector3Int(i, j, k + 1)) != null)
            {
                for (int index = 0; index < 8; index++)
                {
                    ChangeDensity(i, j, k + 1, x + 1, y, 0, index, e);
                }
            }
        }
        else if (x == 31)
        {
            if (chunkManager.CheckGetChunk(new Vector3Int(i + 1, j, k)) != null)
            {
                for (int index = 0; index < 8; index++)
                {
                    ChangeDensity(i + 1, j, k, 0, y, z + 1, index, e);
                }
            }
        }
        else
        {
            for (int index = 0; index < 8; index++)
            {
                ChangeDensity(i, j, k, x + 1, y, z + 1, index, e);
            }
        }

        if (x == 31)
        {
            if (chunkManager.CheckGetChunk(new Vector3Int(i + 1, j, k)) != null)
            {
                for (int index = 0; index < 8; index++)
                {
                    ChangeDensity(i + 1, j, k, 0, y, z, index, e);
                }
            }
        }
        else
        {
            for (int index = 0; index < 8; index++)
            {
                ChangeDensity(i, j, k, x + 1, y, z, index, e);
            }
        }

        if (z == 0 && x == 31)
        {
            if (chunkManager.CheckGetChunk(new Vector3Int(i + 1, j, k - 1)) != null)
            {
                for (int index = 0; index < 8; index++)
                {
                    ChangeDensity(i + 1, j, k - 1, 0, y, 31, index, e);
                }
            }
        }
        else if (z == 0)
        {
            if (chunkManager.CheckGetChunk(new Vector3Int(i, j, k - 1)) != null)
            {
                for (int index = 0; index < 8; index++)
                {
                    ChangeDensity(i, j, k - 1, x + 1, y, 31, index, e);
                }
            }
        }
        else if (x == 31)
        {
            if (chunkManager.CheckGetChunk(new Vector3Int(i + 1, j, k)) != null)
            {
                for (int index = 0; index < 8; index++)
                {
                    ChangeDensity(i + 1, j, k, 0, y, z - 1, index, e);
                }
            }
        }
        else
        {
            for (int index = 0; index < 8; index++)
            {
                ChangeDensity(i, j, k, x + 1, y, z - 1, index, e);
            }
        }

        if (z == 0)
        {
            if (chunkManager.CheckGetChunk(new Vector3Int(i, j, k - 1)) != null)
            {
                for (int index = 0; index < 8; index++)
                {
                    ChangeDensity(i, j, k - 1, x, y, 31, index, e);
                }
            }
        }
        else
        {
            for (int index = 0; index < 8; index++)
            {
                ChangeDensity(i, j, k, x, y, z - 1, index, e);
            }
        }

        if (z == 0 && x == 0)
        {
            if (chunkManager.CheckGetChunk(new Vector3Int(i - 1, j, k - 1)) != null)
            {
                for (int index = 0; index < 8; index++)
                {
                    ChangeDensity(i - 1, j, k - 1, 31, y, 31, index, e);
                }
            }
        }
        else if (z == 0)
        {
            if (chunkManager.CheckGetChunk(new Vector3Int(i, j, k - 1)) != null)
            {
                for (int index = 0; index < 8; index++)
                {
                    ChangeDensity(i, j, k - 1, x - 1, y, 31, index, e);
                }
            }
        }
        else if (x == 0)
        {
            if (chunkManager.CheckGetChunk(new Vector3Int(i - 1, j, k)) != null)
            {
                for (int index = 0; index < 8; index++)
                {
                    ChangeDensity(i - 1, j, k, 31, y, z - 1, index, e);
                }
            }
        }
        else
        {
            for (int index = 0; index < 8; index++)
            {
                ChangeDensity(i, j, k, x - 1, y, z - 1, index, e);
            }
        }

        if (x == 0)
        {
            if (chunkManager.CheckGetChunk(new Vector3Int(i - 1, j, k)) != null)
            {
                for (int index = 0; index < 8; index++)
                {
                    ChangeDensity(i - 1, j, k, 31, y, z, index, e);
                }
            }
        }
        else
        {
            for (int index = 0; index < 8; index++)
            {
                ChangeDensity(i, j, k, x - 1, y, z, index, e);
            }
        }

        if (z == 31 && x == 0)
        {
            if (chunkManager.CheckGetChunk(new Vector3Int(i - 1, j, k + 1)) != null)
            {
                for (int index = 0; index < 8; index++)
                {
                    ChangeDensity(i - 1, j, k + 1, 31, y, 0, index, e);
                }
            }
        }
        else if (z == 31)
        {
            if (chunkManager.CheckGetChunk(new Vector3Int(i, j, k + 1)) != null)
            {
                for (int index = 0; index < 8; index++)
                {
                    ChangeDensity(i, j, k + 1, x - 1, y, 0, index, e);
                }
            }
        }
        else if (x == 0)
        {
            if (chunkManager.CheckGetChunk(new Vector3Int(i - 1, j, k)) != null)
            {
                for (int index = 0; index < 8; index++)
                {
                    ChangeDensity(i - 1, j, k, 31, y, z + 1, index, e);
                }
            }
        }
        else
        {
            for (int index = 0; index < 8; index++)
            {
                ChangeDensity(i, j, k, x - 1, y, z + 1, index, e);
            }
        }
    }

    ErosionData ErosionHelper(int i, int j, int k, int x, int y, int z, int w, float erosionAmount)
    {
        Vector3Int chunkCoord = new Vector3Int(i, j, k);
        Vector3Int cubeCoord = new Vector3Int(x, y, z);

        Cube[,,] chunk = chunkManager.CheckGetChunk(chunkCoord);
        if (chunk == null)
        {
            return new ErosionData(i, j, k, x, y, z, w, erosionAmount, true);
        }
        // if reach chunk limit return
        if (j < -10)
        {
            return new ErosionData(i, j, k, x, y, z, w, erosionAmount, true);
        }

        Cube cube = chunk[x, y, z];
        // if cube vertex is Air move downwards
        if (cube.materials[w] == BlockType.Air)
        {
            origins.Clear();
            int newJ = j;
            int newW = w;
            int newY = y;

            Cube newCube = chunk[x, y, z];
            while (newCube.materials[newW] == BlockType.Air)
            {
                if (newW >= 4)
                {
                    newW -= 4;
                }
                else
                {
                    if (newY == 0)
                    {
                        newJ -= 1;
                        newW += 4;
                        newY = 31;
                    }
                    else
                    {
                        newY -= 1;
                        newW += 4;
                    }
                }

                Cube[,,] newChunk = chunkManager.CheckGetChunk(new Vector3Int(i, newJ, k));
                if (newChunk != null)
                {
                    newCube = newChunk[x, newY, z];
                }
                else
                {
                    return new ErosionData(i, j, k, x, y, z, w, erosionAmount, true);
                }
            }
            return new ErosionData(i, newJ, k, x, newY, z, newW, erosionAmount, false);
        }
        else // if not Air check if adjacent is Air
        {
            Mesh mesh = marchingCubes.meshes[chunkCoord].GetComponent<MeshFilter>().mesh;
            Vector3[] normals = mesh.normals;
            // go through vertex indices of cube and get upwards facing vectors
            Vector3 normalAverage = Vector3.zero;
            int normalNum = 0;
            foreach (int index in cube.meshVertices)
            {
                Vector3 normal = normals[index];
                if (normal.y > 0)
                {
                    normalAverage += normal;
                    normalNum++;
                }
            }
            normalAverage /= normalNum;
            // if normal is close to vertical then deposit droplet
            if (Vector3.Angle(Vector3.up, normalAverage) < erosionThreshold)
            {
                //float e = erosionAmount / 8.0f;
                //for (int index = 0; index < 8; index++)
                //{
                //    //if (chunk[x, y, z].materials[index] != BlockType.Air)
                //    //{
                //    //    ChangeDensity(i, j, k, x, y, z, index, e);
                //    //}
                //    ChangeDensity(i, j, k, x, y, z, index, e);
                //}
                SpreadDensity(i, j, k, x, y, z, erosionAmount);
                return new ErosionData(i, j, k, x, y, z, w, erosionAmount, true);
            }
            else
            {
                // remove density and choose direction to move droplet based on normal
                bool same = false;
                foreach (var origin in origins)
                {
                    if (origin.Item1 == chunkCoord && origin.Item2 == cubeCoord)
                    {
                        same = true;
                        break;
                    }
                }

                if (same)
                {
                    //float e = erosionAmount / 8.0f;
                    //for (int index = 0; index < 8; index++)
                    //{
                    //    //if (chunk[x, y, z].materials[index] != BlockType.Air)
                    //    //{
                    //    //    ChangeDensity(i, j, k, x, y, z, index, e);
                    //    //}
                    //    ChangeDensity(i, j, k, x, y, z, index, e);
                    //}
                    SpreadDensity(i, j, k, x, y, z, erosionAmount);
                    return new ErosionData(i, j, k, x, y, z, w, erosionAmount, true);
                }
                else
                {
                    float e = -erosionValue / 8.0f;
                    for (int index = 0; index < 8; index++)
                    {
                        //if (chunk[x, y, z].materials[index] != BlockType.Air)
                        //{
                        //    ChangeDensity(i, j, k, x, y, z, index, e);
                        //}
                        ChangeDensity(i, j, k, x, y, z, index, e);
                    }
                }

                origins.Add((chunkCoord, cubeCoord));

                float direction = Vector3.SignedAngle(Vector3.forward, new Vector3(normalAverage.x, 0, normalAverage.z), Vector3.up);
                if (direction > -22.5f && direction <= 22.5f)
                {
                    if (z == 31)
                    {
                        if (chunkManager.CheckGetChunk(new Vector3Int(i, j, k + 1)) != null)
                        {
                            return new ErosionData(i, j, k + 1, x, y, 0, w, erosionAmount + erosionValue, false);
                        }
                    }
                    else
                    {
                        return new ErosionData(i, j, k, x, y, z + 1, w, erosionAmount + erosionValue, false);
                    }
                }
                else if (direction > 22.5f && direction <= 67.5f)
                {
                    if (z == 31 && x == 31)
                    {
                        if (chunkManager.CheckGetChunk(new Vector3Int(i + 1, j, k + 1)) != null)
                        {
                            return new ErosionData(i + 1, j, k + 1, 0, y, 0, w, erosionAmount + erosionValue, false);
                        }
                    }
                    else if (z == 31)
                    {
                        if (chunkManager.CheckGetChunk(new Vector3Int(i, j, k + 1)) != null)
                        {
                            return new ErosionData(i, j, k + 1, x + 1, y, 0, w, erosionAmount + erosionValue, false);
                        }
                    }
                    else if (x == 31)
                    {
                        if (chunkManager.CheckGetChunk(new Vector3Int(i + 1, j, k)) != null)
                        {
                            return new ErosionData(i + 1, j, k, 0, y, z + 1, w, erosionAmount + erosionValue, false);
                        }
                    }
                    else
                    {
                        return new ErosionData(i, j, k, x + 1, y, z + 1, w, erosionAmount + erosionValue, false);
                    }
                }
                else if (direction > 67.5f && direction <= 112.5f)
                {
                    if (x == 31)
                    {
                        if (chunkManager.CheckGetChunk(new Vector3Int(i + 1, j, k)) != null)
                        {
                            return new ErosionData(i + 1, j, k, 0, y, z, w, erosionAmount + erosionValue, false);
                        }
                    }
                    else
                    {
                        return new ErosionData(i, j, k, x + 1, y, z, w, erosionAmount + erosionValue, false);
                    }
                }
                else if (direction > 112.5f && direction <= 157.5f)
                {
                    if (z == 0 && x == 31)
                    {
                        if (chunkManager.CheckGetChunk(new Vector3Int(i + 1, j, k - 1)) != null)
                        {
                            return new ErosionData(i + 1, j, k - 1, 0, y, 31, w, erosionAmount + erosionValue, false);
                        }
                    }
                    else if (z == 0)
                    {
                        if (chunkManager.CheckGetChunk(new Vector3Int(i, j, k - 1)) != null)
                        {
                            return new ErosionData(i, j, k - 1, x + 1, y, 31, w, erosionAmount + erosionValue, false);
                        }
                    }
                    else if (x == 31)
                    {
                        if (chunkManager.CheckGetChunk(new Vector3Int(i + 1, j, k)) != null)
                        {
                            return new ErosionData(i + 1, j, k, 0, y, z - 1, w, erosionAmount + erosionValue, false);
                        }
                    }
                    else
                    {
                        return new ErosionData(i, j, k, x + 1, y, z - 1, w, erosionAmount + erosionValue, false);
                    }
                }
                else if (direction > 157.5f || direction <= -157.5f)
                {
                    if (z == 0)
                    {
                        if (chunkManager.CheckGetChunk(new Vector3Int(i, j, k - 1)) != null)
                        {
                            return new ErosionData(i, j, k - 1, x, y, 31, w, erosionAmount + erosionValue, false);
                        }
                    }
                    else
                    {
                        return new ErosionData(i, j, k, x, y, z - 1, w, erosionAmount + erosionValue, false);
                    }
                }
                else if (direction > -157.5f && direction <= -112.5f)
                {
                    if (z == 0 && x == 0)
                    {
                        if (chunkManager.CheckGetChunk(new Vector3Int(i - 1, j, k - 1)) != null)
                        {
                            return new ErosionData(i - 1, j, k - 1, 31, y, 31, w, erosionAmount + erosionValue, false);
                        }
                    }
                    else if (z == 0)
                    {
                        if (chunkManager.CheckGetChunk(new Vector3Int(i, j, k - 1)) != null)
                        {
                            return new ErosionData(i, j, k - 1, x - 1, y, 31, w, erosionAmount + erosionValue, false);
                        }
                    }
                    else if (x == 0)
                    {
                        if (chunkManager.CheckGetChunk(new Vector3Int(i - 1, j, k)) != null)
                        {
                            return new ErosionData(i - 1, j, k, 31, y, z - 1, w, erosionAmount + erosionValue, false);
                        }
                    }
                    else
                    {
                        return new ErosionData(i, j, k, x - 1, y, z - 1, w, erosionAmount + erosionValue, false);
                    }
                }
                else if (direction > -112.5f && direction <= -67.5f)
                {
                    if (x == 0)
                    {
                        if (chunkManager.CheckGetChunk(new Vector3Int(i - 1, j, k)) != null)
                        {
                            return new ErosionData(i - 1, j, k, 31, y, z, w, erosionAmount + erosionValue, false);
                        }
                    }
                    else
                    {
                        return new ErosionData(i, j, k, x - 1, y, z, w, erosionAmount + erosionValue, false);
                    }
                }
                else
                {
                    if (z == 31 && x == 0)
                    {
                        if (chunkManager.CheckGetChunk(new Vector3Int(i - 1, j, k + 1)) != null)
                        {
                            return new ErosionData(i - 1, j, k + 1, 31, y, 0, w, erosionAmount + erosionValue, false);
                        }
                    }
                    else if (z == 31)
                    {
                        if (chunkManager.CheckGetChunk(new Vector3Int(i, j, k + 1)) != null)
                        {
                            return new ErosionData(i, j, k + 1, x - 1, y, 0, w, erosionAmount + erosionValue, false);
                        }
                    }
                    else if (x == 0)
                    {
                        if (chunkManager.CheckGetChunk(new Vector3Int(i - 1, j, k)) != null)
                        {
                            return new ErosionData(i - 1, j, k, 31, y, z + 1, w, erosionAmount + erosionValue, false);
                        }
                    }
                    else
                    {
                        return new ErosionData(i, j, k, x - 1, y, z + 1, w, erosionAmount + erosionValue, false);
                    }
                }
            }
        }
        return new ErosionData(i, j, k, x, y, z, w, erosionAmount, true);
    }

    BlockType TurnToAir(Cube cube, int index)
    {
        BlockType block = cube.materials[index];

        if (cube.surfaceValues[index] <= 0 && block != BlockType.Air)
        {
            block = BlockType.Air;
        }
        else if (cube.surfaceValues[index] > 0 && block == BlockType.Air)
        {
            block = depositMaterial;
        }
        return block;
    }

    void ChangeNeighbor(int val, float density, int i, int j, int k, int x, int y, int z)
    {
        Cube[,,] chunk = chunkManager.CheckGetChunk(new Vector3Int(i, j, k));
        if (chunk != null)
        {
            chunk[x, y, z].surfaceValues[val] += density;
            chunk[x, y, z].materials[val] = TurnToAir(chunk[x, y, z], val);
        }
    }

    void ChangeDensity(int i, int j, int k, int x, int y, int z, int w, float density)
    {
        Cube[,,] chunk = chunkManager.CheckGetChunk(new Vector3Int(i, j, k));
        if (chunk == null)
        {
            return;
        }
        Cube cube = chunk[x, y, z];
        cube.surfaceValues[w] += density;
        cube.materials[w] = TurnToAir(cube, w);


        switch (w)
        {
            case 0:
                int val = 1;
                if (x > 0)
                {
                    ChangeNeighbor(val, density, i, j, k, x - 1, y, z);
                }
                else
                {
                    ChangeNeighbor(val, density, i - 1, j, k, 31, y, z);
                }

                val = 4;
                if (y > 0)
                {
                    ChangeNeighbor(val, density, i, j, k, x, y - 1, z);
                }
                else
                {
                    ChangeNeighbor(val, density, i, j - 1, k, x, 31, z);
                }

                val = 3;
                if (z > 0)
                {
                    ChangeNeighbor(val, density, i, j, k, x, y, z - 1);
                }
                else
                {
                    ChangeNeighbor(val, density, i, j, k - 1, x, y, 31);
                }

                val = 5;
                if (x > 0 && y > 0)
                {
                    ChangeNeighbor(val, density, i, j, k, x - 1, y - 1, z);
                }
                else if (x > 0 && y == 0)
                {
                    ChangeNeighbor(val, density, i, j - 1, k, x - 1, 31, z);
                }
                else if (x == 0 && y > 0)
                {
                    ChangeNeighbor(val, density, i - 1, j, k, 31, y - 1, z);
                }
                else
                {
                    ChangeNeighbor(val, density, i - 1, j - 1, k, 31, 31, z);
                }

                val = 2;
                if (x > 0 && z > 0)
                {
                    ChangeNeighbor(val, density, i, j, k, x - 1, y, z - 1);
                }
                else if (x > 0 && z == 0)
                {
                    ChangeNeighbor(val, density, i, j, k - 1, x - 1, y, 31);
                }
                else if (x == 0 && z > 0)
                {
                    ChangeNeighbor(val, density, i - 1, j, k, 31, y, z - 1);
                }
                else
                {
                    ChangeNeighbor(val, density, i - 1, j, k - 1, 31, y, 31);
                }

                val = 7;
                if (y > 0 && z > 0)
                {
                    ChangeNeighbor(val, density, i, j, k, x, y - 1, z - 1);
                }
                else if (y > 0 && z == 0)
                {
                    ChangeNeighbor(val, density, i, j, k - 1, x, y - 1, 31);
                }
                else if (y == 0 && z > 0)
                {
                    ChangeNeighbor(val, density, i, j - 1, k, x, 31, z - 1);
                }
                else
                {
                    ChangeNeighbor(val, density, i, j - 1, k - 1, x, 31, 31);
                }

                val = 6;
                if (x > 0 && y > 0 && z > 0)
                {
                    ChangeNeighbor(val, density, i, j, k, x - 1, y - 1, z - 1);
                }
                if (x > 0 && y > 0 && z == 0)
                {
                    ChangeNeighbor(val, density, i, j, k - 1, x - 1, y - 1, 31);
                }
                if (x > 0 && y == 0 && z > 0)
                {
                    ChangeNeighbor(val, density, i, j - 1, k, x - 1, 31, z - 1);
                }
                if (x == 0 && y > 0 && z > 0)
                {
                    ChangeNeighbor(val, density, i - 1, j, k, 31, y - 1, z - 1);
                }
                if (x > 0 && y == 0 && z == 0)
                {
                    ChangeNeighbor(val, density, i, j - 1, k - 1, x - 1, 31, 31);
                }
                if (x == 0 && y > 0 && z == 0)
                {
                    ChangeNeighbor(val, density, i - 1, j, k - 1, 31, y - 1, 31);
                }
                if (x == 0 && y == 0 && z > 0)
                {
                    ChangeNeighbor(val, density, i - 1, j - 1, k, 31, 31, z - 1);
                }
                else
                {
                    ChangeNeighbor(val, density, i - 1, j - 1, k - 1, 31, 31, 31);
                }

                break;
            case 1:
                val = 0;
                if (x < 31)
                {
                    ChangeNeighbor(val, density, i, j, k, x + 1, y, z);
                }
                else
                {
                    ChangeNeighbor(val, density, i + 1, j, k, 0, y, z);
                }

                val = 5;
                if (y > 0)
                {
                    ChangeNeighbor(val, density, i, j, k, x, y - 1, z);
                }
                else
                {
                    ChangeNeighbor(val, density, i, j - 1, k, x, 31, z);
                }

                val = 2;
                if (z > 0)
                {
                    ChangeNeighbor(val, density, i, j, k, x, y, z - 1);
                }
                else
                {
                    ChangeNeighbor(val, density, i, j, k - 1, x, y, 31);
                }

                val = 4;
                if (x < 31 && y > 0)
                {
                    ChangeNeighbor(val, density, i, j, k, x + 1, y - 1, z);
                }
                else if (x < 31 && y == 0)
                {
                    ChangeNeighbor(val, density, i, j - 1, k, x + 1, 31, z);
                }
                else if (x == 31 && y > 0)
                {
                    ChangeNeighbor(val, density, i + 1, j, k, 0, y - 1, z);
                }
                else
                {
                    ChangeNeighbor(val, density, i + 1, j - 1, k, 0, 31, z);
                }

                val = 3;
                if (x < 31 && z > 0)
                {
                    ChangeNeighbor(val, density, i, j, k, x + 1, y, z - 1);
                }
                else if (x < 31 && z == 0)
                {
                    ChangeNeighbor(val, density, i, j, k - 1, x + 1, y, 31);
                }
                else if (x == 31 && z > 0)
                {
                    ChangeNeighbor(val, density, i + 1, j, k, 0, y, z - 1);
                }
                else
                {
                    ChangeNeighbor(val, density, i + 1, j, k - 1, 0, y, 31);
                }

                val = 6;
                if (y > 0 && z > 0)
                {
                    ChangeNeighbor(val, density, i, j, k, x, y - 1, z - 1);
                }
                else if (y > 0 && z == 0)
                {
                    ChangeNeighbor(val, density, i, j, k - 1, x, y - 1, 31);
                }
                else if (y == 0 && z > 0)
                {
                    ChangeNeighbor(val, density, i, j - 1, k, x, 31, z - 1);
                }
                else
                {
                    ChangeNeighbor(val, density, i, j - 1, k - 1, x, 31, 31);
                }

                val = 7;
                if (x < 31 && y > 0 && z > 0)
                {
                    ChangeNeighbor(val, density, i, j, k, x + 1, y - 1, z - 1);
                }
                else if (x < 31 && y > 0 && z == 0)
                {
                    ChangeNeighbor(val, density, i, j, k - 1, x + 1, y - 1, 31);
                }
                else if (x < 31 && y == 0 && z > 0)
                {
                    ChangeNeighbor(val, density, i, j - 1, k, x + 1, 31, z - 1);
                }
                else if (x == 31 && y > 0 && z > 0)
                {
                    ChangeNeighbor(val, density, i + 1, j, k, 0, y - 1, z - 1);
                }
                else if (x < 31 && y == 0 && z == 0)
                {
                    ChangeNeighbor(val, density, i, j - 1, k - 1, x + 1, 31, 31);
                }
                else if (x == 31 && y > 0 && z == 0)
                {
                    ChangeNeighbor(val, density, i + 1, j, k - 1, 0, y - 1, 31);
                }
                else if (x == 31 && y == 0 && z > 0)
                {
                    ChangeNeighbor(val, density, i + 1, j - 1, k, 0, 31, z - 1);
                }
                else
                {
                    ChangeNeighbor(val, density, i + 1, j - 1, k - 1, 0, 31, 31);
                }

                break;
            case 2:
                val = 3;
                if (x < 31)
                {
                    ChangeNeighbor(val, density, i, j, k, x + 1, y, z);
                }
                else
                {
                    ChangeNeighbor(val, density, i + 1, j, k, 0, y, z);
                }

                val = 6;
                if (y > 0)
                {
                    ChangeNeighbor(val, density, i, j, k, x, y - 1, z);
                }
                else
                {
                    ChangeNeighbor(val, density, i, j - 1, k, x, 31, z);
                }

                val = 1;
                if (z < 31)
                {
                    ChangeNeighbor(val, density, i, j, k, x, y, z + 1);
                }
                else
                {
                    ChangeNeighbor(val, density, i, j, k + 1, x, y, 0);
                }

                val = 7;
                if (x < 31 && y > 0)
                {
                    ChangeNeighbor(val, density, i, j, k, x + 1, y - 1, z);
                }
                else if (x < 31 && y == 0)
                {
                    ChangeNeighbor(val, density, i, j - 1, k, x + 1, 31, z);
                }
                else if (x == 31 && y > 0)
                {
                    ChangeNeighbor(val, density, i + 1, j, k, 0, y - 1, z);
                }
                else
                {
                    ChangeNeighbor(val, density, i + 1, j - 1, k, 0, 31, z);
                }

                val = 0;
                if (x < 31 && z < 31)
                {
                    ChangeNeighbor(val, density, i, j, k, x + 1, y, z + 1);
                }
                else if (x < 31 && z == 31)
                {
                    ChangeNeighbor(val, density, i, j, k + 1, x + 1, y, 0);
                }
                else if (x == 31 && z < 31)
                {
                    ChangeNeighbor(val, density, i + 1, j, k, 0, y, z + 1);
                }
                else
                {
                    ChangeNeighbor(val, density, i + 1, j, k + 1, 0, y, 0);
                }

                val = 5;
                if (y > 0 && z < 31)
                {
                    ChangeNeighbor(val, density, i, j, k, x, y - 1, z + 1);
                }
                else if (y > 0 && z == 31)
                {
                    ChangeNeighbor(val, density, i, j, k + 1, x, y - 1, 0);
                }
                else if (y == 0 && z < 31)
                {
                    ChangeNeighbor(val, density, i, j - 1, k, x, 31, z + 1);
                }
                else
                {
                    ChangeNeighbor(val, density, i, j - 1, k + 1, x, 31, 0);
                }

                val = 4;
                if (x < 31 && y > 0 && z < 31)
                {
                    ChangeNeighbor(val, density, i, j, k, x + 1, y - 1, z + 1);
                }
                else if (x < 31 && y > 0 && z == 31)
                {
                    ChangeNeighbor(val, density, i, j, k + 1, x + 1, y - 1, 0);
                }
                else if (x < 31 && y == 0 && z < 31)
                {
                    ChangeNeighbor(val, density, i, j - 1, k, x + 1, 31, z + 1);
                }
                else if (x == 31 && y > 0 && z < 31)
                {
                    ChangeNeighbor(val, density, i + 1, j, k, 0, y - 1, z + 1);
                }
                else if (x < 31 && y == 0 && z == 31)
                {
                    ChangeNeighbor(val, density, i, j - 1, k + 1, x + 1, 31, 0);
                }
                else if (x == 31 && y > 0 && z == 31)
                {
                    ChangeNeighbor(val, density, i + 1, j, k + 1, 0, y - 1, 0);
                }
                else if (x == 31 && y == 0 && z < 31)
                {
                    ChangeNeighbor(val, density, i + 1, j - 1, k, 0, 31, z + 1);
                }
                else
                {
                    ChangeNeighbor(val, density, i + 1, j - 1, k + 1, 0, 31, 0);
                }

                break;
            case 3:
                val = 2;
                if (x > 0)
                {
                    ChangeNeighbor(val, density, i, j, k, x - 1, y, z);
                }
                else
                {
                    ChangeNeighbor(val, density, i - 1, j, k, 31, y, z);
                }

                val = 7;
                if (y > 0)
                {
                    ChangeNeighbor(val, density, i, j, k, x, y - 1, z);
                }
                else
                {
                    ChangeNeighbor(val, density, i, j - 1, k, x, 31, z);
                }

                val = 0;
                if (z < 31)
                {
                    ChangeNeighbor(val, density, i, j, k, x, y, z + 1);
                }
                else
                {
                    ChangeNeighbor(val, density, i, j, k + 1, x, y, 0);
                }

                val = 6;
                if (x > 0 && y > 0)
                {
                    ChangeNeighbor(val, density, i, j, k, x - 1, y - 1, z);
                }
                else if (x > 0 && y == 0)
                {
                    ChangeNeighbor(val, density, i, j - 1, k, x - 1, 31, z);
                }
                else if (x == 0 && y > 0)
                {
                    ChangeNeighbor(val, density, i - 1, j, k, 31, y - 1, z);
                }
                else
                {
                    ChangeNeighbor(val, density, i - 1, j - 1, k, 31, 31, z);
                }

                val = 1;
                if (x > 0 && z < 31)
                {
                    ChangeNeighbor(val, density, i, j, k, x - 1, y, z + 1);
                }
                else if (x > 0 && z == 31)
                {
                    ChangeNeighbor(val, density, i, j, k + 1, x - 1, y, 0);
                }
                else if (x == 0 && z < 31)
                {
                    ChangeNeighbor(val, density, i - 1, j, k, 31, y, z + 1);
                }
                else
                {
                    ChangeNeighbor(val, density, i - 1, j, k + 1, 31, y, 0);
                }

                val = 4;
                if (y > 0 && z < 31)
                {
                    ChangeNeighbor(val, density, i, j, k, x, y - 1, z + 1);
                }
                else if (y > 0 && z == 31)
                {
                    ChangeNeighbor(val, density, i, j, k + 1, x, y - 1, 0);
                }
                else if (y == 0 && z < 31)
                {
                    ChangeNeighbor(val, density, i, j - 1, k, x, 31, z + 1);
                }
                else
                {
                    ChangeNeighbor(val, density, i, j - 1, k + 1, x, 31, 0);
                }

                val = 5;
                if (x > 0 && y > 0 && z < 31)
                {
                    ChangeNeighbor(val, density, i, j, k, x - 1, y - 1, z + 1);
                }
                else if (x > 0 && y > 0 && z == 31)
                {
                    ChangeNeighbor(val, density, i, j, k + 1, x - 1, y - 1, 0);
                }
                else if (x > 0 && y == 0 && z < 31)
                {
                    ChangeNeighbor(val, density, i, j - 1, k, x - 1, 31, z + 1);
                }
                else if (x == 0 && y > 0 && z < 31)
                {
                    ChangeNeighbor(val, density, i - 1, j, k, 31, y - 1, z + 1);
                }
                else if (x > 0 && y == 0 && z == 31)
                {
                    ChangeNeighbor(val, density, i, j - 1, k + 1, x - 1, 31, 0);
                }
                else if (x == 0 && y > 0 && z == 31)
                {
                    ChangeNeighbor(val, density, i - 1, j, k + 1, 31, y - 1, 0);
                }
                else if (x == 0 && y == 0 && z < 31)
                {
                    ChangeNeighbor(val, density, i - 1, j - 1, k, 31, 31, z + 1);
                }
                else
                {
                    ChangeNeighbor(val, density, i - 1, j - 1, k + 1, 31, 31, 0);
                }

                break;
            case 4:
                val = 5;
                if (x > 0)
                {
                    ChangeNeighbor(val, density, i, j, k, x - 1, y, z);
                }
                else
                {
                    ChangeNeighbor(val, density, i - 1, j, k, 31, y, z);
                }

                val = 0;
                if (y < 31)
                {
                    ChangeNeighbor(val, density, i, j, k, x, y + 1, z);
                }
                else
                {
                    ChangeNeighbor(val, density, i, j + 1, k, x, 0, z);
                }

                val = 7;
                if (z > 0)
                {
                    ChangeNeighbor(val, density, i, j, k, x, y, z - 1);
                }
                else
                {
                    ChangeNeighbor(val, density, i, j, k - 1, x, y, 31);
                }

                val = 1;
                if (x > 0 && y < 31)
                {
                    ChangeNeighbor(val, density, i, j, k, x - 1, y + 1, z);
                }
                else if (x > 0 && y == 31)
                {
                    ChangeNeighbor(val, density, i, j + 1, k, x - 1, 0, z);
                }
                else if (x == 0 && y < 31)
                {
                    ChangeNeighbor(val, density, i - 1, j, k, 31, y + 1, z);
                }
                else
                {
                    ChangeNeighbor(val, density, i - 1, j + 1, k, 31, 0, z);
                }

                val = 6;
                if (x > 0 && z > 0)
                {
                    ChangeNeighbor(val, density, i, j, k, x - 1, y, z - 1);
                }
                else if (x > 0 && z == 0)
                {
                    ChangeNeighbor(val, density, i, j, k - 1, x - 1, y, 31);
                }
                else if (x == 0 && z > 0)
                {
                    ChangeNeighbor(val, density, i - 1, j, k, 31, y, z - 1);
                }
                else
                {
                    ChangeNeighbor(val, density, i - 1, j, k - 1, 31, y, 31);
                }

                val = 3;
                if (y < 31 && z > 0)
                {
                    ChangeNeighbor(val, density, i, j, k, x, y + 1, z - 1);
                }
                else if (y < 31 && z == 0)
                {
                    ChangeNeighbor(val, density, i, j, k - 1, x, y + 1, 31);
                }
                else if (y == 31 && z > 0)
                {
                    ChangeNeighbor(val, density, i, j + 1, k, x, 0, z - 1);
                }
                else
                {
                    ChangeNeighbor(val, density, i, j + 1, k - 1, x, 0, 31);
                }

                val = 2;
                if (x > 0 && y < 31 && z > 0)
                {
                    ChangeNeighbor(val, density, i, j, k, x - 1, y + 1, z - 1);
                }
                else if (x > 0 && y < 31 && z == 0)
                {
                    ChangeNeighbor(val, density, i, j, k - 1, x - 1, y + 1, 31);
                }
                else if (x > 0 && y == 31 && z > 0)
                {
                    ChangeNeighbor(val, density, i, j + 1, k, x - 1, 0, z - 1);
                }
                else if (x == 0 && y < 31 && z > 0)
                {
                    ChangeNeighbor(val, density, i - 1, j, k, 31, y + 1, z - 1);
                }
                else if (x > 0 && y == 31 && z == 0)
                {
                    ChangeNeighbor(val, density, i, j + 1, k - 1, x - 1, 0, 31);
                }
                else if (x == 0 && y < 31 && z == 0)
                {
                    ChangeNeighbor(val, density, i - 1, j, k - 1, 31, y + 1, 31);
                }
                else if (x == 0 && y == 31 && z > 0)
                {
                    ChangeNeighbor(val, density, i - 1, j + 1, k, 31, 0, z - 1);
                }
                else
                {
                    ChangeNeighbor(val, density, i - 1, j + 1, k - 1, 31, 0, 31);
                }

                break;
            case 5:
                val = 4;
                if (x < 31)
                {
                    ChangeNeighbor(val, density, i, j, k, x + 1, y, z);
                }
                else
                {
                    ChangeNeighbor(val, density, i + 1, j, k, 0, y, z);
                }

                val = 1;
                if (y < 31)
                {
                    ChangeNeighbor(val, density, i, j, k, x, y + 1, z);
                }
                else
                {
                    ChangeNeighbor(val, density, i, j + 1, k, x, 0, z);
                }

                val = 6;
                if (z > 0)
                {
                    ChangeNeighbor(val, density, i, j, k, x, y, z - 1);
                }
                else
                {
                    ChangeNeighbor(val, density, i, j, k - 1, x, y, 31);
                }

                val = 0;
                if (x < 31 && y < 31)
                {
                    ChangeNeighbor(val, density, i, j, k, x + 1, y + 1, z);
                }
                else if (x < 31 && y == 31)
                {
                    ChangeNeighbor(val, density, i, j + 1, k, x + 1, 0, z);
                }
                else if (x == 31 && y < 31)
                {
                    ChangeNeighbor(val, density, i + 1, j, k, 0, y + 1, z);
                }
                else
                {
                    ChangeNeighbor(val, density, i + 1, j + 1, k, 0, 0, z);
                }

                val = 7;
                if (x < 31 && z > 0)
                {
                    ChangeNeighbor(val, density, i, j, k, x + 1, y, z - 1);
                }
                else if (x < 31 && z == 0)
                {
                    ChangeNeighbor(val, density, i, j, k - 1, x + 1, y, 31);
                }
                else if (x == 31 && z > 0)
                {
                    ChangeNeighbor(val, density, i + 1, j, k, 0, y, z - 1);
                }
                else
                {
                    ChangeNeighbor(val, density, i + 1, j, k - 1, 0, y, 31);
                }

                val = 2;
                if (y < 31 && z > 0)
                {
                    ChangeNeighbor(val, density, i, j, k, x, y + 1, z - 1);
                }
                else if (y < 31 && z == 0)
                {
                    ChangeNeighbor(val, density, i, j, k - 1, x, y + 1, 31);
                }
                else if (y == 31 && z > 0)
                {
                    ChangeNeighbor(val, density, i, j + 1, k, x, 0, z - 1);
                }
                else
                {
                    ChangeNeighbor(val, density, i, j + 1, k - 1, x, 0, 31);
                }

                val = 3;
                if (x < 31 && y < 31 && z > 0)
                {
                    ChangeNeighbor(val, density, i, j, k, x + 1, y + 1, z - 1);
                }
                else if (x < 31 && y < 31 && z == 0)
                {
                    ChangeNeighbor(val, density, i, j, k - 1, x + 1, y + 1, 31);
                }
                else if (x < 31 && y == 31 && z > 0)
                {
                    ChangeNeighbor(val, density, i, j + 1, k, x + 1, 0, z - 1);
                }
                else if (x == 31 && y < 31 && z > 0)
                {
                    ChangeNeighbor(val, density, i + 1, j, k, 0, y + 1, z - 1);
                }
                else if (x < 31 && y == 31 && z == 0)
                {
                    ChangeNeighbor(val, density, i, j + 1, k - 1, x + 1, 0, 31);
                }
                else if (x == 31 && y < 31 && z == 0)
                {
                    ChangeNeighbor(val, density, i + 1, j, k - 1, 0, y + 1, 31);
                }
                else if (x == 31 && y == 31 && z > 0)
                {
                    ChangeNeighbor(val, density, i + 1, j + 1, k, 0, 0, z - 1);
                }
                else
                {
                    ChangeNeighbor(val, density, i + 1, j + 1, k - 1, 0, 0, 31);
                }

                break;
            case 6:
                val = 7;
                if (x < 31)
                {
                    ChangeNeighbor(val, density, i, j, k, x + 1, y, z);
                }
                else
                {
                    ChangeNeighbor(val, density, i + 1, j, k, 0, y, z);
                }

                val = 2;
                if (y < 31)
                {
                    ChangeNeighbor(val, density, i, j, k, x, y + 1, z);
                }
                else
                {
                    ChangeNeighbor(val, density, i, j + 1, k, x, 0, z);
                }

                val = 5;
                if (z < 31)
                {
                    ChangeNeighbor(val, density, i, j, k, x, y, z + 1);
                }
                else
                {
                    ChangeNeighbor(val, density, i, j, k + 1, x, y, 0);
                }

                val = 3;
                if (x < 31 && y < 31)
                {
                    ChangeNeighbor(val, density, i, j, k, x + 1, y + 1, z);
                }
                else if (x < 31 && y == 31)
                {
                    ChangeNeighbor(val, density, i, j + 1, k, x + 1, 0, z);
                }
                else if (x == 31 && y < 31)
                {
                    ChangeNeighbor(val, density, i + 1, j, k, 0, y + 1, z);
                }
                else
                {
                    ChangeNeighbor(val, density, i + 1, j + 1, k, 0, 0, z);
                }

                val = 4;
                if (x < 31 && z < 31)
                {
                    ChangeNeighbor(val, density, i, j, k, x + 1, y, z + 1);
                }
                else if (x < 31 && z == 31)
                {
                    ChangeNeighbor(val, density, i, j, k + 1, x + 1, y, 0);
                }
                else if (x == 31 && z < 31)
                {
                    ChangeNeighbor(val, density, i + 1, j, k, 0, y, z + 1);
                }
                else
                {
                    ChangeNeighbor(val, density, i + 1, j, k + 1, 0, y, 0);
                }

                val = 1;
                if (y < 31 && z < 31)
                {
                    ChangeNeighbor(val, density, i, j, k, x, y + 1, z + 1);
                }
                else if (y < 31 && z == 31)
                {
                    ChangeNeighbor(val, density, i, j, k + 1, x, y + 1, 0);
                }
                else if (y == 31 && z < 31)
                {
                    ChangeNeighbor(val, density, i, j + 1, k, x, 0, z + 1);
                }
                else
                {
                    ChangeNeighbor(val, density, i, j + 1, k + 1, x, 0, 0);
                }

                val = 0;
                if (x < 31 && y < 31 && z < 31)
                {
                    ChangeNeighbor(val, density, i, j, k, x + 1, y + 1, z + 1);
                }
                else if (x < 31 && y < 31 && z == 31)
                {
                    ChangeNeighbor(val, density, i, j, k + 1, x + 1, y + 1, 0);
                }
                else if (x < 31 && y == 31 && z < 31)
                {
                    ChangeNeighbor(val, density, i, j + 1, k, x + 1, 0, z + 1);
                }
                else if (x == 31 && y < 31 && z < 31)
                {
                    ChangeNeighbor(val, density, i + 1, j, k, 0, y + 1, z + 1);
                }
                else if (x < 31 && y == 31 && z == 31)
                {
                    ChangeNeighbor(val, density, i, j + 1, k + 1, x + 1, 0, 0);
                }
                else if (x == 31 && y < 31 && z == 31)
                {
                    ChangeNeighbor(val, density, i + 1, j, k + 1, 0, y + 1, 0);
                }
                else if (x == 31 && y == 31 && z < 31)
                {
                    ChangeNeighbor(val, density, i + 1, j + 1, k, 0, 0, z + 1);
                }
                else
                {
                    ChangeNeighbor(val, density, i + 1, j + 1, k + 1, 0, 0, 0);
                }

                break;
            case 7:
                val = 6;
                if (x > 0)
                {
                    ChangeNeighbor(val, density, i, j, k, x - 1, y, z);
                }
                else
                {
                    ChangeNeighbor(val, density, i - 1, j, k, 31, y, z);
                }

                val = 3;
                if (y < 31)
                {
                    ChangeNeighbor(val, density, i, j, k, x, y + 1, z);
                }
                else
                {
                    ChangeNeighbor(val, density, i, j + 1, k, x, 0, z);
                }

                val = 4;
                if (z < 31)
                {
                    ChangeNeighbor(val, density, i, j, k, x, y, z + 1);
                }
                else
                {
                    ChangeNeighbor(val, density, i, j, k + 1, x, y, 0);
                }

                val = 2;
                if (x > 0 && y < 31)
                {
                    ChangeNeighbor(val, density, i, j, k, x - 1, y + 1, z);
                }
                else if (x > 0 && y == 31)
                {
                    ChangeNeighbor(val, density, i, j + 1, k, x - 1, 0, z);
                }
                else if (x == 0 && y < 31)
                {
                    ChangeNeighbor(val, density, i - 1, j, k, 31, y + 1, z);
                }
                else
                {
                    ChangeNeighbor(val, density, i - 1, j + 1, k, 31, 0, z);
                }

                val = 5;
                if (x > 0 && z < 31)
                {
                    ChangeNeighbor(val, density, i, j, k, x - 1, y, z + 1);
                }
                else if (x > 0 && z == 31)
                {
                    ChangeNeighbor(val, density, i, j, k + 1, x - 1, y, 0);
                }
                else if (x == 0 && z < 31)
                {
                    ChangeNeighbor(val, density, i - 1, j, k, 31, y, z + 1);
                }
                else
                {
                    ChangeNeighbor(val, density, i - 1, j, k + 1, 31, y, 0);
                }

                val = 0;
                if (y < 31 && z < 31)
                {
                    ChangeNeighbor(val, density, i, j, k, x, y + 1, z + 1);
                }
                else if (y < 31 && z == 31)
                {
                    ChangeNeighbor(val, density, i, j, k + 1, x, y + 1, 0);
                }
                else if (y == 31 && z < 31)
                {
                    ChangeNeighbor(val, density, i, j + 1, k, x, 0, z + 1);
                }
                else
                {
                    ChangeNeighbor(val, density, i, j + 1, k + 1, x, 0, 0);
                }

                val = 1;
                if (x > 0 && y < 31 && z < 31)
                {
                    ChangeNeighbor(val, density, i, j, k, x - 1, y + 1, z + 1);
                }
                else if (x > 0 && y < 31 && z == 31)
                {
                    ChangeNeighbor(val, density, i, j, k + 1, x - 1, y + 1, 0);
                }
                else if (x > 0 && y == 31 && z < 31)
                {
                    ChangeNeighbor(val, density, i, j + 1, k, x - 1, 0, z + 1);
                }
                else if (x == 0 && y < 31 && z < 31)
                {
                    ChangeNeighbor(val, density, i - 1, j, k, 31, y + 1, z + 1);
                }
                else if (x > 0 && y == 31 && z == 31)
                {
                    ChangeNeighbor(val, density, i, j + 1, k + 1, x - 1, 0, 0);
                }
                else if (x == 0 && y < 31 && z == 31)
                {
                    ChangeNeighbor(val, density, i - 1, j, k + 1, 31, y + 1, 0);
                }
                else if (x == 0 && y == 31 && z < 31)
                {
                    ChangeNeighbor(val, density, i - 1, j + 1, k, 31, 0, z + 1);
                }
                else
                {
                    ChangeNeighbor(val, density, i - 1, j + 1, k + 1, 31, 0, 0);
                }

                break;
        }
    }
}