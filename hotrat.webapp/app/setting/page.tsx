"use client";
import { subtitle, title } from "@/components/primitives";
import { Button } from "@heroui/button";
import { Input } from "@heroui/input";
import { Alert } from "@heroui/react";
import { useState, useEffect } from "react";


export default function SettingLayout() {
  const [serverAddress, setServerAddress] = useState("");
  const [serverApi, setServerApi] = useState("");
  const [serverKey, setServerKey] = useState("");
  const [showSuccess, setShowSuccess] = useState(false);

  useEffect(() => {
    const savedAddress = localStorage.getItem("serveraddress");
    const savedApi = localStorage.getItem("serverapi");
    const savedKey = localStorage.getItem("serverkey");
    
    if (savedAddress) setServerAddress(savedAddress);
    if (savedApi) setServerApi(serverApi);
    if (savedKey) setServerKey(savedKey);
  }, []);

  const handleSave = () => {
    localStorage.setItem("serveraddress", serverAddress);
    localStorage.setItem("serverapi", serverApi);
    localStorage.setItem("serverkey", serverKey);
    setShowSuccess(true);
    
    setTimeout(() => setShowSuccess(false), 3000);
  };

  return (
    <div>
      <h1 className="text-3xl font-bold mb-4">连接设置</h1>
      <h1 className={subtitle()}>服务器地址</h1>
      <Input
        labelPlacement="outside"
        placeholder="localhost:3000"
        value={serverAddress}
        onChange={(e) => setServerAddress(e.target.value)}
        startContent={
          <div className="pointer-events-none flex items-center">
            <span className="text-default-400 text-small">ws://</span>
          </div>
        }
        type="url"
      />

      <h1 className={subtitle()}>API服务器地址</h1>
      <Input
        labelPlacement="outside"
        placeholder="localhost:3001"
        value={serverApi}
        onChange={(e) => setServerApi(e.target.value)}
        startContent={
          <div className="pointer-events-none flex items-center">
            <span className="text-default-400 text-small">http://</span>
          </div>
        }
        type="url"
      />

      <h1 className={subtitle()}>服务器密钥</h1>
      <Input
        labelPlacement="outside"
        placeholder="abcde12345"
        value={serverKey}
        onChange={(e) => setServerKey(e.target.value)}
        type="password"
      />
      
      <div className="mt-4">
        <Button color="primary" onClick={handleSave}>
          保存设置
        </Button>
      </div>

      {showSuccess && (
        <div className="flex items-center justify-center w-full mt-4">
          <div className="flex flex-col w-full">
            <div className="w-full flex items-center my-3">
              <Alert color="success" title="设置保存成功！" />
            </div>
          </div>
        </div>
      )}
    </div>
  );
}