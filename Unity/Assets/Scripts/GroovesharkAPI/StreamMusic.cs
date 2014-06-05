using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Text;
using System.IO;
using System.Threading;

public class RequestState
{
    const int BufferSize = 1024;
    //public StringBuilder RequestData;
    public MemoryStream RequestData;
    public byte[] BufferRead;
    public WebRequest Request;
    public Stream ResponseStream;
    // Create Decoder for appropriate enconding type.
    public Decoder StreamDecode = Encoding.UTF8.GetDecoder();

    public RequestState()
    {
        BufferRead = new byte[BufferSize];
        //RequestData = new StringBuilder(String.Empty);
        RequestData = new MemoryStream();
        Request = null;
        ResponseStream = null;
    }
}

public class StreamMusic {

    private Uri _HttpSite;
    private const int BUFFER_SIZE = 1024;

    public static ManualResetEvent allDone = new ManualResetEvent(false);

    public StreamMusic(string url)
    {
        _HttpSite = new Uri(url);
    }

    public void Start()
    {
        WebRequest wreq = WebRequest.Create(_HttpSite);
        RequestState rs = new RequestState();
        rs.Request = wreq;

        IAsyncResult r = (IAsyncResult)wreq.BeginGetResponse(new AsyncCallback(RespCallback), rs);

        //allDone.WaitOne();
    }

    private void RespCallback(IAsyncResult ar)
    {
        RequestState rs = (RequestState)ar.AsyncState;

        WebRequest req = rs.Request;

        WebResponse resp = req.EndGetResponse(ar);

        Stream responseStream = resp.GetResponseStream();

        rs.ResponseStream = responseStream;

        IAsyncResult iarRead = responseStream.BeginRead(rs.BufferRead, 0, BUFFER_SIZE, new AsyncCallback(ReadCallback), rs);
    }

    private void ReadCallback(IAsyncResult asyncResult)
    {
        RequestState rs = (RequestState)asyncResult.AsyncState;

        Stream responseStream = rs.ResponseStream;

        int read = responseStream.EndRead(asyncResult);
        if (read > 0)
        {
            //byte[] charBuffer = new byte[BUFFER_SIZE];
            //int len = rs.ResponseStream.Read(rs.BufferRead, 0, BUFFER_SIZE);
            //int len = rs.StreamDecode.GetChars(rs.BufferRead, 0, read, charBuffer, 0);

            Debug.Log(read + " received");

            //string str = new string(charBuffer, 0, len);
            //rs.RequestData.Append(Encoding.UTF8.GetString(rs.BufferRead, 0, read));
            rs.RequestData.Write(rs.BufferRead, 0, read);

            IAsyncResult ar = responseStream.BeginRead(rs.BufferRead, 0, BUFFER_SIZE, new AsyncCallback(ReadCallback), rs);
        }
        else
        {
            if (rs.RequestData.Length > 0)
            {
                //string strContent;
                //strContent = rs.RequestData.ToString();
                Debug.Log("All done (" + rs.RequestData.Length + " downloaded)");

                rs.RequestData.Position = 0;
                using (FileStream file = new FileStream("test.mp3", FileMode.Create, System.IO.FileAccess.Write))
                {
                    byte[] bytes = new byte[rs.RequestData.Length];
                    rs.RequestData.Read(bytes, 0, (int)rs.RequestData.Length);
                    file.Write(bytes, 0, bytes.Length);
                    rs.RequestData.Close();
                }

                //TextWriter tw = new StreamWriter("test.mp3");
                //tw.Write(strContent);
                //tw.Close();
            }
            responseStream.Close();
            allDone.Set();
        }
        return;
    }

}
