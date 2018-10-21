using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerialComm : MonoBehaviour {
    float[] lastRot = { 0, 0, 0 };
    float lastRoll = 0;
    float lastPitch = 0;

    public Transform obj_roll;
    public Transform arrow_roll;
    public Transform obj_pitch;
    public Transform arrow_pitch;

    float yaw = 0;
    float pitch = 0;
    float roll = 0;
    
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
                yaw = float.Parse(vec3[1]);
                pitch = float.Parse(vec3[2]);
                roll = float.Parse(vec3[3]);

                //print(yaw + ", " + pitch + ", " + roll);

                float difYaw = yaw - lastRot[0];
                float difPitch = pitch - lastRot[1];
                float difRoll = roll - lastRot[2];

                rotateCamera(difYaw, difPitch, difRoll);
                
                lastRot[0] = transform.rotation.eulerAngles.y;
                lastRot[1] = transform.rotation.eulerAngles.x;
                lastRot[2] = transform.rotation.eulerAngles.z;

                float hudRoll = roll - lastRoll;
                rotateRoll(hudRoll);

                lastRoll = roll;

                float mvPitch = ((pitch + 180) / 360 * 36) - 18;
                float hudpitch = mvPitch - lastPitch;
                movePitch(hudpitch);

                lastPitch = arrow_pitch.position.y;
                //Debug.Log("hud: " + hudRoll + ", roll: " + roll + ", lastRoll: " + lastRoll);
                //Debug.Log("art: " + difYaw + ", " + difPitch + ", " + difRoll);
                Debug.Log("ptc: " + mvPitch + ", " + pitch + ", " + lastPitch + ", " + hudpitch);
            }
        }
    }

    void rotateCamera(float dy, float dp, float dr)
    {
        transform.Rotate(new Vector3(dp, dy, dr), Space.Self);
        obj_roll.Rotate(new Vector3(dp, dy, dr), Space.Self);
        obj_pitch.Rotate(new Vector3(dp, dy, dr), Space.Self);
    }

    void rotateRoll(float vroll)
    {
        arrow_roll.Rotate(new Vector3(0f, 0f, vroll), Space.Self);
        arrow_pitch.Rotate(new Vector3(0f, 0f, vroll), Space.Self);
    }

    void movePitch(float dpitch)
    {
        arrow_pitch.Translate(new Vector3(0f, dpitch, 0f), Space.Self);
        //obj_pitch.transform.position = new Vector3(0.0f, mvPitch, 15.0f);
        //obj_pitch.position.Set(0f, mvPitch, 0f);
    }

    void OnConnectionEvent(bool success)
    {
        if (success)
            Debug.Log("Connection established");
        else
            Debug.Log("Connection attempt failed or disconnection detected");
    }
}
