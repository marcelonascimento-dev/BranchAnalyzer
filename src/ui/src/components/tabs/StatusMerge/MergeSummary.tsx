import type { MergeStatus } from '../../../api/types';
import { Loader2 } from 'lucide-react';

interface Props {
  status: MergeStatus | null;
  branchA: string;
  branchB: string;
  loading: boolean;
}

export default function MergeSummary({ status, branchA, branchB, loading }: Props) {
  if (loading) {
    return (
      <div className="h-[140px] bg-bg-secondary flex items-center justify-center border-b border-border">
        <Loader2 className="animate-spin text-accent-blue mr-3" size={24} />
        <span className="text-text-secondary">Analisando {branchA} vs {branchB}...</span>
      </div>
    );
  }

  if (!status) {
    return (
      <div className="h-[140px] bg-bg-secondary flex items-center justify-center border-b border-border">
        <span className="text-text-muted text-sm">Selecione dois branches e clique em ANALISAR</span>
      </div>
    );
  }

  const isMerged = status.isMerged;
  const circleColor = isMerged ? 'bg-accent-green' : 'bg-accent-red';
  const statusText = isMerged
    ? `${branchB} ja esta MERGED em ${branchA}`
    : `${branchB} possui commits PENDENTES para ${branchA}`;

  return (
    <div className="h-[140px] bg-bg-secondary flex items-center gap-6 px-6 border-b border-border">
      {/* Status circle */}
      <div className={`w-16 h-16 rounded-full ${circleColor} opacity-90 shrink-0`} />

      {/* Info */}
      <div className="flex flex-col gap-1.5">
        <span className={`text-[15px] font-bold ${isMerged ? 'text-accent-green' : 'text-accent-red'}`}>
          {statusText}
        </span>
        <div className="flex gap-6 text-xs">
          <div>
            <span className="text-text-muted">Commits pendentes: </span>
            <span className={`font-bold ${status.pendingCommits > 0 ? 'text-accent-orange' : 'text-accent-green'}`}>
              {status.pendingCommits}
            </span>
          </div>
          <div>
            <span className="text-text-muted">Commits a frente: </span>
            <span className="text-accent-blue font-bold">{status.aheadCommits}</span>
          </div>
          <div>
            <span className="text-text-muted">Merge base: </span>
            <span className="text-accent-purple font-mono text-[11px]">{status.mergeBase?.substring(0, 12)}</span>
          </div>
        </div>
      </div>
    </div>
  );
}
