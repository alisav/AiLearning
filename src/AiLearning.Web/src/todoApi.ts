export type Todo = {
  id: number
  title: string
  isCompleted: boolean
}

async function request<T>(url: string, init?: RequestInit): Promise<T> {
  const res = await fetch(url, {
    ...init,
    headers: { 'Content-Type': 'application/json', ...init?.headers },
  })
  if (!res.ok) throw new Error(`İstek başarısız: ${res.status}`)
  return res.status === 204 ? (undefined as T) : res.json()
}

export const todoApi = {
  getAll: () => request<Todo[]>('/api/todos'),
  create: (title: string) =>
    request<Todo>('/api/todos', {
      method: 'POST',
      body: JSON.stringify({ title }),
    }),
  update: (todo: Todo) =>
    request<Todo>(`/api/todos/${todo.id}`, {
      method: 'PUT',
      body: JSON.stringify({ title: todo.title, isCompleted: todo.isCompleted }),
    }),
  remove: (id: number) =>
    request<void>(`/api/todos/${id}`, { method: 'DELETE' }),
}
