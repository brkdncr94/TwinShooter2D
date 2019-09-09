using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserController2D : MonoBehaviour {

    public string aimButton;
    public float rotationSpeed = 1.0f;

    private LineRenderer line;

    // Use this for initialization
    void Start()
    {
        line = gameObject.GetComponent<LineRenderer>();

        //line.enabled = false;
        line.startWidth = 0.08f;
        line.endWidth = 0.04f;
    }

    private void LateUpdate()
    {
        Vector3[] positions = new Vector3[] { transform.position, (transform.position + (transform.up) * 15) };
        line.SetPositions(positions);

        float rotation = Input.GetAxis(aimButton);
        transform.Rotate(Vector3.back, rotationSpeed * rotation, Space.World);
    }
}
