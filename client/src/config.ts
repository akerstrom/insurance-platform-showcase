interface RuntimeConfig {
  apiBaseUrl: string;
}

let configCache: RuntimeConfig | null = null;

export async function getConfig(): Promise<RuntimeConfig> {
  if (configCache) {
    return configCache;
  }

  // In development, use Vite env var; in production, fetch runtime config
  if (import.meta.env.DEV) {
    configCache = {
      apiBaseUrl: import.meta.env.VITE_API_BASE_URL || '',
    };
    return configCache;
  }

  try {
    const response = await fetch('/config.json');
    if (!response.ok) {
      throw new Error(`Failed to load config: ${response.status}`);
    }
    configCache = await response.json();
    return configCache!;
  } catch (error) {
    console.warn('Failed to load runtime config, using defaults:', error);
    configCache = { apiBaseUrl: '' };
    return configCache;
  }
}
