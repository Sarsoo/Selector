import {SecondWatcher} from "./second";

namespace Selector.Web {
    class Watcher {
        static example: string = "string";
        static ex2: string = "awdwad";
    }
}

let sec = new SecondWatcher();

console.log("hello world!");
console.log(sec);