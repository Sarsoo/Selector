import * as signalR from "@microsoft/signalr";
import { FullTrack, CurrentlyPlayingDTO } from "./HubInterfaces";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hub")
    .build();

connection.on("OnNewPlaying", (context: CurrentlyPlayingDTO) => console.log(context));

connection.start().catch(err => console.error(err));

/// <reference path="Interface/HubInterface.ts" />
namespace Selector.Web {
    
}

console.log("NowPlaying!");