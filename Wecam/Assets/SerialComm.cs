using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerialComm : MonoBehaviour {
    float[] lastRot = { 0, 0, 0 };
    float difYaw = 0;
    float difPitch = 0;
    float difRoll = 0;

    void OnMessageArrived(string msg)
    {
        //print("Message arrived: " + msg);
        string[] vec3 = msg.Split(':');

        //Debug.Log("Header: " + vec3[0]);
        if (vec3[0].Equals("AraTa"))
        {
            if (vec3[1] != "" && vec3[2] != "" && vec3[3] != "")
            {
                //print("Masuk");
                //print(vec3[1] + ", " + vec3[2] + ", " + vec3[3]);
                float yaw = float.Parse(vec3[1]);
                float pitch = float.Parse(vec3[2]);
                float roll = float.Parse(vec3[3]);

                print(yaw + ", " + pitch + ", " + roll);

                difYaw = yaw - lastRot[0];
                difPitch = pitch - lastRot[1];
                difRoll = roll - lastRot[2];

                rotateCamera(difYaw, difPitch, difRoll);

                lastRot[0] = transform.rotation.eulerAngles.y;
                lastRot[1] = transform.rotation.eulerAngles.x;
                lastRot[2] = transform.rotation.eulerAngles.z;
                //Debug.Log("art: " + difYaw + ", " + difPitch + ", " + difRoll);
            }
        }
    }

    void rotateCamera(float dy, float dp, float dr)
    {
        transform.Rotate(new Vector3(dp, dy, dr), Space.Self);
        //if (Mathf.Abs(difYaw) > 0.1) transform.Rotate(new Vector3(0f, dy, 0f), Space.Self);
        //if (Mathf.Abs(difPitch) > 0.1) transform.Rotate(new Vector3(dp, 0f, 0f), Space.Self);
        //if (Mathf.Abs(difRoll) > 0.1) transform.Rotate(new Vector3(0f, 0f, dr), Space.Self);
    }

    void OnConnectionEvent(bool success)
    {
        if (success)
            Debug.Log("Connection established");
        else
            Debug.Log("Connection attempt failed or disconnection detected");
    }
}
