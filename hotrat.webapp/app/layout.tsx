import "@/styles/globals.css";
import clsx from "clsx";
import { Providers } from "./providers";
import { siteConfig } from "@/config/site";
import { fontSans } from "@/config/fonts";
import { Navbar } from "@/components/navbar";
import { WebSocketProvider } from "@/components/WebSocketProvider";
import MobileFooter from "@/components/MobileFooter";

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html suppressHydrationWarning lang="en">
      <head />
      <body className={clsx(
        "min-h-screen bg-background font-sans antialiased",
        fontSans.variable,
      )}>
        <WebSocketProvider>
          <Providers themeProps={{ attribute: "class", defaultTheme: "dark" }}>
            <div className="relative flex flex-col h-screen">
              <Navbar />
              <main className="container mx-auto pt-16 px-6 flex-grow">
                {children}
              </main>
              {/* <MobileFooter/> */}
            </div>
          </Providers>
        </WebSocketProvider>
      </body>
    </html>
  );
}