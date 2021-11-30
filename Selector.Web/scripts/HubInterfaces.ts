
export interface SignalR {  
    nowPlayingHub: nowPlayingProxy;
}
export interface nowPlayingProxy {  
    client: NowPlayingHubClient;
    server: NowPlayingHub;
}

export interface NowPlayingHubClient {
    OnNewPlaying: (context: CurrentlyPlayingDTO) => void;
    OnNewAudioFeature: (features: TrackAudioFeatures) => void;
    OnNewPlayCount: (playCount: PlayCount) => void;
}

export interface NowPlayingHub {
    SendNewPlaying(context: CurrentlyPlayingDTO): void;
}

export interface PlayCount {
    track: number | null;
    album: number | null;
    artist: number | null;
    user: number | null;
    username: string;
    listeningEvent: ListeningChangeEventArgs;
}

export interface CurrentlyPlayingDTO {
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