using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ScreenCorrosionEffect : MonoBehaviour
{
    public Material material;
    void Start()
    {
        
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest) {
        Graphics.Blit(src, dest, material, 0);    
    }

}
