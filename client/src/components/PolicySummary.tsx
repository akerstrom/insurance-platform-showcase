import type { CustomerInsurance } from '../types/api';
import { InsuranceType } from '../types/api';

interface PolicySummaryProps {
  insurances: CustomerInsurance[];
}

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
    <aside>
      <div>
        <h3>Policy Overview</h3>

        <div>
          <div>
            <span>Total Monthly Premium</span>
          </div>
          <p>{formatCurrency(totalPremium)}</p>
          <p>per month</p>
        </div>

        <div>
          <div>
            <span>Active Policies</span>
          </div>

          <div>{insurances.length}</div>

          <div>
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

        <div>
          <div>
            <span>Customer ID</span>
          </div>
          <p>{formatPersonalNumber(insurances[0].pid)}</p>
        </div>
      </div>
    </aside>
  );
}

interface PolicyTypeRowProps {
  label: string;
  count: number;
}

function PolicyTypeRow({ label, count }: PolicyTypeRowProps) {
  return (
    <div>
      <div>
        <span>{label}</span>
      </div>
      <span>{count}</span>
    </div>
  );
}
