using UnityEngine;

public class QuadAutofit : MonoBehaviour
{
    private Camera mainCamera; // Reference to the main camera

    void Start()
    {
        // Get the main camera
        mainCamera = Camera.main;
        // Adjust the Quad size to fit the camera view
        AdjustQuadSize();
    }

    void AdjustQuadSize()
    {
        // Get the camera's orthographic size and aspect ratio
        float cameraHeight = mainCamera.orthographicSize * 2;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        // Set the Quad's scale to match the camera size
        transform.localScale = new Vector3(cameraWidth, cameraHeight, 1);

        // Position the Quad directly in front of the camera
        transform.position = mainCamera.transform.position + mainCamera.transform.forward * (mainCamera.nearClipPlane + 0.01f);
    }

    // void Update()
    // {
    //     // Adjust the Quad size if the screen size changes (optional)
    //     AdjustQuadSize();
    // }
}
