import { useState, useEffect, useMemo } from 'react';
import { createColumnHelper } from '@tanstack/react-table';
import { useAppStore } from '../../../store/appStore';
import { getBranchesMetadata, batchAnalyze } from '../../../api/gitApi';
import type { BranchMetadata, BatchMergeResult } from '../../../api/types';
import DataTable from '../../shared/DataTable';
import DashboardCard from '../../shared/DashboardCard';
import BranchComboBox from '../../shared/BranchComboBox';
import DrillDownModal from './DrillDownModal';
import { FileDown, Loader2 } from 'lucide-react';

const col = createColumnHelper<BatchMergeResult>();
const columns = [
  col.accessor('branchFeature', { header: 'Branch Feature', size: 280 }),
  col.accessor('status', {
    header: 'Status', size: 130,
    cell: (info) => {
      const s = info.getValue();
      const styles: Record<string, string> = {
        MERGED: 'bg-accent-green/20 text-accent-green',
        PENDENTE: 'bg-accent-orange/20 text-accent-orange',
        ERRO: 'bg-accent-red/20 text-accent-red',
        'NAO ENCONTRADO': 'bg-accent-red/20 text-accent-red',
      };
      return <span className={`px-2 py-0.5 rounded text-[11px] font-bold ${styles[s] || ''}`}>{s}</span>;
    },
  }),
  col.accessor('commitsPendentes', { header: 'Commits Pend.', size: 110 }),
  col.accessor('conflitosArquivos', {
    header: 'Conflitos Pot.', size: 110,
    cell: (info) => {
      const v = info.getValue();
      return <span className={v > 0 ? 'text-accent-red font-bold' : ''}>{v}</span>;
    },
  }),
  col.accessor('arquivosAlterados', { header: 'Arq. Alterados', size: 110 }),
  col.accessor('ultimoAutor', { header: 'Ultimo Autor', size: 160 }),
  col.accessor('ultimoCommit', {
    header: 'Ultimo Commit', size: 250,
    cell: (i) => <span title={i.getValue()}>{i.getValue()}</span>,
  }),
];

