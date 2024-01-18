import { EnvService } from './env.service';

export const EnvServiceFactory = () => {
  const env = new EnvService() as any;

  // Read environment variables from the browser window
  const browserWindow: any = window || {};
  const browserWindowEnv = browserWindow['env'] || {};

  // Assign environment variables from the browser window to env
  // In the current implementation, properties from env.js overwrite defaults from the EnvService.
  // If needed, a deep merge can be performed here to merge properties instead of overwriting them.
  for (const key in browserWindowEnv) {
    if (browserWindowEnv.hasOwnProperty(key)) {
      env[key] = window['env' as any][key as any];
    }
  }

  return env;
};

export const EnvServiceProvider = {
  provide: EnvService,
  useFactory: EnvServiceFactory,
  deps: [],
};