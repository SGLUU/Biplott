"use client";

import { useRouter } from "next/navigation";
import { Lock, Sparkles, X, UserPlus, LogIn } from "lucide-react";

interface SaveSlipPromptModalProps {
  isOpen: boolean;
  gameCode: string;
  onClose: () => void;
}

export function SaveSlipPromptModal({
  isOpen,
  gameCode,
  onClose
}: SaveSlipPromptModalProps) {
  const router = useRouter();

  if (!isOpen) return null;

  const returnUrl = `/play/${encodeURIComponent(gameCode)}`;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm animate-in fade-in duration-150">
      <div className="relative w-full max-w-md bg-white dark:bg-zinc-900 border border-zinc-200 dark:border-zinc-800 rounded-3xl p-6 shadow-2xl space-y-6">
        {/* Close button */}
        <button
          type="button"
          onClick={onClose}
          className="absolute top-4 right-4 p-2 rounded-full text-zinc-400 hover:text-zinc-600 dark:hover:text-zinc-200 hover:bg-zinc-100 dark:hover:bg-zinc-800 transition-colors"
        >
          <X className="w-5 h-5" />
        </button>

        {/* Icon & Title */}
        <div className="text-center space-y-2 pt-2">
          <div className="mx-auto w-14 h-14 rounded-2xl bg-gradient-to-tr from-rose-500/20 via-orange-500/20 to-amber-500/20 border border-orange-500/30 flex items-center justify-center text-orange-500">
            <Lock className="w-7 h-7" />
          </div>
          <h2 className="text-xl font-bold text-zinc-900 dark:text-zinc-100">
            Đăng nhập để giữ lại lần nát này
          </h2>
          <p className="text-xs text-zinc-500 dark:text-zinc-400 max-w-xs mx-auto">
            Bạn đang chơi ở chế độ khách vãng lai. Hãy đăng nhập hoặc tạo tài khoản để lưu lại toàn bộ dãy số và câu chuyện Lucky!
          </p>
        </div>

        {/* Notice */}
        <div className="flex items-center gap-2.5 p-3 rounded-2xl bg-amber-50 dark:bg-amber-950/30 border border-amber-200 dark:border-amber-800/40 text-xs text-amber-700 dark:text-amber-300">
          <Sparkles className="w-4 h-4 text-amber-500 flex-shrink-0" />
          <span>Phiếu số hiện tại của bạn sẽ được giữ nguyên sau khi đăng nhập.</span>
        </div>

        {/* Action Buttons */}
        <div className="space-y-2.5 pt-2">
          <button
            type="button"
            onClick={() => {
              onClose();
              router.push(`/login?redirect=${encodeURIComponent(returnUrl)}`);
            }}
            className="w-full flex items-center justify-center gap-2 py-3 px-4 rounded-2xl bg-gradient-to-r from-orange-600 to-amber-500 hover:from-orange-500 hover:to-amber-400 text-white font-bold text-sm shadow-lg shadow-orange-500/25 hover:scale-[1.02] active:scale-[0.98] transition-all"
          >
            <LogIn className="w-4 h-4" />
            <span>Đăng nhập ngay</span>
          </button>

          <button
            type="button"
            onClick={() => {
              onClose();
              router.push(`/register?redirect=${encodeURIComponent(returnUrl)}`);
            }}
            className="w-full flex items-center justify-center gap-2 py-3 px-4 rounded-2xl bg-zinc-100 dark:bg-zinc-800 hover:bg-zinc-200 dark:hover:bg-zinc-700 text-zinc-800 dark:text-zinc-200 font-semibold text-sm transition-colors"
          >
            <UserPlus className="w-4 h-4" />
            <span>Tạo tài khoản mới</span>
          </button>

          <button
            type="button"
            onClick={onClose}
            className="w-full py-2 text-xs font-medium text-zinc-400 hover:text-zinc-600 dark:hover:text-zinc-200 transition-colors"
          >
            Để sau (tiếp tục chơi)
          </button>
        </div>
      </div>
    </div>
  );
}
