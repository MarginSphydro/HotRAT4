"use client";
import { Fragment } from "react";
import { Tab, Tabs } from "@heroui/react";
import { 
  HomeIcon, 
  ComputerDesktopIcon, 
  DocumentTextIcon, 
  Cog6ToothIcon,
  CommandLineIcon
} from "@heroicons/react/24/solid";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { siteConfig } from "@/config/site";

export default function MobileFooter() {
  const pathname = usePathname();
  const navItems = siteConfig.navItems;
  
  const tabs = [
    { 
      name: navItems[0].label, 
      icon: HomeIcon, 
      href: navItems[0].href,
      key: "home"
    },
    { 
      name: navItems[1].label, 
      icon: ComputerDesktopIcon, 
      href: navItems[1].href,
      key: "devices"
    },
    { 
      name: navItems[2].label, 
      icon: CommandLineIcon, 
      href: navItems[2].href,
      key: "control"
    },
    { 
      name: navItems[3].label, 
      icon: DocumentTextIcon, 
      href: navItems[3].href,
      key: "docs"
    },
    { 
      name: navItems[4].label, 
      icon: Cog6ToothIcon, 
      href: navItems[4].href,
      key: "settings"
    },
  ];

  return (
    <div className="fixed bottom-0 left-0 w-full h-16 md:hidden">
      <Tabs 
        aria-label="Mobile Navigation"
        color="primary" 
        variant="bordered"
        fullWidth
        className="h-full "
      >
        {tabs.map((tab) => (
          <Tab
            key={tab.key}
            title={
              <Link href={tab.href} className="flex flex-col items-center justify-center h-full w-full">
                <tab.icon className="w-6 h-6" />
                <span className="text-xs mt-1">{tab.name}</span>
              </Link>
            }
            className="h-full py-2 "
          />
        ))}
      </Tabs>
    </div>
  );
}