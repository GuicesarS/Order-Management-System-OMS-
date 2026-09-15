import { Injectable, signal, inject } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { AuthApiService } from '../api/auth-api.service';
import { AuthResponse, UserRole } from '../models/auth.model';

const TOKEN_KEY = 'oms_token';

interface JwtPayload {
  exp: number;
  [key: string]: unknown;
}

// JwtSecurityTokenHandler (backend) encurta ClaimTypes.Role/NameIdentifier para
// os nomes curtos "role"/"nameid" no token gravado, via seu OutboundClaimTypeMap
// padrão — não usa as URIs longas de ClaimTypes em tempo de execução.
const ROLE_CLAIM = 'role';
const NAME_ID_CLAIM = 'nameid';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly authApi = inject(AuthApiService);
  private readonly router = inject(Router);

  readonly isAuthenticated$ = signal<boolean>(this.hasValidToken());

  login(email: string, password: string): Observable<AuthResponse> {
    return this.authApi.login({ email, password }).pipe(
      tap((res) => {
        localStorage.setItem(TOKEN_KEY, res.accessToken);
        this.isAuthenticated$.set(true);
      })
    );
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    this.isAuthenticated$.set(false);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  isAuthenticated(): boolean {
    return this.hasValidToken();
  }

  getRole(): UserRole | null {
    const payload = this.decodePayload();
    const role = payload?.[ROLE_CLAIM];
    return role === 'Admin' || role === 'Operator' ? role : null;
  }

  getUserId(): string | null {
    const payload = this.decodePayload();
    return (payload?.[NAME_ID_CLAIM] as string) ?? null;
  }

  isAdmin(): boolean {
    return this.getRole() === 'Admin';
  }

  private hasValidToken(): boolean {
    const payload = this.decodePayload();
    if (!payload) return false;
    return payload.exp * 1000 > Date.now();
  }

  // JWT is decoded manually (no verification) purely to read exp/role claims client-side; no external lib needed.
  private decodePayload(): JwtPayload | null {
    const token = localStorage.getItem(TOKEN_KEY);
    if (!token) return null;
    const parts = token.split('.');
    if (parts.length !== 3) return null;
    try {
      const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
      const json = decodeURIComponent(
        atob(base64)
          .split('')
          .map((c) => '%' + c.charCodeAt(0).toString(16).padStart(2, '0'))
          .join('')
      );
      return JSON.parse(json) as JwtPayload;
    } catch {
      return null;
    }
  }
}
