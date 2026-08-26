"use client";

import React, { useState, useEffect } from "react";
import { QuestionDto } from "@/types/lucky";
import { Sparkles, Timer, Zap, Swords } from "lucide-react";

interface QuestionCardProps {
  question: QuestionDto;
  onSelectChoice: (choiceId: number) => void;
  isSubmitting: boolean;
  selectedChoiceId: number | null;
}

export function QuestionCard({
  question,
  onSelectChoice,
  isSubmitting,
  selectedChoiceId
}: QuestionCardProps) {
  // QuickInstinct countdown timer (5 seconds)
  const [timeLeft, setTimeLeft] = useState<number>(5);

  useEffect(() => {
    if (question.questionType === "QuickInstinct") {
      setTimeLeft(5);
      const timer = setInterval(() => {
        setTimeLeft((prev) => {
          if (prev <= 1) {
            clearInterval(timer);
            return 0;
          }
          return prev - 1;
        });
      }, 1000);

      return () => clearInterval(timer);
    }
  }, [question.id, question.questionType]);

  const isThisOrThat = question.questionType === "ThisOrThat" && question.choices.length === 2;
  const isScenario = question.questionType === "Scenario";
  const isQuickInstinct = question.questionType === "QuickInstinct";

  return (
    <div className="space-y-6 animate-in fade-in zoom-in-95 duration-200">
      {/* Theme & Question Type Banner */}
      <div className="flex items-center justify-between gap-2 flex-wrap">
        <div className="inline-flex items-center gap-2 px-3 py-1.5 rounded-xl bg-gradient-to-r from-rose-500/10 via-orange-500/10 to-amber-500/10 border border-rose-500/20 text-rose-600 dark:text-rose-400 font-extrabold text-xs">
          <span className="text-base leading-none">{question.themeIcon || "✨"}</span>
          <span className="uppercase tracking-wider">{question.themeName}</span>
        </div>

        {isQuickInstinct && (
          <div className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full bg-amber-500/15 text-amber-600 dark:text-amber-400 border border-amber-500/30 text-xs font-black animate-pulse">
            <Timer className="w-3.5 h-3.5" />
            <span>Phản xạ: {timeLeft}s</span>
          </div>
        )}

        {isScenario && (
          <div className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full bg-blue-500/10 text-blue-600 dark:text-blue-400 border border-blue-500/20 text-xs font-bold">
            <Zap className="w-3.5 h-3.5" />
            <span>Tình huống kịch tính</span>
          </div>
        )}

        {isThisOrThat && (
          <div className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full bg-purple-500/10 text-purple-600 dark:text-purple-400 border border-purple-500/20 text-xs font-bold">
            <Swords className="w-3.5 h-3.5" />
            <span>Đối đầu 1v1</span>
          </div>
        )}
      </div>

      {/* Question Content */}
      <div className="space-y-2 text-center sm:text-left">
        <h3 className="text-lg sm:text-2xl font-black text-foreground tracking-tight leading-snug">
          {question.content}
        </h3>
        {question.subtitle && (
          <p className="text-xs sm:text-sm text-muted-foreground font-medium">
            {question.subtitle}
          </p>
        )}
      </div>

      {/* Choices Grid */}
      {isThisOrThat ? (
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 relative pt-2">
          {question.choices.map((choice, idx) => {
            const isSelected = selectedChoiceId === choice.id;
            return (
              <button
                key={choice.id}
                type="button"
                disabled={isSubmitting}
                onClick={() => onSelectChoice(choice.id)}
                className={`
                  relative flex flex-col items-center justify-center p-6 rounded-3xl border-2 text-center transition-all duration-200 cursor-pointer min-h-[130px]
                  ${
                    isSelected
                      ? "border-rose-500 bg-rose-500/10 text-rose-600 dark:text-rose-400 shadow-xl shadow-rose-500/20 scale-[1.02]"
                      : "border-border hover:border-primary/60 bg-card hover:bg-muted/40 text-foreground shadow-sm hover:shadow-md active:scale-95"
                  }
                  ${isSubmitting && !isSelected ? "opacity-40 cursor-not-allowed" : ""}
                `}
              >
                <span className="text-xs font-extrabold text-muted-foreground uppercase tracking-widest mb-2">
                  {idx === 0 ? "Lựa chọn A" : "Lựa chọn B"}
                </span>
                <span className="font-extrabold text-base sm:text-lg leading-snug">
                  {choice.content}
                </span>
                {choice.subContent && (
                  <span className="text-xs text-muted-foreground mt-1">
                    {choice.subContent}
                  </span>
                )}
              </button>
            );
          })}
        </div>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-2.5 pt-1">
          {question.choices.map((choice, idx) => {
            const isSelected = selectedChoiceId === choice.id;
            return (
              <button
                key={choice.id}
                type="button"
                disabled={isSubmitting}
                onClick={() => onSelectChoice(choice.id)}
                className={`
                  group relative flex items-start gap-3 p-4 rounded-2xl border text-left transition-all duration-200 cursor-pointer
                  ${
                    isSelected
                      ? "border-rose-500 bg-rose-500/10 text-rose-600 dark:text-rose-400 shadow-lg shadow-rose-500/20 ring-1 ring-rose-500 scale-[1.01]"
                      : "border-border hover:border-primary/50 bg-card hover:bg-muted/40 text-foreground shadow-sm hover:shadow active:scale-98"
                  }
                  ${isSubmitting && !isSelected ? "opacity-40 cursor-not-allowed" : ""}
                `}
              >
                {/* Index badge */}
                <div
                  className={`
                    w-7 h-7 rounded-xl flex items-center justify-center font-black text-xs flex-shrink-0 transition-colors mt-0.5
                    ${
                      isSelected
                        ? "bg-rose-500 text-white"
                        : "bg-muted text-muted-foreground group-hover:bg-primary group-hover:text-primary-foreground"
                    }
                  `}
                >
                  {String.fromCharCode(65 + idx)}
                </div>

                <div className="flex-1 min-w-0">
                  <span className="font-bold text-sm leading-snug block">
                    {choice.content}
                  </span>
                  {choice.subContent && (
                    <span className="text-xs text-muted-foreground block mt-0.5">
                      {choice.subContent}
                    </span>
                  )}
                </div>
              </button>
            );
          })}
        </div>
      )}

      {/* Submitting indicator */}
      {isSubmitting && (
        <div className="flex items-center justify-center gap-2 py-2 text-xs font-bold text-rose-500 animate-pulse">
          <Sparkles className="w-4 h-4" />
          <span>Bịp lót đang suy ngẫm số phận và tính toán năng lượng...</span>
        </div>
      )}
    </div>
  );
}
