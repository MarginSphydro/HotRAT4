"use client";
import "@/styles/globals.css";
import { Metadata, Viewport } from "next";
import { Button, ButtonGroup } from "@heroui/button";
import clsx from "clsx";
import { Providers } from "./providers";
import { siteConfig } from "@/config/site";
import { fontSans } from "@/config/fonts";
import { Navbar } from "@/components/navbar";
import { createContext, useContext, useState, useEffect, ReactNode } from 'react';

interface ClientInfo {
  id: string;
  ip: string;
  port: string;
  deviceName: string;
  userName: string;
  version: string;
  time: string;
}

interface AuthResponse {
  code: number;
  data: string;
}

interface WebSocketContextType {
  clients: ClientInfo[];
  loading: boolean;
  error: string;
  refreshClients: () => Promise<void>;
}

const WebSocketContext = createContext<WebSocketContextType | undefined>(undefined);

export function useWebSocket() {
  const context = useContext(WebSocketContext);
  if (!context) {
    throw new Error('useWebSocket must be used within a WebSocketProvider');
  }
  return context;
}

export function WebSocketProvider({ children }: { children: ReactNode }) {
  const [clients, setClients] = useState<ClientInfo[]>([]);
  const [ws, setWs] = useState<WebSocket | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

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

  const sendAuthRequest = async (websocket: WebSocket) => {
    try {
      const token = await fetchToken();
      websocket.send(`AUTH\n${token}`);
      console.log(`发送AUTH: ${token}`);
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

  useEffect(() => {
    const serverAddress = localStorage.getItem("serveraddress");
    if (!serverAddress) {
      setError("请先设置服务器地址");
      setLoading(false);
      return;
    }
  
    setLoading(true);
    const websocket = new WebSocket(`ws://${serverAddress}`);
  
    websocket.onopen = async () => {
      await sendAuthRequest(websocket);
      setLoading(false);
    };
  
    websocket.onmessage = (event) => {
      try {
        const response = JSON.parse(event.data);
        if (response.type === "CLIENTS") {
          const clientList = response.data.map((client: any) => ({
            id: client.id,
            ip: client.ip,
            port: client.port,
            deviceName: client.deviceName,
            userName: client.userName,
            version: client.version,
            time: client.time
          }));
          setClients(clientList);
        }
      } catch (error) {
        console.error("解析消息失败:", error);
      }
    };
  
    websocket.onerror = () => {
      setError("连接服务器失败");
      setLoading(false);
    };
  
    websocket.onclose = () => {
      setError("连接已关闭");
      setLoading(false);
    };
  
    setWs(websocket);
  
    return () => {
      websocket.close();
    };
  }, []);

  return (
    <WebSocketContext.Provider value={{ clients, loading, error, refreshClients }}>
      {children}
    </WebSocketContext.Provider>
  );
}

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html suppressHydrationWarning lang="en">
      <head />
      <body className={clsx(
        "min-h-screen bg-background font-sans antialiased",
        fontSans.variable,
      )}>
        <Providers themeProps={{ attribute: "class", defaultTheme: "dark" }}>
          <WebSocketProvider>
            <div className="relative flex flex-col h-screen">
              <Navbar />
              <main className="container mx-auto pt-16 px-6 flex-grow">
                {children}
              </main>
            </div>
          </WebSocketProvider>
        </Providers>
      </body>
    </html>
  );
}