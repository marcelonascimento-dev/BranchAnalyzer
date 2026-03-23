import {
  useReactTable,
  getCoreRowModel,
  getSortedRowModel,
  flexRender,
  type ColumnDef,
  type SortingState,
} from '@tanstack/react-table';
import { useState } from 'react';
import { ArrowUp, ArrowDown } from 'lucide-react';

interface Props<T> {
  data: T[];
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  columns: ColumnDef<T, any>[];
  onRowDoubleClick?: (row: T) => void;
  getRowClassName?: (row: T) => string;
}

export default function DataTable<T>({ data, columns, onRowDoubleClick, getRowClassName }: Props<T>) {
  const [sorting, setSorting] = useState<SortingState>([]);

  const table = useReactTable({
    data,
    columns,
    state: { sorting },
    onSortingChange: setSorting,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
  });

  return (
    <div className="flex-1 overflow-auto">
      <table className="w-full border-collapse">
        <thead className="sticky top-0 z-10">
          {table.getHeaderGroups().map((hg) => (
            <tr key={hg.id}>
              {hg.headers.map((header) => (
                <th
                  key={header.id}
                  className="bg-bg-tertiary text-accent-blue text-xs font-bold text-left px-3 py-2 border-b border-border cursor-pointer select-none whitespace-nowrap"
                  style={{ width: header.getSize() !== 150 ? header.getSize() : undefined }}
                  onClick={header.column.getToggleSortingHandler()}
                >
                  <div className="flex items-center gap-1">
                    {flexRender(header.column.columnDef.header, header.getContext())}
                    {header.column.getIsSorted() === 'asc' && <ArrowUp size={12} />}
                    {header.column.getIsSorted() === 'desc' && <ArrowDown size={12} />}
                  </div>
                </th>
              ))}
            </tr>
          ))}
        </thead>
        <tbody>
          {table.getRowModel().rows.map((row) => (
            <tr
              key={row.id}
              className={`border-b border-border/50 hover:bg-bg-hover transition-colors ${
                getRowClassName ? getRowClassName(row.original) : ''
              }`}
              onDoubleClick={() => onRowDoubleClick?.(row.original)}
            >
              {row.getVisibleCells().map((cell) => (
                <td
                  key={cell.id}
                  className="px-3 py-1.5 text-[13px] font-mono whitespace-nowrap overflow-hidden text-ellipsis max-w-[400px]"
                >
                  {flexRender(cell.column.columnDef.cell, cell.getContext())}
                </td>
              ))}
            </tr>
          ))}
          {data.length === 0 && (
            <tr>
              <td colSpan={columns.length} className="text-center py-8 text-text-muted text-sm">
                Nenhum dado para exibir
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}
