import { useState } from 'react';
import type { FormEvent } from 'react';
import { Search } from 'lucide-react';
import type { CustomerInsurance } from '../types/api';
import {
  ApiException,
  customerService,
} from '../services/customerService';
import { InsuranceCard } from './InsuranceCard';
import { ErrorMessage } from './ErrorMessage';
import { LoadingSpinner } from './LoadingSpinner';
import { PolicySummary } from './PolicySummary';
import { EmptyState } from './EmptyState';

export function CustomerSearch() {
  const [pid, setPid] = useState('');
  const [insurances, setInsurances] = useState<CustomerInsurance[] | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);
    setInsurances(null);

    try {
      const data = await customerService.getCustomerInsurances(pid);
      setInsurances(data);
    } catch (err) {
      if (err instanceof ApiException) {
        setError(err.message);
      } else {
        setError('An unexpected error occurred');
      }
    } finally {
      setLoading(false);
    }
  };

  const hasResults = insurances && insurances.length > 0;

  return (
    <div className="space-y-6">
      <div className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
        <div className="max-w-xl">
          <h1 className="text-2xl font-bold text-slate-900">Customer Insurance Lookup</h1>
          <p className="mt-1 text-sm text-slate-500">Search by Swedish personal number (personnummer).</p>

          <form onSubmit={handleSubmit} className="mt-6">
            <div className="flex flex-col gap-3 sm:flex-row">
              <div className="flex-1">
                <label htmlFor="pid" className="sr-only">Personal Number</label>
                <input
                  type="text"
                  id="pid"
                  value={pid}
                  onChange={(e) => setPid(e.target.value)}
                  placeholder="199001011234"
                  disabled={loading}
                  autoComplete="off"
                  inputMode="numeric"
                  pattern="\d*"
                  className="w-full rounded-lg border border-slate-300 px-4 py-3 text-slate-900 placeholder-slate-400 transition-colors focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20 disabled:bg-slate-50 disabled:text-slate-500"
                />
              </div>

              <button
                type="submit"
                disabled={loading}
                className="inline-flex items-center justify-center gap-2 rounded-lg bg-blue-600 px-6 py-3 font-medium text-white transition-colors hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:cursor-not-allowed disabled:bg-blue-400"
              >
                {loading ? (
                  <>
                    <LoadingSpinner />
                    <span>Searching</span>
                  </>
                ) : (
                  <>
                    <Search className="h-4 w-4" />
                    <span>Search</span>
                  </>
                )}
              </button>
            </div>

            <p className="mt-2 text-xs text-slate-400">Format: YYYYMMDDXXXX (12 digits)</p>
          </form>

          <div className="mt-6 rounded-lg border border-amber-200 bg-amber-50 p-4">
            <p className="text-xs font-medium uppercase tracking-wide text-amber-700">Test customers</p>
            <ul className="mt-2 space-y-1 text-sm text-amber-800">
              <li><code className="rounded bg-amber-100 px-1.5 py-0.5 font-mono text-xs">199001011234</code> — Car, Pet, Health</li>
              <li><code className="rounded bg-amber-100 px-1.5 py-0.5 font-mono text-xs">198505152345</code> — Car only</li>
              <li><code className="rounded bg-amber-100 px-1.5 py-0.5 font-mono text-xs">197212123456</code> — Pet, Health</li>
            </ul>
          </div>
        </div>
      </div>

      <div className="grid gap-6 md:grid-cols-3">
        <div className="space-y-4 md:col-span-2">
          {loading && <LoadingSkeletons />}

          {error && <ErrorMessage message={error} />}

          {!loading && !error && !insurances && (
            <div className="rounded-xl border border-slate-200 bg-white shadow-sm">
              <EmptyState
                type="initial"
                title="Ready to search"
                description="Enter a personal number above to find insurance policies."
              />
            </div>
          )}

          {hasResults && (
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <h2 className="text-lg font-semibold text-slate-900">Insurance Policies</h2>
                <span className="rounded-full bg-slate-100 px-3 py-1 text-sm font-medium text-slate-600">
                  {insurances.length} {insurances.length === 1 ? 'policy' : 'policies'} found
                </span>
              </div>

              {insurances.map((insurance) => (
                <InsuranceCard key={insurance.id} insurance={insurance} />
              ))}
            </div>
          )}

          {insurances && insurances.length === 0 && (
            <div className="rounded-xl border border-slate-200 bg-white shadow-sm">
              <EmptyState
                type="no-results"
                title="No policies found"
                description="This customer has no active insurance policies in the system."
              />
            </div>
          )}
        </div>

        {hasResults && (
          <div className="md:col-span-1">
            <PolicySummary insurances={insurances} />
          </div>
        )}
      </div>
    </div>
  );
}

function LoadingSkeletons() {
  return (
    <div className="space-y-4">
      {[1, 2, 3].map((i) => (
        <div key={i} className="animate-pulse overflow-hidden rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
          <div className="flex items-start justify-between">
            <div>
              <div className="flex items-center gap-2">
                <div className="h-6 w-16 rounded-full bg-slate-200" />
                <div className="h-5 w-14 rounded-full bg-slate-200" />
              </div>
              <div className="mt-2 h-4 w-24 rounded bg-slate-200" />
            </div>
            <div className="text-right">
              <div className="h-3 w-12 rounded bg-slate-200" />
              <div className="mt-1 h-6 w-20 rounded bg-slate-200" />
            </div>
          </div>

          {i === 1 && (
            <div className="mt-4 rounded-lg bg-slate-100 p-4">
              <div className="grid grid-cols-2 gap-4">
                {[1, 2, 3, 4].map((j) => (
                  <div key={j}>
                    <div className="h-3 w-20 rounded bg-slate-200" />
                    <div className="mt-1 h-4 w-24 rounded bg-slate-200" />
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      ))}
    </div>
  );
}
