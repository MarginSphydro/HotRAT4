"use client";
import { useWebSocket } from "@/components/WebSocketProvider";
import { Button, Table, TableBody, TableCell, TableColumn, TableHeader, TableRow, Image } from "@heroui/react";

export default function ClientPage() {
  const { 
    clients, 
    loading, 
    error, 
    connectedClientId, 
    connectToClient, 
    disconnectClient 
  } = useWebSocket();

  const handleConnectionAction = (clientId: string) => {
    if (connectedClientId === clientId) {
      disconnectClient();
    } else if (!connectedClientId) {
      connectToClient(clientId);
    }
  };

  return (
    <div className="w-full overflow-x-auto">
      <h1 className="text-xl font-bold mb-4">客户端列表</h1>
      
      {loading && <div className="p-4 text-center">正在连接服务器...</div>}
      {error && <div className="p-4 text-center text-red-500">{error}</div>}

      <div className="w-full max-w-full overflow-hidden">
        <Table aria-label="客户端列表" className="w-full">
          <TableHeader>
            <TableColumn>设备名称</TableColumn>
            <TableColumn className="max-md:hidden">IP地址</TableColumn>
            <TableColumn className="max-md:hidden">用户名</TableColumn>
            <TableColumn className="max-md:hidden">系统版本</TableColumn>
            <TableColumn>连接时间</TableColumn>
            <TableColumn>连接人数</TableColumn>
            <TableColumn>操作</TableColumn>
          </TableHeader>
          <TableBody>
            {clients.map((client) => (
              <TableRow key={client.id}>
                <TableCell>{client.deviceName}</TableCell>
                <TableCell className="max-md:hidden">{client.ip}:{client.port}</TableCell>
                <TableCell className="max-md:hidden">{client.userName}</TableCell>
                <TableCell className="max-md:hidden">{client.version}</TableCell>
                <TableCell>
                  {client.time ? new Date(client.time).toLocaleString('zh-CN', {
                    month: '2-digit',
                    day: '2-digit',
                    hour: '2-digit',
                    minute: '2-digit',
                    hour12: false
                  }).replace(/\//g, '-') : 'N/A'}
                </TableCell>
                <TableCell>{client.count}</TableCell>
                <TableCell>
                  <Button 
                    isIconOnly 
                    onClick={() => handleConnectionAction(client.id)} 
                    aria-label={connectedClientId === client.id ? "disconnect" : "connect"}
                    variant="faded"
                    isDisabled={!!connectedClientId && connectedClientId !== client.id}
                  >
                    <Image 
                      src={connectedClientId === client.id ? "/disc.png" : "/connect.png"} 
                      alt={connectedClientId === client.id ? "断开连接" : "连接"}
                    />
                  </Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
    </div>
  );
}