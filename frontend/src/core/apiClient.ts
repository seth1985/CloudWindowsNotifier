export async function apiGet<T>(url: string, token: string): Promise<T> {
  const res = await fetch(url, {
    headers: { Authorization: `Bearer ${token}` }
  });
  if (!res.ok) {
    const txt = await res.text();
    throw new Error(`GET ${url} failed (${res.status}): ${txt}`);
  }
  return res.json();
}

export async function apiPostJson<TReq, TRes>(url: string, token: string | null, body: TReq): Promise<TRes> {
  const res = await fetch(url, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {})
    },
    body: JSON.stringify(body)
  });
  if (!res.ok) {
    const txt = await res.text();
    throw new Error(`POST ${url} failed (${res.status}): ${txt}`);
  }
  return res.json();
}

export async function apiPostForm(url: string, token: string | null, form: FormData): Promise<any> {
  const res = await fetch(url, {
    method: 'POST',
    headers: {
      ...(token ? { Authorization: `Bearer ${token}` } : {})
    },
    body: form
  });
  if (!res.ok) {
    const txt = await res.text();
    throw new Error(`POST ${url} failed (${res.status}): ${txt}`);
  }
  return res.json();
}
