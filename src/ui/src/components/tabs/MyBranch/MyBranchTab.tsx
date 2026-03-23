import { useState, useEffect } from 'react';
import { createColumnHelper } from '@tanstack/react-table';
import { getMyBranchInfo } from '../../../api/gitApi';
import type { MyBranchInfo, CommitInfo, FileChange, RemoteBranch } from '../../../api/types';
import { useAppStore } from '../../../store/appStore';
import DataTable from '../../shared/DataTable';
import DashboardCard from '../../shared/DashboardCard';
import { RefreshCw, Loader2 } from 'lucide-react';

const commitCol = createColumnHelper<CommitInfo>();
const commitColumns = [
  commitCol.accessor('hash', {
    header: 'Hash', size: 85,
    cell: (i) => <span className="text-accent-purple">{i.getValue().substring(0, 10)}</span>,
  }),
  commitCol.accessor('author', { header: 'Autor', size: 170 }),
  commitCol.accessor('relativeDate', { header: 'Quando', size: 110 }),
  commitCol.accessor('message', {
    header: 'Mensagem', size: 500,
    cell: (i) => <span title={i.getValue()}>{i.getValue()}</span>,
  }),
];

const changeCol = createColumnHelper<FileChange>();
const changeColumns = [
  changeCol.accessor('status', {
    header: 'Status', size: 100,
    cell: (info) => {
      const colors: Record<string, string> = {
        Modified: 'text-accent-orange',
        Added: 'text-accent-green',
        Deleted: 'text-accent-red',
        Untracked: 'text-text-muted',
      };
      return <span className={colors[info.getValue()] || 'text-text-primary'}>{info.getValue()}</span>;
    },
  }),
  changeCol.accessor('filePath', { header: 'Arquivo', size: 500 }),
];

const branchCol = createColumnHelper<RemoteBranch>();
const branchColumns = [
  branchCol.accessor('name', { header: 'Branch', size: 250 }),
  branchCol.accessor('date', { header: 'Data', size: 150 }),
  branchCol.accessor('author', { header: 'Autor', size: 170 }),
  branchCol.accessor('lastCommit', {
    header: 'Ultimo Commit', size: 400,
    cell: (i) => <span title={i.getValue()}>{i.getValue()}</span>,
  }),
];

export default function MyBranchTab() {
  const { repoPath, activeTab } = useAppStore();
  const [info, setInfo] = useState<MyBranchInfo | null>(null);
  const [loading, setLoading] = useState(false);

  const load = async () => {
    if (!repoPath) return;
    setLoading(true);
    try {
      setInfo(await getMyBranchInfo());
    } catch { /* ignore */ }
    finally { setLoading(false); }
  };

  useEffect(() => {
    if (activeTab === 2) load();
  }, [activeTab, repoPath]);

  if (loading) {
    return (
      <div className="flex items-center justify-center h-full">
        <Loader2 className="animate-spin text-accent-blue mr-2" size={20} />
        <span className="text-text-secondary text-sm">Carregando informacoes do branch...</span>
      </div>
    );
  }

  if (!info) {
    return (
      <div className="flex items-center justify-center h-full text-text-muted text-sm">
        Selecione um repositorio para ver as informacoes do branch atual
      </div>
    );
  }

  const synced = info.ahead === 0 && info.behind === 0;

  return (
    <div className="flex flex-col h-full overflow-hidden">
      {/* Top bar */}
      <div className="flex items-center gap-3 px-4 py-2 bg-bg-secondary border-b border-border shrink-0">
        <button
          className="px-3 py-1 text-xs bg-bg-tertiary text-text-primary border border-border rounded hover:bg-bg-hover"
          onClick={load}
        >
          <RefreshCw size={12} className="inline mr-1" />Atualizar
        </button>
      </div>

      {/* Info cards */}
      <div className="flex gap-3 px-4 py-3 shrink-0">
        <DashboardCard title="Branch Atual" value={info.branch} color="text-accent-green" accentColor="#50dc50" />
        <DashboardCard
          title="Sincronizacao"
          value={synced ? 'Em dia' : `+${info.ahead} / -${info.behind}`}
          color={synced ? 'text-accent-green' : 'text-accent-orange'}
          accentColor={synced ? '#50dc50' : '#ffb44c'}
        />
        <DashboardCard
          title="Alteracoes Locais"
          value={info.localChanges.length}
          color={info.localChanges.length > 0 ? 'text-accent-orange' : 'text-accent-green'}
          accentColor={info.localChanges.length > 0 ? '#ffb44c' : '#50dc50'}
        />
        <DashboardCard title="Stashes" value={info.stashes.length} color="text-accent-purple" accentColor="#c8a0ff" />
      </div>

      {/* Grids */}
      <div className="flex-1 flex flex-col overflow-hidden px-4 gap-2 pb-2">
        {/* Local Changes */}
        {info.localChanges.length > 0 && (
          <div className="flex flex-col max-h-[180px]">
            <span className="text-accent-blue text-xs font-bold mb-1">Alteracoes Locais ({info.localChanges.length})</span>
            <DataTable data={info.localChanges} columns={changeColumns} />
          </div>
        )}

        {/* Local Branches */}
        <div className="flex flex-col max-h-[200px]">
          <span className="text-accent-blue text-xs font-bold mb-1">Branches Locais ({info.localBranches.length})</span>
          <DataTable data={info.localBranches} columns={branchColumns} />
        </div>

        {/* Recent Commits */}
        <div className="flex-1 flex flex-col min-h-0">
          <span className="text-accent-blue text-xs font-bold mb-1">Commits Recentes ({info.recentCommits.length})</span>
          <DataTable data={info.recentCommits} columns={commitColumns} />
        </div>
      </div>
    </div>
  );
}
