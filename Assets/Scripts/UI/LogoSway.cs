using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogoSway : MonoBehaviour
{

    [SerializeField]  private float rotationSpeed;  //Speed variable used to control the animation

    [SerializeField] private float rotationOffset;

    [SerializeField] private float startTime;

    void OnEnable()
    {
        startTime = Time.unscaledTime;
        Debug.Log("set start time");
    }

    void Update()
    {
        Vector3 rotatedEulerAngle = gameObject.transform.eulerAngles;
        rotatedEulerAngle.z = Mathf.Sin(Time.unscaledTime - startTime) * rotationSpeed * rotationOffset;
        gameObject.transform.eulerAngles = rotatedEulerAngle;

        //finalAngle = startAngle.y + Mathf.Sin(Time.fixedTime * rotationSpeed) * rotationOffset;  //Calculate animation angle
        //transform.eulerAngles = new Vector3(startAngle.x, finalAngle, startAngle.z); //Apply new angle to object
    }
}
