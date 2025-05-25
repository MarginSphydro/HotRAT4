"use client";
import { title } from "@/components/primitives";
import { useState } from "react";
import { useWebSocket } from "@/components/WebSocketProvider";
import { Button } from "@heroui/button";
import { Input } from "@heroui/input";
import { Card, CardBody } from "@heroui/react";

export default function ControlLayout() {
  const [inputValue, setInputValue] = useState("");
  const { ws, connectedClientId, shellLog, setshellLog } = useWebSocket();

  const handleSendMessage = (flag: string) => {
    if (!ws || ws.readyState !== WebSocket.OPEN) {
      alert("WebSocket连接未就绪");
      return;
    }

    if (!connectedClientId) {
      alert("请先选择要控制的客户端");
      return;
    }

    if (!inputValue.trim()) {
      alert("输入内容不能为空");
      return;
    }

    const message = `CONTROL\n${flag}\\n${inputValue}`;
    ws.send(message);

    setshellLog(`> ${inputValue}\n`, true);
    setInputValue("");
  };

  return (
    <div className="flex flex-col gap-4">
      <h1 className={title()}>远程控制台</h1>

      <div className="flex gap-2">
        <Input
          type="text"
          value={inputValue}
          onChange={(e) => setInputValue(e.target.value)}
          placeholder="输入控制命令..."
          onKeyDown={(e) => e.key === "Enter" && handleSendMessage("shell")}
        />

        <Button
          onClick={()=>handleSendMessage("shell")}
          disabled={!inputValue.trim()}
        >
          发送
        </Button>
      </div>

      <div className="text-sm text-gray-500">
        {connectedClientId
          ? `当前连接: ${connectedClientId}`
          : "未选择客户端"}
      </div>

    <Card>
      <CardBody  
        className="text-md whitespace-pre-line text-left text-green-400 min-h-[300px] max-h-[600px] overflow-auto"       
        dangerouslySetInnerHTML={{
          __html: shellLog
            ?.replaceAll("\\n", "")
            .replaceAll("\r\n", "<br/>")
        }}>

      </CardBody>
    </Card>
    </div>
  );
}