export default function BatchTab() {
  const { repoPath, allBranches, localBranches, batchResults, addBatchResult, clearBatchResults, setStatus } = useAppStore();

  const [metadata, setMetadata] = useState<BranchMetadata[]>([]);
  const [receptor, setReceptor] = useState('');
  const [selected, setSelected] = useState<Set<string>>(new Set());
  const [analyzing, setAnalyzing] = useState(false);
  const [progress, setProgress] = useState({ current: 0, total: 0 });
  const [drillDown, setDrillDown] = useState<BatchMergeResult | null>(null);

  // Filters
  const [filterName, setFilterName] = useState('');
  const [filterAuthor, setFilterAuthor] = useState('');
  const [filterPrefix, setFilterPrefix] = useState('');
  const [filterPeriod, setFilterPeriod] = useState('');

  useEffect(() => {
    if (repoPath) {
      getBranchesMetadata().then(setMetadata).catch(() => {});
    }
  }, [repoPath]);

  const authors = useMemo(() => [...new Set(metadata.map((m) => m.author).filter(Boolean))].sort(), [metadata]);
  const prefixes = useMemo(() => [...new Set(metadata.map((m) => m.prefix).filter(Boolean))].sort(), [metadata]);

  const filtered = useMemo(() => {
    return metadata.filter((m) => {
      if (filterName && !m.shortName.toLowerCase().includes(filterName.toLowerCase())) return false;
      if (filterAuthor && m.author !== filterAuthor) return false;
      if (filterPrefix && m.prefix !== filterPrefix) return false;
      if (filterPeriod) {
        const days = parseInt(filterPeriod);
        const cutoff = new Date();
        cutoff.setDate(cutoff.getDate() - days);
        if (new Date(m.date) < cutoff) return false;
      }
      return true;
    });
  }, [metadata, filterName, filterAuthor, filterPrefix, filterPeriod]);

  const toggleBranch = (name: string) => {
    setSelected((prev) => {
      const next = new Set(prev);
      if (next.has(name)) next.delete(name); else next.add(name);
      return next;
    });
  };

  const selectAll = () => setSelected(new Set(filtered.map((m) => m.shortName)));
  const deselectAll = () => setSelected(new Set());
  const invertSel = () => {
    const all = new Set(filtered.map((m) => m.shortName));
    setSelected((prev) => {
      const next = new Set<string>();
      all.forEach((n) => { if (!prev.has(n)) next.add(n); });
      return next;
    });
  };

  const handleAnalyze = async () => {
    if (!receptor || selected.size === 0) return;
    setAnalyzing(true);
    clearBatchResults();
    setProgress({ current: 0, total: selected.size });
    setStatus('Analise em lote iniciada...');

    await batchAnalyze(receptor, [...selected], (event) => {
      if (event.type === 'result' && event.result) {
        addBatchResult(event.result);
        setProgress({ current: event.current || 0, total: event.total || selected.size });
      }
    });

    setAnalyzing(false);
    setStatus('Analise em lote concluida.');
  };

  const merged = batchResults.filter((r) => r.status === 'MERGED').length;
  const pending = batchResults.filter((r) => r.status === 'PENDENTE').length;
  const conflicts = batchResults.filter((r) => r.conflitosArquivos > 0).length;

  const exportTxt = () => {
    const lines = ['RELATORIO EM LOTE - VERIFICACAO DE MERGE', `Data: ${new Date().toLocaleString()}`, `Receptor: ${receptor}`, '', ''];
    batchResults.forEach((r) => {
      lines.push(`${r.branchFeature} | ${r.status} | ${r.commitsPendentes} commits | ${r.conflitosArquivos} conflitos | ${r.ultimoAutor}`);
    });
    lines.push('', `Total: ${batchResults.length} | Merged: ${merged} | Pendentes: ${pending} | Com conflitos: ${conflicts}`);
    downloadFile('batch_report.txt', lines.join('\n'), 'text/plain');
  };

  return (
    <div className="flex h-full">
      {/* Left panel - Filters + Branch list */}
      <div className="w-[350px] border-r border-border flex flex-col shrink-0 bg-bg-secondary">
        {/* Filters */}
        <div className="p-3 border-b border-border space-y-2">
          <div>
            <label className="text-[10px] text-text-muted uppercase">Filtrar por nome</label>
            <input
              className="w-full px-2 py-1 text-xs bg-bg-input border border-border rounded font-mono text-text-primary focus:outline-none focus:border-accent-blue"
              value={filterName}
              onChange={(e) => setFilterName(e.target.value)}
              placeholder="Buscar branch..."
            />
          </div>
          <div>
            <label className="text-[10px] text-text-muted uppercase">Autor</label>
            <select
              className="w-full px-2 py-1 text-xs bg-bg-input border border-border rounded text-text-primary"
              value={filterAuthor}
              onChange={(e) => setFilterAuthor(e.target.value)}
            >
              <option value="">(Todos os autores)</option>
              {authors.map((a) => <option key={a} value={a}>{a}</option>)}
            </select>
          </div>
          <div className="flex gap-2">
            <div className="flex-1">
              <label className="text-[10px] text-text-muted uppercase">Tipo</label>
              <select
                className="w-full px-2 py-1 text-xs bg-bg-input border border-border rounded text-text-primary"
                value={filterPrefix}
                onChange={(e) => setFilterPrefix(e.target.value)}
              >
                <option value="">(Todos)</option>
                {prefixes.map((p) => <option key={p} value={p}>{p}</option>)}
              </select>
            </div>
            <div className="flex-1">
              <label className="text-[10px] text-text-muted uppercase">Periodo</label>
              <select
                className="w-full px-2 py-1 text-xs bg-bg-input border border-border rounded text-text-primary"
                value={filterPeriod}
                onChange={(e) => setFilterPeriod(e.target.value)}
              >
                <option value="">Todos</option>
                <option value="7">7 dias</option>
                <option value="15">15 dias</option>
                <option value="30">30 dias</option>
                <option value="60">60 dias</option>
                <option value="90">90 dias</option>
              </select>
            </div>
          </div>
          <div className="flex gap-1">
            <button className="flex-1 px-2 py-1 text-[10px] bg-bg-tertiary border border-border rounded hover:bg-bg-hover text-text-secondary" onClick={selectAll}>Selec. Todos</button>
            <button className="flex-1 px-2 py-1 text-[10px] bg-bg-tertiary border border-border rounded hover:bg-bg-hover text-text-secondary" onClick={deselectAll}>Desmarcar</button>
            <button className="flex-1 px-2 py-1 text-[10px] bg-bg-tertiary border border-border rounded hover:bg-bg-hover text-text-secondary" onClick={invertSel}>Inverter</button>
          </div>
          <div className="text-[10px] text-text-muted">
            {selected.size} selecionado(s) | {filtered.length} listado(s)
          </div>
        </div>

        {/* Branch checklist */}
        <div className="flex-1 overflow-y-auto">
          {filtered.map((m) => (
            <label
              key={m.shortName}
              className="flex items-center gap-2 px-3 py-1 hover:bg-bg-hover cursor-pointer text-[12px] font-mono text-text-primary"
            >
              <input
                type="checkbox"
                checked={selected.has(m.shortName)}
                onChange={() => toggleBranch(m.shortName)}
                className="accent-accent-blue"
              />
              <span className="truncate">{m.shortName}</span>
            </label>
          ))}
        </div>
      </div>

      {/* Right panel */}
      <div className="flex-1 flex flex-col">
        {/* Top bar */}
        <div className="flex items-center gap-3 px-4 py-2 bg-bg-secondary border-b border-border shrink-0">
          <span className="text-accent-green text-xs font-bold">Receptor (A):</span>
          <div className="w-[280px]">
            <BranchComboBox value={receptor} onChange={setReceptor} branches={allBranches} localBranches={localBranches} placeholder="Selecione..." />
          </div>
          <button
            className="px-4 py-1.5 bg-btn-primary text-white text-xs font-bold rounded border border-accent-blue hover:bg-btn-hover disabled:opacity-50"
            onClick={handleAnalyze}
            disabled={analyzing || !receptor || selected.size === 0}
          >
            {analyzing ? <><Loader2 size={12} className="inline animate-spin mr-1" />ANALISANDO...</> : 'ANALISAR SELECIONADOS'}
          </button>
          <div className="ml-auto flex gap-2">
            <button className="px-2 py-1 text-[10px] bg-bg-tertiary border border-border rounded hover:bg-bg-hover text-text-secondary disabled:opacity-50" onClick={exportTxt} disabled={batchResults.length === 0}>
              <FileDown size={10} className="inline mr-1" />TXT
            </button>
          </div>
        </div>

        {/* Progress */}
        {analyzing && (
          <div className="px-4 py-1 bg-bg-secondary border-b border-border">
            <div className="w-full h-2 bg-bg-input rounded overflow-hidden">
              <div className="h-full bg-accent-blue transition-all" style={{ width: `${(progress.current / Math.max(progress.total, 1)) * 100}%` }} />
            </div>
            <span className="text-[10px] text-text-muted">{progress.current}/{progress.total}</span>
          </div>
        )}

        {/* Dashboard cards */}
        {batchResults.length > 0 && (
          <div className="flex gap-3 px-4 py-2 shrink-0">
            <DashboardCard title="Total" value={batchResults.length} color="text-accent-blue" accentColor="#78b4ff" />
            <DashboardCard title="Merged" value={merged} color="text-accent-green" accentColor="#50dc50" />
            <DashboardCard title="Pendentes" value={pending} color="text-accent-orange" accentColor="#ffb44c" />
            <DashboardCard title="Com Conflitos" value={conflicts} color="text-accent-red" accentColor="#dc5050" />
          </div>
        )}

        {/* Results table */}
        <div className="flex-1 overflow-hidden flex flex-col px-4 pb-2">
          <DataTable
            data={batchResults}
            columns={columns}
            onRowDoubleClick={setDrillDown}
            getRowClassName={(row) => {
              if (row.status === 'MERGED') return 'bg-accent-green/5';
              if (row.status === 'PENDENTE' && row.conflitosArquivos > 0) return 'bg-accent-red/5';
              if (row.status === 'PENDENTE') return 'bg-accent-orange/5';
              if (row.status === 'ERRO') return 'bg-accent-red/10';
              return '';
            }}
          />
        </div>
      </div>

      {/* Drill-down modal */}
      {drillDown && (
        <DrillDownModal result={drillDown} receptor={receptor} onClose={() => setDrillDown(null)} />
      )}
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
