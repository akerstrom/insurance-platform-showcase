import { FileSearch, SearchX } from 'lucide-react';

interface EmptyStateProps {
  type: 'initial' | 'no-results';
  title: string;
  description: string;
}

export function EmptyState({ type, title, description }: EmptyStateProps) {
  const Icon = type === 'initial' ? FileSearch : SearchX;

  return (
    <div className="flex flex-col items-center justify-center py-16 text-center">
      <Icon className="mb-4 h-16 w-16 text-slate-300" />
      <h3 className="text-lg font-medium text-slate-700">{title}</h3>
      <p className="mt-1 text-sm text-slate-500">{description}</p>
    </div>
  );
}
