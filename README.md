# Selector

![ci](https://github.com/sarsoo/Selector/actions/workflows/ci.yml/badge.svg)

Selector is a web app for monitoring what you're listening to on Spotify. The Player Watcher keeps an eye on what you're currently playing and fires off events when things change.
The live updating dashboard shows data that the Spotify API shows you for a song, but that isn't visible in the Spotify client itself including the key, BPM, time signature, popularity and audio features.

The audio features are shown in the radar graph below, it's a vector of 0.0 - 1.0 values for 7 categories including Energy, Acoustic-ness, Acapella-ness, Speech-iness and Dance-iness. These numbers are calculated by Spotify's AI/ML department and can be used for describing a track. This feature is perfect for comparing tracks in an ML context, although Spotify's TOS says you can't do that 😭.

You can connect your Last.fm account in order to show the cumulative play counts for the artist, album and track.

[Read the blog post.](https://sarsoo.xyz/selector/)

![Dashboard Example](docs/dashboard.png)