import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { User, LoginRequest, RegisterRequest, AuthState, SigninResponse, RefreshTokenRequest, RefreshTokenResponse } from '../models/user.model';
import { SentryService } from './sentry.service';
import { extractErrorMessage } from '../utils/error.utils';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly API_URL = environment.apiUrl;
  private readonly TOKEN_KEY = 'auth_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';
  private readonly USER_KEY = 'user_data';

  private authStateSubject = new BehaviorSubject<AuthState>({
    isAuthenticated: false,
    user: null,
    token: null,
    refreshToken: null,
    isLoading: false,
    error: null
  });

  public authState$ = this.authStateSubject.asObservable();

  constructor(private http: HttpClient, private sentryService: SentryService) {
    this.initializeAuthState();
  }

  /**
   * Initialize authentication state from localStorage
   */
  private initializeAuthState(): void {
    const token = localStorage.getItem(this.TOKEN_KEY);
    const refreshToken = localStorage.getItem(this.REFRESH_TOKEN_KEY);
    const userData = localStorage.getItem(this.USER_KEY);

    if (token && refreshToken && userData) {
      try {
        const user = JSON.parse(userData);
        this.authStateSubject.next({
          isAuthenticated: true,
          user,
          token,
          refreshToken,
          isLoading: false,
          error: null
        });
      } catch (error) {
        this.clearAuthData();
      }
    }
  }

  /**
   * Login user
   */
  login(credentials: LoginRequest): Observable<SigninResponse> {
    this.setLoading(true);
    this.sentryService.addBreadcrumb('User attempting login', 'auth', 'info');
    
    return this.http.post<SigninResponse>(`${this.API_URL}/auth/login`, credentials)
      .pipe(
        map(response => {
          this.setAuthData(response);
          this.sentryService.setUser({
            id: response.user.id,
            email: response.user.email,
            username: response.user.nome
          });
          this.sentryService.addBreadcrumb('User logged in successfully', 'auth', 'info');
          return response;
        }),
        catchError(error => {
          this.sentryService.captureApiError(error, '/auth/login', 'POST', { email: credentials.email });
          const errorMessage = extractErrorMessage(error, 'Falha ao efetuar login');
          this.setError(errorMessage);
          return throwError(() => error);
        }),
        tap(() => this.setLoading(false))
      );
  }

  /**
   * Register new user
   */
  register(userData: RegisterRequest): Observable<SigninResponse> {
    this.setLoading(true);
    
    return this.http.post<SigninResponse>(`${this.API_URL}/auth/register`, userData)
      .pipe(
        map(response => {
          this.setAuthData(response);
          return response;
        }),
        catchError(error => {
          this.sentryService.captureApiError(error, '/auth/register', 'POST');
          const errorMessage = extractErrorMessage(error, 'Falha ao efetuar cadastro');
          this.setError(errorMessage);
          return throwError(() => error);
        }),
        tap(() => this.setLoading(false))
      );
  }

  /**
   * Logout user
   */
  logout(): Observable<any> {
    const refreshToken = this.getRefreshToken();
    const body = refreshToken ? { refreshToken } : {};
    
    this.sentryService.addBreadcrumb('User logging out', 'auth', 'info');
    
    return this.http.post(`${this.API_URL}/auth/logout`, body)
      .pipe(
        tap(() => {
          this.clearAuthData();
          this.sentryService.setUser({});
          this.sentryService.addBreadcrumb('User logged out successfully', 'auth', 'info');
        }),
        catchError(error => {
          this.sentryService.captureApiError(error, '/auth/logout', 'POST');
          this.clearAuthData();
          this.sentryService.setUser({});
          return throwError(() => error);
        })
      );
  }


  /**
   * Get current user profile
   */
  getCurrentUser(): Observable<User> {
    return this.http.get<User>(`${this.API_URL}/auth/me`)
      .pipe(
        map(response => {
          this.updateUserData(response);
          return response;
        }),
        catchError(error => {
          this.sentryService.captureApiError(error, '/auth/profile', 'GET');
          const errorMessage = extractErrorMessage(error, 'Falha ao obter dados do usuário');
          this.setError(errorMessage);
          return throwError(() => error);
        })
      );
  }

  /**
   * Refresh access token
   */
  refreshToken(): Observable<RefreshTokenResponse> {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      return throwError(() => new Error('No refresh token available'));
    }

    return this.http.post<RefreshTokenResponse>(`${this.API_URL}/auth/refresh-token`, { refreshToken })
      .pipe(
        map(response => {
          // Update stored tokens
          localStorage.setItem(this.TOKEN_KEY, response.token);
          localStorage.setItem(this.REFRESH_TOKEN_KEY, response.refreshToken);
          
          // Update auth state
          const currentState = this.authStateSubject.value;
          this.authStateSubject.next({
            ...currentState,
            token: response.token,
            refreshToken: response.refreshToken
          });
          
          return response;
        }),
        catchError(error => {
          // If refresh fails, logout user
          this.clearAuthData();
          return throwError(() => error);
        })
      );
  }

  /**
   * Check if user is authenticated
   */
  isAuthenticated(): boolean {
    return this.authStateSubject.value.isAuthenticated;
  }

  /**
   * Check if there's a valid token in localStorage without initializing auth state
   */
  hasValidToken(): boolean {
    const token = localStorage.getItem(this.TOKEN_KEY);
    const refreshToken = localStorage.getItem(this.REFRESH_TOKEN_KEY);
    const userData = localStorage.getItem(this.USER_KEY);
    return !!(token && refreshToken && userData);
  }

  /**
   * Get current access token
   */
  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  /**
   * Get current refresh token
   */
  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  /**
   * Clear all authentication data (useful for debugging)
   */
  clearAllAuthData(): void {
    this.clearAuthData();
  }

  /**
   * Get current user
   */
  getCurrentUserSync(): User | null {
    return this.authStateSubject.value.user;
  }

  /**
   * Set authentication data
   */
  private setAuthData(data: SigninResponse): void {
    localStorage.setItem(this.TOKEN_KEY, data.token);
    localStorage.setItem(this.REFRESH_TOKEN_KEY, data.refreshToken);
    localStorage.setItem(this.USER_KEY, JSON.stringify(data.user));

    this.authStateSubject.next({
      isAuthenticated: true,
      user: data.user,
      token: data.token,
      refreshToken: data.refreshToken,
      isLoading: false,
      error: null
    });
  }

  /**
   * Update user data
   */
  private updateUserData(user: User): void {
    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
    
    const currentState = this.authStateSubject.value;
    this.authStateSubject.next({
      ...currentState,
      user
    });
  }

  /**
   * Clear authentication data
   */
  private clearAuthData(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);

    this.authStateSubject.next({
      isAuthenticated: false,
      user: null,
      token: null,
      refreshToken: null,
      isLoading: false,
      error: null
    });
  }

  /**
   * Set loading state
   */
  private setLoading(loading: boolean): void {
    const currentState = this.authStateSubject.value;
    this.authStateSubject.next({
      ...currentState,
      isLoading: loading
    });
  }

  /**
   * Set error state
   */
  private setError(error: string): void {
    const currentState = this.authStateSubject.value;
    this.authStateSubject.next({
      ...currentState,
      error,
      isLoading: false
    });
  }
}
