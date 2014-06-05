using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Collections;
using SimpleJSON;

public struct Country
{
    public float CC1;
    public float CC2;
    public float CC3;
    public float CC4;
    public float DMA;
    public float ID;
    public float IPR;

    public override string ToString()
    {
        StringBuilder stringBuilder = new StringBuilder();

        stringBuilder.Append("\"ID\":" + ID);
        stringBuilder.Append("\"CC1\":" + CC1);
        stringBuilder.Append("\"CC2\":" + CC2);
        stringBuilder.Append("\"CC3\":" + CC3);
        stringBuilder.Append("\"CC4\":" + CC4);
        stringBuilder.Append("\"DMA\":" + DMA);
        stringBuilder.Append("\"IPR\":" + IPR);

        return stringBuilder.ToString();
    }

    public JSONObject ToJSON()
    {
        JSONObject country = new JSONObject();
        country.AddField("ID", ID);
        country.AddField("CC1", CC1);
        country.AddField("CC2", CC2);
        country.AddField("CC3", CC3);
        country.AddField("CC4", CC4);
        country.AddField("DMA", DMA);
        country.AddField("IPR", IPR);
        return country;
    }
}

public struct DownloadServer
{
    public string StreamKey;
    public string URL;
    public float StreamServerID;
    public float USecs;
}

public class Grooveshark : MonoBehaviour
{
    private readonly string _Key = "";		// can't show it
    private readonly string _Secret = "";	// can't show it
    private readonly Encoding _Encoding = Encoding.UTF8;
    private string _SessionId = string.Empty;
    private Country _Country;
    private DownloadServer _DownloadServer;
    private string _UserID = string.Empty;

    public string Login;
    public bool IsLogged;

    private const string _ApiHost = "api.grooveshark.com";
    private const string _ApiEndpoint = "/ws3.php";

    // old impl
    private JSONObject _RootLastRequest;

    private JSONNode _RootNode;

    void Start()
    {
        DontDestroyOnLoad(gameObject);

        _Country = new Country();
    }

    public IEnumerator PingService()
    {
        yield return StartCoroutine(MakeCall("pingService", null));
        Debug.Log("Ping done");
    }

    public IEnumerator StartSession()
    {
        yield return StartCoroutine(MakeCall("startSession", null, true));
        Debug.Log("StartSession done");
        Dictionary<string, object> dico = new Dictionary<string, object>();
        AccessData(_RootLastRequest, ref dico);

        _SessionId = (string)dico["sessionID"];
        Debug.Log("SessionID = " + dico["sessionID"]);
    }

    public IEnumerator GetCountry()
    {
        yield return StartCoroutine(MakeCall("getCountry", null, true));
        Debug.Log("GetCountry done");
        Dictionary<string, object> dico = new Dictionary<string, object>();
        AccessData(_RootLastRequest, ref dico);

        object ID  = dico["ID"];
        object CC1 = dico["CC1"];
        object CC2 = dico["CC2"];
        object CC3 = dico["CC3"];
        object CC4 = dico["CC4"];
        object DMA = dico["DMA"];
        object IPR = dico["IPR"];

        _Country.ID  = (float)ID;
        _Country.CC1 = (float)CC1;
        _Country.CC2 = (float)CC2;
        _Country.CC3 = (float)CC3;
        _Country.CC4 = (float)CC4;
        _Country.DMA = (float)DMA;
        _Country.IPR = (float)IPR;
    }

