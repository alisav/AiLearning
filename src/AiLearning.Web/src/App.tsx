import { useEffect, useState } from 'react'
import TodoList from './TodoList'

type ApiStatus = 'checking' | 'connected' | 'error'

const statusText: Record<ApiStatus, string> = {
  checking: 'Kontrol ediliyor...',
  connected: 'Bağlandı',
  error: 'Bağlantı kurulamadı',
}

function App() {
  const [status, setStatus] = useState<ApiStatus>('checking')

  useEffect(() => {
    fetch('/api/health')
      .then((res) => (res.ok ? res.json() : Promise.reject(res.status)))
      .then((data: { status: string }) =>
        setStatus(data.status === 'ok' ? 'connected' : 'error'),
      )
      .catch(() => setStatus('error'))
  }, [])

  return (
    <main>
      <h1>AiLearning çalışıyor!</h1>
      <p>
        API durumu: <strong className={status}>{statusText[status]}</strong>
      </p>
      <TodoList />
    </main>
  )
}

export default App
