import { useState, useEffect, useCallback } from 'react';
import { createColumnHelper } from '@tanstack/react-table';
import { useAppStore } from '../../../store/appStore';
import { getMergeStatus, getPendingCommits } from '../../../api/gitApi';
import type { MergeStatus, CommitInfo } from '../../../api/types';
import DataTable from '../../shared/DataTable';
import MergeSummary from './MergeSummary';
import { FileDown } from 'lucide-react';

const col = createColumnHelper<CommitInfo>();
const columns = [
  col.accessor('hash', {
    header: 'Hash',
    size: 100,
    cell: (info) => <span className="text-accent-purple">{info.getValue().substring(0, 12)}</span>,
  }),
  col.accessor('author', { header: 'Autor', size: 200 }),
  col.accessor('relativeDate', { header: 'Quando', size: 140 }),
  col.accessor('message', {
    header: 'Mensagem',
    size: 600,
    cell: (info) => <span className="text-text-primary" title={info.getValue()}>{info.getValue()}</span>,
  }),
];

export default function StatusMergeTab() {
  const { branchA, branchB, setStatus } = useAppStore();
  const [mergeStatus, setMergeStatus] = useState<MergeStatus | null>(null);
  const [commits, setCommits] = useState<CommitInfo[]>([]);
  const [loading, setLoading] = useState(false);

  const analyze = useCallback(async () => {
    if (!branchA || !branchB) return;
    setLoading(true);
    setStatus('Analisando merge status...');
    try {
      const [status, pendingCommits] = await Promise.all([
        getMergeStatus(branchA, branchB),
        getPendingCommits(branchA, branchB),
      ]);
      setMergeStatus(status);
      setCommits(pendingCommits);
      setStatus(`Analise concluida. ${pendingCommits.length} commits pendentes.`);
    } catch (e) {
      setStatus(`Erro: ${e}`);
    } finally {
      setLoading(false);
    }
  }, [branchA, branchB, setStatus]);

  useEffect(() => {
    const handler = () => analyze();
    window.addEventListener('analyze-merge', handler);
    return () => window.removeEventListener('analyze-merge', handler);
  }, [analyze]);

  const exportCsv = () => {
    if (commits.length === 0) return;
    const header = 'Hash;Autor;Quando;Mensagem\n';
    const rows = commits.map((c) => `${c.hash};${c.author};${c.relativeDate};"${c.message.replace(/"/g, '""')}"`).join('\n');
    downloadFile(`commits_${branchB}.csv`, header + rows, 'text/csv');
  };

  const exportJson = () => {
    if (commits.length === 0) return;
    const data = {
      branchReceptor: branchA,
      branchFeature: branchB,
      exportDate: new Date().toISOString(),
      totalRegistros: commits.length,
      dados: commits,
    };
    downloadFile(`commits_${branchB}.json`, JSON.stringify(data, null, 2), 'application/json');
  };

  return (
    <div className="flex flex-col h-full">
      {/* Summary Panel */}
      <MergeSummary status={mergeStatus} branchA={branchA} branchB={branchB} loading={loading} />

      {/* Export Bar */}
      <div className="flex items-center gap-3 px-4 py-1.5 bg-bg-secondary border-b border-border shrink-0">
        <span className="text-text-muted text-xs">Commits pendentes de merge:</span>
        <span className="text-accent-orange text-sm font-bold">{commits.length}</span>
        <div className="ml-auto flex gap-2">
          <button
            className="px-3 py-1 text-[11px] bg-[#284046] text-accent-green border border-border rounded hover:brightness-125 disabled:opacity-50"
            onClick={exportCsv}
            disabled={commits.length === 0}
          >
            <FileDown size={11} className="inline mr-1" />CSV
          </button>
          <button
            className="px-3 py-1 text-[11px] bg-[#283c50] text-accent-blue border border-border rounded hover:brightness-125 disabled:opacity-50"
            onClick={exportJson}
            disabled={commits.length === 0}
          >
            <FileDown size={11} className="inline mr-1" />JSON
          </button>
        </div>
      </div>

      {/* Grid */}
      <DataTable data={commits} columns={columns} />
    </div>
  );
}

function downloadFile(name: string, content: string, type: string) {
  const blob = new Blob([content], { type });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = name;
  a.click();
  URL.revokeObjectURL(url);
}
