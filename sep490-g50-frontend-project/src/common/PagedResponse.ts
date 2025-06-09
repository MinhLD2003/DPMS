export type PagedResponse<T> = {
  data: T[];
  pageNumber: number;
  pageSize: number;
 // totalPages: number;
  totalRecords: number;
  // hasPrevious: boolean;
  // hasNext: boolean;
};
