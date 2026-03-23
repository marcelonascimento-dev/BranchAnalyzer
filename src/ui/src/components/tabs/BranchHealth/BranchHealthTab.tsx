import { useState, useEffect } from 'react';
import { createColumnHelper } from '@tanstack/react-table';
import { getBranchHealth } from '../../../api/gitApi';
import type { BranchHealthResponse, BranchHealthItem } from '../../../api/types';
import { useAppStore } from '../../../store/appStore';
import DataTable from '../../shared/DataTable';
import DashboardCard from '../../shared/DashboardCard';
import { Loader2 } from 'lucide-react';

const col = createColumnHelper<BranchHealthItem>();
const columns = [
  col.accessor('shortName', { header: 'Branch', size: 300 }),
  col.accessor('author', { header: 'Autor', size: 180 }),
  col.accessor('dateShort', { header: 'Ultimo Commit', size: 120 }),
  col.accessor('daysInactive', {
    header: 'Dias Inativo',
    size: 100,
    cell: (info) => {
      const d = info.getValue();
      const color = d > 180 ? 'text-accent-red' : d > 60 ? 'text-accent-orange' : 'text-accent-green';
      return <span className={`font-bold ${color}`}>{d}d</span>;
    },
  }),
  col.accessor('status', {
    header: 'Status',
    size: 120,
    cell: (info) => {
      const s = info.getValue();
      const styles: Record<string, string> = {
        ATIVO: 'bg-accent-green/20 text-accent-green',
        INATIVO: 'bg-accent-orange/20 text-accent-orange',
        OBSOLETO: 'bg-accent-red/20 text-accent-red',
      };
      return (
        <span className={`px-2 py-0.5 rounded text-[11px] font-bold ${styles[s] || ''}`}>
          {s}
        </span>
      );
    },
  }),
  col.accessor('prefix', { header: 'Tipo', size: 100 }),
];

export default function BranchHealthTab() {
  const { repoPath, activeTab } = useAppStore();
  const [data, setData] = useState<BranchHealthResponse | null>(null);
  const [loading, setLoading] = useState(false);

  const load = async () => {
    if (!repoPath) return;
    setLoading(true);
    try {
      setData(await getBranchHealth());
    } catch { /* ignore */ }
    finally { setLoading(false); }
  };

  useEffect(() => {
    if (activeTab === 3) load();
  }, [activeTab, repoPath]);

  if (loading) {
    return (
      <div className="flex items-center justify-center h-full">
        <Loader2 className="animate-spin text-accent-blue mr-2" size={20} />
        <span className="text-text-secondary text-sm">Carregando saude dos branches...</span>
      </div>
    );
  }

  if (!data) {
    return (
      <div className="flex items-center justify-center h-full text-text-muted text-sm">
        Selecione um repositorio para ver a saude dos branches
      </div>
    );
  }

  return (
    <div className="flex flex-col h-full">
      {/* Dashboard cards */}
      <div className="flex gap-3 px-4 py-3 shrink-0">
        <DashboardCard title="Total" value={data.total} color="text-accent-blue" accentColor="#78b4ff" />
        <DashboardCard title="Ativos" value={data.active} color="text-accent-green" accentColor="#50dc50" />
        <DashboardCard title="Inativos" value={data.stale} color="text-accent-orange" accentColor="#ffb44c" />
        <DashboardCard title="Obsoletos" value={data.obsolete} color="text-accent-red" accentColor="#dc5050" />
      </div>

      {/* Table */}
      <div className="flex-1 px-4 pb-2 overflow-hidden flex flex-col">
        <DataTable
          data={data.branches}
          columns={columns}
          getRowClassName={(row) => {
            if (row.status === 'OBSOLETO') return 'bg-accent-red/5';
            if (row.status === 'INATIVO') return 'bg-accent-orange/5';
            return '';
          }}
        />
      </div>
    </div>
  );
}
