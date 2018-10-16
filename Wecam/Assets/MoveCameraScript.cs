using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCameraScript : MonoBehaviour
{
    public float mSpeed;

    // Use this for initialization
    void Start()
    {
        mSpeed = 20f;
    }

    float inputX, inputZ;
    // Update is called once per frame
    void Update()
    {
        inputX = mSpeed * Input.GetAxis("Horizontal");
        inputZ = mSpeed * Input.GetAxis("Vertical");

        if (inputX != 0)
            rotateLeftRight();
        if (inputZ != 0)
            rotateUpDown();
    }

    private void move()
    {
        transform.position += transform.forward * inputZ * Time.deltaTime;
    }

    private void rotateLeftRight()
    {
        transform.Rotate(new Vector3(0f, inputX * Time.deltaTime, 0f));
    }

    private void rotateUpDown()
    {
        transform.Rotate(new Vector3(-inputZ * Time.deltaTime, 0f, 0f));
    }
}