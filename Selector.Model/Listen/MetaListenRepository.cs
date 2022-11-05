using System;
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
        DateTime? to = null) => GetAll(userId: userId,
            username: username,
            trackName: trackName,
            albumName: albumName,
            artistName: artistName,
            from: from,
            to:to,
            tracking: false).Count();

    public IEnumerable<IListen> GetAll(
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
            orderTime: true).ToArray();

        var spotListens = spotifyRepository.GetAll(
            include: includes,
            userId: userId,
            username: username,
            trackName: trackName,
            albumName: albumName,
            artistName: artistName,
            from: from,
            to: scrobbles.LastOrDefault()?.Timestamp,
            tracking: tracking,
            orderTime: orderTime);

        return spotListens.Concat(scrobbles);
    }
}

