let baseUrl = 'http://localhost:5391';

export function setBaseUrl(url: string) {
  baseUrl = url;
}

export function getBaseUrl() {
  return baseUrl;
}

export async function api<T>(path: string, options?: RequestInit): Promise<T> {
  const res = await fetch(`${baseUrl}${path}`, {
    headers: { 'Content-Type': 'application/json' },
    ...options,
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({ error: res.statusText }));
    throw new Error(err.error || res.statusText);
  }
  return res.json();
}

export function apiStream(path: string, body: unknown, onEvent: (data: Record<string, unknown>) => void): Promise<void> {
  return new Promise((resolve, reject) => {
    fetch(`${baseUrl}${path}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    })
      .then(async (res) => {
        if (!res.ok) {
          reject(new Error(res.statusText));
          return;
        }
        const reader = res.body?.getReader();
        if (!reader) { resolve(); return; }

        const decoder = new TextDecoder();
        let buffer = '';

        while (true) {
          const { done, value } = await reader.read();
          if (done) break;

          buffer += decoder.decode(value, { stream: true });
          const lines = buffer.split('\n');
          buffer = lines.pop() || '';

          for (const line of lines) {
            if (line.startsWith('data: ')) {
              try {
                const data = JSON.parse(line.slice(6));
                onEvent(data);
                if (data.type === 'done' || data.type === 'error') {
                  resolve();
                  return;
                }
              } catch { /* ignore parse errors */ }
            }
          }
        }
        resolve();
      })
      .catch(reject);
  });
}
