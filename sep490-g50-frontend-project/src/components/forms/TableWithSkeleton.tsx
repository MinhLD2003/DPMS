import React, { useEffect, useState } from "react";
import { Table, Skeleton } from "antd";
import type { TableProps } from "antd/es/table";

interface TableWithSkeletonProps<T> extends TableProps<T> {
  loading: boolean;
  skeletonRowCount?: number;
}

const TableWithSkeleton = <T extends object>({
  loading,
  skeletonRowCount = 5,
  columns,
  ...rest
}: TableWithSkeletonProps<T>) => {
  const [displayLoading, setDisplayLoading] = useState(loading);

  useEffect(() => {
    // Immediately show loading state when loading prop becomes true
    if (loading) {
      setDisplayLoading(true);
    } else {
      // Add delay when turning off loading
      const timer = setTimeout(() => setDisplayLoading(false), 1000);
      return () => clearTimeout(timer);
    }
  }, [loading]);

  const skeletonData = React.useMemo(
    () =>
      Array.from({ length: skeletonRowCount }, (_, index) => ({
        key: `skeleton-${index}`,
      })) as T[],
    [skeletonRowCount]
  );

  const skeletonColumns = columns?.map((col) => ({
    ...col,
    render: () => <Skeleton active paragraph={false} title={{ width: "80%" }} />,
  }));

  return (
    <Table
      columns={displayLoading ? skeletonColumns : columns}
      dataSource={displayLoading ? skeletonData : rest.dataSource}
      pagination={displayLoading ? false : rest.pagination}
      {...rest}
    />
  );
};

export default TableWithSkeleton;
