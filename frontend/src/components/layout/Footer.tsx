import { ShieldAlert, Heart } from "lucide-react";

export function Footer() {
  return (
    <footer className="w-full border-t border-zinc-200 dark:border-zinc-800/80 bg-zinc-50 dark:bg-zinc-950/80 py-8 px-4 mt-auto transition-colors">
      <div className="max-w-4xl mx-auto flex flex-col items-center text-center gap-4">
        {/* Satirical Disclaimer Box */}
        <div className="flex items-start sm:items-center gap-3 p-4 rounded-2xl bg-amber-500/10 border border-amber-500/20 text-amber-900 dark:text-amber-200 text-xs sm:text-sm leading-relaxed max-w-2xl text-left sm:text-center">
          <ShieldAlert className="w-5 h-5 text-amber-500 shrink-0 mt-0.5 sm:mt-0" />
          <p>
            <strong className="font-semibold text-amber-700 dark:text-amber-400">Tuyên bố độc lập & miễn trừ trách nhiệm: </strong>
            Bịp lót là nền tảng giải trí và châm biếm độc lập, KHÔNG thuộc sở hữu hay liên kết với Vietlott hay bất kỳ công ty xổ số nào. 
            Mọi tính toán chỉ mang tính giải trí cá nhân hóa và KHÔNG cam kết làm tăng xác suất trúng thưởng.
          </p>
        </div>

        {/* Tagline & Copyright */}
        <div className="flex flex-col sm:flex-row items-center gap-2 text-xs text-zinc-500 dark:text-zinc-400">
          <span>© {new Date().getFullYear()} Bịp lót — <em>Cơ hội để nát hơn</em>.</span>
          <span className="hidden sm:inline">•</span>
          <span className="flex items-center gap-1">
            Thiết kế với <Heart className="w-3.5 h-3.5 text-rose-500 fill-rose-500" /> cho dân văn phòng & người yêu meme.
          </span>
        </div>
      </div>
    </footer>
  );
}
