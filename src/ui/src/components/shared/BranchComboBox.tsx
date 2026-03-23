import { useState, useRef, useEffect } from 'react';
import { Circle } from 'lucide-react';

interface Props {
  value: string;
  onChange: (v: string) => void;
  branches: string[];
  localBranches: string[];
  placeholder?: string;
}

export default function BranchComboBox({ value, onChange, branches, localBranches, placeholder }: Props) {
  const [isOpen, setIsOpen] = useState(false);
  const [filter, setFilter] = useState('');
  const inputRef = useRef<HTMLInputElement>(null);
  const dropRef = useRef<HTMLDivElement>(null);

  const localSet = new Set(localBranches.map((b) => b.toLowerCase()));

  const filteredLocal = localBranches.filter((b) =>
    !filter || b.toLowerCase().includes(filter.toLowerCase())
  );
  const filteredRemote = branches.filter(
    (b) => !localSet.has(b.toLowerCase()) && (!filter || b.toLowerCase().includes(filter.toLowerCase()))
  ).slice(0, 30);

  useEffect(() => {
    const handleClick = (e: MouseEvent) => {
      if (dropRef.current && !dropRef.current.contains(e.target as Node) &&
          inputRef.current && !inputRef.current.contains(e.target as Node)) {
        setIsOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClick);
    return () => document.removeEventListener('mousedown', handleClick);
  }, []);

  const selectBranch = (branch: string) => {
    onChange(branch);
    setFilter('');
    setIsOpen(false);
  };

  return (
    <div className="relative">
      <input
        ref={inputRef}
        type="text"
        value={isOpen ? filter : value}
        placeholder={placeholder}
        className="w-full px-3 py-1.5 bg-bg-input text-text-primary border border-border rounded font-mono text-[13px] focus:outline-none focus:border-accent-blue"
        onFocus={() => { setIsOpen(true); setFilter(value); }}
        onChange={(e) => { setFilter(e.target.value); setIsOpen(true); }}
        onKeyDown={(e) => {
          if (e.key === 'Enter') {
            if (filteredLocal.length > 0) selectBranch(filteredLocal[0]);
            else if (filteredRemote.length > 0) selectBranch(filteredRemote[0]);
            else { onChange(filter); setIsOpen(false); }
          }
          if (e.key === 'Escape') setIsOpen(false);
        }}
      />
      {isOpen && (filteredLocal.length > 0 || filteredRemote.length > 0) && (
        <div
          ref={dropRef}
          className="absolute z-50 top-full left-0 right-0 mt-1 bg-bg-secondary border border-border rounded shadow-xl max-h-[300px] overflow-y-auto"
        >
          {filteredLocal.length > 0 && (
            <>
              <div className="px-3 py-1 text-[10px] text-text-muted font-bold uppercase border-b border-border bg-bg-tertiary">
                Locais (recentes)
              </div>
              {filteredLocal.map((b) => (
                <button
                  key={`local-${b}`}
                  className="w-full text-left px-3 py-1.5 text-[13px] font-mono hover:bg-bg-hover flex items-center gap-2 text-accent-green"
                  onClick={() => selectBranch(b)}
                >
                  <Circle size={7} fill="currentColor" /> {b}
                </button>
              ))}
            </>
          )}
          {filteredRemote.length > 0 && (
            <>
              <div className="px-3 py-1 text-[10px] text-text-muted font-bold uppercase border-b border-border bg-bg-tertiary">
                Remotos
              </div>
              {filteredRemote.map((b) => (
                <button
                  key={`remote-${b}`}
                  className="w-full text-left px-3 py-1.5 text-[13px] font-mono hover:bg-bg-hover text-text-primary"
                  onClick={() => selectBranch(b)}
                >
                  {b}
                </button>
              ))}
            </>
          )}
        </div>
      )}
    </div>
  );
}
