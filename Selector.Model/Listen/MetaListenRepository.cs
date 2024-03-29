﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Selector.Model;

public class MetaListenRepository: IListenRepository
{
    private readonly IScrobbleRepository scrobbleRepository;
    private readonly ISpotifyListenRepository spotifyRepository;

    public MetaListenRepository(IScrobbleRepository scrobbleRepository, ISpotifyListenRepository listenRepository)
    {
        this.scrobbleRepository = scrobbleRepository;
        spotifyRepository = listenRepository;
    }

    public int Count(
        string userId = null,
        string username = null,
        string trackName = null,
        string albumName = null,
        string artistName = null,
        DateTime? from = null,
        DateTime? to = null)
    {
        var scrobbles = scrobbleRepository.GetAll(
            userId: userId,
            username: username,
            trackName: trackName,
            albumName: albumName,
            artistName: artistName,
            from: from,
            to: to,
            tracking: false,
            orderTime: true);

        var spotListens = spotifyRepository.GetAll(
            userId: userId,
            username: username,
            trackName: trackName,
            albumName: albumName,
            artistName: artistName,
            from: from,
            to: scrobbles.FirstOrDefault()?.Timestamp,
            tracking: false);

        return scrobbles.Count() + spotListens.Count();
    }

    public IQueryable<IListen> GetAll(
        string includes = null,
        string userId = null,
        string username = null,
        string trackName = null,
        string albumName = null,
        string artistName = null,
        DateTime? from = null,
        DateTime? to = null,
        bool tracking = true,
        bool orderTime = false)
    {
        var scrobbles = scrobbleRepository.GetAll(
            include: includes,
            userId: userId,
            username: username,
            trackName: trackName,
            albumName: albumName,
            artistName: artistName,
            from: from,
            to: to,
            tracking: tracking,
            orderTime: true);

        var spotListens = spotifyRepository.GetAll(
            include: includes,
            userId: userId,
            username: username,
            trackName: trackName,
            albumName: albumName,
            artistName: artistName,
            from: from,
            to: scrobbles.FirstOrDefault()?.Timestamp,
            tracking: tracking,
            orderTime: orderTime);

        return spotListens.Select(sp => new Listen
        {
            Timestamp = sp.Timestamp,
            TrackName = sp.TrackName,
            AlbumName = sp.AlbumName,
            ArtistName = sp.ArtistName
        }).Concat(scrobbles.Select(sp => new Listen
        {
            Timestamp = sp.Timestamp,
            TrackName = sp.TrackName,
            AlbumName = sp.AlbumName,
            ArtistName = sp.ArtistName
        }));
    }
}

