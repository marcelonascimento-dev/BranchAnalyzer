import { useEffect } from 'react';
import { useAppStore } from './store/appStore';
import { getSettings, getBranches, getCurrentBranch, setRepo, fetchOrigin } from './api/gitApi';
import TopBar from './components/layout/TopBar';
import TabNavigation from './components/layout/TabNavigation';
import StatusBar from './components/layout/StatusBar';
import StatusMergeTab from './components/tabs/StatusMerge/StatusMergeTab';
import BatchTab from './components/tabs/Batch/BatchTab';
import MyBranchTab from './components/tabs/MyBranch/MyBranchTab';
import BranchHealthTab from './components/tabs/BranchHealth/BranchHealthTab';

function App() {
  const {
    activeTab, setRepoPath, setCurrentBranch, setAllBranches,
    setLocalBranches, setBranchA, setBranchB, setStatus, setActiveTab,
    setIsFetching,
  } = useAppStore();

  useEffect(() => {
    const init = async () => {
      try {
        const settings = await getSettings();
        if (settings.lastRepoPath) {
          await setRepo(settings.lastRepoPath);
          setRepoPath(settings.lastRepoPath);
          setStatus('Repositorio carregado. Buscando branches...');

          // Auto-fetch
          if (settings.fetchOnOpen) {
            setIsFetching(true);
            try { await fetchOrigin(); } catch { /* ignore */ }
            setIsFetching(false);
          }

          const [branches, cur] = await Promise.all([
            getBranches(),
            getCurrentBranch(),
          ]);
          setAllBranches(branches.prioritized);
          setLocalBranches(branches.local);
          setCurrentBranch(cur.branch);

          if (settings.lastBranchA) setBranchA(settings.lastBranchA);
          if (settings.lastBranchB) setBranchB(settings.lastBranchB);
          if (settings.lastSelectedTab >= 0) setActiveTab(settings.lastSelectedTab);

          setStatus(`Pronto. ${branches.prioritized.length} branches carregados.`);
        }
      } catch {
        setStatus('API nao disponivel. Inicie o backend C#.');
      }
    };
    init();
  }, []);

  const renderTab = () => {
    switch (activeTab) {
      case 0: return <StatusMergeTab />;
      case 1: return <BatchTab />;
      case 2: return <MyBranchTab />;
      case 3: return <BranchHealthTab />;
      default: return <StatusMergeTab />;
    }
  };

  return (
    <>
      <TopBar />
      <TabNavigation />
      <div className="flex-1 overflow-hidden">
        {renderTab()}
      </div>
      <StatusBar />
    </>
  );
}

export default App;
