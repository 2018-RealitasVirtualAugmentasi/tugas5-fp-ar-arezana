using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class FligthController : MonoBehaviour {
    public static string flight_data = "";

    float[] lastRot = { 0, 0, 0 };
    float lastRoll = 0;
    float lastPitch = 0;

    public Transform plane;
    public Transform obj_roll;
    public Transform arrow_roll;
    public Transform obj_pitch;
    public Transform arrow_pitch;
    public Transform pin;
    public Transform direction;
    public Transform arrow_direction;

    public Text altitude_text;
    public Text distance_text;

    float yaw = 0;
    float pitch = 0;
    float roll = 0;

    float altitude = 0;
    float longitude = 0;
    float latitude = 0;

    public float target_long;
    public float target_lat;

    public static void ServerThread()
    {
        UdpClient udpServer = new UdpClient(10000);

        while (true)
        {
            var remoteEP = new IPEndPoint(IPAddress.Any, 10000);
            var data = udpServer.Receive(ref remoteEP);
            //print("receive data from " + remoteEP.ToString());

            string message = Encoding.ASCII.GetString(data);
            //print("Messages " + message);
            //byte[] send_buffer = Encoding.ASCII.GetBytes(message);
            //udpServer.Send(send_buffer, send_buffer.Length, remoteEP);

            flight_data = message;
        }
    }

    // Use this for initialization
    void Start () {
        ThreadStart childref = new ThreadStart(ServerThread);
        Thread childThread = new Thread(childref);
        childThread.IsBackground = true;
        childThread.Start();
    }
	
	// Update is called once per frame
	void Update () {
        //print("Message arrived: " + msg);

        string[] alldata = flight_data.Split(':');

        //print("Header: " + alldata[4]);
        if (alldata[0].Equals("AraTa"))
        {
            if (alldata[1] != "" && alldata[2] != "" && alldata[3] != "")
            {
                //print("Masuk");
                //print(vec3[1] + ", " + vec3[2] + ", " + vec3[3]);
                yaw = float.Parse(alldata[1]);
                pitch = float.Parse(alldata[2]) * -1;
                roll = float.Parse(alldata[3]);
                altitude = float.Parse(alldata[4]);
                longitude = float.Parse(alldata[5]);
                latitude = float.Parse(alldata[6]);

                string alt_text = "Altitude: " + altitude.ToString() + " m";
                altitude_text.text = alt_text;
                movePlane(altitude);
                //print(alt_text);
                //print(altitude + ", " + longitude + ", " + latitude);
                //print(yaw + ", " + pitch + ", " + roll);

                double distanceTo = DistanceTo(latitude, longitude, target_lat, target_long, 'K') * 1000;
                double targetHeading = angleFromCoordinate(latitude, longitude, target_lat, target_long);

                double targetHeadingRad = targetHeading * Math.PI / 180;
                double pos_pin_x = Math.Sin(targetHeadingRad) * distanceTo;
                double pos_pin_y = Math.Cos(targetHeadingRad) * distanceTo;
                pin.transform.position = new Vector3((float)pos_pin_x, 0f, (float)pos_pin_y);

                rotateDirection(yaw, (float)targetHeading);

                string dst_text = string.Format("{0:F2}", distanceTo) + " m";
                distance_text.text = dst_text;
                //print(targetHeading + ", " + targetHeadingRad + ", " + distanceTo + ", " + pos_pin_x + ", " + pos_pin_y + ", " + latitude + ", " + longitude);

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

                float mvPitch = pitch / 10;
                float hudpitch = mvPitch - lastPitch;
                movePitch(hudpitch, roll);

                lastPitch = arrow_pitch.position.y;
                //Debug.Log("hud: " + hudRoll + ", roll: " + roll + ", lastRoll: " + lastRoll);
                //Debug.Log("art: " + difYaw + ", " + difPitch + ", " + difRoll);
                //Debug.Log("ptc: " + mvPitch + ", " + pitch + ", " + lastPitch + ", " + hudpitch);
            }
        }
    }

   private double DistanceTo(double lat1, double lon1, double lat2, double lon2, char unit = 'K')
   {
        double rlat1 = Math.PI * lat1 / 180;
        double rlat2 = Math.PI * lat2 / 180;
        double theta = lon1 - lon2;
        double rtheta = Math.PI * theta / 180;
        double dist =
            Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) *
            Math.Cos(rlat2) * Math.Cos(rtheta);
        dist = Math.Acos(dist);
        dist = dist * 180 / Math.PI;
        dist = dist * 60 * 1.1515;

        switch (unit)
        {
            case 'K': //Kilometers -> default
                return dist * 1.609344;
            case 'N': //Nautical Miles 
                return dist * 0.8684;
            case 'M': //Miles
                return dist;
        }

        return dist;
    }

    private double angleFromCoordinate(double lat1, double long1, double lat2, double long2)
    {

        double dLon = (long2 - long1);

        double y = Math.Sin(dLon) * Math.Cos(lat2);
        double x = Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1)
                * Math.Cos(lat2) * Math.Cos(dLon);

        double brng = Math.Atan2(y, x);

        brng = toDegrees(brng);
        brng = (brng + 360) % 360;
        if (brng > 180) brng = brng - 360;
        //brng = 360 - brng; // count degrees counter-clockwise - remove to make clockwise

        return brng;
    }

    private double toDegrees(double radians)
    {
        double degrees = (180 / Math.PI) * radians;
        return (degrees);
    }

    void movePlane(float altitude)
    {
        Vector3 prevAlt = plane.transform.position;
        plane.transform.position = new Vector3(prevAlt.x, altitude, prevAlt.z);
    }

    void rotateDirection(float heading, float target_heading)
    {
        float dHeading = target_heading - heading;
        float prevHeading = arrow_direction.eulerAngles.y;
        if (prevHeading > 180) prevHeading = prevHeading - 360;
        float dy = dHeading- prevHeading;
        arrow_direction.Rotate(new Vector3(0f, dy, 0f), Space.Self);

        //Quaternion prev = arrow_direction.rotation;
        //Quaternion newQ = new Quaternion(prev.x, prev.y+2, prev.z, prev.w);
        //arrow_direction.rotation = newQ;
        print(heading + ", " + target_heading + ", " + dHeading + ", " + prevHeading + ", " + dy);
    }

    void rotateCamera(float dy, float dp, float dr)
    {
        transform.Rotate(new Vector3(dp, dy, dr), Space.Self);
        obj_roll.Rotate(new Vector3(dp, dy, dr), Space.Self);
        obj_pitch.Rotate(new Vector3(dp, dy, dr), Space.Self);
        direction.Rotate(new Vector3(dp, dy, dr), Space.Self);
    }

    void rotateRoll(float vroll)
    {
        arrow_roll.Rotate(new Vector3(0f, 0f, vroll), Space.Self);
        arrow_pitch.Rotate(new Vector3(0f, 0f, vroll), Space.Self);
    }

    void movePitch(float dpitch, float teta)
    {
        double rad = teta * Math.PI / 180;
        double xPitch = Math.Sin(rad) * dpitch;
        double yPitch = Math.Cos(rad) * dpitch;
        obj_pitch.Translate(new Vector3((float)xPitch, (float)yPitch, 0f), Space.Self);
        //arrow_pitch.Translate(new Vector3(0f, dpitch, 0f), Space.Self);
        //arrow_pitch.transform.position = new Vector3(0f, 0f, 0f);
        //obj_pitch.position.Set(0f, mvPitch, 0f);
    }
}
