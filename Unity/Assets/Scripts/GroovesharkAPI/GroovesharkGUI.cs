using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.BassWrapper;

public struct SearchResult
{
    // shared between all search result
    public string ID;
    public string Name;

    public Dictionary<string, string> Others;
}

public class GroovesharkGUI : MonoBehaviour {

    private Grooveshark _Grooveshark;
    private BassManager _BassManager;

    private string _Login = "delporte.valentin@gmail.com";
    private string _Password = "18121991";
    private string _Nickname = "";
    private bool _Logged = false;

    private string _Search = string.Empty;
    private List<SearchResult> _results;
    private int _ResultIndex = 0;
    private int _ResultType = 0;
    private string[] _SearchBy = { "Song", "Artist", "Album" };
    private int _SearchByIndex = 0;

    private string[] _DebugMenu = { "Search", "Playlists" };
    private int _DebugMenuIndex = 0;

    private List<Playlist> _UserPlaylists = new List<Playlist>();

    private Vector2 _PlaylistScrollPosition = Vector2.zero;

    private bool _DrawMenu = false;
    
    void Start()
    {
        _Grooveshark = GameObject.FindGameObjectWithTag("GroovesharkAPI").GetComponent<Grooveshark>();
        _BassManager = GameObject.FindGameObjectWithTag("BassManager").GetComponent<BassManager>();
        _results = new List<SearchResult>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            _DrawMenu = !_DrawMenu;
    }

    void OnGUI()
    {
        if (!_DrawMenu)
            return;
        GUILayout.BeginArea(new Rect(10.0f, 10.0f, 500.0f, 800.0f));
        GUILayout.BeginVertical();
        OnSearchArea();
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    public void FillSearchResultCallback(List<SearchResult> results, int searchType)
    {
        _results = results;
        _SearchByIndex = searchType;
    }

    public void LogginCallback(bool logged, string nickname)
    {
        _Logged = logged;
        _Nickname = nickname;

        // login routine
        // get playlist
        StartCoroutine(_Grooveshark.GetUserInfo(null));
        StartCoroutine(_Grooveshark.GetUserPlaylists(GetUserPlaylistCallback));
    }

    public void LogoutCallback(bool success)
    {
        _Logged = !success;
    }

    public void GetUserPlaylistCallback(List<Playlist> playlists)
    {
        _UserPlaylists = playlists;
    }

    public void GetPlaylistCallback(List<Song> songs)
    {
        _results.Clear();
        foreach (var song in songs)
        {
            SearchResult r = new SearchResult();
            r.ID = song.ID;
            r.Name = song.Name;
            r.Others = new Dictionary<string, string>();
            _results.Add(r);
        }
        _DebugMenuIndex = 0;
    }

    private void OnSearchArea()
    {
        GUILayout.BeginHorizontal();
        if (!_Grooveshark.IsLogged)
        {
            _Login = GUILayout.TextField(_Login);
            _Password = GUILayout.PasswordField(_Password, "*"[0]);
            if (GUILayout.Button("Login"))
                StartCoroutine(_Grooveshark.Authenticate(LogginCallback, _Login, _Password));
        }
        else
        {
            GUILayout.Label("Logged as " + _Grooveshark.Login + " (" + _Login + ")");
            if (GUILayout.Button("Disconnect"))
                StartCoroutine(_Grooveshark.Logout(LogoutCallback));
        }
        GUILayout.EndHorizontal();

        _DebugMenuIndex = GUILayout.SelectionGrid(_DebugMenuIndex, _DebugMenu, 2);

        if (_DebugMenuIndex == 0)
        {
            _Search = GUILayout.TextField(_Search);
            _SearchByIndex = GUILayout.SelectionGrid(_SearchByIndex, _SearchBy, 3);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Search"))
            {
                if (_SearchByIndex == 0)
                    StartCoroutine(_Grooveshark.GetSongSearchResults(FillSearchResultCallback, _Search));
                else if (_SearchByIndex == 1)
                    StartCoroutine(_Grooveshark.GetArtistSearchResults(FillSearchResultCallback, _Search));
                else if (_SearchByIndex == 2)
                    StartCoroutine(_Grooveshark.GetAlbumSearchResults(FillSearchResultCallback, _Search));
            }
            if (GUILayout.Button("Clear"))
            {
                _results.Clear();
            }
            GUILayout.EndHorizontal();

            if (_results.Count > 0 && _results[0].Others.Count > 0)
            {
                GUILayout.BeginHorizontal();
                foreach (var entry in _results[0].Others)
                    GUILayout.Label(entry.Key);
                GUILayout.EndHorizontal();
            }
            for (int i = _ResultIndex; i < _results.Count && i < _ResultIndex + 10; ++i)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(_results[i].Name);
                foreach (var entry in _results[i].Others)
                    GUILayout.Label(entry.Value);
                if (_SearchByIndex == 0 && GUILayout.Button("Play"))
                    StartCoroutine(_Grooveshark.GetStreamKeyStreamServer(_BassManager.StartGroovesharkStream, _results[i].ID));
                if (_SearchByIndex == 1 && GUILayout.Button("OK"))
                    StartCoroutine(_Grooveshark.GetSongSearchResults(FillSearchResultCallback, _results[i].Name));
                if (_SearchByIndex == 2 && GUILayout.Button("OK"))
                    StartCoroutine(_Grooveshark.GetAlbumSongs(FillSearchResultCallback, _results[i].ID));
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            if (_ResultIndex >= 10 && GUILayout.Button("Previous"))
                _ResultIndex -= 10;
            if (_ResultIndex <= _results.Count - 10 && GUILayout.Button("Next"))
                _ResultIndex += 10;
            GUILayout.EndHorizontal();
        }
        else if (_DebugMenuIndex == 1)
        {
            _PlaylistScrollPosition = GUILayout.BeginScrollView(_PlaylistScrollPosition);
            foreach (var playlist in _UserPlaylists)
            {
                if (GUILayout.Button(playlist.Name))
                    StartCoroutine(_Grooveshark.GetPlaylist(GetPlaylistCallback, playlist.ID));
            }
            GUILayout.EndScrollView();
        }
    }

}
