"use client";
import { useWebSocket } from "@/app/layout";
import { Button, Table, TableBody, TableCell, TableColumn, TableHeader, TableRow } from "@heroui/react";
export default function ClientPage() {
  const { clients, loading, error } = useWebSocket();

  const copyToClipboard = (text: string) => {
    navigator.clipboard.writeText(text);
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
            <TableColumn>系统版本</TableColumn>
            <TableColumn>连接时间</TableColumn>
            <TableColumn>操作</TableColumn>
          </TableHeader>
          <TableBody>
            {clients.map((client) => (
              <TableRow key={client.id}>
                                <TableCell>{client.deviceName}</TableCell>
                <TableCell className="max-md:hidden">{client.ip}:{client.port}</TableCell>
                <TableCell className="max-md:hidden">{client.userName}</TableCell>
                <TableCell>{client.version}</TableCell>
                <TableCell>
                  {client.time ? new Date(client.time).toLocaleString('zh-CN', {
                    month: '2-digit',
                    day: '2-digit',
                    hour: '2-digit',
                    minute: '2-digit',
                    hour12: false
                  }).replace(/\//g, '-') : 'N/A'}
                </TableCell>
                <TableCell>
                  <Button 
                    size="sm" 
                    onClick={() => copyToClipboard(client.id)}
                  >
                    ID
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