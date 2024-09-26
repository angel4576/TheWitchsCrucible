using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;
    [SerializeField] private float globalShakeForce = 1f;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;  // Reference to your Cinemachine camera

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void CamereaShake(CinemachineImpulseSource impulseSource)
    {
        impulseSource.GenerateImpulseWithForce(globalShakeForce);
    }

    public void ChangeFOV(float newFOV)
    {
        if (virtualCamera != null)
        {
            virtualCamera.m_Lens.FieldOfView = newFOV;
        }
        else
        {
            Debug.LogWarning("Virtual Camera is not assigned in CameraManager.");
        }
    }
}
