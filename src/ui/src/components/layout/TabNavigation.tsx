import { useAppStore } from '../../store/appStore';

const TABS = [
  { id: 0, label: 'Status Merge' },
  { id: 1, label: 'Lote (Multi-Branch)' },
  { id: 2, label: 'Meu Branch' },
  { id: 3, label: 'Branch Health' },
];

export default function TabNavigation() {
  const { activeTab, setActiveTab } = useAppStore();

  return (
    <div className="flex bg-bg-secondary border-b border-border shrink-0">
      {TABS.map((tab) => (
        <button
          key={tab.id}
          className={`px-5 py-2.5 text-sm font-medium transition-colors ${
            activeTab === tab.id
              ? 'text-accent-blue border-b-2 border-accent-blue bg-bg-primary'
              : 'text-text-secondary hover:text-text-primary hover:bg-bg-tertiary'
          }`}
          onClick={() => setActiveTab(tab.id)}
        >
          {tab.label}
        </button>
      ))}
    </div>
  );
}
