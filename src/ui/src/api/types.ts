export interface MergeStatus {
  isMerged: boolean;
  pendingCommits: number;
  aheadCommits: number;
  mergeBase: string;
}

export interface CommitInfo {
  hash: string;
  author: string;
  relativeDate: string;
  date: string;
  message: string;
}

export interface FileChange {
  status: string;
  statusCode: string;
  filePath: string;
}

export interface ContributorInfo {
  name: string;
  email: string;
  commitCount: number;
}

export interface DiffStats {
  summary: string;
  filesByExtension: Record<string, number>;
}

export interface BranchInfo {
  divergenceDate: string;
  firstCommitDate: string;
  lastCommitDate: string;
  lastCommitAuthor: string;
  lastCommitMessage: string;
}

export interface LargeCommit {
  hash: string;
  linesChanged: number;
  author: string;
  message: string;
}

export interface RemoteBranch {
  name: string;
  date: string;
  author: string;
  lastCommit: string;
}

export interface BranchMetadata {
  fullName: string;
  shortName: string;
  dateShort: string;
  date: string;
  author: string;
  prefix: string;
}

export interface BatchMergeResult {
  branchFeature: string;
  status: string;
  commitsPendentes: number;
  conflitosArquivos: number;
  arquivosAlterados: number;
  ultimoAutor: string;
  ultimoCommit: string;
  isMerged: boolean;
}

export interface BranchesResponse {
  prioritized: string[];
  local: string[];
}

export interface MyBranchInfo {
  branch: string;
  ahead: number;
  behind: number;
  recentCommits: CommitInfo[];
  localChanges: FileChange[];
  stashes: string[];
  localBranches: RemoteBranch[];
}

export interface BranchHealthItem {
  shortName: string;
  fullName: string;
  dateShort: string;
  date: string;
  author: string;
  prefix: string;
  daysInactive: number;
  status: string;
}

export interface BranchHealthResponse {
  total: number;
  active: number;
  stale: number;
  obsolete: number;
  branches: BranchHealthItem[];
}

export interface AppSettings {
  lastRepoPath: string;
  lastRepoUrl?: string;
  recentRepoPaths: string[];
  lastBranchA: string;
  lastBranchB: string;
  lastBatchReceptor: string;
  windowWidth: number;
  windowHeight: number;
  windowX: number;
  windowY: number;
  windowMaximized: boolean;
  lastSelectedTab: number;
  fetchOnOpen: boolean;
  cloneCachePath: string;
}
