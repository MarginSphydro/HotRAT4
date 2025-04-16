export type SiteConfig = typeof siteConfig;

export const siteConfig = {
  name: "HotRAT4",
  description: "热老鼠4噢噢噢",
  navItems: [
    {
      label: "主页",
      href: "/"
    },
    {
      label: "设备",
      href: "/client"
    },
    {
      label: "终端",
      href: "/console"
    },
    {
      label: "文档",
      href: "/docs"
    },
    {
      label: "文件",
      href: "/explorer"
    },
    {
      label: "设置",
      href: "/setting"
    }
  ],
  links: {
    github: "https://github.com/Yuxi-IT/HotRAT4",
    sponsor: "",
    docs: "/docs"
  },
};
