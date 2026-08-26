"use client";

import { useEffect } from "react";
import { useAuthStore } from "@/stores/useAuthStore";

export function AuthInitializer() {
  const initAuth = useAuthStore((s) => s.initAuth);

  useEffect(() => {
    initAuth();
  }, [initAuth]);

  return null;
}
