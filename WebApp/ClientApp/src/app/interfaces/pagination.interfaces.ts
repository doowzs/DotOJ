export interface PaginatedList<T> {
  (arg: T): T;
  pageIndex: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  items: T[];
}
