import axios from 'axios';
import { ApiResponse, Session, ClientInfo ,Message} from './ApiModels';

const BaseApiUrl = "http://localhost:5000/api";

export async function GetTime(): Promise<ApiResponse<string>> {
    try {
        const response = await axios.get<ApiResponse<string>>(`${BaseApiUrl}/time`);
        return response.data;
    } catch (error) {
        console.error("Error authenticating token:", error);
        throw error;
    }
}

export async function buildToken(key: string): Promise<ApiResponse<string>> {
    try {
        const response = await axios.get<ApiResponse<string>>(`${BaseApiUrl}/auth/build`, {
            params: { key }
        });
        return response.data;
    } catch (error) {
        console.error("Error authenticating token:", error);
        throw error;
    }
}

export async function authToken(token: string): Promise<ApiResponse<boolean>> {
    try {
        const response = await axios.get<ApiResponse<boolean>>(`${BaseApiUrl}/auth`, {
            params: { token }
        });
        return response.data;
    } catch (error) {
        console.error("Error authenticating token:", error);
        throw error;
    }
}

export async function getAllSessions(token: string): Promise<ApiResponse<Session[]>> {
    try {
        const response = await axios.get<ApiResponse<Session[]>>(`${BaseApiUrl}/connect/sessions`, {
            params: { token }
        });
        return response.data;
    } catch (error) {
        console.error("Error fetching sessions:", error);
        throw error;
    }
}

export async function getAllClients(token: string): Promise<ApiResponse<ClientInfo[]>> {
    try {
        const response = await axios.get<ApiResponse<ClientInfo[]>>(`${BaseApiUrl}/connect/clients`, {
            params: { token }
        });
        return response.data;
    } catch (error) {
        console.error("Error fetching clients:", error);
        throw error;
    }
}

export async function connectClient(
    token: string,
    ip: string,
    port: number
): Promise<ApiResponse<string>> {
    try {
        const response = await axios.get<ApiResponse<string>>(`${BaseApiUrl}/connect/connect`, {
            params: { token, ip, port }
        });
        return response.data;
    } catch (error) {
        console.error("Error connecting client:", error);
        throw error;
    }
}

export async function disconnectSession(token: string, id: string): Promise<ApiResponse<string>> {
    try {
        const response = await axios.get<ApiResponse<string>>(`${BaseApiUrl}/connect/disconnect`, {
            params: { token, id }
        });
        return response.data;
    } catch (error) {
        console.error("Error disconnecting session:", error);
        throw error;
    }
}

export async function addMessage(
    token: string,
    id: string,
    text: string
): Promise<ApiResponse<boolean>> {
    try {
        const response = await axios.get<ApiResponse<boolean>>(`${BaseApiUrl}/connect/addmsg`, {
            params: { token, id, text }
        });
        return response.data;
    } catch (error) {
        console.error("Error adding message:", error);
        throw error;
    }
}

export async function getSession(token: string, id: string): Promise<ApiResponse<Session>> {
    try {
        const response = await axios.get<ApiResponse<Session>>(`${BaseApiUrl}/connect/getsession`, {
            params: { token, id }
        });
        return response.data;
    } catch (error) {
        console.error("Error fetching session:", error);
        throw error;
    }
}

export async function getClient(token: string,ip: string, port: number): Promise<ApiResponse<ClientInfo>> {
    try {
        const response = await axios.get<ApiResponse<ClientInfo>>(`${BaseApiUrl}/connect/getclient`, {
            params: { token, ip, port }
        });
        return response.data;
    } catch (error) {
        console.error("Error fetching client:", error);
        throw error;
    }
}

export async function getWork(token: string,id: string,code: string): Promise<ApiResponse<Message>> {
    try {
        const response = await axios.get<ApiResponse<Message>>(`${BaseApiUrl}/connect/getwork`, {
            params: { token, id, code }
        });
        return response.data;
    } catch (error) {
        console.error("Error fetching client:", error);
        throw error;
    }
}
