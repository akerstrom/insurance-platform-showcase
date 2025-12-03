export function LoadingSpinner() {
  return (
    <div role="status" aria-label="Loading" className="flex items-center gap-2">
      <div className="h-4 w-4 animate-spin rounded-full border-2 border-blue-600 border-t-transparent" />
      <span className="sr-only">Loading...</span>
    </div>
  );
}
