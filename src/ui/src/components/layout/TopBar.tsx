import { useState } from 'react';
import { GitBranch, Download, ChevronDown, ArrowLeftRight } from 'lucide-react';
import { useAppStore } from '../../store/appStore';
import { fetchOrigin, fetchPrune, getBranches, getCurrentBranch } from '../../api/gitApi';
import BranchComboBox from '../shared/BranchComboBox';

export default function TopBar() {
  const {
    repoPath, currentBranch, branchA, branchB,
    setBranchA, setBranchB, setStatus, isFetching, setIsFetching,
    allBranches, localBranches, setAllBranches, setLocalBranches, setCurrentBranch,
  } = useAppStore();

  const [showFetchMenu, setShowFetchMenu] = useState(false);

  const handleFetch = async (prune = false) => {
    setIsFetching(true);
    setStatus(prune ? 'Fetch + prune em andamento...' : 'Fetch em andamento...');
    try {
      if (prune) await fetchPrune(); else await fetchOrigin();
      const [branches, cur] = await Promise.all([getBranches(), getCurrentBranch()]);
      setAllBranches(branches.prioritized);
      setLocalBranches(branches.local);
      setCurrentBranch(cur.branch);
      setStatus('Fetch concluido. Branches atualizados.');
    } catch (e) {
      setStatus(`Erro no fetch: ${e}`);
    } finally {
      setIsFetching(false);
    }
  };

  const handleSwap = () => {
    const a = branchA;
    setBranchA(branchB);
    setBranchB(a);
  };

  return (
    <div className="h-[110px] bg-bg-secondary flex flex-col px-4 py-2 border-b border-border shrink-0">
      {/* Row 1: Title + Repo info + Buttons */}
      <div className="flex items-center justify-between h-[34px]">
        <div className="flex items-center gap-4">
          <h1 className="text-accent-blue font-bold text-[15px] tracking-wide">BRANCH ANALYZER</h1>
          <div className="flex items-center gap-2 text-xs">
            <span className="text-text-muted">Repo:</span>
            <span className="text-accent-orange font-mono text-[12px] max-w-[500px] truncate">
              {repoPath || '(nenhum)'}
            </span>
          </div>
          <div className="flex items-center gap-2 text-xs">
            <GitBranch size={12} className="text-text-muted" />
            <span className="text-accent-green font-mono text-[12px] font-bold">
              {currentBranch || ''}
            </span>
          </div>
        </div>

        <div className="flex items-center gap-1">
          <button
            className="px-3 py-1 text-xs bg-bg-tertiary text-text-primary border border-border rounded hover:bg-bg-hover transition"
            onClick={() => {/* TODO: RepoSelectDialog */}}
          >
            Selecionar Repo
          </button>
          <button
            className="px-3 py-1 text-xs bg-bg-tertiary text-text-primary border border-border rounded hover:bg-bg-hover transition disabled:opacity-50"
            onClick={() => handleFetch(false)}
            disabled={isFetching || !repoPath}
          >
            <Download size={12} className="inline mr-1" />
            Fetch
          </button>
          <div className="relative">
            <button
              className="px-1.5 py-1 text-xs bg-bg-tertiary text-text-secondary border border-border rounded hover:bg-bg-hover"
              onClick={() => setShowFetchMenu(!showFetchMenu)}
            >
              <ChevronDown size={12} />
            </button>
            {showFetchMenu && (
              <div className="absolute right-0 top-full mt-1 bg-bg-secondary border border-border rounded shadow-lg z-50 min-w-[200px]">
                <button
                  className="w-full text-left px-3 py-2 text-xs hover:bg-bg-hover text-text-primary"
                  onClick={() => { handleFetch(false); setShowFetchMenu(false); }}
                >
                  Fetch Origin (rapido)
                </button>
                <button
                  className="w-full text-left px-3 py-2 text-xs hover:bg-bg-hover text-text-primary"
                  onClick={() => { handleFetch(true); setShowFetchMenu(false); }}
                >
                  Fetch + Prune (completo)
                </button>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Row 2: Branch selectors */}
      <div className="flex items-end gap-2 mt-1 flex-1">
        <div className="flex-1">
          <label className="text-accent-green text-xs font-bold block mb-1">Receptor (A):</label>
          <BranchComboBox
            value={branchA}
            onChange={setBranchA}
            branches={allBranches}
            localBranches={localBranches}
            placeholder="Selecione o branch receptor..."
          />
        </div>

        <button
          className="px-2 py-1.5 bg-bg-tertiary text-accent-purple border border-border rounded hover:bg-bg-hover text-lg font-bold mb-0.5"
          onClick={handleSwap}
          title="Trocar branches"
        >
          <ArrowLeftRight size={18} />
        </button>

        <div className="flex-1">
          <label className="text-accent-orange text-xs font-bold block mb-1">Feature (B):</label>
          <BranchComboBox
            value={branchB}
            onChange={setBranchB}
            branches={allBranches}
            localBranches={localBranches}
            placeholder="Selecione o branch feature..."
          />
        </div>

        <button
          className="px-6 py-1.5 bg-btn-primary text-white font-bold text-sm rounded border border-accent-blue hover:bg-btn-hover transition mb-0.5"
          onClick={() => {
            const event = new CustomEvent('analyze-merge');
            window.dispatchEvent(event);
          }}
          disabled={!branchA || !branchB}
        >
          ANALISAR
        </button>
      </div>
    </div>
  );
}
