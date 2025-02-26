import type { ComponentType, JSX } from 'react';

import { IndexPage } from '@/pages/IndexPage/IndexPage';
import { InitDataPage } from '@/pages/UserDataPage';
import { ClientListPage } from '@/pages/ClientListPage';
import ControlPage from '@/pages/ControlPage.tsx';
import RtmpPlayer from '@/pages/RtmpPlayer.tsx';
interface Route {
  path: string;
  Component: ComponentType;
  title?: string;
  icon?: JSX.Element;
}

export const routes: Route[] = [
  { path: '/', Component: IndexPage },
  { path: '/user-data', Component: InitDataPage, title: '个人信息' },
  { path :'/clients', Component: ClientListPage, title: '客户端列表' },
  { path :'/control', Component: ControlPage, title: '操控客户端' },
  { path :'/sc', Component: RtmpPlayer, title: '屏幕监视' }
];
