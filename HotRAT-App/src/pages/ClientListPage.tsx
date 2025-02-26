import { postEvent, on } from '@telegram-apps/sdk';
import React, { useEffect, useState } from 'react';
import { initData } from '@telegram-apps/sdk-react';
import { Button, List, Card } from '@telegram-apps/telegram-ui';
import { Loading } from '@/components/Loading.tsx';
import { getAllClients, connectClient, disconnectSession } from '@/api/ClientInfo';
import { setSessionID, setConnectIp, setConnectPort, GetToken, SessionID, ConnectIp, ConnectPort,isConnect,setConnect } from '../runtimes';
import { ClientInfo } from "@/api/ApiModels";

postEvent('web_app_setup_back_button', { is_visible: true });

on('back_button_pressed', () => {
    window.history.back();
});


console.log(`SessionID：${SessionID}\nConnectIp：${ConnectIp}\nConnectPort：${ConnectPort}\n${isConnect}`)

const CustomListItem: React.FC<{
    client: ClientInfo;
    index: number;
    onConnect: (client: ClientInfo) => void;
    onDisconnect: (client: ClientInfo) => void;
    isConnected: boolean;
}> = ({ client, index, onConnect, onDisconnect }) => {
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const timer = setTimeout(() => {
            setLoading(false);
        }, 500);

        return () => clearTimeout(timer);
    }, []);

    if (loading) {
        return <Loading />;
    }

    return (
        <Card style={styles.infocard}>
            <div style={styles.row}>
                <div style={styles.index}>{index + 1}</div>
                <div style={styles.textContainer}>
                    <p><strong>客户端ID:</strong> {client.clientID}</p>
                    <p><strong>用户名:</strong> {client.userName}</p>
                    <p><strong>IP地址:</strong> {client.ip}</p>
                    <p><strong>端口:</strong> {client.port}</p>
                    <p><strong>进程名:</strong> {client.fileName}</p>
                </div>
                {isConnect && client.ip === ConnectIp && client.port === ConnectPort ? (
                    <Button style={styles.connectButton} onClick={() => onDisconnect(client)}>
                        Disconnect
                    </Button>
                ) : (
                    <Button
                        style={styles.connectButton}
                        onClick={() => onConnect(client)}
                        disabled={isConnect} 
                    >
                        Connect
                    </Button>
                )}
            </div>
        </Card>
    );
};


const TOKEN = await GetToken();

export const ClientListPage: React.FC = () => {
    const [clients, setClients] = useState<ClientInfo[]>([]);
    const [connectedClient, setConnectedClient] = useState<{ ip: string; port: string; sessionId: string } | null>(null);
    const userId = initData.state()?.user?.id;

    useEffect(() => {
        if (!userId) return;

        const fetchData = async () => {
            try {
                const response = await getAllClients(TOKEN);
                if (response.code === 200) {
                    setClients(response.data); // 提取数据
                } else {
                    alert(`请求客户端列表失败: ${response.data}`);
                    console.error("Failed to fetch clients:", response.data);
                }
            } catch (err) {
                alert(`请求客户端列表失败: ${err}`);
                console.error("Error fetching clients:", err);
            }
        };

        fetchData();
    }, [userId]);

    const handleConnect = async (client: ClientInfo) => {
        try {
            if(!isConnect){
                const response = await connectClient(TOKEN, client.ip, client.port);
                if (response.code === 400) {
                    alert("该客户端被占用，请稍等或选择其他客户端.");
                    return;
                }
                const sessionId = response.data;
                setConnectedClient({ ip: client.ip, port: client.port.toString(), sessionId });
                setSessionID(sessionId);
                setConnectIp(client.ip);
                setConnectPort(client.port);
                setConnect(true);

                alert(`已连接到会话： ${response.data}`);
                setClients([]); // 刷新客户端列表
                var res = await getAllClients(TOKEN);
                if (res.code === 200) {
                    setClients(res.data);
                }
            }
        } catch (err) {
            alert(`未连接到客户端: :${err}`);
            console.error('Could not connect to client: ', err);
        }
    };

    const handleDisconnect = async () => {
        try {
            const response = await disconnectSession(TOKEN, SessionID);
            if (response.code === 200) {
                setConnectedClient(null);
                setSessionID('');
                setConnectIp('');
                setConnectPort(0);
                setConnect(false);
                alert('已断开连接');

                setClients([]);
                var res = await getAllClients(TOKEN);
                if (res.code === 200) {
                    setClients(res.data);
                }
            } else {
                alert(`断开连接失败: ${response.data}`);
                console.error('Failed to disconnect client:', response.data);
            }
        } catch (err) {
            alert(`断开连接失败: ${err}`);
            console.error('Error disconnecting client:', err);
        }
    };

    return (
        <div>
            <div style={{ textAlign: 'center' }}>
                <h2>在线客户端列表</h2>
                <span style={{ fontSize: 130 }}>🖥️</span>
            </div>
            {clients.length === 0 ? (
                <Loading />
            ) : (
                <List>
                    {clients.map((client, index) => (
                        <CustomListItem
                            key={index}
                            index={index}
                            client={client}
                            onConnect={handleConnect}
                            onDisconnect={handleDisconnect}
                            isConnected={connectedClient?.ip === client.ip && connectedClient?.port === client.port.toString()}
                        />
                    ))}
                </List>
            )}
        </div>
    );
};


// 自定义样式
import { CSSProperties } from 'react';

const styles: { [key: string]: CSSProperties } = {
    infocard: {
        padding: '10px',
        borderRadius: '10px',
        boxShadow: '0 2px 4px rgba(0,0,0,0.1)',
        backgroundColor: 'rgba(0,0,0,0.05)',
        width: '90%',
        margin: '10px 0 10px 10px'
    },
    row: {
        display: 'flex',
        alignItems: 'center',
    },
    index: {
        minWidth: '30px',
        textAlign: 'center',
        fontWeight: 'bold',
        marginRight: '10px',
    },
    textContainer: {
        flex: 1,
    },
    connectButton: {
        minWidth: '80px',
    },
};

export default ClientListPage;
