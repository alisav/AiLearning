import { vi } from 'vitest'
import type { Todo } from '../todoApi'

const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/json' },
  })

/**
 * Replaces global fetch with an in-memory version of the backend API,
 * so tests never reach a real server or database.
 */
export function installFakeApi(initialTodos: Todo[] = []) {
  const todos = initialTodos.map((t) => ({ ...t }))
  let nextId = Math.max(0, ...todos.map((t) => t.id)) + 1

  const handler = async (input: RequestInfo | URL, init?: RequestInit) => {
    const url = String(input)
    const method = init?.method ?? 'GET'
    const idMatch = url.match(/^\/api\/todos\/(\d+)$/)

    if (url === '/api/health') return json({ status: 'ok' })

    if (url === '/api/todos' && method === 'GET') return json(todos)

    if (url === '/api/todos' && method === 'POST') {
      const { title } = JSON.parse(String(init?.body))
      const todo: Todo = { id: nextId++, title, isCompleted: false }
      todos.push(todo)
      return json(todo, 201)
    }

    if (idMatch) {
      const index = todos.findIndex((t) => t.id === Number(idMatch[1]))
      if (index < 0) return new Response(null, { status: 404 })

      if (method === 'PUT') {
        const { title, isCompleted } = JSON.parse(String(init?.body))
        todos[index] = { ...todos[index], title, isCompleted }
        return json(todos[index])
      }

      if (method === 'DELETE') {
        todos.splice(index, 1)
        return new Response(null, { status: 204 })
      }
    }

    return new Response(null, { status: 404 })
  }

  const fetchMock = vi.fn(handler)
  vi.stubGlobal('fetch', fetchMock)
  return { fetchMock, todos }
}
