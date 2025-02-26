import { buildToken , GetTime} from "./api/ClientInfo";

export let SessionID: string = '';

export let ConnectIp: string = '';
export let ConnectPort: number = 0;
export let Key: string = "2077576874888";
export let isConnect: boolean = false;
export let Token:string = "";

export function setSessionID(value: string): void {
    SessionID = value;
}

export function setConnectIp(value: string): void {
    ConnectIp = value;
}

export function setConnectPort(value: number): void {
    ConnectPort = value;
}

export function setKey(value: string): void {
    Key = value;
}

export function updateToken(): string {
    GetToken();
    return Token;
}

export function setConnect(value: boolean): void {
    isConnect = value;
}

export async function GetToken(): Promise<string> {
    const key = Key + (await GetTime()).data;
    
    var tokenResponse = await buildToken(key);
    Token = tokenResponse.data.toString();
    return Token;
}