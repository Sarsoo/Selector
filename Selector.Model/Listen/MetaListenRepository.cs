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
            to:to).Count();

    public IEnumerable<IListen> GetAll(
        string includes = null,
        string userId = null,
        string username = null,
        string trackName = null,
        string albumName = null,
        string artistName = null,
        DateTime? from = null,
        DateTime? to = null)
    {
        var scrobbles = scrobbleRepository.GetAll(
            include: includes,
            userId: userId,
            username: username,
            trackName: trackName,
            albumName: albumName,
            artistName: artistName,
            from: from,
            to: to)
            .OrderBy(x => x.Timestamp)
            .ToArray();

        var spotListens = spotifyRepository.GetAll(
            include: includes,
            userId: userId,
            username: username,
            trackName: trackName,
            albumName: albumName,
            artistName: artistName,
            from: from,
            to: scrobbles.FirstOrDefault()?.Timestamp)
            .OrderBy(x => x.Timestamp)
            .ToArray();
        

        return spotListens.Concat(scrobbles);
    }
}

