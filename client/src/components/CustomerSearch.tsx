import { useState } from 'react';
import type { FormEvent } from 'react';
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
    <div>
      <div>
        <div>
          <h1>Customer Insurance Lookup</h1>
          <p>Search by Swedish personal number (personnummer).</p>

          <form onSubmit={handleSubmit}>
            <div>
              <div>
                <label htmlFor="pid">Personal Number</label>
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
                />
              </div>

              <button type="submit" disabled={loading}>
                {loading ? (
                  <>
                    <LoadingSpinner />
                    <span>Searching</span>
                  </>
                ) : (
                  <span>Search</span>
                )}
              </button>
            </div>

            <p>Format: YYYYMMDDXXXX (12 digits)</p>
          </form>
        </div>
      </div>

      <div>
        <div>
          {loading && <LoadingSkeletons />}

          {error && <ErrorMessage message={error} />}

          {!loading && !error && !insurances && (
            <EmptyState
              type="initial"
              title="Ready to search"
              description="Enter a personal number above to find insurance policies."
            />
          )}

          {hasResults && (
            <div>
              <div>
                <h2>Insurance Policies</h2>
                <span>
                  {insurances.length} {insurances.length === 1 ? 'policy' : 'policies'} found
                </span>
              </div>

              {insurances.map((insurance) => (
                <InsuranceCard key={insurance.id} insurance={insurance} />
              ))}
            </div>
          )}

          {insurances && insurances.length === 0 && (
            <EmptyState
              type="no-results"
              title="No policies found"
              description="This customer has no active insurance policies in the system."
            />
          )}
        </div>

        {hasResults && (
          <div>
            <div>
              <PolicySummary insurances={insurances} />
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

function LoadingSkeletons() {
  return (
    <div>
      {[1, 2, 3].map((i) => (
        <div key={i}>
          <div>
            <div>
              <div>
                <div />
                <div />
              </div>
              <div />
            </div>
            <div>
              <div />
              <div />
            </div>
          </div>

          {i === 1 && (
            <div>
              <div>
                {[1, 2, 3, 4].map((j) => (
                  <div key={j}>
                    <div />
                    <div />
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
