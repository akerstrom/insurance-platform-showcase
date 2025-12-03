import { AlertCircle } from 'lucide-react';

interface ErrorMessageProps {
  message: string;
}

export function ErrorMessage({ message }: ErrorMessageProps) {
  return (
    <div
      role="alert"
      className="rounded-xl border border-red-200 bg-red-50 p-5"
    >
      <div className="flex gap-4">
        <AlertCircle className="h-6 w-6 shrink-0 text-red-500" />
        <div>
          <h3 className="font-semibold text-red-800">Unable to Complete Request</h3>
          <p className="mt-1 text-sm text-red-700">{message}</p>
          <p className="mt-2 text-sm text-red-600">
            Please verify the personal number and try again. If the problem persists,
            contact support.
          </p>
        </div>
      </div>
    </div>
  );
}
