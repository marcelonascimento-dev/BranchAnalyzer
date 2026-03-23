import { useState, useEffect } from 'react';
import { createColumnHelper } from '@tanstack/react-table';
import { getPendingCommits, getChangedFiles, getConflicts } from '../../../api/gitApi';
import type { BatchMergeResult, CommitInfo, FileChange } from '../../../api/types';
import DataTable from '../../shared/DataTable';
import { X, Loader2 } from 'lucide-react';

interface Props {
  result: BatchMergeResult;
  receptor: string;
  onClose: () => void;
}

const commitCol = createColumnHelper<CommitInfo>();
const commitColumns = [
  commitCol.accessor('hash', {
    header: 'Hash', size: 100,
    cell: (i) => <span className="text-accent-purple">{i.getValue().substring(0, 12)}</span>,
  }),
  commitCol.accessor('author', { header: 'Autor', size: 180 }),
  commitCol.accessor('relativeDate', { header: 'Quando', size: 120 }),
  commitCol.accessor('message', { header: 'Mensagem', size: 400 }),
];

const fileCol = createColumnHelper<FileChange>();
const fileColumns = [
  fileCol.accessor('status', {
    header: 'Status', size: 100,
    cell: (info) => {
      const colors: Record<string, string> = {
        Modified: 'text-accent-orange',
        Added: 'text-accent-green',
        Deleted: 'text-accent-red',
      };
      return <span className={colors[info.getValue()] || ''}>{info.getValue()}</span>;
    },
  }),
  fileCol.accessor('filePath', { header: 'Arquivo', size: 600 }),
];

export default function DrillDownModal({ result, receptor, onClose }: Props) {
  const [tab, setTab] = useState(0);
  const [commits, setCommits] = useState<CommitInfo[]>([]);
  const [files, setFiles] = useState<FileChange[]>([]);
  const [conflicts, setConflicts] = useState<string[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const load = async () => {
      try {
        const [c, f, cf] = await Promise.all([
          getPendingCommits(receptor, result.branchFeature),
          getChangedFiles(receptor, result.branchFeature),
          getConflicts(receptor, result.branchFeature),
        ]);
        setCommits(c);
        setFiles(f);
        setConflicts(cf);
      } catch { /* ignore */ }
      finally { setLoading(false); }
    };
    load();
  }, [receptor, result.branchFeature]);

  const tabs = [
    { label: 'Resumo', id: 0 },
    { label: `Commits (${commits.length})`, id: 1 },
    { label: `Arquivos (${files.length})`, id: 2 },
    { label: `Conflitos (${conflicts.length})`, id: 3 },
  ];

  return (
    <div className="fixed inset-0 bg-black/60 flex items-center justify-center z-50" onClick={onClose}>
      <div
        className="bg-bg-secondary border border-border rounded-lg w-[900px] h-[600px] flex flex-col shadow-2xl"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div className="flex items-center justify-between px-4 py-3 border-b border-border">
          <div>
            <span className="text-accent-blue font-bold text-sm">{result.branchFeature}</span>
            <span className="text-text-muted text-xs ml-3">→ {receptor}</span>
          </div>
          <button className="text-text-muted hover:text-text-primary" onClick={onClose}>
            <X size={18} />
          </button>
        </div>

        {/* Tabs */}
        <div className="flex border-b border-border">
          {tabs.map((t) => (
            <button
              key={t.id}
              className={`px-4 py-2 text-xs font-medium ${
                tab === t.id ? 'text-accent-blue border-b-2 border-accent-blue' : 'text-text-secondary hover:text-text-primary'
              }`}
              onClick={() => setTab(t.id)}
            >
              {t.label}
            </button>
          ))}
        </div>

        {/* Content */}
        <div className="flex-1 overflow-hidden">
          {loading ? (
            <div className="flex items-center justify-center h-full">
              <Loader2 className="animate-spin text-accent-blue mr-2" size={20} />
              <span className="text-text-secondary text-sm">Carregando detalhes...</span>
            </div>
          ) : (
            <>
              {tab === 0 && (
                <div className="p-6 space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <InfoItem label="Branch" value={result.branchFeature} />
                    <InfoItem label="Status" value={result.status} />
                    <InfoItem label="Commits Pendentes" value={String(result.commitsPendentes)} />
                    <InfoItem label="Conflitos Potenciais" value={String(result.conflitosArquivos)} />
                    <InfoItem label="Arquivos Alterados" value={String(result.arquivosAlterados)} />
                    <InfoItem label="Ultimo Autor" value={result.ultimoAutor} />
                    <InfoItem label="Ultimo Commit" value={result.ultimoCommit} className="col-span-2" />
                  </div>
                </div>
              )}
              {tab === 1 && (
                <div className="h-full flex flex-col p-2">
                  <DataTable data={commits} columns={commitColumns} />
                </div>
              )}
              {tab === 2 && (
                <div className="h-full flex flex-col p-2">
                  <DataTable data={files} columns={fileColumns} />
                </div>
              )}
              {tab === 3 && (
                <div className="p-4">
                  {conflicts.length === 0 ? (
                    <span className="text-accent-green text-sm">Nenhum conflito potencial detectado</span>
                  ) : (
                    <ul className="space-y-1">
                      {conflicts.map((f, i) => (
                        <li key={i} className="text-accent-red text-sm font-mono flex items-center gap-2">
                          <span className="w-2 h-2 rounded-full bg-accent-red inline-block" />
                          {f}
                        </li>
                      ))}
                    </ul>
                  )}
                </div>
              )}
            </>
          )}
        </div>
      </div>
    </div>
  );
}

function InfoItem({ label, value, className = '' }: { label: string; value: string; className?: string }) {
  return (
    <div className={className}>
      <span className="text-text-muted text-[11px] uppercase block">{label}</span>
      <span className="text-text-primary text-sm font-mono">{value || '-'}</span>
    </div>
  );
}
