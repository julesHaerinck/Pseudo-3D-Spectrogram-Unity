using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform Target;
    public float     Speed;

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(Target);
        transform.Translate(Vector3.right * Speed * Time.deltaTime);
    }
}
