namespace Selector.Data;

public record struct EndSong
{
    public string conn_country { get; set; }
    public string episode_name { get; set; }
    public string episode_show_name { get; set; }
    public bool? incognito_mode { get; set; }
    public string ip_addr_decrypted { get; set; }
    public string master_metadata_album_album_name { get; set; }
    public string master_metadata_album_artist_name { get; set; }
    public string master_metadata_track_name { get; set; }
    public int ms_played { get; set; }
    public bool? offline { get; set; }
    public long? offline_timestamp { get; set; }
    public string platform { get; set; }
    public string reason_end { get; set; }
    public string reason_start { get; set; }
    public bool shuffle { get; set; }
    public bool? skipped { get; set; }
    public string spotify_episode_uri { get; set; }
    public string spotify_track_uri { get; set; }
    public string ts { get; set; }
    public string user_agent_decrypted { get; set; }
    public string username { get; set; }
}

