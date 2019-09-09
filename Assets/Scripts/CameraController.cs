using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public GameObject player;
    public float smoothTime;

    private Vector3 offset;
    private Vector3 velocity;

    // Use this for initialization
    void Start()
    {
        offset = transform.position - player.transform.position;
        velocity = Vector3.zero;
    }


    void FixedUpdate()
    {
        Vector3 targetPos = player.transform.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
    }

    public void CamShake(float shakeTimer, float shakeIntensity)
    {
        if (shakeTimer > 0)
        {
            transform.localPosition = transform.position + Random.insideUnitSphere * shakeIntensity;

            shakeTimer -= Time.deltaTime;
        }
    }
}
