"use client";
import React, { useState, useEffect } from "react";
import { Alert, Button, Input } from "@heroui/react";
import {
  Table,
  TableHeader,
  TableBody,
  TableRow,
  TableCell,
} from "@heroui/table";
import { useWebSocket, FileItem } from "@/components/WebSocketProvider";

export default function ExplorerLayout() {
  const { fileList, ws } = useWebSocket();
  // 当前路径状态，默认访问 C:盘
  const [currentPath, setCurrentPath] = useState("C:\\");
  // 地址栏输入状态
  const [addressInput, setAddressInput] = useState("C:\\");

  // 请求文件列表的函数，通过 ws 发送消息到服务端
  const fetchFiles = () => {
    if (ws && ws.readyState === WebSocket.OPEN) {
      // 服务器期待的格式为 "FILES\n{path}"
      ws.send("CONTROL\nFILES\n" + currentPath);
    }
  };

  // 当 WebSocket 建立连接且当前路径发生变化时请求文件列表
  useEffect(() => {
    if (ws && ws.readyState === WebSocket.OPEN) {
      fetchFiles();
    }
  }, [currentPath, ws]);

  // 刷新按钮的事件处理
  const handleRefresh = () => {
    fetchFiles();
  };

  // 返回上一级目录按钮的事件处理
  const handleBack = () => {
    // 去除末尾多余的 \（若存在且路径长度大于3，如 "C:\" 则不处理）
    let trimmed = currentPath;
    if (trimmed.endsWith("\\") && trimmed.length > 3) {
      trimmed = trimmed.slice(0, -1);
    }
    const lastIndex = trimmed.lastIndexOf("\\");
    if (lastIndex >= 2) {
      const parent = trimmed.substring(0, lastIndex);
      const newPath = parent + "\\";
      setCurrentPath(newPath);
      setAddressInput(newPath);
    }
  };

  // 地址栏确认按钮事件，更新当前目录
  const handleAddressEnter = () => {
    setCurrentPath(addressInput);
  };

  // 点击文件夹行时事件，如果点击的是文件夹则进入该目录
  const handleRowClick = (file: FileItem) => {
    if (file.type === "DIR") {
      // 确保当前路径以 "\" 结尾
      let newPath = currentPath;
      if (!newPath.endsWith("\\")) {
        newPath += "\\";
      }
      // 拼接上点击的文件夹名称，并加上结尾 "\"
      newPath += file.name + "\\";
      setCurrentPath(newPath);
      setAddressInput(newPath);
    }
  };

  return (
    <div
      style={{
        margin: "16px",
        backgroundColor: "#ffffff",
        border: "1px solid #e0e0e0",
        borderRadius: "8px",
        padding: "16px",
        boxShadow: "0 2px 4px rgba(0,0,0,0.1)",
      }}
    >
      {/* 地址栏及操作按钮 */}
      <div style={{ marginBottom: "16px", display: "flex", gap: "8px", alignItems: "center" }}>
        <Input
          value={addressInput}
          onChange={(e) => setAddressInput(e.target.value)}
          style={{ flexGrow: 1 }}
        />
        <Button onClick={handleAddressEnter}>Go</Button>
        <Button onClick={handleRefresh}>刷新</Button>
        <Button onClick={handleBack}>返回上一级</Button>
      </div>
      <h2 style={{ marginBottom: "16px" }}>文件列表 - {currentPath}</h2>
      {/* 如果列表为空，则显示提示 */}
      {fileList.length === 0 ? (
        <Alert color="danger" style={{ margin: "16px" }}>
          当前文件列表为空
        </Alert>
      ) : (
        <Table>
          <TableHeader>
            <TableRow>
              <TableCell style={{ fontWeight: "bold" }}>文件名</TableCell>
              <TableCell style={{ fontWeight: "bold" }}>修改时间</TableCell>
              <TableCell style={{ fontWeight: "bold" }}>类型</TableCell>
            </TableRow>
          </TableHeader>
          <TableBody>
            {fileList.map((file: FileItem, index: number) => (
              <TableRow
                key={index}
                onClick={() => handleRowClick(file)}
                style={{
                  cursor: file.type === "DIR" ? "pointer" : "default",
                }}
              >
                <TableCell>{file.name}</TableCell>
                <TableCell>{file.time}</TableCell>
                <TableCell>{file.type}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
    </div>
  );
}
