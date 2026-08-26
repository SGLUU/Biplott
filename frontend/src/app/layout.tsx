import type { Metadata } from "next";
import "./globals.css";
import { ThemeProvider } from "@/components/theme-provider";
import { Header } from "@/components/layout/Header";
import { Footer } from "@/components/layout/Footer";
import { AuthInitializer } from "@/components/auth/AuthInitializer";

export const metadata: Metadata = {
  title: "Bịp lót — Cơ hội để nát hơn",
  description: "Nền tảng tạo bộ số xổ số giải trí, châm biếm văn minh và cá nhân hóa (Power 6/55, Mega 6/45, Lotto 5/35).",
  keywords: ["xổ số", "vietlott", "bịp lót", "tạo số", "lucky journey", "thần tài"],
  authors: [{ name: "Bịp lót Team" }],
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="vi" suppressHydrationWarning>
      <body className="flex flex-col min-h-screen">
        <ThemeProvider
          attribute="class"
          defaultTheme="system"
          enableSystem
          disableTransitionOnChange
        >
          <AuthInitializer />
          <Header />
          <main className="flex-1 w-full max-w-6xl mx-auto px-4 py-6">
            {children}
          </main>
          <Footer />
        </ThemeProvider>
      </body>
    </html>
  );
}
