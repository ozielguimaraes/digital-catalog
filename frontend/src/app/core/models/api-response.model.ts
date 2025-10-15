export interface ApiResponse<T = any> {
  isSuccess: boolean;
  data: T;
  message: string;
  type: string;
  errors?: string[];
}

export interface PaginatedResponse<T = any> {
  success: boolean;
  data: T[];
  pagination: {
    page: number;
    limit: number;
    total: number;
    totalPages: number;
  };
  message: string;
}
