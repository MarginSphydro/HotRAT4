// WebSocketProvider.tsx
"use client";

import React, {
  createContext,
  useContext,
  useState,
  useEffect,
  useCallback,
  ReactNode,
} from "react";
import { json } from "stream/consumers";

// 定义客户端信息结构（根据实际需求可进行扩展）
export interface ClientInfo {
  id: string;
  ip: string;
  port: string;
  deviceName: string;
  userName: string;
  version: string;
  time: string;
  count: number;
}

// 文件项结构
export interface FileItem {
  name: string;
  time: string;
  type: "FILE" | "DIR";
}

// 认证响应
interface AuthResponse {
  code: number;
  data: string;
}

// 定义 WebSocket 上下文类型
interface WebSocketContextType {
  clients: ClientInfo[];
  loading: boolean;
  error: string;
  ws: WebSocket | null;
  connectedClientId: string | null;
  shellLog: string;
  fileList: FileItem[];
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
    throw new Error("useWebSocket 必须在 WebSocketProvider 内部使用");
  }
  return context;
}

interface Props {
  children: ReactNode;
}

export function WebSocketProvider({ children }: Props) {
  const [clients, setClients] = useState<ClientInfo[]>([]);
  const [ws, setWs] = useState<WebSocket | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [connectedClientId, setConnectedClientId] = useState<string | null>(null);
  const [shellLog, setShellLog] = useState<string>("");
  const [fileList, setFileList] = useState<FileItem[]>([]);

  const setshellLog = (log: string, append: boolean = false) => {
    setShellLog((prev) => (append ? `${prev}${log}` : log));
  };

  // 用于从服务器 API 获取验证 token
  const fetchToken = async (): Promise<string> => {
    try {
      const apiUrl = localStorage.getItem("serverapi");
      const key = localStorage.getItem("serverkey");

      if (!apiUrl || !key) {
        throw new Error("请先设置服务器API地址和密钥");
      }

      const response = await fetch(`http://${apiUrl}/api/auth/build?key=${key}`);
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

  // 发送鉴权请求
  const sendAuthRequest = async (websocket: WebSocket) => {
    try {
      const token = await fetchToken();
      websocket.send(`AUTH\n${token}`);
    } catch (error) {
      console.error("获取token失败:", error);
      setError("获取验证token失败");
    }
  };

  // 刷新客户端列表
  const refreshClients = async () => {
    if (!ws || ws.readyState !== WebSocket.OPEN) {
      setError("WebSocket连接未就绪");
      return;
    }
    setLoading(true);
    await sendAuthRequest(ws);
    setLoading(false);
  };

  // 连接至指定客户端
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

  // 断开客户端连接
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

  // 文件列表处理：解析服务器返回的文件列表内容
  const handleFilesList = (raw: string) => {
    // 服务器把换行符替换成了 "\n"（字符串形式）
    // 如果是原始换行符，请使用 raw.split("\n");
    const lines = raw.split("\\n");
    const list: FileItem[] = lines
      .map((line) => {
        const parts = line.split("|");
        if (parts.length >= 3) {
          const [name, time, type] = parts;
          return { name, time, type: type === "DIR" ? "DIR" : "FILE" };
        }
        return null;
      })
      .filter((item): item is FileItem => item !== null);
    setFileList(list);
  };

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
            console.log("接收到消息:", response);
            switch (response.type) {
              case "CLIENTS":
                // 更新客户端列表（示例中未作详细处理）
                if (Array.isArray(response.data)) {
                  setClients(response.data);
                } else {
                  setClients([response.data]);
                }
                break;
              case "CONNECTION_STATUS":
                setConnectedClientId(response.connected ? response.clientId : null);
                break;
              case "ERROR":
                setError(response.message);
                break;
              case "SHELL":
                setshellLog(response.data + "\n", true);
                break;
              case "FILES":
                // 文件列表消息
                handleFilesList(response.data);
                break;
              default:
                console.warn("未知消息类型:", response.type);
            }
          } catch (error) {
            console.error("消息解析失败:", event.data);
          }
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
        fileList,
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
