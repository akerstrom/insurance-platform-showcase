import { Car, PawPrint, Heart, AlertTriangle } from 'lucide-react';
import type { CustomerInsurance } from '../types/api';
import { InsuranceType } from '../types/api';
import { VehicleDetails } from './VehicleDetails';

interface InsuranceCardProps {
  insurance: CustomerInsurance;
}

const typeIcons = {
  [InsuranceType.Car]: Car,
  [InsuranceType.Pet]: PawPrint,
  [InsuranceType.Health]: Heart,
};

const typeColors = {
  [InsuranceType.Car]: 'bg-blue-100 text-blue-700',
  [InsuranceType.Pet]: 'bg-amber-100 text-amber-700',
  [InsuranceType.Health]: 'bg-rose-100 text-rose-700',
};

export function InsuranceCard({ insurance }: InsuranceCardProps) {
  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('sv-SE', {
      style: 'currency',
      currency: 'SEK',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(amount);
  };

  const Icon = typeIcons[insurance.type] || Car;
  const colorClass = typeColors[insurance.type] || 'bg-slate-100 text-slate-700';

  return (
    <article className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
      <div className="p-5">
        <div className="flex items-start justify-between">
          <div>
            <div className="flex items-center gap-2">
              <span className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium ${colorClass}`}>
                <Icon className="h-3.5 w-3.5" />
                {insurance.type}
              </span>
              <span className="inline-flex items-center rounded-full bg-emerald-100 px-2 py-0.5 text-xs font-medium text-emerald-700">
                {insurance.status}
              </span>
            </div>

            <p className="mt-2 text-sm text-slate-500">Policy #{insurance.id}</p>
          </div>

          <div className="text-right">
            <p className="text-xs text-slate-500">Premium</p>
            <div className="flex items-baseline gap-1">
              <p className="text-xl font-semibold text-slate-900">{formatCurrency(insurance.premium)}</p>
              <p className="text-sm text-slate-500">/month</p>
            </div>
          </div>
        </div>

        {insurance.type === InsuranceType.Car && insurance.vehicle && (
          <VehicleDetails vehicle={insurance.vehicle} />
        )}

        {insurance.type === InsuranceType.Car && !insurance.vehicle && (
          <div className="mt-4 rounded-lg border border-amber-200 bg-amber-50 p-4">
            <div className="flex gap-3">
              <AlertTriangle className="h-5 w-5 shrink-0 text-amber-500" />
              <div>
                <p className="text-sm font-medium text-amber-800">Vehicle Details Unavailable</p>
                <p className="mt-1 text-sm text-amber-700">Unable to retrieve vehicle information for this policy.</p>
              </div>
            </div>
          </div>
        )}
      </div>
    </article>
  );
}
