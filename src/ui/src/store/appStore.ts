import { create } from 'zustand';
import type { BatchMergeResult } from '../api/types';

interface AppState {
  // Repo
  repoPath: string;
  repoUrl: string;
  currentBranch: string;
  setRepoPath: (path: string) => void;
  setRepoUrl: (url: string) => void;
  setCurrentBranch: (branch: string) => void;

  // Branches
  allBranches: string[];
  localBranches: string[];
  setAllBranches: (branches: string[]) => void;
  setLocalBranches: (branches: string[]) => void;

  // Status
  status: string;
  setStatus: (msg: string) => void;
  isFetching: boolean;
  setIsFetching: (v: boolean) => void;

  // Active tab
  activeTab: number;
  setActiveTab: (tab: number) => void;

  // Branch selectors
  branchA: string;
  branchB: string;
  setBranchA: (v: string) => void;
  setBranchB: (v: string) => void;

  // Batch results
  batchResults: BatchMergeResult[];
  setBatchResults: (results: BatchMergeResult[]) => void;
  addBatchResult: (result: BatchMergeResult) => void;
  clearBatchResults: () => void;
}

export const useAppStore = create<AppState>((set) => ({
  repoPath: '',
  repoUrl: '',
  currentBranch: '',
  setRepoPath: (path) => set({ repoPath: path }),
  setRepoUrl: (url) => set({ repoUrl: url }),
  setCurrentBranch: (branch) => set({ currentBranch: branch }),

  allBranches: [],
  localBranches: [],
  setAllBranches: (branches) => set({ allBranches: branches }),
  setLocalBranches: (branches) => set({ localBranches: branches }),

  status: 'Pronto. Selecione um repositorio para comecar.',
  setStatus: (msg) => set({ status: msg }),
  isFetching: false,
  setIsFetching: (v) => set({ isFetching: v }),

  activeTab: 0,
  setActiveTab: (tab) => set({ activeTab: tab }),

  branchA: '',
  branchB: '',
  setBranchA: (v) => set({ branchA: v }),
  setBranchB: (v) => set({ branchB: v }),

  batchResults: [],
  setBatchResults: (results) => set({ batchResults: results }),
  addBatchResult: (result) => set((s) => ({ batchResults: [...s.batchResults, result] })),
  clearBatchResults: () => set({ batchResults: [] }),
}));
