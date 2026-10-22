import { useEffect, useState, type FormEvent } from 'react'
import { todoApi, type Todo } from './todoApi'

function TodoList() {
  const [todos, setTodos] = useState<Todo[]>([])
  const [title, setTitle] = useState('')
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    todoApi
      .getAll()
      .then(setTodos)
      .catch(() => setError('Görevler yüklenemedi.'))
      .finally(() => setLoading(false))
  }, [])

  async function handleAdd(e: FormEvent) {
    e.preventDefault()
    const trimmed = title.trim()
    if (!trimmed) return
    try {
      const created = await todoApi.create(trimmed)
      setTodos((prev) => [...prev, created])
      setTitle('')
      setError(null)
    } catch {
      setError('Görev eklenemedi.')
    }
  }

  async function handleToggle(todo: Todo) {
    try {
      const updated = await todoApi.update({ ...todo, isCompleted: !todo.isCompleted })
      setTodos((prev) => prev.map((t) => (t.id === updated.id ? updated : t)))
      setError(null)
    } catch {
      setError('Görev güncellenemedi.')
    }
  }

  async function handleDelete(id: number) {
    try {
      await todoApi.remove(id)
      setTodos((prev) => prev.filter((t) => t.id !== id))
      setError(null)
    } catch {
      setError('Görev silinemedi.')
    }
  }

  return (
    <section className="todos">
      <h2>Yapılacaklar</h2>
      <form onSubmit={handleAdd}>
        <input
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          placeholder="Yeni görev..."
          maxLength={200}
          aria-label="Yeni görev"
        />
        <button type="submit" disabled={!title.trim()}>
          Ekle
        </button>
      </form>
      {error && <p className="error">{error}</p>}
      {loading ? (
        <p className="checking">Yükleniyor...</p>
      ) : todos.length === 0 ? (
        <p className="checking">Henüz görev yok.</p>
      ) : (
        <ul>
          {todos.map((todo) => (
            <li key={todo.id}>
              <label className={todo.isCompleted ? 'done' : undefined}>
                <input
                  type="checkbox"
                  checked={todo.isCompleted}
                  onChange={() => handleToggle(todo)}
                />
                {todo.title}
              </label>
              <button type="button" onClick={() => handleDelete(todo.id)} aria-label="Sil">
                Sil
              </button>
            </li>
          ))}
        </ul>
      )}
    </section>
  )
}

export default TodoList
