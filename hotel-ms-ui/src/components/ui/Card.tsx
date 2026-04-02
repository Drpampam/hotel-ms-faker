import React from 'react';
import { cn } from '../../lib/utils';

interface CardProps {
  children: React.ReactNode;
  className?: string;
  glass?: boolean;
  hover?: boolean;
  padding?: 'none' | 'sm' | 'md' | 'lg';
  onClick?: () => void;
}

const paddings = {
  none: '',
  sm: 'p-4',
  md: 'p-6',
  lg: 'p-8',
};

export function Card({ children, className, glass = false, hover = false, padding = 'md', onClick }: CardProps) {
  return (
    <div
      onClick={onClick}
      className={cn(
        'rounded-2xl border transition-all duration-200',
        glass
          ? 'bg-white/70 dark:bg-slate-800/70 backdrop-blur-md border-white/20 dark:border-slate-700/50'
          : 'bg-white dark:bg-slate-800 border-slate-100 dark:border-slate-700',
        hover && 'hover:shadow-md cursor-pointer',
        'shadow-sm',
        paddings[padding],
        className
      )}
    >
      {children}
    </div>
  );
}

export function CardHeader({
  children,
  className,
}: {
  children: React.ReactNode;
  className?: string;
}) {
  return (
    <div className={cn('flex items-center justify-between mb-4', className)}>{children}</div>
  );
}

export function CardTitle({
  children,
  className,
}: {
  children: React.ReactNode;
  className?: string;
}) {
  return (
    <h3 className={cn('text-base font-semibold text-slate-900 dark:text-slate-100', className)}>
      {children}
    </h3>
  );
}
