using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunRotation : MonoBehaviour
{
    [Header("Inner Sun")]
    public float rotateSpeed1;
    
    [Header("Outer Sun")]
    public float rotateSpeed2;
    private Transform sun3;
    private Transform sun4;
    
    // Start is called before the first frame update
    void Start()
    {
        sun3 = transform.Find("Sun_3");
        sun4 = transform.Find("Sun_4");
    }

    // Update is called once per frame
    void Update()
    {
        sun3.Rotate(0f, 0f, rotateSpeed1 * Time.deltaTime);
        sun4.Rotate(0f, 0f, rotateSpeed2 * Time.deltaTime);
    }
}
