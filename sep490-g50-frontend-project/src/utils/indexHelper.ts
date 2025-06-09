export const addIndexToData = <T extends object>(
  data: T[],
  currentPage: number,
  pageSize: number
): (T & { index: number })[] => {
  return data.map((item, index) => ({
    ...item,
    index: index + 1 + (currentPage - 1) * pageSize,
  }));
};
export const addIndexToUnpagedData = <T extends object>(
  data: T[],
): (T & { index: number })[] => {
  return data.map((item, index) => ({
    ...item,
    index: index + 1,
  }));
};

