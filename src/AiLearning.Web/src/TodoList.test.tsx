import { cleanup, render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, describe, expect, it, vi } from 'vitest'
import TodoList from './TodoList'
import { installFakeApi } from './test/fakeApi'

afterEach(() => {
  cleanup()
  vi.unstubAllGlobals()
})

const rowOf = (title: string) => screen.getByText(title).closest('li')!

describe('TodoList', () => {
  it('lists todos returned by the API', async () => {
    installFakeApi([
      { id: 1, title: 'Süt al', isCompleted: false },
      { id: 2, title: 'Kod yaz', isCompleted: true },
    ])

    render(<TodoList />)

    expect(await screen.findByText('Süt al')).toBeTruthy()
    expect(within(rowOf('Süt al')).getByRole<HTMLInputElement>('checkbox').checked).toBe(false)
    expect(within(rowOf('Kod yaz')).getByRole<HTMLInputElement>('checkbox').checked).toBe(true)
  })

  it('shows an empty message when there are no todos', async () => {
    installFakeApi()

    render(<TodoList />)

    expect(await screen.findByText('Henüz görev yok.')).toBeTruthy()
  })

  it('shows an error when todos cannot be loaded', async () => {
    vi.stubGlobal('fetch', vi.fn(async () => new Response(null, { status: 500 })))

    render(<TodoList />)

    expect(await screen.findByText('Görevler yüklenemedi.')).toBeTruthy()
  })

  it('adds a todo with a trimmed title and clears the input', async () => {
    const user = userEvent.setup()
    const { fetchMock, todos } = installFakeApi()
    render(<TodoList />)
    await screen.findByText('Henüz görev yok.')

    const input = screen.getByRole<HTMLInputElement>('textbox', { name: 'Yeni görev' })
    await user.type(input, '  Ekmek al  ')
    await user.click(screen.getByRole('button', { name: 'Ekle' }))

    expect(await screen.findByText('Ekmek al')).toBeTruthy()
    expect(input.value).toBe('')
    expect(fetchMock).toHaveBeenCalledWith(
      '/api/todos',
      expect.objectContaining({ method: 'POST', body: JSON.stringify({ title: 'Ekmek al' }) }),
    )
    expect(todos).toEqual([{ id: 1, title: 'Ekmek al', isCompleted: false }])
  })

  it('disables the add button for a blank title', async () => {
    const user = userEvent.setup()
    const { fetchMock } = installFakeApi()
    render(<TodoList />)
    await screen.findByText('Henüz görev yok.')

    await user.type(screen.getByRole('textbox', { name: 'Yeni görev' }), '   ')

    expect(screen.getByRole<HTMLButtonElement>('button', { name: 'Ekle' }).disabled).toBe(true)
    expect(fetchMock).toHaveBeenCalledTimes(1) // only the initial GET
  })

  it('deletes only the selected todo', async () => {
    const user = userEvent.setup()
    const { fetchMock, todos } = installFakeApi([
      { id: 1, title: 'Süt al', isCompleted: false },
      { id: 2, title: 'Kod yaz', isCompleted: false },
    ])
    render(<TodoList />)
    await screen.findByText('Süt al')

    await user.click(within(rowOf('Süt al')).getByRole('button', { name: 'Sil' }))

    await vi.waitFor(() => expect(screen.queryByText('Süt al')).toBeNull())
    expect(screen.getByText('Kod yaz')).toBeTruthy()
    expect(fetchMock).toHaveBeenCalledWith('/api/todos/1', expect.objectContaining({ method: 'DELETE' }))
    expect(todos.map((t) => t.id)).toEqual([2])
  })

  it('keeps the todo and shows an error when delete fails', async () => {
    const user = userEvent.setup()
    const { fetchMock } = installFakeApi([{ id: 1, title: 'Süt al', isCompleted: false }])
    render(<TodoList />)
    await screen.findByText('Süt al')
    fetchMock.mockImplementationOnce(async () => new Response(null, { status: 500 }))

    await user.click(within(rowOf('Süt al')).getByRole('button', { name: 'Sil' }))

    expect(await screen.findByText('Görev silinemedi.')).toBeTruthy()
    expect(screen.getByText('Süt al')).toBeTruthy()
  })

  it('marks a todo as completed and back as not completed', async () => {
    const user = userEvent.setup()
    const { fetchMock, todos } = installFakeApi([{ id: 1, title: 'Süt al', isCompleted: false }])
    render(<TodoList />)
    await screen.findByText('Süt al')
    const checkbox = () => within(rowOf('Süt al')).getByRole<HTMLInputElement>('checkbox')

    await user.click(checkbox())

    await vi.waitFor(() => expect(checkbox().checked).toBe(true))
    expect(rowOf('Süt al').querySelector('label')!.className).toBe('done')
    expect(fetchMock).toHaveBeenLastCalledWith(
      '/api/todos/1',
      expect.objectContaining({
        method: 'PUT',
        body: JSON.stringify({ title: 'Süt al', isCompleted: true }),
      }),
    )
    expect(todos).toEqual([{ id: 1, title: 'Süt al', isCompleted: true }])

    await vi.waitFor(() => expect(checkbox().disabled).toBe(false))
    await user.click(checkbox())

    await vi.waitFor(() => expect(checkbox().checked).toBe(false))
    expect(rowOf('Süt al').querySelector('label')!.className).toBe('')
    expect(fetchMock).toHaveBeenLastCalledWith(
      '/api/todos/1',
      expect.objectContaining({
        method: 'PUT',
        body: JSON.stringify({ title: 'Süt al', isCompleted: false }),
      }),
    )
    expect(todos).toEqual([{ id: 1, title: 'Süt al', isCompleted: false }])
  })

  it('keeps the previous state and shows an error when the update fails', async () => {
    const user = userEvent.setup()
    const { fetchMock } = installFakeApi([{ id: 1, title: 'Süt al', isCompleted: false }])
    render(<TodoList />)
    await screen.findByText('Süt al')
    fetchMock.mockImplementationOnce(async () => new Response(null, { status: 500 }))

    await user.click(within(rowOf('Süt al')).getByRole('checkbox'))

    expect(await screen.findByText('Görev güncellenemedi.')).toBeTruthy()
    const checkbox = within(rowOf('Süt al')).getByRole<HTMLInputElement>('checkbox')
    expect(checkbox.checked).toBe(false)
    expect(checkbox.disabled).toBe(false)
  })

  it('disables the checkbox while the update is in flight', async () => {
    const user = userEvent.setup()
    const { fetchMock } = installFakeApi([
      { id: 1, title: 'Süt al', isCompleted: false },
      { id: 2, title: 'Kod yaz', isCompleted: false },
    ])
    render(<TodoList />)
    await screen.findByText('Süt al')
    let respond!: (response: Response) => void
    fetchMock.mockImplementationOnce(() => new Promise<Response>((resolve) => (respond = resolve)))
    const checkbox = () => within(rowOf('Süt al')).getByRole<HTMLInputElement>('checkbox')

    await user.click(checkbox())

    expect(checkbox().disabled).toBe(true)
    expect(within(rowOf('Kod yaz')).getByRole<HTMLInputElement>('checkbox').disabled).toBe(false)
    await user.click(checkbox())
    expect(fetchMock).toHaveBeenCalledTimes(2) // initial GET + a single PUT

    respond(Response.json({ id: 1, title: 'Süt al', isCompleted: true }))

    await vi.waitFor(() => expect(checkbox().disabled).toBe(false))
    expect(checkbox().checked).toBe(true)
  })
})
