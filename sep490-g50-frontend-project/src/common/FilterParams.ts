export type FilterParams = {
  pageNumber: number;
  pageSize: number;
  filters?: Record<string, string|boolean>;
  sortBy?: string;
  sortDirection?: string;
  fromDate?: Date;
  toDate?: Date;
}