    public IEnumerator GetStreamKeyStreamServer(Action<string> callback, string songId)
    {
        JSONObject parameters = new JSONObject();
        parameters.AddField("songID", songId);
        parameters.AddField("country", _Country.ToJSON());
        yield return StartCoroutine(MakeCall("getStreamKeyStreamServer", parameters, true));
        Debug.Log("GetStreamKeyStreamServer done");

        Dictionary<string, object> dico = new Dictionary<string, object>();
        AccessData(_RootLastRequest, ref dico);
        _DownloadServer = new DownloadServer();
        _DownloadServer.StreamKey = (string)dico["StreamKey"];
        _DownloadServer.URL = (string)dico["url"];
        _DownloadServer.URL = _DownloadServer.URL.Replace(@"\", "");
        _DownloadServer.StreamServerID = (float)dico["StreamServerID"];
        _DownloadServer.USecs = (float)dico["uSecs"];

        Debug.Log(_DownloadServer.URL);

        callback.Invoke(_DownloadServer.URL);
    }

    public IEnumerator GetSongSearchResults(Action<List<SearchResult>, int> callback, string query)
    {
        JSONObject parameters = new JSONObject();
        parameters.AddField("query", query);
        parameters.AddField("country", _Country.ToJSON());
        parameters.AddField("limit", 50);
        yield return StartCoroutine(MakeCall("getSongSearchResults", parameters, false));
        Debug.Log("GetSongSearchResults done");

        List<SearchResult> results = new List<SearchResult>();

        for (int i = 0; i < _RootNode["result"]["songs"].AsArray.Count; ++i)
        {
            SearchResult res = new SearchResult();
            res.ID = _RootNode["result"]["songs"][i]["SongID"];
            res.Name = _RootNode["result"]["songs"][i]["SongName"];
            res.Others = new Dictionary<string, string>();
            results.Add(res);
        }

        callback.Invoke(results, 0);
    }

    public IEnumerator GetArtistSearchResults(Action<List<SearchResult>, int> callback, string query)
    {
        JSONObject parameters = new JSONObject();
        parameters.AddField("query", query);
        parameters.AddField("limit", 50);
        yield return StartCoroutine(MakeCall("getArtistSearchResults", parameters, false));
        Debug.Log("GetArtistSearchResults done");

        List<SearchResult> results = new List<SearchResult>();

        for (int i = 0; i < _RootNode["result"]["artists"].AsArray.Count; ++i)
        {
            SearchResult res = new SearchResult();
            res.ID = _RootNode["result"]["artists"][i]["ArtistID"];
            res.Name = _RootNode["result"]["artists"][i]["ArtistName"];
            res.Others = new Dictionary<string, string>();
            results.Add(res);
        }

        callback.Invoke(results, 1);
    }

    public IEnumerator GetAlbumSearchResults(Action<List<SearchResult>, int> callback, string query)
    {
        JSONObject parameters = new JSONObject();
        parameters.AddField("query", query);
        parameters.AddField("limit", 50);
        yield return StartCoroutine(MakeCall("getAlbumSearchResults", parameters, false));
        Debug.Log("GetAlbumSearchResults done");

        List<SearchResult> results = new List<SearchResult>();

        for (int i = 0; i < _RootNode["result"]["albums"].AsArray.Count; ++i)
        {
            SearchResult res = new SearchResult();
            res.ID = _RootNode["result"]["albums"][i]["AlbumID"];
            res.Name = _RootNode["result"]["albums"][i]["AlbumName"];
            res.Others = new Dictionary<string, string>();
            results.Add(res);
        }

        callback.Invoke(results, 2);
    }

    public IEnumerator GetAlbumSongs(Action<List<SearchResult>, int> callback, string query)
    {
        JSONObject parameters = new JSONObject();
        parameters.AddField("albumID", query);
        parameters.AddField("limit", 50);
        yield return StartCoroutine(MakeCall("getAlbumSongs", parameters, false));
        Debug.Log("GetAlbumSongs done");

        List<SearchResult> results = new List<SearchResult>();

        for (int i = 0; i < _RootNode["result"]["songs"].AsArray.Count; ++i)
        {
            SearchResult res = new SearchResult();
            res.ID = _RootNode["result"]["songs"][i]["SongID"];
            res.Name = _RootNode["result"]["songs"][i]["Name"];
            res.Others = new Dictionary<string, string>();
            results.Add(res);
        }

        callback.Invoke(results, 0);
    }

    public IEnumerator Authenticate(Action<bool, string> callback, string login, string password)
    {
        JSONObject parameters = new JSONObject();
        parameters.AddField("login", login);
        parameters.AddField("password", CreateMD5(password).ToLower());
        yield return StartCoroutine(MakeCall("authenticate", parameters, true));
        Debug.Log("Authenticate done");

        IsLogged = _RootNode["result"]["success"].AsBool;
        if (IsLogged)
        {
            Login = _RootNode["result"]["FName"];
            IsLogged = true;
        }

        _UserID = _RootNode["result"]["UserID"];
        callback.Invoke(_RootNode["result"]["success"].AsBool, _RootNode["result"]["FName"]);
    }

    public IEnumerator Logout(Action<bool> callback)
    {
        yield return StartCoroutine(MakeCall("logout", null, true));
        Debug.Log("Logout done");

        callback.Invoke(_RootNode["result"]["success"].AsBool);
    }

    public IEnumerator GetUserInfo(Action<bool> callback)
    {
        yield return StartCoroutine(MakeCall("getUserInfo", null, true));
        Debug.Log("GetUserInfo done");

        //callback.Invoke(_RootNode["result"]["success"].AsBool);
    }

    public IEnumerator GetUserPlaylists(Action<List<Playlist>> callback)
    {
        yield return StartCoroutine(MakeCall("getUserPlaylists", null, true));
        Debug.Log("GetUserPlaylists done");

        List<Playlist> playlists = new List<Playlist>();
        for (int i = 0; i < _RootNode["result"]["playlists"].AsArray.Count; ++i)
        {
            Playlist playlist = new Playlist();
            playlist.ID = _RootNode["result"]["playlists"][i]["PlaylistID"];
            playlist.Name = _RootNode["result"]["playlists"][i]["PlaylistName"];
            playlist.Date = _RootNode["result"]["playlists"][i]["TSAdded"];
            playlists.Add(playlist);
        }

        callback.Invoke(playlists);
    }

    public IEnumerator GetPlaylist(Action<List<Song>> callback, string playlistID)
    {
        JSONObject parameters = new JSONObject();
        parameters.AddField("playlistID", playlistID);
        yield return StartCoroutine(MakeCall("getPlaylist", parameters, true));
        Debug.Log("GetPlaylist done");

        List<Song> songs = new List<Song>();
        Debug.Log(_RootNode["result"]["Songs"].AsArray.Count);
        for (int i = 0; i < _RootNode["result"]["Songs"].AsArray.Count; ++i)
        {
            Song song = new Song();
            song.ID = _RootNode["result"]["Songs"][i]["SongID"];
            song.Name = _RootNode["result"]["Songs"][i]["SongName"];
            Debug.Log(song.Name);
            Artist artist = new Artist();
            artist.ID = _RootNode["result"]["Songs"][i]["ArtistID"].AsInt;
            artist.Name = _RootNode["result"]["Song"][i]["ArtistName"];
            song.Artist = artist;
            songs.Add(song);
        }

        callback.Invoke(songs);

        //List<Playlist> playlists = new List<Playlist>();
        //for (int i = 0; i < _RootNode["result"]["playlists"].AsArray.Count; ++i)
        //{
        //    Playlist playlist = new Playlist();
        //    playlist.ID = _RootNode["result"]["playlists"][i]["PlaylistID"];
        //    playlist.Name = _RootNode["result"]["playlists"][i]["PlaylistName"];
        //    playlist.Date = _RootNode["result"]["playlists"][i]["TSAdded"];
        //    playlists.Add(playlist);
        //}

        //callback.Invoke(playlists);
    }

    private IEnumerator MakeCall(string method, JSONObject parameters, bool https = false, bool debug = false)
    {
        string scheme = string.Empty;
        if (https)
            scheme = "https://";
        else
            scheme = "http://";
        string host = _ApiHost;
        string postData = string.Empty; // not encoded
        string sig = string.Empty; // encoded (md5)

        using (var memoryStream = new MemoryStream())
        {
            JSONObject root = new JSONObject(JSONObject.Type.OBJECT);
            root.AddField("method", method);
            root.AddField("header", GetHeader());
            root.AddField("parameters", parameters);

            postData = root.Print();
            Debug.Log("\"postData\": " + postData);

            sig = CreateMessageSig(postData);
        }

        string url = scheme + host + _ApiEndpoint + "?sig=" + sig.ToLower();

        //Debug.Log(url);

        Hashtable headers = new Hashtable();
        headers.Add("Content-Type", postData);
        headers.Add("Accept-Charset", "utf-8");
        var www = new WWW(url, _Encoding.GetBytes(postData), headers);
        yield return www;

        string rawResponse = _Encoding.GetString(www.bytes);
        Debug.Log(rawResponse);
        JSONObject JSONResponse = new JSONObject(rawResponse);
        //AccessData(JSONResponse);

        _RootLastRequest = JSONResponse;
        _RootNode = JSON.Parse(rawResponse);
    }

    private JSONObject GetHeader()
    {
        JSONObject header = new JSONObject(JSONObject.Type.OBJECT);
        header.AddField("wsKey", _Key);
        header.AddField("sessionID", _SessionId);
        return header;
    }

    private string CreateMessageSig(string postData)
    {
        var keyByte = _Encoding.GetBytes(_Secret);
        using (var hmacmd5 = new HMACMD5(keyByte))
        {
            hmacmd5.ComputeHash(_Encoding.GetBytes(postData));
            return ByteToString(hmacmd5.Hash);
        }
    }

    private string CreateMD5(string str)
    {
        System.Security.Cryptography.MD5CryptoServiceProvider M5 = new MD5CryptoServiceProvider();
        byte[] byteString = _Encoding.GetBytes(str);
        byteString = M5.ComputeHash(byteString);
        string finalString = string.Empty;
        foreach (byte bt in byteString)
            finalString += bt.ToString("x2");
        return finalString;
    }

    private string ByteToString(byte[] buff)
    {
        string sBinary = string.Empty;
        for (int i = 0; i < buff.Length; ++i)
            sBinary += buff[i].ToString("X2");
        return sBinary;
    }

    private void AccessData(JSONObject obj, ref Dictionary<string, object> dico, string keyToAdd = "")
    {
        switch (obj.type)
        {
            case JSONObject.Type.OBJECT:
                for (int i = 0; i < obj.list.Count; i++)
                {
                    string key = (string)obj.keys[i];
                    JSONObject j = (JSONObject)obj.list[i];
                    keyToAdd = key;
                    AccessData(j, ref dico, keyToAdd);
                }
                break;
            case JSONObject.Type.ARRAY:
                foreach (JSONObject j in obj.list)
                    AccessData(j, ref dico, keyToAdd);
                break;
            case JSONObject.Type.STRING:
                dico.Add(keyToAdd, obj.str);
                keyToAdd = "";
                break;
            case JSONObject.Type.NUMBER:
                dico.Add(keyToAdd, obj.n);
                keyToAdd = "";
                break;
            case JSONObject.Type.BOOL:
                dico.Add(keyToAdd, obj.b);
                keyToAdd = "";
                break;
            case JSONObject.Type.NULL:
                Debug.Log("NULL");
                break;
        }
    }
}
