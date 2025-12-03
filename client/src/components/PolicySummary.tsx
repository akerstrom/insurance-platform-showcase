import { Banknote, ClipboardList, User, Car, PawPrint, Heart } from 'lucide-react';
import type { CustomerInsurance } from '../types/api';
import { InsuranceType } from '../types/api';

interface PolicySummaryProps {
  insurances: CustomerInsurance[];
}

const typeIcons = {
  Car,
  Pet: PawPrint,
  Health: Heart,
};

export function PolicySummary({ insurances }: PolicySummaryProps) {
  const totalPremium = insurances.reduce((sum, ins) => sum + ins.premium, 0);

  const policyBreakdown = {
    car: insurances.filter((i) => i.type === InsuranceType.Car).length,
    pet: insurances.filter((i) => i.type === InsuranceType.Pet).length,
    health: insurances.filter((i) => i.type === InsuranceType.Health).length,
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('sv-SE', {
      style: 'currency',
      currency: 'SEK',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(amount);
  };

  const formatPersonalNumber = (pid: string) => {
    if (pid.length === 12) {
      return `${pid.slice(0, 8)}-${pid.slice(8)}`;
    }
    return pid;
  };

  return (
    <aside className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm md:sticky md:top-24">
      <div className="space-y-5">
        <h3 className="text-lg font-semibold text-slate-900">Policy Overview</h3>

        <div className="rounded-lg bg-blue-50 p-4">
          <div className="flex items-center gap-2 text-blue-700">
            <Banknote className="h-4 w-4" />
            <span className="text-xs font-medium uppercase tracking-wide">Total Monthly Premium</span>
          </div>
          <p className="mt-2 text-3xl font-bold text-blue-700">{formatCurrency(totalPremium)}</p>
          <p className="text-sm text-blue-600">per month</p>
        </div>

        <div>
          <div className="flex items-center gap-2 text-slate-600">
            <ClipboardList className="h-4 w-4" />
            <span className="text-xs font-medium uppercase tracking-wide">Active Policies</span>
          </div>

          <div className="mt-2 text-4xl font-bold text-slate-900">{insurances.length}</div>

          <div className="mt-3 space-y-2">
            {policyBreakdown.car > 0 && (
              <PolicyTypeRow label="Car" count={policyBreakdown.car} />
            )}
            {policyBreakdown.pet > 0 && (
              <PolicyTypeRow label="Pet" count={policyBreakdown.pet} />
            )}
            {policyBreakdown.health > 0 && (
              <PolicyTypeRow label="Health" count={policyBreakdown.health} />
            )}
          </div>
        </div>

        <div className="border-t border-slate-200 pt-4">
          <div className="flex items-center gap-2 text-slate-600">
            <User className="h-4 w-4" />
            <span className="text-xs font-medium uppercase tracking-wide">Customer ID</span>
          </div>
          <p className="mt-1 font-mono text-sm text-slate-900">{formatPersonalNumber(insurances[0].pid)}</p>
        </div>
      </div>
    </aside>
  );
}

interface PolicyTypeRowProps {
  label: 'Car' | 'Pet' | 'Health';
  count: number;
}

function PolicyTypeRow({ label, count }: PolicyTypeRowProps) {
  const Icon = typeIcons[label];

  return (
    <div className="flex items-center justify-between rounded-lg bg-slate-50 px-3 py-2">
      <div className="flex items-center gap-2">
        <Icon className="h-4 w-4 text-slate-500" />
        <span className="text-sm text-slate-700">{label}</span>
      </div>
      <span className="font-semibold text-slate-900">{count}</span>
    </div>
  );
}
