interface ErrorMessageProps {
  message: string;
}

export function ErrorMessage({ message }: ErrorMessageProps) {
  return (
    <div role="alert">
      <div>
        <div>
          <h3>Unable to Complete Request</h3>
          <p>{message}</p>
          <p>
            Please verify the personal number and try again. If the problem persists,
            contact support.
          </p>
        </div>
      </div>
    </div>
  );
}
