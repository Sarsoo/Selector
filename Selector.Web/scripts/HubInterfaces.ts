
export interface SignalR {  
    nowPlayingHub: nowPlayingProxy;
}
export interface nowPlayingProxy {  
    client: NowPlayingHubClient;
    server: NowPlayingHub;
}

export interface NowPlayingHubClient {
    OnNewPlayingSpotify: (context: SpotifyCurrentlyPlayingDTO) => void;
    OnNewPlayingApple: (context: AppleCurrentlyPlayingDTO) => void;
    OnNewAudioFeature: (features: TrackAudioFeatures) => void;
    OnNewPlayCount: (playCount: PlayCount) => void;
}

export interface NowPlayingHub {
    SendNewPlaying(context: SpotifyCurrentlyPlayingDTO): void;
}

export interface PlayCount {
    track: number | null;
    album: number | null;
    artist: number | null;
    user: number | null;
    username: string;
    trackCountData: CountSample[];
    albumCountData: CountSample[];
    artistCountData: CountSample[];
    listeningEvent: ListeningChangeEventArgs;
}

export interface PastParams {
    track: string;
    album: string;
    artist: string;
    from: string;
    to: string;
}

export interface RankResult {
    trackEntries: RankEntry[];
    albumEntries: RankEntry[];
    artistEntries: RankEntry[];
    totalCount: number;
    resampledSeries: CountSample[];
}

export interface RankEntry {
    name: string;
    value: number;
}

export interface CountSample {
    timeStamp: Date;
    value: number;
}

export interface SpotifyCurrentlyPlayingDTO {
    context: CurrentlyPlayingContextDTO;
    username: string;
    track: FullTrack;
    episode: FullEpisode;
}

export interface ListeningChangeEventArgs {
    previous: CurrentlyPlayingContext;
    current: CurrentlyPlayingContext;
    username: string;
}

export interface CurrentlyPlayingContextDTO {
    device: Device;
    repeatState: string;
    shuffleState: boolean;
    context: Context;
    timestamp: number;
    progressMs: number;
    isPlaying: boolean;
    currentlyPlayingType: string;
    actions: Actions;
}

export interface CurrentlyPlayingContext {
    device: Device;
    repeatState: string;
    shuffleState: boolean;
    context: Context;
    timestamp: number;
    progressMs: number;
    isPlaying: boolean;
    item: PlayableItem;
    currentlyPlayingType: string;
    actions: Actions;
}

export interface Device {
    id: string;
    isActive: boolean;
    isPrivateSession: boolean;
    isRestricted: boolean;
    name: string;
    type: string;
    volumePercent: number | null;
}

export interface Context {
    externalUrls: { [key: string]: string; };
    href: string;
    type: string;
    uri: string;
}

export interface PlayableItem {
    type: ItemType;
}

export enum ItemType {
    Track = 0,
    Episode = 1
}

export interface Actions {
    disallows: { [key: string]: boolean; };
}

export interface FullTrack {
    type: ItemType;
    trackNumber: number;
    previewUrl: string;
    popularity: number;
    name: string;
    restrictions: { [key: string]: string; };
    linkedFrom: LinkedTrack;
    isPlayable: boolean;
    uri: string;
    id: string;
    externalUrls: { [key: string]: string; };
    externalIds: { [key: string]: string; };
    explicit: boolean;
    durationMs: number;
    discNumber: number;
    availableMarkets: string[];
    artists: SimpleArtist[];
    album: SimpleAlbum;
    href: string;
    isLocal: boolean;
}

export interface TrackAudioFeatures {
    type: string;
    trackHref: string;
    timeSignature: number;
    tempo: number;
    speechiness: number;
    mode: number;
    loudness: number;
    uri: string;
    liveness: number;
    instrumentalness: number;
    id: string;
    energy: number;
    durationMs: number;
    danceability: number;
    analysisUrl: string;
    acousticness: number;
    key: number;
    valence: number;
}

export interface LinkedTrack {
    externalUrls: { [key: string]: string; };
    href: string;
    id: string;
    type: string;
    uri: string;
}

export interface SimpleArtist {
    externalUrls: { [key: string]: string; };
    href: string;
    id: string;
    name: string;
    type: string;
    uri: string;
}

export interface SimpleAlbum {
    albumGroup: string;
    albumType: string;
    artists: SimpleArtist[];
    availableMarkets: string[];
    externalUrls: { [key: string]: string; };
    href: string;
    id: string;
    images: Image[];
    name: string;
    releaseDate: string;
    releaseDatePrecision: string;
    restrictions: { [key: string]: string; };
    totalTracks: number;
    type: string;
    uri: string;
}

export interface Image {
    height: number;
    width: number;
    url: string;
}

export interface FullEpisode {
    show: SimpleShow;
    resumePoint: ResumePoint;
    releaseDatePrecision: string;
    releaseDate: string;
    name: string;
    languages: string[];
    isPlayable: boolean;
    type: ItemType;
    isExternallyHosted: boolean;
    id: string;
    href: string;
    externalUrls: { [key: string]: string; };
    explicit: boolean;
    durationMs: number;
    description: string;
    audioPreviewUrl: string;
    images: Image[];
    uri: string;
}

export interface SimpleShow {
    availableMarkets: string[];
    copyrights: Copyright[];
    description: string;
    explicit: boolean;
    externalUrls: { [key: string]: string; };
    href: string;
    id: string;
    images: Image[];
    isExternallyHosted: boolean;
    languages: string[];
    mediaType: string;
    name: string;
    publisher: string;
    type: string;
    uri: string;
}

export interface Copyright {
    text: string;
    type: string;
}

export interface ResumePoint {
    fullyPlayed: boolean;
    resumePositionMs: number;
}

export interface AppleCurrentlyPlayingDTO {
    track: AppleTrack;
}

export interface AppleTrack {
    albumName: string;
    trackNumber: number,
    durationInMillis: number;
    releaseDate: string;
    isrc: string;
    composerName: string;
    url: string;
    discNumber: number;
    name: string;
    artistName: string;
    artwork: AppleArtwork;
}

export interface AppleArtwork {
    url: string;
}