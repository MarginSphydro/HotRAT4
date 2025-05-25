import { Link } from "@heroui/link";
import { Snippet } from "@heroui/snippet";
import { Code } from "@heroui/code";
import { button as buttonStyles } from "@heroui/theme";

import { siteConfig } from "@/config/site";
import { title, subtitle } from "@/components/primitives";
import { GithubIcon } from "@/components/icons";
import { Button } from "@heroui/button";

export default function Home() {
  return (
    <section className="flex flex-col items-center justify-center gap-4 py-8 md:py-10">
      <div className="inline-block max-w-xl text-center justify-center font-[SimSun]">
        <span className={title()}>开始使用</span>
        <br />
        <span className={title({ color: "violet" })}>𝓂ℴ𝒹ℯ𝓇𝓃 & 𝒷ℯ𝒶𝓊𝓉𝒾𝒻𝓊𝓁</span>
        <br />
        <span className={title()}>
        的远程控制应用程序
        <br/><br/>
        </span>
      </div>

      <div className="flex gap-3">
        <Link
          isExternal
          className={buttonStyles({
            color: "primary",
            radius: "full",
            variant: "shadow",
          })}
          href={siteConfig.links.docs}
        >
          &nbsp;&nbsp;使用文档&nbsp;&nbsp;
        </Link>
        <Link
          isExternal
          className={buttonStyles({ variant: "bordered", radius: "full" })}
          href={siteConfig.links.github}
        >
          <GithubIcon size={20} />
          GitHub
        </Link>
      </div>

      <div className="mt-8">
        <Snippet hideCopyButton hideSymbol variant="bordered">
          <span>
            Set the <Code color="primary">API</Code> and 
            <Code color="primary">Key</Code> and get started
          </span>
        </Snippet>
      </div>
    </section>
  );
}
