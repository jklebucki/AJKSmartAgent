import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter } from 'react-router-dom'
import { describe, expect, it } from 'vitest'
import { App } from './App'
import './shared/i18n/i18n'

describe('App', () => {
  it('renders the protected browser workspace shell and navigates to IFS endpoint management', async () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter>
          <App />
        </MemoryRouter>
      </QueryClientProvider>,
    )

    expect(screen.getByRole('heading', { name: 'Praxiara' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Przeglądarka' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Zatwierdzenie' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Środowiska IFS' })).toBeInTheDocument()

    await userEvent.click(screen.getByRole('link', { name: 'Endpointy IFS' }))

    expect(screen.getByRole('heading', { name: 'Zarządzanie endpointami IFS' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Skonfigurowane środowiska' })).toBeInTheDocument()
  })
})
