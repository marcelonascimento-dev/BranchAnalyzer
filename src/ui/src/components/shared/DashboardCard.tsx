interface Props {
  title: string;
  value: string | number;
  color: string; // tailwind text color class
  accentColor: string; // CSS color for left bar
}

export default function DashboardCard({ title, value, color, accentColor }: Props) {
  return (
    <div className="bg-bg-card border border-border rounded relative overflow-hidden min-w-[160px]">
      <div className="absolute left-0 top-0 bottom-0 w-1" style={{ backgroundColor: accentColor }} />
      <div className="px-4 py-3 pl-5">
        <div className={`text-2xl font-bold ${color}`}>{value}</div>
        <div className="text-[11px] text-text-muted uppercase tracking-wide mt-0.5">{title}</div>
      </div>
    </div>
  );
}
