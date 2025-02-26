export interface ApiResponse<T> {
    code: number;
    data: T;
}

export interface Session {
    id: string;
    token: string;
    creatTime: Date;
    clientIp: string;
    clientPort: string;
    historys: Message[];
}

export interface ClientInfo {
    clientID: string;
    ip: string;
    port: number;
    userName: string;
    systemVer: string;
    connectTime: Date;
    processID: number;
    runPath: string;
    fileName: string;
}

export interface Message {
    state: number;
    code: string;
    content: string;
    result: string;
    time: Date;
}
