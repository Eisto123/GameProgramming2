using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public class MidTerrainSetting
{
    public int mDivisions;
    public float mSize;
    public float mHeight;
}

public class MidPointTerrain_2 : MonoBehaviour
{
    public MidTerrainSetting settings;

    Vector3[] mVerts;
    int mVertCount;

    private void Start()
    {
        CreateMidTerrain();
    }

    void CreateMidTerrain()
    {
        int mDivisions = settings.mDivisions;
        float mSize = settings.mSize;
        float mHeight = settings.mHeight;

        //vertex array
        mVertCount = (mDivisions + 1) * (mDivisions + 1);
        mVerts = new Vector3[mVertCount];
        Vector2[] uvs = new Vector2[mVertCount];
        int[] tris = new int[mDivisions * mDivisions * 2 * 3];

        float halfSize = mSize * 0.5f;
        float divisionSize = mSize / mDivisions;

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        //build up triangles
        int triOffset = 0;

        for (int i = 0; i <= mDivisions; i++)
        {
            for (int j = 0; j <= mDivisions; j++)
            {
                mVerts[i * (mDivisions + 1) + j] = new Vector3(-halfSize + j * divisionSize, 0.0f, halfSize - i * divisionSize);
                uvs[i * (mDivisions + 1) + j] = new Vector2((float)i / mDivisions, (float)j / mDivisions);

                if (i < mDivisions && j < mDivisions)
                {
                    int topleft = i * (mDivisions + 1) + j;
                    int botleft = (i + 1) * (mDivisions + 1) + j;

                    //first triangle
                    tris[triOffset] = topleft;
                    tris[triOffset + 1] = topleft + 1;
                    tris[triOffset + 2] = botleft + 1;

                    //second triangle
                    tris[triOffset + 3] = topleft;
                    tris[triOffset + 4] = botleft + 1;
                    tris[triOffset + 5] = botleft;

                    triOffset += 6;
                }
            }
        }

        //set the 4 corners of the array to initial value
        mVerts[0].y = Random.Range(-mHeight, mHeight);
        mVerts[mDivisions].y = Random.Range(-mHeight, mHeight);
        mVerts[mVerts.Length - 1].y = Random.Range(-mHeight, mHeight);
        mVerts[mVerts.Length - 1 - mDivisions].y = Random.Range(-mHeight, mHeight);

        //iteration loop
        int iterations = (int)Mathf.Log(mDivisions, 2);
        int numSquares = 1;
        int squareSize = mDivisions;

        for (int i = 0; i < iterations; i++)
        {
            int row = 0;
            for (int j = 0; j < numSquares; j++)
            {
                int col = 0;
                for (int k = 0; k < numSquares; k++)
                {
                    //call diamond square
                    DiamondSquare(row, col, squareSize, mHeight);

                    col += squareSize;

                }
                row += squareSize;
            }
            numSquares *= 2;
            squareSize /= 2;
            mHeight *= 0.5f;//height go down speed
        }

        //setup mesh
        mesh.vertices = mVerts;
        mesh.uv = uvs;
        mesh.triangles = tris;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    void DiamondSquare(int row, int col, int size, float offset)
    {
        //diamond step
        //set the midpoint of that square to be the average of the four corner points plus a random value.
        int halfSize = (int)(size * 0.5f);
        int mDivisions = settings.mDivisions;
        int topleft = row * (mDivisions + 1) + col;
        int botleft = (row + size) * (mDivisions + 1) + col;

        int mid = (int)(row + halfSize) * (mDivisions + 1) + (int)(col + halfSize);
        mVerts[mid].y = (mVerts[topleft].y + mVerts[topleft + size].y + mVerts[botleft].y + mVerts[botleft + size].y) * 0.25f + Random.Range(-offset, offset);

        //square step
        //For each diamond in the array, set the midpoint of that diamond to be the average of the four corner points plus a random value.
        mVerts[topleft + halfSize].y = (mVerts[topleft].y + mVerts[topleft + size].y + mVerts[mid].y) / 3 + Random.Range(-offset, offset);
        mVerts[mid - halfSize].y = (mVerts[topleft].y + mVerts[botleft].y + mVerts[mid].y) / 3 + Random.Range(-offset, offset);
        mVerts[mid + halfSize].y = (mVerts[topleft + size].y + mVerts[botleft + size].y + mVerts[mid].y) / 3 + Random.Range(-offset, offset);
        mVerts[botleft + halfSize].y = (mVerts[botleft].y + mVerts[botleft + size].y + mVerts[mid].y) / 3 + Random.Range(-offset, offset);
    }


}