import { type FC, useMemo } from 'react';
import { initData, type User, useSignal } from '@telegram-apps/sdk-react';
import { List, Placeholder } from '@telegram-apps/telegram-ui';

import { DisplayData, type DisplayDataRow } from '@/components/DisplayData/DisplayData.tsx';
import { Page } from '@/components/Page.tsx';

function getUserRows(user: User): DisplayDataRow[] {
  return [
    { title: '用户ID', value: user.id.toString() },
    { title: '用户名', value: `@${user.username}` },
    { title: '名', value: user.lastName },
    { title: '姓氏', value: user.firstName },
    { title: 'Premium用户', value: user.isPremium },
    { 
      title: '头像链接(点击复制)', 
      value: (
      <span 
        style={{ cursor: 'pointer' }} 
        onClick={() => navigator.clipboard.writeText(user.photoUrl?.toString() || "")}
      >
        {user.photoUrl}
      </span>
      ) 
    },
    { title: '语言', value: user.languageCode },
  ];
}

export const InitDataPage: FC = () => {
  const initDataState = useSignal(initData.state);

  const userRows = useMemo<DisplayDataRow[] | undefined>(() => {
    return initDataState && initDataState.user
      ? getUserRows(initDataState.user)
      : undefined;
  }, [initDataState]);

  if (!initDataState) {
    return (
      <Page>
        <Placeholder
          header="哎呀"
          description="应用程序启动时缺少初始化数据"
        >
          <img
            alt="Telegram sticker"
            src="https://xelene.me/telegram.gif"
            style={{ display: 'block', width: '144px', height: '144px' }}
          />
        </Placeholder>
      </Page>
    );
  }

  return (
    <Page>
      <List>
        {userRows && <DisplayData header={'User Data'} rows={userRows}/>}
      </List>
    </Page>
  );
};
