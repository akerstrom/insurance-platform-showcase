import { Shield } from 'lucide-react';
import { CustomerSearch } from './components/CustomerSearch';

function App() {
  return (
    <div className="min-h-screen bg-slate-50">
      <header className="sticky top-0 z-10 border-b border-slate-200 bg-white shadow-sm">
        <div className="mx-auto max-w-5xl px-4 py-4">
          <a href="/" className="inline-flex items-center gap-2">
            <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-blue-600">
              <Shield className="h-5 w-5 text-white" />
            </div>
            <span className="text-xl font-bold text-slate-900">ThreadPilot</span>
          </a>
        </div>
      </header>

      <main className="mx-auto max-w-5xl px-4 py-6">
        <CustomerSearch />
      </main>
    </div>
  );
}

export default App;
