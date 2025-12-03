import type { CustomerInsurance } from '../types/api';
import { InsuranceType } from '../types/api';
import { VehicleDetails } from './VehicleDetails';

interface InsuranceCardProps {
  insurance: CustomerInsurance;
}

export function InsuranceCard({ insurance }: InsuranceCardProps) {
  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('sv-SE', {
      style: 'currency',
      currency: 'SEK',
      minimumFractionDigits: 0,
      maximumFractionDigits: 0,
    }).format(amount);
  };

  return (
    <article>
      <div>
        <div>
          <div>
            <div>
              <span>{insurance.type}</span>
              <span>{insurance.status}</span>
            </div>

            <p>Policy #{insurance.id}</p>
          </div>

          <div>
            <p>Premium</p>
            <div>
              <p>{formatCurrency(insurance.premium)}</p>
              <p>/month</p>
            </div>
          </div>
        </div>

        {insurance.type === InsuranceType.Car && insurance.vehicle && (
          <div>
            <VehicleDetails vehicle={insurance.vehicle} />
          </div>
        )}

        {insurance.type === InsuranceType.Car && !insurance.vehicle && (
          <div>
            <div>
              <div>
                <p>Vehicle Details Unavailable</p>
                <p>Unable to retrieve vehicle information for this policy.</p>
              </div>
            </div>
          </div>
        )}
      </div>
    </article>
  );
}
