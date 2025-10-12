import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { User, LoginRequest, RegisterRequest, AuthState, SigninResponse } from '../models/user.model';
import { ApiResponse } from '../models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly API_URL = environment.apiUrl;
  private readonly TOKEN_KEY = 'auth_token';
  private readonly USER_KEY = 'user_data';

  private authStateSubject = new BehaviorSubject<AuthState>({
    isAuthenticated: false,
    user: null,
    token: null,
    isLoading: false,
    error: null
  });

  public authState$ = this.authStateSubject.asObservable();

  constructor(private http: HttpClient) {
    this.initializeAuthState();
  }

  /**
   * Initialize authentication state from localStorage
   */
  private initializeAuthState(): void {
    const token = localStorage.getItem(this.TOKEN_KEY);
    const userData = localStorage.getItem(this.USER_KEY);

    if (token && userData) {
      try {
        const user = JSON.parse(userData);
        this.authStateSubject.next({
          isAuthenticated: true,
          user,
          token,
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
    
    return this.http.post<ApiResponse<SigninResponse>>(`${this.API_URL}/auth/login`, credentials)
      .pipe(
        map(response => {
          const signinResponse: SigninResponse = {
            token: response.data.token,
            user: response.data.user
          };
          this.setAuthData(signinResponse);
          return signinResponse;
        }),
        catchError(error => {
          this.setError(error.error?.message || 'Falha ao efetuar login');
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
    
    return this.http.post<ApiResponse<SigninResponse>>(`${this.API_URL}/auth/register`, userData)
      .pipe(
        map(response => {
          this.setAuthData(response.data);
          return response.data;
        }),
        catchError(error => {
          this.setError(error.error?.message || 'Falha ao efetuar cadastro');
          return throwError(() => error);
        }),
        tap(() => this.setLoading(false))
      );
  }

  /**
   * Logout user
   */
  logout(): Observable<any> {
    return this.http.post<ApiResponse<any>>(`${this.API_URL}/auth/logout`, {})
      .pipe(
        tap(() => this.clearAuthData()),
        catchError(error => {
          this.clearAuthData();
          return throwError(() => error);
        })
      );
  }


  /**
   * Get current user profile
   */
  getCurrentUser(): Observable<User> {
    return this.http.get<ApiResponse<User>>(`${this.API_URL}/auth/me`)
      .pipe(
        map(response => {
          this.updateUserData(response.data);
          return response.data;
        }),
        catchError(error => {
          this.setError(error.error?.message || 'Falha ao obter dados do usuário');
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
    const userData = localStorage.getItem(this.USER_KEY);
    return !!(token && userData);
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
   * Get current token
   */
  getToken(): string | null {
    return this.authStateSubject.value.token;
  }

  /**
   * Set authentication data
   */
  private setAuthData(data: SigninResponse): void {
    localStorage.setItem(this.TOKEN_KEY, data.token);
    localStorage.setItem(this.USER_KEY, JSON.stringify(data.user));

    this.authStateSubject.next({
      isAuthenticated: true,
      user: data.user,
      token: data.token,
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
    localStorage.removeItem(this.USER_KEY);

    this.authStateSubject.next({
      isAuthenticated: false,
      user: null,
      token: null,
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
