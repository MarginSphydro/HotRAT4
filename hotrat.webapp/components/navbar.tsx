"use client";

import {
  Navbar as HeroUINavbar,
  NavbarContent,
  NavbarMenu,
  NavbarMenuToggle,
  NavbarBrand,
  NavbarItem,
  NavbarMenuItem,
} from "@heroui/navbar";
import { Button, ButtonGroup } from "@heroui/button";
import { Kbd } from "@heroui/kbd";
import { Link } from "@heroui/link";
import { Input } from "@heroui/input";
import { link as linkStyles } from "@heroui/theme";
import NextLink from "next/link";
import clsx from "clsx";

import { siteConfig } from "@/config/site";
import { ThemeSwitch } from "@/components/theme-switch";
import {
  TwitterIcon,
  GithubIcon,
  DiscordIcon,
  HeartFilledIcon,
  SearchIcon,
  Logo,
} from "@/components/icons";
import { Modal, ModalBody, ModalContent, ModalFooter, ModalHeader, useDisclosure } from "@heroui/modal";
import React from "react";
import { Image } from "@heroui/react";

export const Navbar = () => {
  const {isOpen, onOpen, onClose} = useDisclosure();
  const [backdrop, setBackdrop] = React.useState("opaque");
  
  const [selected, setSelected] = React.useState("wechat");

  const handleOpen = (backdrop: React.SetStateAction<string>) => {
    setBackdrop(backdrop);
    onOpen();
  };
  const imageMap: Record<string, string> = {
    wechat: "/wxcode.jpg",
    alipay: "/alipay.jpg",
    usdt: "/usdt.jpg",
  };

  return (
    <HeroUINavbar maxWidth="xl" position="sticky">
      <NavbarContent className="basis-1/5 sm:basis-full" justify="start">
        <NavbarBrand as="li" className="gap-3 max-w-fit">
          <NextLink className="flex justify-start items-center gap-1" href="/">
            <Logo />
            <p className="font-bold text-inherit">HotRAT4</p>
          </NextLink>
        </NavbarBrand>
        <ul className="hidden md:flex gap-4 justify-start ml-2">
          {siteConfig.navItems.map((item, index) => (
            <NavbarItem key={item.href}>
              <NextLink
                className={clsx(
                  "data-[active=true]:text-primary data-[active=true]:font-medium",
                )}
                href={item.href}
              >
                {item.label}
              </NextLink>
            </NavbarItem>
          ))}
        </ul>
      </NavbarContent>

      <NavbarContent
        className="hidden md:flex basis-1/5 sm:basis-full"
        justify="end"
      >
        <NavbarItem className="hidden md:flex gap-2">
          <Link isExternal aria-label="Github" href={siteConfig.links.github}>
            <GithubIcon className="text-default-500" />
          </Link>
          <ThemeSwitch />
          <Button
            className="capitalize"
            color="warning"
            variant="flat"
            onPress={() => handleOpen("blur")}
          >
            赞助
          </Button>
        </NavbarItem>
      </NavbarContent>

      <NavbarContent className="md:hidden basis-1 pl-4" justify="end">
        <Link isExternal aria-label="Github" href={siteConfig.links.github}>
          <GithubIcon className="text-default-500" />
        </Link>
        <ThemeSwitch />
        <NavbarMenuToggle />
      </NavbarContent>

      <NavbarMenu>
        <div className="mx-4 mt-2 flex flex-col gap-2">
        <Button
              isExternal
              as={Link}
              className="text-sm font-normal text-default-600 bg-default-100"
              href={siteConfig.links.sponsor}
              startContent={<HeartFilledIcon className="text-danger" />}
              variant="flat"
            >
              赞助
            </Button>
          {siteConfig.navItems.map((item, index, href) => (
            <NavbarMenuItem key={`${item}-${index}-${href}`}>
              <Link
                color={
                  index === 0
                    ? "primary"
                      : "foreground"
                }
                href={siteConfig.navItems[index].href}
                size="lg"
              >
                {item.label}
              </Link>
            </NavbarMenuItem>
          ))}
        </div>
      </NavbarMenu>

      <Modal backdrop="blur" isOpen={isOpen} onClose={onClose}>
        <ModalContent>
          {(onClose) => (
            <>
              <ModalHeader className="flex flex-col gap-1">
                使用微信、支付宝或USDT赞助我们
              </ModalHeader>
              <ModalBody className="flex flex-col items-center gap-4">
                <ButtonGroup>
                  <Button
                    onPress={() => setSelected("wechat")}
                    color={selected === "wechat" ? "primary" : "default"}
                  >
                    微信
                  </Button>
                  <Button
                    onPress={() => setSelected("alipay")}
                    color={selected === "alipay" ? "primary" : "default"}
                  >
                    支付宝
                  </Button>
                  <Button
                    onPress={() => setSelected("usdt")}
                    color={selected === "usdt" ? "primary" : "default"}
                  >
                    USDT
                  </Button>
                </ButtonGroup>

                <Image
                  src={imageMap[selected]}
                  alt="收款二维码"
                  width={200}
                  height={200}
                  className="rounded-lg"
                />
              </ModalBody>
              <ModalFooter>
                <Button color="danger" variant="light" onPress={onClose}>
                  下次吧
                </Button>
                <Button color="primary" onPress={onClose}>
                  谢谢您
                </Button>
              </ModalFooter>
            </>
          )}
        </ModalContent>
      </Modal>
      
    </HeroUINavbar>
  );
};
