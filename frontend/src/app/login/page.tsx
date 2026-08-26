"use client";

import { useState, Suspense } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import Link from "next/link";
import { useAuthStore } from "@/stores/useAuthStore";
import { Dices, LogIn, Sparkles, AlertCircle, ArrowRight } from "lucide-react";

function LoginForm() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const redirectUrl = searchParams.get("redirect") || "/";

  const { login } = useAuthStore();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!email || !password) {
      setError("Vui lòng nhập đầy đủ email và mật khẩu.");
      return;
    }

    try {
      setLoading(true);
      await login({ email, password });
      router.push(redirectUrl);
    } catch (err: unknown) {
      const errorMsg = err instanceof Error ? err.message : "Đăng nhập thất bại. Vui lòng kiểm tra lại thông tin.";
      setError(errorMsg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="w-full max-w-md mx-auto py-8 px-4 space-y-6 animate-in fade-in duration-200">
      {/* Brand Header */}
      <div className="text-center space-y-2">
        <div className="inline-flex p-3 rounded-2xl bg-gradient-to-tr from-rose-600 via-orange-500 to-amber-400 text-zinc-950 shadow-lg shadow-orange-500/25">
          <Dices className="w-8 h-8" />
        </div>
        <h1 className="text-2xl font-black tracking-tight text-zinc-900 dark:text-zinc-100">
          Đăng nhập Bịp lót
        </h1>
        <p className="text-xs text-zinc-500 dark:text-zinc-400">
          Đăng nhập để giữ lại những con số tâm linh và theo dõi lịch sử đu đỉnh của bạn.
        </p>
      </div>

      {/* Error Alert */}
      {error && (
        <div className="flex items-center gap-2.5 p-3.5 rounded-2xl bg-red-50 dark:bg-red-950/40 border border-red-200 dark:border-red-800/60 text-xs text-red-700 dark:text-red-300">
          <AlertCircle className="w-4 h-4 text-red-500 flex-shrink-0" />
          <span>{error}</span>
        </div>
      )}

      {/* Login Card */}
      <div className="p-6 rounded-3xl bg-card border border-border shadow-xl space-y-5">
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-1.5">
            <label className="text-xs font-bold text-zinc-700 dark:text-zinc-300">
              Email
            </label>
            <input
              type="email"
              required
              placeholder="tenban@example.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full px-4 py-2.5 rounded-2xl bg-zinc-50 dark:bg-zinc-900 border border-zinc-200 dark:border-zinc-800 focus:outline-none focus:ring-2 focus:ring-orange-500 text-sm"
            />
          </div>

          <div className="space-y-1.5">
            <div className="flex items-center justify-between">
              <label className="text-xs font-bold text-zinc-700 dark:text-zinc-300">
                Mật khẩu
              </label>
            </div>
            <input
              type="password"
              required
              placeholder="••••••••"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full px-4 py-2.5 rounded-2xl bg-zinc-50 dark:bg-zinc-900 border border-zinc-200 dark:border-zinc-800 focus:outline-none focus:ring-2 focus:ring-orange-500 text-sm"
            />
          </div>

          <button
            type="submit"
            disabled={loading}
            className="w-full flex items-center justify-center gap-2 py-3 px-4 rounded-2xl bg-gradient-to-r from-orange-600 to-amber-500 hover:from-orange-500 hover:to-amber-400 text-white font-bold text-sm shadow-lg shadow-orange-500/25 active:scale-[0.99] transition-all disabled:opacity-50"
          >
            <LogIn className="w-4 h-4" />
            <span>{loading ? "Đang xác thực..." : "Đăng nhập"}</span>
          </button>
        </form>

        <div className="relative flex items-center justify-center">
          <div className="absolute inset-0 flex items-center">
            <div className="w-full border-t border-border" />
          </div>
          <span className="relative px-3 bg-card text-[11px] text-zinc-400 font-medium">
            Chưa có tài khoản?
          </span>
        </div>

        <Link
          href={`/register?redirect=${encodeURIComponent(redirectUrl)}`}
          className="w-full flex items-center justify-center gap-2 py-2.5 px-4 rounded-2xl bg-zinc-100 dark:bg-zinc-800 hover:bg-zinc-200 dark:hover:bg-zinc-700 text-zinc-800 dark:text-zinc-200 font-semibold text-xs transition-colors"
        >
          <span>Tạo tài khoản mới</span>
          <ArrowRight className="w-3.5 h-3.5" />
        </Link>
      </div>

      {/* Guest Notice */}
      <div className="text-center">
        <Link
          href={redirectUrl}
          className="inline-flex items-center gap-1 text-xs text-zinc-500 hover:text-zinc-700 dark:hover:text-zinc-300 font-medium transition-colors"
        >
          <Sparkles className="w-3.5 h-3.5 text-amber-500" />
          <span>Tiếp tục chơi với tư cách Khách</span>
        </Link>
      </div>
    </div>
  );
}

export default function LoginPage() {
  return (
    <Suspense fallback={<div className="text-center py-12 text-sm text-zinc-400">Đang tải...</div>}>
      <LoginForm />
    </Suspense>
  );
}
