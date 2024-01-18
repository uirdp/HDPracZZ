using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeController : MonoBehaviour
{
    


    // Update is called once per frame
    void Update()
    {
        float t = Mathf.Sin(Time.deltaTime * 0.2f);
        transform.Translate(0, t * 2.0f, 0);        
    }
}
