"use client";

import React, { useState } from "react";
import { AdminQuestionDetail } from "@/types/admin";
import { X, Sparkles, Eye, CheckCircle2 } from "lucide-react";

interface QuestionPreviewModalProps {
  question: AdminQuestionDetail | null;
  isOpen: boolean;
  onClose: () => void;
}

export function QuestionPreviewModal({ question, isOpen, onClose }: QuestionPreviewModalProps) {
  const [selectedChoiceId, setSelectedChoiceId] = useState<number | null>(null);

  if (!isOpen || !question) return null;

  const selectedChoice = question.choices.find((c) => c.id === selectedChoiceId);

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/75 p-4 backdrop-blur-sm animate-in fade-in duration-150">
      <div className="flex w-full max-w-2xl flex-col rounded-2xl border border-zinc-800 bg-zinc-900 shadow-2xl overflow-hidden max-h-[90vh]">
        {/* Header */}
        <div className="flex items-center justify-between border-b border-zinc-800 px-6 py-4 bg-zinc-950/60">
          <div className="flex items-center gap-2 text-amber-400">
            <Eye className="h-5 w-5" />
            <h3 className="font-bold text-zinc-100">Xem trước Trải nghiệm (Simulated Preview)</h3>
          </div>
          <button
            onClick={onClose}
            className="rounded-lg p-1.5 text-zinc-400 hover:bg-zinc-800 hover:text-zinc-200 transition"
          >
            <X className="h-5 w-5" />
          </button>
        </div>

        {/* Content */}
        <div className="overflow-y-auto p-6 space-y-6">
          <div className="rounded-xl bg-amber-500/10 p-3 text-xs text-amber-300 border border-amber-500/20">
            💡 Chế độ xem trước mô phỏng giao diện người chơi. Không phát sinh số ngẫu nhiên, không ghi lịch sử hoặc ảnh hưởng Novelty Engine.
          </div>

          {/* Question Card Display */}
          <div className="rounded-2xl border border-amber-500/30 bg-gradient-to-b from-zinc-900 to-zinc-950 p-6 shadow-lg">
            <div className="flex items-center justify-between text-xs font-semibold text-amber-400 mb-3">
              <span className="rounded-full bg-amber-500/20 px-2.5 py-0.5 border border-amber-500/30">
                {question.themeName} ({question.themeCode})
              </span>
              <span className="text-zinc-400">{question.questionType}</span>
            </div>

            <h2 className="text-xl font-bold text-zinc-100 leading-snug">{question.content}</h2>
            {question.subtitle && (
              <p className="mt-1.5 text-sm text-zinc-400 italic">{question.subtitle}</p>
            )}

            {/* Choices */}
            <div className="mt-6 space-y-3">
              {question.choices.map((choice, idx) => {
                const isSelected = selectedChoiceId === choice.id;
                return (
                  <button
                    key={choice.id || idx}
                    type="button"
                    onClick={() => setSelectedChoiceId(choice.id)}
                    className={`w-full text-left rounded-xl p-4 transition-all flex items-center justify-between border ${
                      isSelected
                        ? "border-amber-500 bg-amber-500/15 shadow-md shadow-amber-500/10 text-zinc-100"
                        : "border-zinc-800 bg-zinc-900/80 hover:border-zinc-700 hover:bg-zinc-800 text-zinc-300"
                    } ${!choice.isActive ? "opacity-50 line-through" : ""}`}
                  >
                    <div>
                      <div className="font-semibold text-sm">{choice.content}</div>
                      {choice.subContent && (
                        <div className="text-xs text-zinc-400 mt-0.5">{choice.subContent}</div>
                      )}
                    </div>
                    {isSelected && <CheckCircle2 className="h-5 w-5 text-amber-400 shrink-0 ml-3" />}
                  </button>
                );
              })}
            </div>
          </div>

          {/* Trait breakdown of selected choice */}
          {selectedChoice && (
            <div className="rounded-xl border border-zinc-800 bg-zinc-950/60 p-4 space-y-3">
              <div className="flex items-center gap-2 text-xs font-bold text-zinc-300 uppercase tracking-wider">
                <Sparkles className="h-4 w-4 text-amber-400" />
                Thuộc tính tác động đến Lucky Number:
              </div>

              {selectedChoice.choiceTraits.length === 0 ? (
                <p className="text-xs text-zinc-400 italic">Lựa chọn này chưa gắn trọng số thuộc tính nào.</p>
              ) : (
                <div className="grid grid-cols-2 gap-2">
                  {selectedChoice.choiceTraits.map((ct) => (
                    <div
                      key={ct.traitId}
                      className="flex items-center justify-between rounded-lg bg-zinc-900 px-3 py-2 text-xs border border-zinc-800"
                    >
                      <span className="font-medium text-zinc-200">{ct.traitName || ct.traitCode}</span>
                      <span className="font-bold text-amber-400">+{ct.weight.toFixed(2)}</span>
                    </div>
                  ))}
                </div>
              )}
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="flex justify-end border-t border-zinc-800 px-6 py-4 bg-zinc-950/60">
          <button
            type="button"
            onClick={onClose}
            className="rounded-lg bg-zinc-800 px-5 py-2 text-sm font-medium text-zinc-200 hover:bg-zinc-700 transition"
          >
            Đóng xem trước
          </button>
        </div>
      </div>
    </div>
  );
}