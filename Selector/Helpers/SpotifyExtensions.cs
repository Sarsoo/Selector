﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using SpotifyAPI.Web;

namespace Selector
{
    public static class SpotifyExtensions
    {
        public static string DisplayString(this FullTrack track) => $"{track.Name} / {track.Album.Name} / {track.Artists.DisplayString()}";
        public static string DisplayString(this SimpleAlbum album) => $"{album.Name} / {album.Artists.DisplayString()}";
        public static string DisplayString(this SimpleArtist artist) => artist.Name;

        public static string DisplayString(this FullEpisode ep) => $"{ep.Name} / {ep.Show.DisplayString()}";
        public static string DisplayString(this SimpleShow show) => $"{show.Name} / {show.Publisher}";


        public static string DisplayString(this CurrentlyPlayingContext currentPlaying) {
            
            if (currentPlaying.Item is FullTrack track)
            {
                return $"{currentPlaying.IsPlaying}, {track.DisplayString()}, {currentPlaying.Device.DisplayString()}";
            }
            else if (currentPlaying.Item is FullEpisode episode)
            {
                return $"{currentPlaying.IsPlaying}, {episode.DisplayString()}, {currentPlaying.Device.DisplayString()}";
            }
            else
            {
                throw new ArgumentException("Unknown playing type");
            }
        }

        public static string DisplayString(this Context context) => $"{context.Type}, {context.Uri}";
        public static string DisplayString(this Device device) => $"{device.Name} ({device.Id}) {device.VolumePercent}%";
        public static string DisplayString(this TrackAudioFeatures feature) => $"Acou. {feature.Acousticness}, Dance {feature.Danceability}, Energy {feature.Energy}, Instru. {feature.Instrumentalness}, Key {feature.Key}, Live {feature.Liveness}, Loud {feature.Loudness}dB, Mode {feature.Mode}, Speech {feature.Speechiness}, Tempo {feature.Tempo}BPM, Time Sig. {feature.TimeSignature}, Valence {feature.Valence}";

        public static string DisplayString(this IEnumerable<SimpleArtist> artists) => string.Join(", ", artists.Select(a => a.DisplayString()));

        public static bool IsInstrumental(this TrackAudioFeatures feature) => feature.Instrumentalness > 0.5;
        public static bool IsLive(this TrackAudioFeatures feature) => feature.Liveness > 0.8f;
        public static bool IsSpokenWord(this TrackAudioFeatures feature) => feature.Speechiness > 0.66f;
        public static bool IsSpeechAndMusic(this TrackAudioFeatures feature) => feature.Speechiness is >= 0.33f and <= 0.66f;
        public static bool IsNotSpeech(this TrackAudioFeatures feature) => feature.Speechiness < 0.33f;
    }
}
