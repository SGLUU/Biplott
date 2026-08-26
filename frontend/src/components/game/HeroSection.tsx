import { Flame, Sparkles, Wand2, Shield, HeartHandshake } from "lucide-react";

export function HeroSection() {
  return (
    <section className="relative w-full pt-6 pb-10 sm:pt-10 sm:pb-14 flex flex-col items-center text-center px-4 overflow-hidden">
      {/* Background Glows */}
      <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-96 h-96 bg-gradient-to-tr from-rose-500/15 via-orange-500/15 to-amber-400/10 rounded-full blur-3xl -z-10 pointer-events-none" />

      {/* Pill Badge */}
      <div className="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-full bg-gradient-to-r from-rose-500/10 via-orange-500/10 to-amber-500/10 border border-orange-500/20 text-orange-600 dark:text-orange-400 text-xs font-bold mb-6 shadow-sm">
        <Flame className="w-4 h-4 text-rose-500 animate-bounce" />
        <span>Nền tảng sinh số châm biếm & giải trí số 1 Việt Nam</span>
      </div>

      {/* Main Heading */}
      <h1 className="text-3xl sm:text-5xl lg:text-6xl font-black tracking-tight text-zinc-950 dark:text-zinc-50 max-w-3xl leading-[1.15] mb-4">
        Cơ hội để{" "}
        <span className="bg-gradient-to-r from-rose-600 via-orange-500 to-amber-500 bg-clip-text text-transparent underline decoration-amber-400/40 decoration-wavy decoration-2">
          nát hơn
        </span>{" "}
        cùng số phận!
      </h1>

      {/* Subheading */}
      <p className="text-base sm:text-lg text-zinc-600 dark:text-zinc-300 max-w-2xl leading-relaxed mb-8">
        Không còn phải vò đầu bứt tai chọn ngày sinh hay biển số xe. Trải nghiệm mở từng con số qua các câu chuyện công sở, tình duyên và tâm linh hài hước!
      </p>

      {/* Quick Feature Highlights */}
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-3 w-full max-w-3xl text-left">
        <div className="p-3.5 rounded-2xl bg-white/70 dark:bg-zinc-900/60 border border-zinc-200/70 dark:border-zinc-800/70 shadow-sm backdrop-blur-sm">
          <div className="w-8 h-8 rounded-xl bg-rose-500/10 text-rose-600 dark:text-rose-400 flex items-center justify-center mb-2">
            <Wand2 className="w-4 h-4" />
          </div>
          <h4 className="font-bold text-xs text-zinc-900 dark:text-zinc-100">Lucky Journey</h4>
          <p className="text-[11px] text-zinc-500 dark:text-zinc-400">1 câu hỏi = 1 số mở ngay</p>
        </div>

        <div className="p-3.5 rounded-2xl bg-white/70 dark:bg-zinc-900/60 border border-zinc-200/70 dark:border-zinc-800/70 shadow-sm backdrop-blur-sm">
          <div className="w-8 h-8 rounded-xl bg-orange-500/10 text-orange-600 dark:text-orange-400 flex items-center justify-center mb-2">
            <Sparkles className="w-4 h-4" />
          </div>
          <h4 className="font-bold text-xs text-zinc-900 dark:text-zinc-100">Thần Tài Random</h4>
          <p className="text-[11px] text-zinc-500 dark:text-zinc-400">4 phong cách số học</p>
        </div>

        <div className="p-3.5 rounded-2xl bg-white/70 dark:bg-zinc-900/60 border border-zinc-200/70 dark:border-zinc-800/70 shadow-sm backdrop-blur-sm">
          <div className="w-8 h-8 rounded-xl bg-amber-500/10 text-amber-600 dark:text-amber-400 flex items-center justify-center mb-2">
            <HeartHandshake className="w-4 h-4" />
          </div>
          <h4 className="font-bold text-xs text-zinc-900 dark:text-zinc-100">Mixed Mode</h4>
          <p className="text-[11px] text-zinc-500 dark:text-zinc-400">Tự do phối trộn nguồn số</p>
        </div>

        <div className="p-3.5 rounded-2xl bg-white/70 dark:bg-zinc-900/60 border border-zinc-200/70 dark:border-zinc-800/70 shadow-sm backdrop-blur-sm">
          <div className="w-8 h-8 rounded-xl bg-zinc-500/10 text-zinc-600 dark:text-zinc-400 flex items-center justify-center mb-2">
            <Shield className="w-4 h-4" />
          </div>
          <h4 className="font-bold text-xs text-zinc-900 dark:text-zinc-100">Chơi ngay (Guest)</h4>
          <p className="text-[11px] text-zinc-500 dark:text-zinc-400">Không cần đăng nhập</p>
        </div>
      </div>
    </section>
  );
}
