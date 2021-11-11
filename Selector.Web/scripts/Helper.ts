
const keyStrings: string[] = [
    "C",
    "C#",
    "D",
    "D#",
    "E",
    "F",
    "F#",
    "G",
    "G#",
    "A",
    "A#",
    "B",
];

export function KeyString(keyCode: number): string
{
    return keyStrings[keyCode];
}

export function ModeString(modeCode: number): string
{
    if(modeCode === 1)
    {
        return "Major";
    }
    else {
        return "Minor";
    }
}