import { type ClassValue, clsx } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

export function formatNumber(num: number): string {
  return num < 10 ? `0${num}` : `${num}`;
}

export interface SortableSlipNumber {
  value: number;
  poolIndex?: number;
}

export function sortSlipNumbers<T extends SortableSlipNumber>(numbers: T[]): T[] {
  return [...numbers].sort((a, b) => {
    const poolDiff = (a.poolIndex ?? 0) - (b.poolIndex ?? 0);
    if (poolDiff !== 0) return poolDiff;
    return a.value - b.value;
  });
}

export function getOrCreateGuestSessionToken(): string {
  if (typeof window === "undefined") return "";
  let token = localStorage.getItem("guestSessionToken");
  if (!token) {
    token = "GUEST-" + Math.random().toString(36).substring(2, 15).toUpperCase();
    localStorage.setItem("guestSessionToken", token);
  }
  return token;
}
