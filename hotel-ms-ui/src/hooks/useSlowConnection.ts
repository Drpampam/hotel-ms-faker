import { useEffect, useRef } from 'react';
import { useToast } from '../lib/store';

/**
 * Shows a "Server is waking up…" toast if the loading state stays true for longer
 * than `thresholdMs` (default 8 s). Useful for Render.com free-tier cold starts.
 */
export function useSlowConnection(isLoading: boolean, thresholdMs = 8000) {
  const toast = useToast();
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const shownRef = useRef(false);

  useEffect(() => {
    if (isLoading && !shownRef.current) {
      timerRef.current = setTimeout(() => {
        toast.info(
          'Server is waking up',
          'The server may be starting up after inactivity. This usually takes 20–40 seconds on the first load.'
        );
        shownRef.current = true;
      }, thresholdMs);
    }

    if (!isLoading) {
      if (timerRef.current) {
        clearTimeout(timerRef.current);
        timerRef.current = null;
      }
      // Reset so the warning can fire again after a full page refresh
      shownRef.current = false;
    }

    return () => {
      if (timerRef.current) clearTimeout(timerRef.current);
    };
  }, [isLoading, thresholdMs, toast]);
}
