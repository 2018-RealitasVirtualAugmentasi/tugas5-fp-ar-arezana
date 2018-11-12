using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
using System.IO.Pipes;
using System.Diagnostics;
using System.Threading;
using System.Security.Principal;
using System.Text;

public class PipeServer : MonoBehaviour {
    public static NamedPipeServerStream pipeServer =
            new NamedPipeServerStream("testpipe", PipeDirection.InOut);
    public static string msgs;
    Process pipeClient = new Process();

    public static void ServerThread()
    {
        print("Waiting client to connect\n");
        // Wait for a client to connect
        pipeServer.WaitForConnection();

        print("Connected\n");
        StreamString ss = new StreamString(pipeServer);

        while (true)
        {
            string messages = "";

            if (ss.ReadString(ref messages))
            {
                //Console.WriteLine("Read : " + messages);
                print("Read : " + messages);
            }
        }
    }

    // Use this for initialization
    void Start () {
        print("Initialize Pipe Server\n");
        ThreadStart childref = new ThreadStart(ServerThread);
        Thread childThread = new Thread(childref);
        childThread.IsBackground = true;
        childThread.Start();

        pipeClient.StartInfo.FileName = "D:/Reza/Dokumen/Visual studio project/WebCam_C_sharp/WebCam_C_sharp/bin/Debug/WebCam_C_sharp.exe";
        pipeClient.Start();
    }

    // Update is called once per frame
    void Update () {
		
	}

    private void OnApplicationQuit()
    {
        pipeServer.Close();
        pipeClient.Kill();
        print("Close");
    }
}

public class StreamString
{
    private Stream ioStream;
    private UnicodeEncoding streamEncoding;

    public StreamString(Stream ioStream)
    {
        this.ioStream = ioStream;
        streamEncoding = new UnicodeEncoding();
    }

    public bool ReadString(ref string msg)
    {
        Console.WriteLine("read");
        int len;
        len = ioStream.ReadByte() * 256;
        len += ioStream.ReadByte();
        Console.WriteLine("Len: " + len);
        if (len > 0)
        {
            byte[] inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);

            msg = streamEncoding.GetString(inBuffer);

            return true;
        }
        else
        {
            msg = "";
            return false;
        }
    }

    public int WriteString(string outString)
    {
        byte[] outBuffer = streamEncoding.GetBytes(outString);
        int len = outBuffer.Length;
        if (len > UInt16.MaxValue)
        {
            len = (int)UInt16.MaxValue;
        }
        ioStream.WriteByte((byte)(len / 256));
        ioStream.WriteByte((byte)(len & 255));
        ioStream.Write(outBuffer, 0, len);
        ioStream.Flush();

        return outBuffer.Length + 2;
    }
}
