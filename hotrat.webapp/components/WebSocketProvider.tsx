"use client";

import {
  createContext,
  useContext,
  useState,
  useEffect,
  ReactNode,
  useCallback,
} from "react";

interface ClientInfo {
  id: string;
  ip: string;
  port: string;
  deviceName: string;
  userName: string;
  version: string;
  time: string;
  count: number;
}

interface AuthResponse {
  code: number;
  data: string;
}

interface WebSocketContextType {
  clients: ClientInfo[];
  loading: boolean;
  error: string;
  ws: WebSocket | null;
  connectedClientId: string | null;
  shellLog: string;
  setshellLog: (log: string, append?: boolean) => void;
  connectToClient: (id: string) => void;
  disconnectClient: () => void;
  refreshClients: () => void;
}

const WebSocketContext = createContext<WebSocketContextType | undefined>(
  undefined
);

export function useWebSocket() {
  const context = useContext(WebSocketContext);
  if (!context) {
    throw new Error("useWebSocket must be used within a WebSocketProvider");
  }
  return context;
}

export function WebSocketProvider({ children }: { children: ReactNode }) {
  const [clients, setClients] = useState<ClientInfo[]>([]);
  const [ws, setWs] = useState<WebSocket | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [connectedClientId, setConnectedClientId] = useState<string | null>(
    null
  );
  const [shellLog, setShellLog] = useState<string>("");

  const setshellLog = (log: string, append: boolean = false) => {
    setShellLog((prev) => (append ? `${prev}${log}` : log));
  };

  const fetchToken = async (): Promise<string> => {
    try {
      const apiUrl = localStorage.getItem("serverapi");
      const key = localStorage.getItem("serverkey");

      if (!apiUrl || !key) {
        throw new Error("请先设置服务器API地址和密钥");
      }

      const response = await fetch(
        `http://${apiUrl}/api/auth/build?key=${key}`
      );
      const data: AuthResponse = await response.json();

      if (data.code !== 200 || !data.data) {
        throw new Error("获取TOKEN失败");
      }

      return data.data;
    } catch (err) {
      setError(err instanceof Error ? err.message : "获取TOKEN时发生错误");
      throw err;
    }
  };

  const sendAuthRequest = async (websocket: WebSocket) => {
    try {
      const token = await fetchToken();
      websocket.send(`AUTH\n${token}`);
    } catch (error) {
      console.error("获取token失败:", error);
      setError("获取验证token失败");
    }
  };

  const refreshClients = async () => {
    if (!ws || ws.readyState !== WebSocket.OPEN) {
      setError("WebSocket连接未就绪");
      return;
    }
    setLoading(true);
    await sendAuthRequest(ws);
    setLoading(false);
  };

  const connectToClient = useCallback(
    (id: string) => {
      if (!ws || ws.readyState !== WebSocket.OPEN) {
        setError("WebSocket连接未就绪");
        return;
      }

      setConnectedClientId(id);
      ws.send(`CONNECT\n${id}`);
      console.log(`请求连接客户端: ${id}`);
    },
    [ws]
  );

  const disconnectClient = useCallback(() => {
    if (!ws || ws.readyState !== WebSocket.OPEN) {
      setError("WebSocket连接未就绪");
      return;
    }

    if (connectedClientId) {
      ws.send(`DISCONNECT\n${connectedClientId}`);
      console.log(`断开连接客户端: ${connectedClientId}`);
    }
    setConnectedClientId(null);
  }, [ws, connectedClientId]);

  useEffect(() => {
    const initWebSocket = async () => {
      const serverAddress = localStorage.getItem("serveraddress");
      if (!serverAddress) {
        setError("请先在设置中配置服务器地址");
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        const websocket = new WebSocket(`ws://${serverAddress}`);

        websocket.onopen = async () => {
          console.log("WebSocket 连接已建立");
          try {
            const token = await fetchToken();
            websocket.send(`AUTH\n${token}`);
          } catch (error) {
            setError(error instanceof Error ? error.message : "认证失败");
          }
        };

        websocket.onmessage = (event) => {
          try {
            const response = JSON.parse(event.data);
            console.log(response);
            switch (response.type) {
              case "CLIENTS":
                handleClientsUpdate(response.data);
                break;
              case "CONNECTION_STATUS":
                setConnectedClientId(
                  response.connected ? response.clientId : null
                );
                break;
              case "ERROR":
                setError(response.message);
                break;
              case "SHELL":
                setshellLog(response.data + "\n", true);
                break;
              default:
                console.warn("未知消息类型:", response.type);
            }
          } catch (error) {
            console.error("消息解析失败:", event.data);
          }
        };

        const handleClientsUpdate = (data: any) => {
          setClients((prev) => {
            const normalizedData = Array.isArray(data) ? data : [data];
            const clientMap = new Map(prev.map((c) => [c.id, c]));

            normalizedData.forEach((client: ClientInfo) => {
              clientMap.set(client.id, {
                ...client,
                count: client.count ?? clientMap.get(client.id)?.count ?? 0,
              });
            });

            return Array.from(clientMap.values());
          });
        };

        websocket.onerror = (error) => {
          setError(`WebSocket 错误: ${error}`);
        };

        websocket.onclose = (event) => {
          if (event.code !== 1000) {
            setTimeout(initWebSocket, 3000);
          }
        };

        setWs(websocket);
      } catch (error) {
        setError(`连接失败: ${error}`);
      } finally {
        setLoading(false);
      }
    };

    initWebSocket();
  }, []);

  return (
    <WebSocketContext.Provider
      value={{
        clients,
        loading,
        error,
        ws,
        connectedClientId,
        shellLog,
        setshellLog,
        connectToClient,
        disconnectClient,
        refreshClients,
      }}
    >
      {children}
    </WebSocketContext.Provider>
  );
}
