import { render, screen } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, expect, it } from 'vitest'
import { App } from './App'
import './shared/i18n/i18n'

describe('App', () => {
  it('renders the protected browser workspace shell', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={queryClient}>
        <App />
      </QueryClientProvider>,
    )

    expect(screen.getByRole('heading', { name: 'Praxiara' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Przeglądarka' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Zatwierdzenie' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Środowiska IFS' })).toBeInTheDocument()
  })
})
