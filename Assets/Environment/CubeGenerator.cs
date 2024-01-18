using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CubeGenerator : MonoBehaviour
{
    [SerializeField] private int x_start = -5;
    [SerializeField] private int z_start = -5;
    [SerializeField] private int length = 10;

    [SerializeField] private bool useMyPerlin = false;
    [SerializeField] private float ampl = 2.0f;

    [SerializeField] private GameObject shpere;

    private float HermiteInterpolation(float t) { return t * t * t * (t * (t * 6 - 15) + 10); }
    private float lerp(float t, float a, float b) {  return a + t * (b - a); }

    private Vector3 GenerateHash(Vector3 v)
    {

        //this cannot convert the binary itself to uint
        //rather converts the value of the binary to uint
        //therefore it does not work, I may have to make my own floatToBits function
        uint x = BitConverter.ToUInt32(BitConverter.GetBytes(v.x));
        uint y = BitConverter.ToUInt32(BitConverter.GetBytes(v.y));
        uint z = BitConverter.ToUInt32(BitConverter.GetBytes(v.z));

        x ^= (y << 1);
        y ^= (z << 2);
        z ^= (x << 3);

        x ^= y >> 1;
        y ^= z >> 2;
        z ^= x >> 3;

        x *= 0x456789abu;
        y *= 0x6789ab45u;
        z *= 0x89ab4567u;

        x ^= y << 1;
        y ^= z << 2;
        z ^= x << 3;

        x *= 0x456789abu;
        y *= 0x6789ab45u;
        z *= 0x89ab4567u;

        v = new Vector3(x, y, z);
        v.x /= 0xffffffffu;
        v.y /= 0xffffffffu;
        v.z /= 0xffffffffu;

        return v;
    }

    private float CalculateGradient(Vector3 lattice,Vector3 p)
    {

        uint ind = (uint)UnityEngine.Random.Range(0.0f, 256.0f) & 14;

        UnityEngine.Debug.Log(ind);

        float u = ind < 8 ? p.x : p.y;
        float v = ind < 4 ? p.y : ind == 12 || ind == 14 ? p.x : p.z;

        return((ind & 1) == 0 ? u : -u) + ((ind & 2) == 0 ? v : -v);

    }
    private float GeneratePerlinNoise(Vector3 position)
    {
        Vector3 lattice = new Vector3(Mathf.Floor(position.x),
                                      Mathf.Floor(position.y),
                                      Mathf.Floor(position.z));

        //relative position to the lattice
        Vector3 relativePosition = new Vector3(position.x - lattice.x, 
                                               position.y - lattice.y, 
                                               position.z - lattice.z);

        float[] v = new float[8];

        for (int k = 0; k < 2; k++)
        {
            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < 2; i++)
                {
                    v[i + 2 * j + 4 * k] = CalculateGradient((lattice + new Vector3(i, j, k)), 
                                                             (relativePosition - new Vector3(i, j, k))) * 0.70710678f;
                }

            }
        }

        relativePosition.x = HermiteInterpolation(relativePosition.x);
        relativePosition.y = HermiteInterpolation(relativePosition.y);
        relativePosition.z = HermiteInterpolation(relativePosition.z);
        
        float[] w = new float[2];

        for (int i = 0; i < 2; i++)
        {
            w[i] = lerp(lerp(v[4 * i], v[4 * i + 1], relativePosition.x), lerp(v[4 * i + 2], v[4 * i + 3], relativePosition.x), relativePosition.y);
        }

        return 0.5f * lerp(w[0], w[1], relativePosition.z) + 0.5f;

    }

    float CalculateDistanceFromSphere(Vector2 position, float r)
    {
        return position.magnitude - r;
    }


    void GenerateCube(int x, int z)
    {
        float offset = UnityEngine.Random.Range(0.0f, 256.0f) * ampl;
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        Material mat = cube.GetComponent<Renderer>().material;
        mat.color = Color.gray;

        float pvalue;

        if (useMyPerlin) pvalue = GeneratePerlinNoise(new Vector3(x, z, UnityEngine.Random.Range(0.0f, 10.0f)));
        else pvalue = Mathf.PerlinNoise(x + offset, z);

        cube.transform.position = new Vector3(x, pvalue, z);
        cube.transform.SetParent(transform);
    }

    private void Awake()
    {
        
        for(int x = x_start; x <= x_start + length; x++)
        {
            for (int z = z_start; z <= z_start + length; z++)
            { 
                bool flag = true;
                for(int i = 0; i < 4; i++)
                {
                    float sx = shpere.transform.GetChild(i).gameObject.transform.position.x;
                    float sz = shpere.transform.GetChild(i).gameObject.transform.position.z;

                    if(CalculateDistanceFromSphere(new Vector2(sx - x, sz - z), 12.0f) < 0) flag = false;
                }
                
                if(flag) GenerateCube(x, z);
            }
            
        }

       

    }
}
