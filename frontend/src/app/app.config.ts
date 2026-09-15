import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    // Interceptors funcionais (Angular 15+): rodam em cadeia na ordem listada.
    // authInterceptor anexa o Bearer token; errorInterceptor trata respostas
    // de erro (401/403/404/...) de forma centralizada.
    provideHttpClient(withInterceptors([authInterceptor, errorInterceptor])),
    // Necessário para as animações do Angular Material (ripple, overlay, etc.)
    provideAnimationsAsync()
  ]
};
