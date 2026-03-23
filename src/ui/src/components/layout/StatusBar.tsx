import { useAppStore } from '../../store/appStore';

export default function StatusBar() {
  const status = useAppStore((s) => s.status);
  return (
    <div className="h-[30px] bg-bg-secondary border-t border-border flex items-center px-4 shrink-0">
      <span className="text-text-secondary text-xs truncate">{status}</span>
    </div>
  );
}
