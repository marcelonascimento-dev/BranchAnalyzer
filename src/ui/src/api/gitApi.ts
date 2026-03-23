import { api, apiStream } from './client';
import type {
  MergeStatus, CommitInfo, FileChange, BranchInfo,
  BranchesResponse, BranchMetadata, BatchMergeResult,
  MyBranchInfo, BranchHealthResponse, AppSettings,
} from './types';

// ── Repo ──
export const setRepo = (path: string) =>
  api<{ path: string }>('/api/repo/set', { method: 'POST', body: JSON.stringify({ path }) });

export const getSettings = () =>
  api<AppSettings>('/api/repo/settings');

export const saveSettings = (settings: Partial<AppSettings>) =>
  api<AppSettings>('/api/repo/settings', { method: 'POST', body: JSON.stringify(settings) });

export const validateUrl = (url: string) =>
  api<{ ok: boolean; error: string }>('/api/repo/validate-url', { method: 'POST', body: JSON.stringify({ url }) });

export const cloneRepo = (url: string, cachePath: string | null, onEvent: (data: Record<string, unknown>) => void) =>
  apiStream('/api/repo/clone', { url, cachePath }, onEvent);

// ── Branches ──
export const getBranches = () =>
  api<BranchesResponse>('/api/branches');

export const getBranchesMetadata = () =>
  api<BranchMetadata[]>('/api/branches/metadata');

export const getCurrentBranch = () =>
  api<{ branch: string }>('/api/branches/current');

export const resolveBranch = (name: string) =>
  api<{ resolved: string }>(`/api/branches/resolve?name=${encodeURIComponent(name)}`);

export const fetchOrigin = () =>
  api<{ message: string }>('/api/branches/fetch', { method: 'POST' });

export const fetchPrune = () =>
  api<{ message: string }>('/api/branches/fetch-prune', { method: 'POST' });

export const getLocalBranchesInfo = () =>
  api<{ name: string; date: string; author: string; lastCommit: string }[]>('/api/branches/local-info');

export const getAheadBehind = () =>
  api<{ ahead: number; behind: number }>('/api/branches/ahead-behind');

// ── Merge ──
export const getMergeStatus = (a: string, b: string) =>
  api<MergeStatus>(`/api/merge/status?a=${enc(a)}&b=${enc(b)}`);

export const getPendingCommits = (a: string, b: string) =>
  api<CommitInfo[]>(`/api/merge/pending-commits?a=${enc(a)}&b=${enc(b)}`);

export const getChangedFiles = (a: string, b: string) =>
  api<FileChange[]>(`/api/merge/changed-files?a=${enc(a)}&b=${enc(b)}`);

export const getConflicts = (a: string, b: string) =>
  api<string[]>(`/api/merge/conflicts?a=${enc(a)}&b=${enc(b)}`);

export const getBranchInfo = (a: string, b: string) =>
  api<BranchInfo>(`/api/merge/branch-info?a=${enc(a)}&b=${enc(b)}`);

// ── Batch ──
export const batchAnalyze = (
  receptor: string,
  branches: string[],
  onEvent: (data: { type: string; current?: number; total?: number; result?: BatchMergeResult; message?: string }) => void
) => apiStream('/api/batch/analyze', { receptor, branches }, onEvent as (data: Record<string, unknown>) => void);

// ── My Branch ──
export const getMyBranchInfo = () =>
  api<MyBranchInfo>('/api/mybranch/info');

// ── Health ──
export const getBranchHealth = () =>
  api<BranchHealthResponse>('/api/health/branches');

// ── Helper ──
const enc = encodeURIComponent;
