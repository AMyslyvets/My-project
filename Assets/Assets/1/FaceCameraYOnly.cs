using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCameraYOnly : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float yAngleOffset = 0f;

    private void LateUpdate()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
            return;

        Vector3 dir = targetCamera.transform.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
            return;

        Quaternion lookRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
        Quaternion offsetRot = Quaternion.Euler(0f, yAngleOffset, 0f);

        transform.rotation = lookRot * offsetRot;
    }
}