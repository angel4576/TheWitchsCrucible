using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavOrigin : MonoBehaviour
{
    private void Awake()
    {
    }

    private void Start()
    {
        NavManager.Instance?.SetNavOrigin(transform);
        NavManager.Instance?.GenerateNavMesh();
        
    }
}
