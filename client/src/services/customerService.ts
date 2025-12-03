import type { ApiError, CustomerInsurance } from '../types/api';
import { getConfig } from '../config';

export class ApiException extends Error {
  statusCode: number;
  apiError?: ApiError;

  constructor(message: string, statusCode: number, apiError?: ApiError) {
    super(message);
    this.name = 'ApiException';
    this.statusCode = statusCode;
    this.apiError = apiError;
  }
}

export class CustomerServiceClient {
  async getCustomerInsurances(pid: string): Promise<CustomerInsurance[]> {
    if (!pid || !/^\d{12}$/.test(pid)) {
      throw new ApiException(
        'Invalid personal number format. Use 12 digits (YYYYMMDDXXXX)',
        400
      );
    }

    try {
      const config = await getConfig();
      const response = await fetch(
        `${config.apiBaseUrl}/customers/${pid}/insurances`
      );

      if (!response.ok) {
        if (response.status === 404) {
          throw new ApiException(
            'No insurances found for this customer',
            404
          );
        }

        if (response.status === 503 || response.status === 504) {
          throw new ApiException(
            'Service temporarily unavailable. Please try again in a moment.',
            response.status
          );
        }

        if (response.status === 400) {
          const error = await response.json().catch(() => null);
          throw new ApiException(
            error?.message || 'Invalid request',
            400,
            error
          );
        }

        throw new ApiException('Unexpected error occurred', response.status);
      }

      return response.json();
    } catch (error) {
      if (error instanceof ApiException) {
        throw error;
      }

      if (error instanceof TypeError) {
        throw new ApiException(
          'Unable to connect to the service. Please check your connection.',
          0
        );
      }

      throw new ApiException('An unexpected error occurred', 0);
    }
  }
}

export const customerService = new CustomerServiceClient();
