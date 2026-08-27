"use client";

import React, { useState, useEffect } from "react";
import { AdminSettings } from "@/types/admin";
import { getAdminSettings, updateAdminSettings, resetAdminSettings } from "@/lib/adminApi";
import {
  Sliders,
  Sparkles,
  Compass,
  Dice5,
  Save,
  RotateCcw,
  Loader2,
  CheckCircle2,
  AlertCircle,
  Info
} from "lucide-react";

export default function EngineSettingsPage() {
  const [settings, setSettings] = useState<AdminSettings | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const loadSettings = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await getAdminSettings();
      setSettings(data);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : "Lỗi khi tải cấu hình thuật toán");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadSettings();
  }, []);

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!settings) return;

    try {
      setSaving(true);
      setError(null);
      setSuccessMessage(null);
      const updated = await updateAdminSettings(settings);
      setSettings(updated);
      setSuccessMessage("Cấu hình hệ thống thuật toán đã được lưu và có hiệu lực ngay lập tức!");
      setTimeout(() => setSuccessMessage(null), 4000);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : "Lỗi khi lưu cấu hình");
    } finally {
      setSaving(false);
    }
  };

  const handleReset = async () => {
    if (!confirm("Bạn có chắc chắn muốn khôi phục tất cả thông số thuật toán về giá trị mặc định?")) return;

    try {
      setSaving(true);
      setError(null);
      setSuccessMessage(null);
      const res = await resetAdminSettings();
      setSettings(res);
      setSuccessMessage("Đã khôi phục toàn bộ cấu hình về mặc định thành công.");
      setTimeout(() => setSuccessMessage(null), 4000);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : "Lỗi khi khôi phục mặc định");
    } finally {
      setSaving(false);
    }
  };

  if (loading && !settings) {
    return (
      <div className="flex min-h-[50vh] flex-col items-center justify-center gap-3">
        <Loader2 className="h-8 w-8 animate-spin text-amber-500" />
        <p className="text-sm text-zinc-400">Đang tải cấu hình thuật toán...</p>
      </div>
    );
  }

  return (
    <div className="space-y-8 max-w-5xl">
      {/* Header */}
      <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-zinc-100 flex items-center gap-2.5">
            <Sliders className="h-6 w-6 text-amber-400" /> Cấu hình Thuật toán (Engine Parameters)
          </h1>
          <p className="mt-1 text-sm text-zinc-400">
            Tinh chỉnh trọng số sinh số tâm linh, chọn câu hỏi và chiến lược Thần Tài theo thời gian thực.
          </p>
        </div>
      </div>

      {/* Info Notice */}
      <div className="rounded-2xl border border-amber-500/20 bg-amber-500/10 p-4 text-xs text-amber-300 flex items-start gap-3">
        <Info className="h-5 w-5 shrink-0 text-amber-400" />
        <div>
          <strong className="font-bold">Cập nhật thời gian thực (Hot-Reload):</strong>
          <p className="mt-0.5 text-zinc-300">
            Các thay đổi cấu hình được lưu vào cơ sở dữ liệu và áp dụng tức thì vào các lượt sinh số tiếp theo mà không cần khởi động lại Backend server.
          </p>
        </div>
      </div>

      {/* Status Messages */}
      {successMessage && (
        <div className="rounded-2xl border border-emerald-500/30 bg-emerald-500/10 p-4 text-xs text-emerald-300 flex items-center gap-2 font-semibold">
          <CheckCircle2 className="h-4 w-4 text-emerald-400 shrink-0" />
          {successMessage}
        </div>
      )}

      {error && (
        <div className="rounded-2xl border border-red-500/30 bg-red-500/10 p-4 text-xs text-red-400 flex items-center gap-2">
          <AlertCircle className="h-4 w-4 text-red-400 shrink-0" />
          {error}
        </div>
      )}

      {settings && (
        <form onSubmit={handleSave} className="space-y-8">
          {/* Section 1: Lucky Number Engine */}
          <div className="rounded-2xl border border-zinc-800 bg-zinc-900/40 p-6 space-y-5 backdrop-blur">
            <div className="flex items-center gap-2.5 text-amber-400 border-b border-zinc-800/80 pb-3">
              <Sparkles className="h-5 w-5" />
              <h2 className="text-base font-bold text-zinc-100">
                1. Lucky Number Engine (Thuật toán Sinh số Tâm linh)
              </h2>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-5 text-xs">
              <div>
                <label className="block font-semibold text-zinc-300 mb-1">
                  Trọng số cơ sở (BaseWeight): <span className="font-mono text-amber-400">{settings.lucky.baseWeight}</span>
                </label>
                <input
                  type="number"
                  step="0.5"
                  min="1"
                  max="100"
                  value={settings.lucky.baseWeight}
                  onChange={(e) =>
                    setSettings({
                      ...settings,
                      lucky: { ...settings.lucky, baseWeight: parseFloat(e.target.value) || 10.0 }
                    })
                  }
                  className="w-full rounded-xl border border-zinc-800 bg-zinc-950 px-3.5 py-2 text-sm text-zinc-100 focus:border-amber-500 focus:outline-none"
                />
                <span className="text-[11px] text-zinc-500">Trọng số khởi điểm cho mỗi số trong dải (mặc định: 10.0).</span>
              </div>

              <div>
                <label className="block font-semibold text-zinc-300 mb-1">
                  Hệ số nhân thuộc tính (TraitAffinityMultiplier): <span className="font-mono text-amber-400">{settings.lucky.traitAffinityMultiplier}</span>
                </label>
                <input
                  type="number"
                  step="0.5"
                  min="0"
                  max="50"
                  value={settings.lucky.traitAffinityMultiplier}
                  onChange={(e) =>
                    setSettings({
                      ...settings,
                      lucky: { ...settings.lucky, traitAffinityMultiplier: parseFloat(e.target.value) || 5.0 }
                    })
                  }
                  className="w-full rounded-xl border border-zinc-800 bg-zinc-950 px-3.5 py-2 text-sm text-zinc-100 focus:border-amber-500 focus:outline-none"
                />
                <span className="text-[11px] text-zinc-500">Mức độ tác động của lựa chọn người chơi tới con số (mặc định: 5.0).</span>
              </div>

              <div>
                <label className="block font-semibold text-zinc-300 mb-1">
                  Độ nhiễu ngẫu nhiên (NoiseMagnitude): <span className="font-mono text-amber-400">±{settings.lucky.noiseMagnitude}</span>
                </label>
                <input
                  type="number"
                  step="0.1"
                  min="0"
                  max="10"
                  value={settings.lucky.noiseMagnitude}
                  onChange={(e) =>
                    setSettings({
                      ...settings,
                      lucky: { ...settings.lucky, noiseMagnitude: parseFloat(e.target.value) || 2.0 }
                    })
                  }
                  className="w-full rounded-xl border border-zinc-800 bg-zinc-950 px-3.5 py-2 text-sm text-zinc-100 focus:border-amber-500 focus:outline-none"
                />
                <span className="text-[11px] text-zinc-500">Khoảng dao động entropy chống thiên vị (mặc định: 2.0).</span>
              </div>

              <div>
                <label className="block font-semibold text-zinc-300 mb-1">
                  Trọng số tối thiểu (MinWeight): <span className="font-mono text-amber-400">{settings.lucky.minWeight}</span>
                </label>
                <input
                  type="number"
                  step="0.1"
                  min="0.1"
                  max="10"
                  value={settings.lucky.minWeight}
                  onChange={(e) =>
                    setSettings({
                      ...settings,
                      lucky: { ...settings.lucky, minWeight: parseFloat(e.target.value) || 1.0 }
                    })
                  }
                  className="w-full rounded-xl border border-zinc-800 bg-zinc-950 px-3.5 py-2 text-sm text-zinc-100 focus:border-amber-500 focus:outline-none"
                />
                <span className="text-[11px] text-zinc-500">Đảm bảo mọi con số luôn có xác suất &gt; 0 (mặc định: 1.0).</span>
              </div>
            </div>
          </div>

          {/* Section 2: Novelty Engine */}
          <div className="rounded-2xl border border-zinc-800 bg-zinc-900/40 p-6 space-y-5 backdrop-blur">
            <div className="flex items-center gap-2.5 text-blue-400 border-b border-zinc-800/80 pb-3">
              <Compass className="h-5 w-5" />
              <h2 className="text-base font-bold text-zinc-100">
                2. Novelty Engine (Thuật toán Chọn Câu hỏi Tươi mới)
              </h2>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-5 text-xs">
              <div>
                <label className="block font-semibold text-zinc-300 mb-1">
                  Điểm thưởng câu hỏi chưa từng thấy (NeverSeenBonus): <span className="font-mono text-blue-400">+{settings.novelty.neverSeenBonus}</span>
                </label>
                <input
                  type="number"
                  step="5"
                  min="0"
                  max="300"
                  value={settings.novelty.neverSeenBonus}
                  onChange={(e) =>
                    setSettings({
                      ...settings,
                      novelty: { ...settings.novelty, neverSeenBonus: parseFloat(e.target.value) || 50.0 }
                    })
                  }
                  className="w-full rounded-xl border border-zinc-800 bg-zinc-950 px-3.5 py-2 text-sm text-zinc-100 focus:border-amber-500 focus:outline-none"
                />
                <span className="text-[11px] text-zinc-500">Ưu tiên câu hỏi mới chưa xuất hiện với người chơi (mặc định: 50.0).</span>
              </div>

              <div>
                <label className="block font-semibold text-zinc-300 mb-1">
                  Phạt câu hỏi vừa xuất hiện (RecentlySeenPenalty): <span className="font-mono text-red-400">-{settings.novelty.recentlySeenPenalty}</span>
                </label>
                <input
                  type="number"
                  step="5"
                  min="0"
                  max="300"
                  value={settings.novelty.recentlySeenPenalty}
                  onChange={(e) =>
                    setSettings({
                      ...settings,
                      novelty: { ...settings.novelty, recentlySeenPenalty: parseFloat(e.target.value) || 70.0 }
                    })
                  }
                  className="w-full rounded-xl border border-zinc-800 bg-zinc-950 px-3.5 py-2 text-sm text-zinc-100 focus:border-amber-500 focus:outline-none"
                />
                <span className="text-[11px] text-zinc-500">Giảm xác suất lặp lại câu hỏi vừa thấy (mặc định: 70.0).</span>
              </div>

              <div>
                <label className="block font-semibold text-zinc-300 mb-1">
                  Phạt lặp chủ đề trong lượt (RepeatedThemePenalty): <span className="font-mono text-red-400">-{settings.novelty.repeatedThemePenalty}</span>
                </label>
                <input
                  type="number"
                  step="5"
                  min="0"
                  max="300"
                  value={settings.novelty.repeatedThemePenalty}
                  onChange={(e) =>
                    setSettings({
                      ...settings,
                      novelty: { ...settings.novelty, repeatedThemePenalty: parseFloat(e.target.value) || 60.0 }
                    })
                  }
                  className="w-full rounded-xl border border-zinc-800 bg-zinc-950 px-3.5 py-2 text-sm text-zinc-100 focus:border-amber-500 focus:outline-none"
                />
                <span className="text-[11px] text-zinc-500">Đa dạng hóa chủ đề trong 1 dòng chơi (mặc định: 60.0).</span>
              </div>

              <div>
                <label className="block font-semibold text-zinc-300 mb-1">
                  Thưởng đa dạng loại câu hỏi (QuestionTypeDiversityBonus): <span className="font-mono text-emerald-400">+{settings.novelty.questionTypeDiversityBonus}</span>
                </label>
                <input
                  type="number"
                  step="5"
                  min="0"
                  max="200"
                  value={settings.novelty.questionTypeDiversityBonus}
                  onChange={(e) =>
                    setSettings({
                      ...settings,
                      novelty: { ...settings.novelty, questionTypeDiversityBonus: parseFloat(e.target.value) || 25.0 }
                    })
                  }
                  className="w-full rounded-xl border border-zinc-800 bg-zinc-950 px-3.5 py-2 text-sm text-zinc-100 focus:border-amber-500 focus:outline-none"
                />
                <span className="text-[11px] text-zinc-500">Tránh 2 câu cùng dạng liên tiếp (mặc định: 25.0).</span>
              </div>
            </div>
          </div>

          {/* Section 3: Random Engine */}
          <div className="rounded-2xl border border-zinc-800 bg-zinc-900/40 p-6 space-y-5 backdrop-blur">
            <div className="flex items-center gap-2.5 text-emerald-400 border-b border-zinc-800/80 pb-3">
              <Dice5 className="h-5 w-5" />
              <h2 className="text-base font-bold text-zinc-100">
                3. Random Engine (Thuật toán Thần Tài)
              </h2>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 gap-5 text-xs">
              <div>
                <label className="block font-semibold text-zinc-300 mb-1">
                  Độ lệch tối đa chiến lược Cân bằng (BalancedMaxDeviation): <span className="font-mono text-emerald-400">{settings.random.balancedMaxDeviation}</span>
                </label>
                <input
                  type="number"
                  min="0"
                  max="3"
                  value={settings.random.balancedMaxDeviation}
                  onChange={(e) =>
                    setSettings({
                      ...settings,
                      random: { ...settings.random, balancedMaxDeviation: parseInt(e.target.value) || 1 }
                    })
                  }
                  className="w-full rounded-xl border border-zinc-800 bg-zinc-950 px-3.5 py-2 text-sm text-zinc-100 focus:border-amber-500 focus:outline-none"
                />
                <span className="text-[11px] text-zinc-500">Độ chẵn/lẻ và cao/thấp cho phép lệch (mặc định: 1).</span>
              </div>

              <div>
                <label className="block font-semibold text-zinc-300 mb-1">
                  Số phân vùng chiến lược Trải đều (SpreadMinPartitions): <span className="font-mono text-emerald-400">{settings.random.spreadMinPartitions}</span>
                </label>
                <input
                  type="number"
                  min="2"
                  max="6"
                  value={settings.random.spreadMinPartitions}
                  onChange={(e) =>
                    setSettings({
                      ...settings,
                      random: { ...settings.random, spreadMinPartitions: parseInt(e.target.value) || 3 }
                    })
                  }
                  className="w-full rounded-xl border border-zinc-800 bg-zinc-950 px-3.5 py-2 text-sm text-zinc-100 focus:border-amber-500 focus:outline-none"
                />
                <span className="text-[11px] text-zinc-500">Chia dải số thành các phân vùng tối thiểu (mặc định: 3).</span>
              </div>
            </div>
          </div>

          {/* Form Actions */}
          <div className="flex items-center justify-between pt-4 border-t border-zinc-800">
            <button
              type="button"
              onClick={handleReset}
              disabled={saving}
              className="inline-flex items-center gap-2 rounded-xl bg-zinc-800 px-4 py-2.5 text-xs font-bold text-zinc-300 hover:bg-zinc-700 transition"
            >
              <RotateCcw className="h-4 w-4" /> Khôi phục mặc định (Reset)
            </button>

            <button
              type="submit"
              disabled={saving}
              className="inline-flex items-center gap-2 rounded-xl bg-amber-500 px-6 py-2.5 text-xs font-extrabold text-zinc-950 hover:bg-amber-400 transition shadow-lg shadow-amber-500/20 disabled:opacity-50"
            >
              {saving && <Loader2 className="h-4 w-4 animate-spin" />}
              <Save className="h-4 w-4" /> Lưu cấu hình
            </button>
          </div>
        </form>
      )}
    </div>
  );
}